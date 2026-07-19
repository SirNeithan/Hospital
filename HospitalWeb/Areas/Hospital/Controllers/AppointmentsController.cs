using HospitalManagement.Core;
using HospitalManagement.Core.Models;
using HospitalManagement.Data;
using HospitalManagement.Services;
using HospitalWeb.ViewModels.Appointments;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HospitalWeb.Areas.Hospital.Controllers;

[Area("Hospital")]
[Authorize(Policy = "AdminOnly")]
public class AppointmentsController : Controller
{
    private readonly AppointmentService _appts;
    private readonly HospitalDbContext  _db;

    public AppointmentsController(AppointmentService appts, HospitalDbContext db)
    { _appts = appts; _db = db; }

    // GET /Appointments
    public IActionResult Index(string filter = "upcoming", int pageNumber = 1)
    {
        var vm = new AppointmentListViewModel
        {
            Filter      = filter,
            PageNumber  = pageNumber,
            Appointments = PagedResult<Appointment>.Create(_appts.Query(filter), pageNumber, 15)
        };
        return View(vm);
    }

    // GET /Appointments/Create
    public IActionResult Create(int? patientId)
    {
        var vm = new AppointmentFormViewModel
        {
            Patients  = _db.Patients.OrderBy(p => p.LastName).ToList(),
            Doctors   = _db.Doctors.Where(d => d.IsAvailable).ToList(),
            PatientId = patientId ?? 0
        };
        return View(vm);
    }

    // POST /Appointments/Create
    [HttpPost][ValidateAntiForgeryToken]
    public IActionResult Create(AppointmentFormViewModel vm)
    {
        vm.Patients = _db.Patients.OrderBy(p => p.LastName).ToList();
        vm.Doctors  = _db.Doctors.Where(d => d.IsAvailable).ToList();
        if (!ModelState.IsValid) return View(vm);

        var localDate = DateTime.SpecifyKind(vm.AppointmentDate, DateTimeKind.Unspecified);
        var (success, message, appt) = _appts.ScheduleAppointment(
            vm.PatientId, vm.DoctorId, localDate, vm.Reason, vm.Notes);

        if (!success) { vm.ErrorMessage = message; return View(vm); }
        TempData["Success"] = $"Appointment #{appt!.Id} scheduled.";
        return RedirectToAction(nameof(Index));
    }

    // POST /Appointments/Cancel/5
    [HttpPost][ValidateAntiForgeryToken]
    public IActionResult Cancel(int id)
    {
        _appts.Cancel(id);
        return RedirectToAction(nameof(Index));
    }
}
