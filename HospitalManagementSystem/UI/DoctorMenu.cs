using HospitalManagementSystem.Models;
using HospitalManagementSystem.Services;

namespace HospitalManagementSystem.UI;

public class DoctorMenu
{
    private readonly DoctorService _doctorService;

    public DoctorMenu(DoctorService doctorService) => _doctorService = doctorService;

    public void Show()
    {
        while (true)
        {
            ConsoleHelper.PrintHeader("Doctor Management");
            Console.WriteLine("  1. Add New Doctor");
            Console.WriteLine("  2. View All Doctors");
            Console.WriteLine("  3. Search Doctor");
            Console.WriteLine("  4. View Doctor Details");
            Console.WriteLine("  5. Filter by Specialization");
            Console.WriteLine("  6. Toggle Availability");
            Console.WriteLine("  0. Back to Main Menu");
            Console.WriteLine();

            string choice = ConsoleHelper.ReadInput("Select option");
            switch (choice)
            {
                case "1": AddDoctor(); break;
                case "2": ViewAll(); break;
                case "3": SearchDoctor(); break;
                case "4": ViewDetails(); break;
                case "5": FilterBySpec(); break;
                case "6": ToggleAvailability(); break;
                case "0": return;
                default: ConsoleHelper.PrintError("Invalid option."); break;
            }
        }
    }

    private void AddDoctor()
    {
        ConsoleHelper.PrintSubHeader("Add New Doctor");
        string firstName = ConsoleHelper.ReadInput("First Name");
        string lastName = ConsoleHelper.ReadInput("Last Name");
        Specialization spec = ConsoleHelper.ReadEnum<Specialization>("Specialization");
        string phone = ConsoleHelper.ReadInput("Phone Number");
        string email = ConsoleHelper.ReadOptionalInput("Email (optional)");
        string license = ConsoleHelper.ReadInput("License Number");
        decimal fee = ConsoleHelper.ReadDecimal("Consultation Fee ($)");

        Console.WriteLine("\n  Available days (enter numbers separated by commas):");
        Console.WriteLine("  0=Sun, 1=Mon, 2=Tue, 3=Wed, 4=Thu, 5=Fri, 6=Sat");
        Console.Write("  Days: ");
        string dayInput = Console.ReadLine() ?? "1,2,3,4,5";
        var days = dayInput.Split(',')
            .Select(s => s.Trim())
            .Where(s => int.TryParse(s, out _))
            .Select(s => (DayOfWeek)int.Parse(s))
            .Distinct()
            .ToList();

        if (days.Count == 0) days = new List<DayOfWeek> { DayOfWeek.Monday, DayOfWeek.Wednesday, DayOfWeek.Friday };

        var doctor = _doctorService.AddDoctor(firstName, lastName, spec, phone, email, license, fee, days);
        ConsoleHelper.PrintSuccess($"Doctor added successfully! ID: {doctor.Id}");
        ConsoleHelper.PressAnyKey();
    }

    private void ViewAll()
    {
        ConsoleHelper.PrintSubHeader("All Doctors");
        var doctors = _doctorService.GetAll();
        if (doctors.Count == 0)
        {
            ConsoleHelper.PrintInfo("No doctors registered.");
        }
        else
        {
            foreach (var d in doctors)
            {
                Console.ForegroundColor = d.IsAvailable ? ConsoleColor.White : ConsoleColor.DarkGray;
                Console.WriteLine($"  {d} {(d.IsAvailable ? "" : "[UNAVAILABLE]")}");
                Console.ResetColor();
            }
            ConsoleHelper.PrintInfo($"Total: {doctors.Count} doctors");
        }
        ConsoleHelper.PressAnyKey();
    }

    private void SearchDoctor()
    {
        ConsoleHelper.PrintSubHeader("Search Doctor");
        string query = ConsoleHelper.ReadInput("Search (name / specialization / ID)");
        var results = _doctorService.Search(query);
        if (results.Count == 0)
            ConsoleHelper.PrintInfo("No doctors found.");
        else
            foreach (var d in results)
                Console.WriteLine($"  {d}");
        ConsoleHelper.PressAnyKey();
    }

    private void ViewDetails()
    {
        ConsoleHelper.PrintSubHeader("Doctor Details");
        int id = ConsoleHelper.ReadInt("Doctor ID");
        var d = _doctorService.GetById(id);
        if (d == null) { ConsoleHelper.PrintError("Doctor not found."); ConsoleHelper.PressAnyKey(); return; }

        ConsoleHelper.PrintSeparator();
        Console.WriteLine($"  ID:               {d.Id}");
        Console.WriteLine($"  Name:             {d.FullName}");
        Console.WriteLine($"  Specialization:   {d.Specialization}");
        Console.WriteLine($"  License:          {d.LicenseNumber}");
        Console.WriteLine($"  Phone:            {d.PhoneNumber}");
        Console.WriteLine($"  Email:            {d.Email}");
        Console.WriteLine($"  Consultation Fee: ${d.ConsultationFee:F2}");
        Console.WriteLine($"  Working Hours:    {d.WorkStartTime:hh\\:mm} - {d.WorkEndTime:hh\\:mm}");
        Console.WriteLine($"  Available Days:   {string.Join(", ", d.AvailableDays)}");
        Console.WriteLine($"  Status:           {(d.IsAvailable ? "Available" : "Unavailable")}");
        ConsoleHelper.PrintSeparator();
        ConsoleHelper.PressAnyKey();
    }

    private void FilterBySpec()
    {
        ConsoleHelper.PrintSubHeader("Filter by Specialization");
        Specialization spec = ConsoleHelper.ReadEnum<Specialization>("Specialization");
        var results = _doctorService.GetBySpecialization(spec);
        if (results.Count == 0)
            ConsoleHelper.PrintInfo($"No available doctors found for {spec}.");
        else
            foreach (var d in results)
                Console.WriteLine($"  {d}");
        ConsoleHelper.PressAnyKey();
    }

    private void ToggleAvailability()
    {
        ConsoleHelper.PrintSubHeader("Toggle Doctor Availability");
        int id = ConsoleHelper.ReadInt("Doctor ID");
        var doctor = _doctorService.GetById(id);
        if (doctor == null) { ConsoleHelper.PrintError("Doctor not found."); ConsoleHelper.PressAnyKey(); return; }

        _doctorService.ToggleAvailability(id);
        string status = doctor.IsAvailable ? "Available" : "Unavailable";
        ConsoleHelper.PrintSuccess($"{doctor.FullName} is now marked as {status}.");
        ConsoleHelper.PressAnyKey();
    }
}
