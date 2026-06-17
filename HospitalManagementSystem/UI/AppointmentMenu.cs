using HospitalManagementSystem.Models;
using HospitalManagementSystem.Services;

namespace HospitalManagementSystem.UI;

public class AppointmentMenu
{
    private readonly AppointmentService _appointmentService;
    private readonly PatientService _patientService;
    private readonly DoctorService _doctorService;

    public AppointmentMenu(AppointmentService appointmentService,
        PatientService patientService, DoctorService doctorService)
    {
        _appointmentService = appointmentService;
        _patientService = patientService;
        _doctorService = doctorService;
    }

    public void Show()
    {
        while (true)
        {
            ConsoleHelper.PrintHeader("Appointment Management");
            Console.WriteLine("  1. Schedule Appointment");
            Console.WriteLine("  2. View Today's Appointments");
            Console.WriteLine("  3. View Upcoming Appointments (7 days)");
            Console.WriteLine("  4. View All Appointments");
            Console.WriteLine("  5. View Patient Appointments");
            Console.WriteLine("  6. View Doctor Appointments");
            Console.WriteLine("  7. Update Appointment Status");
            Console.WriteLine("  8. Cancel Appointment");
            Console.WriteLine("  0. Back to Main Menu");
            Console.WriteLine();

            string choice = ConsoleHelper.ReadInput("Select option");
            switch (choice)
            {
                case "1": Schedule(); break;
                case "2": ViewToday(); break;
                case "3": ViewUpcoming(); break;
                case "4": ViewAll(); break;
                case "5": ViewByPatient(); break;
                case "6": ViewByDoctor(); break;
                case "7": UpdateStatus(); break;
                case "8": CancelAppointment(); break;
                case "0": return;
                default: ConsoleHelper.PrintError("Invalid option."); break;
            }
        }
    }

    private void Schedule()
    {
        ConsoleHelper.PrintSubHeader("Schedule Appointment");

        Console.WriteLine("\n  Registered Patients:");
        foreach (var p in _patientService.GetAll())
            Console.WriteLine($"    {p}");

        int patientId = ConsoleHelper.ReadInt("Patient ID");

        Console.WriteLine("\n  Available Doctors:");
        foreach (var d in _doctorService.GetAll().Where(d => d.IsAvailable))
            Console.WriteLine($"    {d}");

        int doctorId = ConsoleHelper.ReadInt("Doctor ID");
        DateTime dateTime = ConsoleHelper.ReadDateTime("Appointment Date & Time");
        string reason = ConsoleHelper.ReadInput("Reason for visit");
        string notes = ConsoleHelper.ReadOptionalInput("Additional notes (optional)");

        var (success, message, appointment) = _appointmentService.ScheduleAppointment(
            patientId, doctorId, dateTime, reason, notes);

        if (success)
            ConsoleHelper.PrintSuccess($"Appointment #{appointment!.Id} scheduled for {appointment.AppointmentDate:dd/MM/yyyy HH:mm}");
        else
            ConsoleHelper.PrintError(message);
        ConsoleHelper.PressAnyKey();
    }

    private void ViewToday()
    {
        ConsoleHelper.PrintSubHeader($"Today's Appointments ({DateTime.Today:dd/MM/yyyy})");
        var appts = _appointmentService.GetToday();
        PrintAppointments(appts);
        ConsoleHelper.PressAnyKey();
    }

    private void ViewUpcoming()
    {
        ConsoleHelper.PrintSubHeader("Upcoming Appointments (Next 7 Days)");
        var appts = _appointmentService.GetUpcoming(7);
        PrintAppointments(appts);
        ConsoleHelper.PressAnyKey();
    }

    private void ViewAll()
    {
        ConsoleHelper.PrintSubHeader("All Appointments");
        var appts = _appointmentService.GetAll().OrderByDescending(a => a.AppointmentDate).ToList();
        PrintAppointments(appts);
        ConsoleHelper.PressAnyKey();
    }

    private void ViewByPatient()
    {
        ConsoleHelper.PrintSubHeader("Patient Appointments");
        int patientId = ConsoleHelper.ReadInt("Patient ID");
        var patient = _patientService.GetById(patientId);
        if (patient == null) { ConsoleHelper.PrintError("Patient not found."); ConsoleHelper.PressAnyKey(); return; }

        ConsoleHelper.PrintInfo($"Appointments for {patient.FullName}:");
        var appts = _appointmentService.GetByPatient(patientId);
        PrintAppointments(appts);
        ConsoleHelper.PressAnyKey();
    }

    private void ViewByDoctor()
    {
        ConsoleHelper.PrintSubHeader("Doctor Appointments");
        int doctorId = ConsoleHelper.ReadInt("Doctor ID");
        var doctor = _doctorService.GetById(doctorId);
        if (doctor == null) { ConsoleHelper.PrintError("Doctor not found."); ConsoleHelper.PressAnyKey(); return; }

        ConsoleHelper.PrintInfo($"Appointments for {doctor.FullName}:");
        var appts = _appointmentService.GetByDoctor(doctorId);
        PrintAppointments(appts);
        ConsoleHelper.PressAnyKey();
    }

    private void UpdateStatus()
    {
        ConsoleHelper.PrintSubHeader("Update Appointment Status");
        int id = ConsoleHelper.ReadInt("Appointment ID");
        var appt = _appointmentService.GetById(id);
        if (appt == null) { ConsoleHelper.PrintError("Appointment not found."); ConsoleHelper.PressAnyKey(); return; }

        Console.WriteLine($"\n  Current status: {appt.Status}");
        AppointmentStatus newStatus = ConsoleHelper.ReadEnum<AppointmentStatus>("New Status");
        _appointmentService.UpdateStatus(id, newStatus);
        ConsoleHelper.PrintSuccess("Status updated.");
        ConsoleHelper.PressAnyKey();
    }

    private void CancelAppointment()
    {
        ConsoleHelper.PrintSubHeader("Cancel Appointment");
        int id = ConsoleHelper.ReadInt("Appointment ID");
        var appt = _appointmentService.GetById(id);
        if (appt == null) { ConsoleHelper.PrintError("Appointment not found."); ConsoleHelper.PressAnyKey(); return; }

        Console.WriteLine($"  Appointment: {appt}");
        if (!ConsoleHelper.Confirm("Are you sure you want to cancel this appointment?"))
        { ConsoleHelper.PrintInfo("Cancelled operation."); ConsoleHelper.PressAnyKey(); return; }

        bool success = _appointmentService.Cancel(id);
        if (success)
            ConsoleHelper.PrintSuccess("Appointment cancelled.");
        else
            ConsoleHelper.PrintError("Cannot cancel a completed appointment.");
        ConsoleHelper.PressAnyKey();
    }

    private void PrintAppointments(List<Appointment> appts)
    {
        if (appts.Count == 0)
        {
            ConsoleHelper.PrintInfo("No appointments found.");
            return;
        }

        foreach (var a in appts)
        {
            Console.ForegroundColor = a.Status switch
            {
                AppointmentStatus.Confirmed => ConsoleColor.Green,
                AppointmentStatus.Completed => ConsoleColor.DarkGray,
                AppointmentStatus.Cancelled => ConsoleColor.DarkRed,
                AppointmentStatus.NoShow => ConsoleColor.DarkYellow,
                AppointmentStatus.InProgress => ConsoleColor.Cyan,
                _ => ConsoleColor.White
            };
            Console.WriteLine($"  {a}");
            Console.ResetColor();
        }
        ConsoleHelper.PrintInfo($"Total: {appts.Count}");
    }
}
