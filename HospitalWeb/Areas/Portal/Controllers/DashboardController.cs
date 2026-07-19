using HospitalManagement.Core;
using HospitalManagement.Core.Models;
using HospitalManagement.Data;
using HospitalManagement.Services;
using HospitalWeb.ViewModels.Dashboard;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HospitalWeb.Areas.Portal.Controllers;

[Authorize(Policy = "PatientOnly")]
[Area("Portal")]
public class DashboardController : Controller
{
    private readonly HospitalDbContext    _db;
    private readonly AppointmentService   _appts;
    private readonly BillingService       _billing;
    private readonly AuthService          _auth;

    public DashboardController(HospitalDbContext db, AppointmentService appts,
        BillingService billing, AuthService auth)
    { _db = db; _appts = appts; _billing = billing; _auth = auth; }

    // GET /Dashboard
    public IActionResult Index()
    {
        var patientId = User.GetPatientId();
        if (patientId == null) return RedirectToAction("Login", "Auth", new { area = "Admin" });

        var patient  = _db.Patients.Find(patientId.Value);
        var allAppts = _appts.GetByPatient(patientId.Value);
        var bills    = _billing.GetByPatient(patientId.Value);

        var vm = new DashboardViewModel
        {
            CurrentPatient = patient,
            UpcomingAppointments = allAppts
                .Where(a => a.AppointmentDate >= DateTime.Now &&
                            a.Status != AppointmentStatus.Cancelled &&
                            a.Status != AppointmentStatus.Completed)
                .OrderBy(a => a.AppointmentDate).Take(5).ToList(),
            PastAppointments = allAppts
                .Where(a => a.AppointmentDate < DateTime.Now ||
                            a.Status == AppointmentStatus.Completed)
                .OrderByDescending(a => a.AppointmentDate).Take(3).ToList(),
            TotalAppointments = allAppts.Count,
            RecentRecords = _db.MedicalRecords
                .Where(r => r.PatientId == patientId.Value)
                .OrderByDescending(r => r.RecordDate).Take(3).ToList(),
            RecentBills = bills.Take(3).ToList(),
            OutstandingBalance = bills
                .Where(b => b.Status != BillStatus.Paid && b.Status != BillStatus.Cancelled)
                .Sum(b => b.Balance)
        };
        return View(vm);
    }

    // GET /Dashboard/BookAppointment
    public IActionResult BookAppointment(int? doctorId)
    {
        var vm = new BookAppointmentViewModel
        {
            Doctors  = _db.Doctors.Where(d => d.IsAvailable)
                          .OrderBy(d => d.Specialization).ToList(),
            DoctorId = doctorId ?? 0
        };
        return View(vm);
    }

    // POST /Dashboard/BookAppointment
    [HttpPost][ValidateAntiForgeryToken]
    public IActionResult BookAppointment(BookAppointmentViewModel vm)
    {
        vm.Doctors = _db.Doctors.Where(d => d.IsAvailable)
                        .OrderBy(d => d.Specialization).ToList();
        if (!ModelState.IsValid) return View(vm);

        var patientId = User.GetPatientId();
        if (patientId == null) return RedirectToAction("Login", "Auth", new { area = "Admin" });

        var localDate = DateTime.SpecifyKind(vm.AppointmentDate, DateTimeKind.Unspecified);
        var (success, message, appt) = _appts.ScheduleAppointment(
            patientId.Value, vm.DoctorId, localDate, vm.Reason, vm.Notes ?? "");

        if (!success) { vm.ErrorMessage = message; return View(vm); }
        TempData["Success"] = $"Appointment booked for {appt!.AppointmentDate:MMM dd, yyyy} at {appt.AppointmentDate:HH:mm}.";
        return RedirectToAction(nameof(Index), "Dashboard", new { area = "Portal" });
    }

    // GET /Dashboard/Doctors
    public IActionResult Doctors(string? search, string? specialization)
    {
        var q = _db.Doctors.Where(d => d.IsAvailable).AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.ToLower();
            q = q.Where(d => d.FirstName.ToLower().Contains(s) ||
                              d.LastName.ToLower().Contains(s));
        }
        if (Enum.TryParse<Specialization>(specialization, out var spec))
            q = q.Where(d => d.Specialization == spec);

        ViewBag.Search         = search;
        ViewBag.Specialization = specialization;
        ViewBag.Specializations = Enum.GetNames<Specialization>();
        return View(q.OrderBy(d => d.Specialization).ToList());
    }

    // GET /Dashboard/MyRecords
    public IActionResult MyRecords()
    {
        var patientId = User.GetPatientId();
        if (patientId == null) return RedirectToAction("Login", "Auth", new { area = "Admin" });
        var records = _db.MedicalRecords
            .Where(r => r.PatientId == patientId.Value)
            .OrderByDescending(r => r.RecordDate).ToList();
        return View(records);
    }

    // GET /Dashboard/MyBills
    public IActionResult MyBills()
    {
        var patientId = User.GetPatientId();
        if (patientId == null) return RedirectToAction("Login", "Auth", new { area = "Admin" });
        var bills = _billing.GetByPatient(patientId.Value);
        ViewBag.TotalPaid = bills.Sum(b => b.AmountPaid);
        ViewBag.Outstanding = bills
            .Where(b => b.Status != BillStatus.Paid && b.Status != BillStatus.Cancelled)
            .Sum(b => b.Balance);
        return View(bills);
    }

    // GET /Dashboard/Profile
    public IActionResult Profile()
    {
        var patientId = User.GetPatientId();
        if (patientId == null) return RedirectToAction("Login", "Auth", new { area = "Admin" });
        var patient = _db.Patients.Find(patientId.Value);
        var vm = new ProfileViewModel
        {
            CurrentPatient        = patient,
            PhoneNumber           = patient?.PhoneNumber ?? "",
            Email                 = patient?.Email ?? "",
            Address               = patient?.Address ?? "",
            EmergencyContactName  = patient?.EmergencyContactName ?? "",
            EmergencyContactPhone = patient?.EmergencyContactPhone ?? ""
        };
        return View(vm);
    }

    // POST /Dashboard/UpdateProfile
    [HttpPost][ValidateAntiForgeryToken]
    public IActionResult UpdateProfile(ProfileViewModel vm)
    {
        var patientId = User.GetPatientId();
        if (patientId == null) return RedirectToAction("Login", "Auth", new { area = "Admin" });
        var patient = _db.Patients.Find(patientId.Value);
        if (patient != null)
        {
            patient.PhoneNumber           = vm.PhoneNumber;
            patient.Email                 = vm.Email;
            patient.Address               = vm.Address;
            patient.EmergencyContactName  = vm.EmergencyContactName;
            patient.EmergencyContactPhone = vm.EmergencyContactPhone;
            _db.SaveChanges();
        }
        vm.CurrentPatient  = patient;
        vm.SuccessMessage  = "Profile updated successfully.";
        return View("Profile", vm);
    }

    // POST /Dashboard/ChangePassword
    [HttpPost][ValidateAntiForgeryToken]
    public IActionResult ChangePassword(ProfileViewModel vm)
    {
        var userId    = User.GetUserId();
        var patientId = User.GetPatientId();
        if (userId == null) return RedirectToAction("Login", "Auth", new { area = "Admin" });

        vm.CurrentPatient = patientId != null ? _db.Patients.Find(patientId.Value) : null;

        if (string.IsNullOrEmpty(vm.CurrentPassword) || string.IsNullOrEmpty(vm.NewPassword))
        {
            vm.ErrorMessage = "Please fill in all password fields.";
            return View("Profile", vm);
        }
        var (success, message) = _auth.ChangePassword(userId.Value, vm.CurrentPassword, vm.NewPassword);
        if (success) vm.SuccessMessage = message;
        else         vm.ErrorMessage   = message;
        return View("Profile", vm);
    }
}
