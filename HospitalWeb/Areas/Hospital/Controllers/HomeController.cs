using HospitalManagement.Core.Models;
using HospitalManagement.Data;
using HospitalManagement.Services;
using HospitalWeb.ViewModels.Home;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HospitalWeb.Areas.Hospital.Controllers;

[Area("Hospital")]
[Authorize(Policy = "AdminOnly")]
public class HomeController : Controller
{
    private readonly HospitalDbContext _db;
    private readonly BillingService _billing;

    public HomeController(HospitalDbContext db, BillingService billing)
    { _db = db; _billing = billing; }

    // GET /Home  or  /Home/Index
    public IActionResult Index()
    {
        var today    = DateTime.Today;
        var tomorrow = today.AddDays(1);

        var vm = new HomeViewModel
        {
            TotalPatients    = _db.Patients.Count(),
            AdmittedPatients = _db.Patients.Count(p => p.IsAdmitted),
            TotalDoctors     = _db.Doctors.Count(),
            AvailableDoctors = _db.Doctors.Count(d => d.IsAvailable),
            TodayAppointments = _db.Appointments.Count(a =>
                a.AppointmentDate >= today &&
                a.AppointmentDate < tomorrow &&
                a.Status != AppointmentStatus.Cancelled),
            TotalRooms     = _db.Rooms.Count(),
            AvailableRooms = _db.Rooms.Count(r =>
                r.CurrentOccupancy < r.Capacity &&
                r.Status != RoomStatus.UnderMaintenance),
            TotalRevenue = _billing.GetTotalRevenue(),
            Outstanding  = _billing.GetOutstandingBalance(),
            UnpaidBills  = _billing.GetUnpaid().Count,
            UpcomingAppointments = _db.Appointments
                .Where(a => a.AppointmentDate >= DateTime.Now &&
                            a.Status != AppointmentStatus.Cancelled)
                .OrderBy(a => a.AppointmentDate)
                .Take(6).ToList(),
            RecentPatients = _db.Patients
                .OrderByDescending(p => p.RegistrationDate)
                .Take(5).ToList()
        };

        return View(vm);
    }
}
