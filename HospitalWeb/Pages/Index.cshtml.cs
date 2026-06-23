using HospitalWeb.Data;
using HospitalWeb.Models;
using HospitalWeb.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HospitalWeb.Pages;

[Authorize(Roles = "Admin")]
public class IndexModel : PageModel
{
    private readonly HospitalDbContext _db;
    private readonly BillingService _billing;

    public IndexModel(HospitalDbContext db, BillingService billing)
    { _db = db; _billing = billing; }

    public int TotalPatients { get; set; }
    public int AdmittedPatients { get; set; }
    public int TotalDoctors { get; set; }
    public int AvailableDoctors { get; set; }
    public int TodayAppointments { get; set; }
    public int TotalRooms { get; set; }
    public int AvailableRooms { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal Outstanding { get; set; }
    public int UnpaidBills { get; set; }
    public List<Appointment> UpcomingAppointments { get; set; } = new();
    public List<Patient> RecentPatients { get; set; } = new();

    public void OnGet()
    {
        TotalPatients    = _db.Patients.Count();
        AdmittedPatients = _db.Patients.Count(p => p.IsAdmitted);
        TotalDoctors     = _db.Doctors.Count();
        AvailableDoctors = _db.Doctors.Count(d => d.IsAvailable);
        var today = DateTime.Today;
        var tomorrow = today.AddDays(1);
        TodayAppointments = _db.Appointments.Count(a =>
            a.AppointmentDate >= today &&
            a.AppointmentDate < tomorrow &&
            a.Status != AppointmentStatus.Cancelled);
        TotalRooms     = _db.Rooms.Count();
        AvailableRooms = _db.Rooms.Count(r =>
            r.CurrentOccupancy < r.Capacity && r.Status != RoomStatus.UnderMaintenance);
        TotalRevenue = _billing.GetTotalRevenue();
        Outstanding  = _billing.GetOutstandingBalance();
        UnpaidBills  = _billing.GetUnpaid().Count;
        UpcomingAppointments = _db.Appointments
            .Where(a => a.AppointmentDate >= DateTime.Now && a.Status != AppointmentStatus.Cancelled)
            .OrderBy(a => a.AppointmentDate)
            .Take(6).ToList();
        RecentPatients = _db.Patients
            .OrderByDescending(p => p.RegistrationDate)
            .Take(5).ToList();
    }
}
