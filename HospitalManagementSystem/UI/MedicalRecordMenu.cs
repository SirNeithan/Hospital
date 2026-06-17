using HospitalManagementSystem.Models;
using HospitalManagementSystem.Services;

namespace HospitalManagementSystem.UI;

public class MedicalRecordMenu
{
    private readonly MedicalRecordService _recordService;
    private readonly PatientService _patientService;
    private readonly DoctorService _doctorService;
    private readonly AppointmentService _appointmentService;

    public MedicalRecordMenu(MedicalRecordService recordService, PatientService patientService,
        DoctorService doctorService, AppointmentService appointmentService)
    {
        _recordService = recordService;
        _patientService = patientService;
        _doctorService = doctorService;
        _appointmentService = appointmentService;
    }

    public void Show()
    {
        while (true)
        {
            ConsoleHelper.PrintHeader("Medical Records");
            Console.WriteLine("  1. Create Medical Record");
            Console.WriteLine("  2. View Patient Medical History");
            Console.WriteLine("  3. View Record Details");
            Console.WriteLine("  4. View All Records");
            Console.WriteLine("  0. Back to Main Menu");
            Console.WriteLine();

            string choice = ConsoleHelper.ReadInput("Select option");
            switch (choice)
            {
                case "1": CreateRecord(); break;
                case "2": ViewPatientHistory(); break;
                case "3": ViewDetails(); break;
                case "4": ViewAll(); break;
                case "0": return;
                default: ConsoleHelper.PrintError("Invalid option."); break;
            }
        }
    }

    private void CreateRecord()
    {
        ConsoleHelper.PrintSubHeader("Create Medical Record");

        Console.WriteLine("\n  Registered Patients:");
        foreach (var p in _patientService.GetAll())
            Console.WriteLine($"    {p}");
        int patientId = ConsoleHelper.ReadInt("Patient ID");

        Console.WriteLine("\n  Doctors:");
        foreach (var d in _doctorService.GetAll())
            Console.WriteLine($"    {d}");
        int doctorId = ConsoleHelper.ReadInt("Doctor ID");

        // Optionally link to appointment
        int? appointmentId = null;
        if (ConsoleHelper.Confirm("Link to an appointment?"))
        {
            int apptId = ConsoleHelper.ReadInt("Appointment ID");
            var appt = _appointmentService.GetById(apptId);
            if (appt != null && appt.PatientId == patientId)
                appointmentId = apptId;
            else
                ConsoleHelper.PrintWarning("Appointment not found or doesn't match patient. Skipping link.");
        }

        string diagnosis = ConsoleHelper.ReadInput("Diagnosis");
        string symptoms = ConsoleHelper.ReadInput("Symptoms");
        string treatmentPlan = ConsoleHelper.ReadInput("Treatment Plan");
        string labResults = ConsoleHelper.ReadOptionalInput("Lab Results (optional)");
        string allergies = ConsoleHelper.ReadOptionalInput("Allergies (optional)");
        string notes = ConsoleHelper.ReadOptionalInput("Additional Notes (optional)");

        // Prescriptions
        var prescriptions = new List<Prescription>();
        while (ConsoleHelper.Confirm("Add a prescription?"))
        {
            var rx = new Prescription
            {
                MedicineName = ConsoleHelper.ReadInput("Medicine Name"),
                Dosage = ConsoleHelper.ReadInput("Dosage (e.g. 500mg)"),
                Frequency = ConsoleHelper.ReadInput("Frequency (e.g. twice daily)"),
                DurationDays = ConsoleHelper.ReadInt("Duration (days)", 1, 365),
                Instructions = ConsoleHelper.ReadOptionalInput("Special Instructions")
            };
            prescriptions.Add(rx);
        }

        var record = _recordService.CreateRecord(patientId, doctorId, appointmentId,
            diagnosis, symptoms, treatmentPlan, prescriptions, labResults, allergies, notes);

        ConsoleHelper.PrintSuccess($"Medical record #{record.Id} created successfully.");
        ConsoleHelper.PressAnyKey();
    }

    private void ViewPatientHistory()
    {
        ConsoleHelper.PrintSubHeader("Patient Medical History");
        int patientId = ConsoleHelper.ReadInt("Patient ID");
        var patient = _patientService.GetById(patientId);
        if (patient == null) { ConsoleHelper.PrintError("Patient not found."); ConsoleHelper.PressAnyKey(); return; }

        ConsoleHelper.PrintInfo($"Medical history for {patient.FullName}:");
        var records = _recordService.GetByPatient(patientId);
        if (records.Count == 0)
        {
            ConsoleHelper.PrintInfo("No medical records found.");
        }
        else
        {
            foreach (var r in records)
            {
                ConsoleHelper.PrintSeparator();
                Console.WriteLine($"  Record #{r.Id} | {r.RecordDate:dd/MM/yyyy HH:mm}");
                Console.WriteLine($"  Doctor:    {r.DoctorName}");
                Console.WriteLine($"  Diagnosis: {r.Diagnosis}");
                Console.WriteLine($"  Symptoms:  {r.Symptoms}");
                Console.WriteLine($"  Treatment: {r.TreatmentPlan}");
                if (r.Prescriptions.Count > 0)
                {
                    Console.WriteLine("  Prescriptions:");
                    foreach (var rx in r.Prescriptions)
                        Console.WriteLine($"    • {rx}");
                }
                if (!string.IsNullOrEmpty(r.Allergies))
                    Console.WriteLine($"  Allergies: {r.Allergies}");
                if (!string.IsNullOrEmpty(r.LabResults))
                    Console.WriteLine($"  Lab:       {r.LabResults}");
            }
        }
        ConsoleHelper.PrintSeparator();
        ConsoleHelper.PressAnyKey();
    }

    private void ViewDetails()
    {
        ConsoleHelper.PrintSubHeader("Record Details");
        int id = ConsoleHelper.ReadInt("Record ID");
        var r = _recordService.GetById(id);
        if (r == null) { ConsoleHelper.PrintError("Record not found."); ConsoleHelper.PressAnyKey(); return; }

        ConsoleHelper.PrintSeparator();
        Console.WriteLine($"  Record ID:     {r.Id}");
        Console.WriteLine($"  Date:          {r.RecordDate:dd/MM/yyyy HH:mm}");
        Console.WriteLine($"  Patient:       {r.PatientName}");
        Console.WriteLine($"  Doctor:        {r.DoctorName}");
        if (r.AppointmentId.HasValue)
            Console.WriteLine($"  Appointment:   #{r.AppointmentId}");
        Console.WriteLine($"  Diagnosis:     {r.Diagnosis}");
        Console.WriteLine($"  Symptoms:      {r.Symptoms}");
        Console.WriteLine($"  Treatment:     {r.TreatmentPlan}");
        if (!string.IsNullOrEmpty(r.Allergies)) Console.WriteLine($"  Allergies:     {r.Allergies}");
        if (!string.IsNullOrEmpty(r.LabResults)) Console.WriteLine($"  Lab Results:   {r.LabResults}");
        if (!string.IsNullOrEmpty(r.Notes)) Console.WriteLine($"  Notes:         {r.Notes}");
        if (r.Prescriptions.Count > 0)
        {
            Console.WriteLine("  Prescriptions:");
            foreach (var rx in r.Prescriptions)
                Console.WriteLine($"    • {rx}");
        }
        ConsoleHelper.PrintSeparator();
        ConsoleHelper.PressAnyKey();
    }

    private void ViewAll()
    {
        ConsoleHelper.PrintSubHeader("All Medical Records");
        var records = _recordService.GetAll();
        if (records.Count == 0)
            ConsoleHelper.PrintInfo("No records found.");
        else
        {
            foreach (var r in records)
                Console.WriteLine($"  {r}");
            ConsoleHelper.PrintInfo($"Total: {records.Count} records");
        }
        ConsoleHelper.PressAnyKey();
    }
}
