using HospitalManagementSystem.Models;
using HospitalManagementSystem.Services;

namespace HospitalManagementSystem.UI;

public class PatientMenu
{
    private readonly PatientService _patientService;
    private readonly DataStore _store;

    public PatientMenu(PatientService patientService, DataStore store)
    {
        _patientService = patientService;
        _store = store;
    }

    public void Show()
    {
        while (true)
        {
            ConsoleHelper.PrintHeader("Patient Management");
            Console.WriteLine("  1. Register New Patient");
            Console.WriteLine("  2. View All Patients");
            Console.WriteLine("  3. Search Patient");
            Console.WriteLine("  4. View Patient Details");
            Console.WriteLine("  5. Admit Patient");
            Console.WriteLine("  6. Discharge Patient");
            Console.WriteLine("  7. View Admitted Patients");
            Console.WriteLine("  8. Update Patient Info");
            Console.WriteLine("  0. Back to Main Menu");
            Console.WriteLine();

            string choice = ConsoleHelper.ReadInput("Select option");
            switch (choice)
            {
                case "1": RegisterPatient(); break;
                case "2": ViewAll(); break;
                case "3": SearchPatient(); break;
                case "4": ViewDetails(); break;
                case "5": AdmitPatient(); break;
                case "6": DischargePatient(); break;
                case "7": ViewAdmitted(); break;
                case "8": UpdatePatient(); break;
                case "0": return;
                default: ConsoleHelper.PrintError("Invalid option."); break;
            }
        }
    }

    private void RegisterPatient()
    {
        ConsoleHelper.PrintSubHeader("Register New Patient");
        string firstName = ConsoleHelper.ReadInput("First Name");
        string lastName = ConsoleHelper.ReadInput("Last Name");
        DateTime dob = ConsoleHelper.ReadDate("Date of Birth");
        Gender gender = ConsoleHelper.ReadEnum<Gender>("Gender");
        BloodType bloodType = ConsoleHelper.ReadEnum<BloodType>("Blood Type");
        string phone = ConsoleHelper.ReadInput("Phone Number");
        string email = ConsoleHelper.ReadOptionalInput("Email (optional)");
        string address = ConsoleHelper.ReadOptionalInput("Address (optional)");
        string emergencyName = ConsoleHelper.ReadInput("Emergency Contact Name");
        string emergencyPhone = ConsoleHelper.ReadInput("Emergency Contact Phone");

        var patient = _patientService.RegisterPatient(firstName, lastName, dob, gender,
            bloodType, phone, email, address, emergencyName, emergencyPhone);

        ConsoleHelper.PrintSuccess($"Patient registered successfully! ID: {patient.Id}");
        ConsoleHelper.PressAnyKey();
    }

    private void ViewAll()
    {
        ConsoleHelper.PrintSubHeader("All Patients");
        var patients = _patientService.GetAll();
        if (patients.Count == 0)
        {
            ConsoleHelper.PrintInfo("No patients registered.");
        }
        else
        {
            foreach (var p in patients)
            {
                Console.Write(p.IsAdmitted ? "  🏥 " : "  👤 ");
                Console.WriteLine(p);
            }
            ConsoleHelper.PrintInfo($"Total: {patients.Count} patients");
        }
        ConsoleHelper.PressAnyKey();
    }

    private void SearchPatient()
    {
        ConsoleHelper.PrintSubHeader("Search Patient");
        string query = ConsoleHelper.ReadInput("Search (name / phone / email / ID)");
        var results = _patientService.Search(query);
        if (results.Count == 0)
        {
            ConsoleHelper.PrintInfo("No patients found.");
        }
        else
        {
            foreach (var p in results)
                Console.WriteLine($"  {p}");
        }
        ConsoleHelper.PressAnyKey();
    }

    private void ViewDetails()
    {
        ConsoleHelper.PrintSubHeader("Patient Details");
        int id = ConsoleHelper.ReadInt("Patient ID");
        var p = _patientService.GetById(id);
        if (p == null) { ConsoleHelper.PrintError("Patient not found."); ConsoleHelper.PressAnyKey(); return; }

        ConsoleHelper.PrintSeparator();
        Console.WriteLine($"  ID:               {p.Id}");
        Console.WriteLine($"  Name:             {p.FullName}");
        Console.WriteLine($"  Date of Birth:    {p.DateOfBirth:dd/MM/yyyy} (Age: {p.Age})");
        Console.WriteLine($"  Gender:           {p.Gender}");
        Console.WriteLine($"  Blood Type:       {p.BloodType}");
        Console.WriteLine($"  Phone:            {p.PhoneNumber}");
        Console.WriteLine($"  Email:            {p.Email}");
        Console.WriteLine($"  Address:          {p.Address}");
        Console.WriteLine($"  Emergency:        {p.EmergencyContactName} ({p.EmergencyContactPhone})");
        Console.WriteLine($"  Registered:       {p.RegistrationDate:dd/MM/yyyy}");
        Console.WriteLine($"  Status:           {(p.IsAdmitted ? "Admitted" : "Outpatient")}");

        if (p.IsAdmitted && p.AssignedRoomId.HasValue)
        {
            var room = _store.Rooms.FirstOrDefault(r => r.Id == p.AssignedRoomId);
            Console.WriteLine($"  Room:             {room?.RoomNumber ?? "N/A"} ({room?.Type})");
        }

        // Appointment count
        int apptCount = _store.Appointments.Count(a => a.PatientId == id);
        int recordCount = _store.MedicalRecords.Count(r => r.PatientId == id);
        int billCount = _store.Bills.Count(b => b.PatientId == id);
        Console.WriteLine($"  Appointments:     {apptCount}");
        Console.WriteLine($"  Medical Records:  {recordCount}");
        Console.WriteLine($"  Bills:            {billCount}");
        ConsoleHelper.PrintSeparator();
        ConsoleHelper.PressAnyKey();
    }

    private void AdmitPatient()
    {
        ConsoleHelper.PrintSubHeader("Admit Patient");
        int patientId = ConsoleHelper.ReadInt("Patient ID");
        var patient = _patientService.GetById(patientId);
        if (patient == null) { ConsoleHelper.PrintError("Patient not found."); ConsoleHelper.PressAnyKey(); return; }
        if (patient.IsAdmitted) { ConsoleHelper.PrintWarning("Patient is already admitted."); ConsoleHelper.PressAnyKey(); return; }

        // Show available rooms
        var availableRooms = _store.Rooms.Where(r => r.HasAvailableBed && r.Status != RoomStatus.UnderMaintenance).ToList();
        if (availableRooms.Count == 0) { ConsoleHelper.PrintError("No available rooms."); ConsoleHelper.PressAnyKey(); return; }

        Console.WriteLine("\n  Available Rooms:");
        foreach (var room in availableRooms)
            Console.WriteLine($"    {room}");

        int roomId = ConsoleHelper.ReadInt("Room ID");
        bool success = _patientService.AdmitPatient(patientId, roomId);
        if (success)
            ConsoleHelper.PrintSuccess($"{patient.FullName} admitted successfully.");
        else
            ConsoleHelper.PrintError("Could not admit patient. Check room availability.");
        ConsoleHelper.PressAnyKey();
    }

    private void DischargePatient()
    {
        ConsoleHelper.PrintSubHeader("Discharge Patient");
        ViewAdmitted();
        int id = ConsoleHelper.ReadInt("Patient ID to discharge");
        bool success = _patientService.DischargePatient(id);
        if (success)
            ConsoleHelper.PrintSuccess("Patient discharged successfully.");
        else
            ConsoleHelper.PrintError("Patient not found or not currently admitted.");
        ConsoleHelper.PressAnyKey();
    }

    private void ViewAdmitted()
    {
        ConsoleHelper.PrintSubHeader("Currently Admitted Patients");
        var admitted = _patientService.GetAdmitted();
        if (admitted.Count == 0)
            ConsoleHelper.PrintInfo("No patients currently admitted.");
        else
            foreach (var p in admitted)
            {
                var room = _store.Rooms.FirstOrDefault(r => r.Id == p.AssignedRoomId);
                Console.WriteLine($"  {p} | Room: {room?.RoomNumber ?? "N/A"}");
            }
        ConsoleHelper.PressAnyKey();
    }

    private void UpdatePatient()
    {
        ConsoleHelper.PrintSubHeader("Update Patient Info");
        int id = ConsoleHelper.ReadInt("Patient ID");
        var patient = _patientService.GetById(id);
        if (patient == null) { ConsoleHelper.PrintError("Patient not found."); ConsoleHelper.PressAnyKey(); return; }

        Console.WriteLine($"\n  Updating: {patient.FullName} (press Enter to keep current value)");
        Console.Write($"  Phone [{patient.PhoneNumber}]: ");
        string phone = Console.ReadLine()?.Trim() ?? "";
        Console.Write($"  Email [{patient.Email}]: ");
        string email = Console.ReadLine()?.Trim() ?? "";
        Console.Write($"  Address [{patient.Address}]: ");
        string address = Console.ReadLine()?.Trim() ?? "";

        _patientService.UpdatePatient(id, p =>
        {
            if (!string.IsNullOrEmpty(phone)) p.PhoneNumber = phone;
            if (!string.IsNullOrEmpty(email)) p.Email = email;
            if (!string.IsNullOrEmpty(address)) p.Address = address;
        });

        ConsoleHelper.PrintSuccess("Patient information updated.");
        ConsoleHelper.PressAnyKey();
    }
}
