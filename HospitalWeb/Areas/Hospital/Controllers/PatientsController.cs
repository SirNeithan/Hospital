using HospitalManagement.Core;
using HospitalManagement.Core.Models;
using HospitalManagement.Data;
using HospitalManagement.Services;
using HospitalWeb.ViewModels.Patients;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HospitalWeb.Areas.Hospital.Controllers;

[Area("Hospital")]
[Authorize(Policy = "AdminOnly")]
public class PatientsController : Controller
{
    private readonly PatientService     _patients;
    private readonly AppointmentService _appts;
    private readonly MedicalRecordService _records;
    private readonly BillingService     _billing;
    private readonly HospitalDbContext  _db;

    public PatientsController(PatientService patients, AppointmentService appts,
        MedicalRecordService records, BillingService billing, HospitalDbContext db)
    { _patients = patients; _appts = appts; _records = records; _billing = billing; _db = db; }

    // GET /Patients
    public IActionResult Index(string? search, int pageNumber = 1)
    {
        var vm = new PatientListViewModel
        {
            Search     = search,
            PageNumber = pageNumber,
            Patients   = PagedResult<Patient>.Create(_patients.Query(search), pageNumber, 15)
        };
        return View(vm);
    }

    // GET /Patients/Details/5
    public IActionResult Details(int id)
    {
        var patient = _patients.GetById(id);
        if (patient == null) return NotFound();
        var vm = new PatientDetailsViewModel
        {
            Patient      = patient,
            AssignedRoom = patient.AssignedRoomId.HasValue
                           ? _db.Rooms.Find(patient.AssignedRoomId.Value) : null,
            Appointments = _appts.GetByPatient(id),
            Records      = _records.GetByPatient(id),
            Bills        = _billing.GetByPatient(id)
        };
        return View(vm);
    }

    // GET /Patients/Create
    public IActionResult Create() => View(new PatientFormViewModel());

    // POST /Patients/Create
    [HttpPost][ValidateAntiForgeryToken]
    public IActionResult Create(PatientFormViewModel vm)
    {
        if (!ModelState.IsValid) return View(vm);
        var p = _patients.RegisterPatient(vm.FirstName, vm.LastName, vm.DateOfBirth,
            vm.Gender, vm.BloodType, vm.PhoneNumber, vm.Email,
            vm.Address, vm.EmergencyContactName, vm.EmergencyContactPhone);
        TempData["Success"] = $"Patient {p.FullName} registered (ID: {p.Id}).";
        return RedirectToAction(nameof(Index));
    }

    // GET /Patients/Edit/5
    public IActionResult Edit(int id)
    {
        var p = _patients.GetById(id);
        if (p == null) return NotFound();
        var vm = new PatientFormViewModel
        {
            Id = p.Id, FirstName = p.FirstName, LastName = p.LastName,
            DateOfBirth = p.DateOfBirth, Gender = p.Gender, BloodType = p.BloodType,
            PhoneNumber = p.PhoneNumber, Email = p.Email, Address = p.Address,
            EmergencyContactName = p.EmergencyContactName,
            EmergencyContactPhone = p.EmergencyContactPhone
        };
        return View(vm);
    }

    // POST /Patients/Edit/5
    [HttpPost][ValidateAntiForgeryToken]
    public IActionResult Edit(PatientFormViewModel vm)
    {
        if (!ModelState.IsValid) return View(vm);
        _patients.UpdatePatient(vm.Id, p =>
        {
            p.FirstName = vm.FirstName; p.LastName = vm.LastName;
            p.PhoneNumber = vm.PhoneNumber; p.Email = vm.Email;
            p.Address = vm.Address;
            p.EmergencyContactName  = vm.EmergencyContactName;
            p.EmergencyContactPhone = vm.EmergencyContactPhone;
        });
        TempData["Success"] = "Patient updated.";
        return RedirectToAction(nameof(Details), new { id = vm.Id });
    }

    // GET /Patients/Admit/5
    public IActionResult Admit(int id)
    {
        var patient = _patients.GetById(id);
        if (patient == null) return NotFound();
        var vm = new AdmitPatientViewModel
        {
            Patient = patient,
            AvailableRooms = _db.Rooms
                .Where(r => r.CurrentOccupancy < r.Capacity &&
                            r.Status != RoomStatus.UnderMaintenance)
                .ToList()
        };
        return View(vm);
    }

    // POST /Patients/Admit/5
    [HttpPost][ValidateAntiForgeryToken]
    public IActionResult Admit(int id, AdmitPatientViewModel vm)
    {
        bool ok = _patients.AdmitPatient(id, vm.RoomId);
        TempData[ok ? "Success" : "Error"] = ok
            ? "Patient admitted successfully."
            : "Could not admit patient — room may be full.";
        return RedirectToAction(nameof(Details), new { id });
    }

    // POST /Patients/Discharge/5
    [HttpPost][ValidateAntiForgeryToken]
    public IActionResult Discharge(int id)
    {
        _patients.DischargePatient(id);
        TempData["Success"] = "Patient discharged successfully.";
        return RedirectToAction(nameof(Details), new { id });
    }
}
