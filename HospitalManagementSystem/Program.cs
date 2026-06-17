using HospitalManagementSystem.Models;
using HospitalManagementSystem.Services;
using HospitalManagementSystem.UI;

// ─── Bootstrap ──────────────────────────────────────────────────────────────
var store = new DataStore();
store.SeedData();

var patientService = new PatientService(store);
var doctorService = new DoctorService(store);
var appointmentService = new AppointmentService(store);
var recordService = new MedicalRecordService(store);
var billingService = new BillingService(store);

var patientMenu = new PatientMenu(patientService, store);
var doctorMenu = new DoctorMenu(doctorService);
var appointmentMenu = new AppointmentMenu(appointmentService, patientService, doctorService);
var recordMenu = new MedicalRecordMenu(recordService, patientService, doctorService, appointmentService);
var billingMenu = new BillingMenu(billingService, patientService);
var roomMenu = new RoomMenu(store);

// ─── Main Loop ───────────────────────────────────────────────────────────────
Console.Title = "Hospital Management System";

while (true)
{
    PrintMainMenu();

    string choice = Console.ReadLine()?.Trim() ?? "";
    switch (choice)
    {
        case "1": patientMenu.Show(); break;
        case "2": doctorMenu.Show(); break;
        case "3": appointmentMenu.Show(); break;
        case "4": recordMenu.Show(); break;
        case "5": billingMenu.Show(); break;
        case "6": roomMenu.Show(); break;
        case "7": PrintDashboard(store, billingService); break;
        case "0":
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("\n  Thank you for using Hospital Management System. Goodbye!");
            Console.ResetColor();
            return;
        default:
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("  ✖ Invalid option. Please try again.");
            Console.ResetColor();
            break;
    }
}

// ─── Main Menu Display ───────────────────────────────────────────────────────
static void PrintMainMenu()
{
    Console.Clear();
    Console.ForegroundColor = ConsoleColor.Cyan;
    Console.WriteLine();
    Console.WriteLine("  ╔══════════════════════════════════════════════════════╗");
    Console.WriteLine("  ║         HOSPITAL MANAGEMENT SYSTEM  v1.0            ║");
    Console.WriteLine("  ╚══════════════════════════════════════════════════════╝");
    Console.ResetColor();
    Console.WriteLine($"  {DateTime.Now:dddd, MMMM dd, yyyy  HH:mm}");
    Console.WriteLine();
    Console.ForegroundColor = ConsoleColor.Yellow;
    Console.WriteLine("  MAIN MENU");
    Console.ResetColor();
    Console.WriteLine("  ┌─────────────────────────────────────┐");
    Console.WriteLine("  │  1. 👥  Patient Management          │");
    Console.WriteLine("  │  2. 🩺  Doctor Management           │");
    Console.WriteLine("  │  3. 📅  Appointments                │");
    Console.WriteLine("  │  4. 📋  Medical Records             │");
    Console.WriteLine("  │  5. 💳  Billing & Payments          │");
    Console.WriteLine("  │  6. 🏥  Room & Ward Management      │");
    Console.WriteLine("  │  7. 📊  Dashboard                   │");
    Console.WriteLine("  │  0. 🚪  Exit                        │");
    Console.WriteLine("  └─────────────────────────────────────┘");
    Console.Write("\n  Select option: ");
}

// ─── Dashboard ───────────────────────────────────────────────────────────────
static void PrintDashboard(DataStore store, BillingService billingService)
{
    Console.Clear();
    Console.ForegroundColor = ConsoleColor.Cyan;
    Console.WriteLine("\n  ══════════════════════════════════════════════════════");
    Console.WriteLine("  HOSPITAL DASHBOARD");
    Console.WriteLine("  ══════════════════════════════════════════════════════");
    Console.ResetColor();
    Console.WriteLine();

    // Patients
    int totalPatients = store.Patients.Count;
    int admitted = store.Patients.Count(p => p.IsAdmitted);
    Console.ForegroundColor = ConsoleColor.White;
    Console.WriteLine("  PATIENTS");
    Console.ResetColor();
    Console.WriteLine($"    Total Registered : {totalPatients}");
    Console.WriteLine($"    Currently Admitted: {admitted}");
    Console.WriteLine($"    Outpatients:        {totalPatients - admitted}");
    Console.WriteLine();

    // Doctors
    int totalDoctors = store.Doctors.Count;
    int availDoctors = store.Doctors.Count(d => d.IsAvailable);
    Console.ForegroundColor = ConsoleColor.White;
    Console.WriteLine("  DOCTORS");
    Console.ResetColor();
    Console.WriteLine($"    Total:            {totalDoctors}");
    Console.WriteLine($"    Available:        {availDoctors}");
    Console.WriteLine($"    Unavailable:      {totalDoctors - availDoctors}");
    Console.WriteLine();

    // Appointments today
    var todayAppts = store.Appointments.Where(a => a.AppointmentDate.Date == DateTime.Today && a.Status != AppointmentStatus.Cancelled).ToList();
    Console.ForegroundColor = ConsoleColor.White;
    Console.WriteLine("  TODAY'S APPOINTMENTS");
    Console.ResetColor();
    Console.WriteLine($"    Scheduled:        {todayAppts.Count(a => a.Status == AppointmentStatus.Scheduled)}");
    Console.WriteLine($"    Confirmed:        {todayAppts.Count(a => a.Status == AppointmentStatus.Confirmed)}");
    Console.WriteLine($"    Completed:        {todayAppts.Count(a => a.Status == AppointmentStatus.Completed)}");
    Console.WriteLine($"    Total:            {todayAppts.Count}");
    Console.WriteLine();

    // Rooms
    int totalRooms = store.Rooms.Count;
    int availRooms = store.Rooms.Count(r => r.HasAvailableBed && r.Status != RoomStatus.UnderMaintenance);
    int occupiedRooms = store.Rooms.Count(r => r.Status == RoomStatus.Occupied);
    Console.ForegroundColor = ConsoleColor.White;
    Console.WriteLine("  ROOMS");
    Console.ResetColor();
    Console.WriteLine($"    Total Rooms:      {totalRooms}");
    Console.WriteLine($"    Available Beds:   {availRooms}");
    Console.WriteLine($"    Fully Occupied:   {occupiedRooms}");
    int totalBeds = store.Rooms.Sum(r => r.Capacity);
    int usedBeds = store.Rooms.Sum(r => r.CurrentOccupancy);
    Console.WriteLine($"    Bed Occupancy:    {usedBeds}/{totalBeds} ({(totalBeds > 0 ? (usedBeds * 100 / totalBeds) : 0)}%)");
    Console.WriteLine();

    // Billing
    Console.ForegroundColor = ConsoleColor.White;
    Console.WriteLine("  FINANCIALS");
    Console.ResetColor();
    Console.WriteLine($"    Total Revenue:    ${billingService.GetTotalRevenue():F2}");
    Console.ForegroundColor = ConsoleColor.Yellow;
    Console.WriteLine($"    Outstanding:      ${billingService.GetOutstandingBalance():F2}");
    Console.ResetColor();
    Console.WriteLine($"    Unpaid Bills:     {billingService.GetUnpaid().Count}");
    Console.WriteLine();

    Console.ForegroundColor = ConsoleColor.DarkGray;
    Console.Write("  Press any key to return to main menu...");
    Console.ResetColor();
    Console.ReadKey(true);
}
