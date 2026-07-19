using HospitalManagement.Core;
using HospitalManagement.Core.Models;
using HospitalManagement.Data;
using HospitalManagement.Services;
using HospitalWeb.ViewModels.Records;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace HospitalWeb.Areas.Hospital.Controllers;

[Area("Hospital")]
[Authorize(Policy = "AdminOnly")]
public class RecordsController : Controller
{
    private readonly MedicalRecordService _records;
    private readonly HospitalDbContext    _db;

    public RecordsController(MedicalRecordService records, HospitalDbContext db)
    { _records = records; _db = db; }

    // GET /Records
    public IActionResult Index(string? search, int pageNumber = 1)
    {
        var vm = new RecordListViewModel
        {
            Search     = search,
            PageNumber = pageNumber,
            Records    = PagedResult<MedicalRecord>.Create(_records.Query(search), pageNumber, 15)
        };
        return View(vm);
    }

    // GET /Records/Details/5
    public IActionResult Details(int id)
    {
        var record = _records.GetById(id);
        if (record == null) return NotFound();
        return View(record);
    }

    // GET /Records/Create
    public IActionResult Create(int? patientId)
    {
        var vm = new RecordFormViewModel
        {
            Patients     = _db.Patients.OrderBy(p => p.LastName).ToList(),
            Doctors      = _db.Doctors.OrderBy(d => d.LastName).ToList(),
            Appointments = _db.Appointments
                .Where(a => a.Status != AppointmentStatus.Cancelled).ToList(),
            PatientId    = patientId ?? 0
        };
        return View(vm);
    }

    // POST /Records/Create
    [HttpPost][ValidateAntiForgeryToken]
    public IActionResult Create(RecordFormViewModel vm)
    {
        vm.Patients     = _db.Patients.OrderBy(p => p.LastName).ToList();
        vm.Doctors      = _db.Doctors.OrderBy(d => d.LastName).ToList();
        vm.Appointments = _db.Appointments
            .Where(a => a.Status != AppointmentStatus.Cancelled).ToList();
        if (!ModelState.IsValid) return View(vm);

        List<Prescription> prescriptions = new();
        try { prescriptions = JsonSerializer.Deserialize<List<Prescription>>(vm.PrescriptionsJson) ?? new(); }
        catch { }

        var record = _records.CreateRecord(vm.PatientId, vm.DoctorId,
            vm.AppointmentId == 0 ? null : vm.AppointmentId,
            vm.Diagnosis, vm.Symptoms, vm.TreatmentPlan,
            prescriptions, vm.LabResults, vm.Allergies, vm.Notes);

        TempData["Success"] = $"Medical record #{record.Id} created.";
        return RedirectToAction(nameof(Index));
    }
}
