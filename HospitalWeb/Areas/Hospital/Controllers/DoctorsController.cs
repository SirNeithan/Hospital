using HospitalManagement.Core;
using HospitalManagement.Core.Models;
using HospitalManagement.Services;
using HospitalWeb.ViewModels.Doctors;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HospitalWeb.Areas.Hospital.Controllers;

[Area("Hospital")]
[Authorize(Policy = "AdminOnly")]
public class DoctorsController : Controller
{
    private readonly DoctorService _doctors;
    public DoctorsController(DoctorService doctors) => _doctors = doctors;

    // GET /Doctors
    public IActionResult Index(string? search, string? spec, int pageNumber = 1)
    {
        Specialization? specFilter = Enum.TryParse<Specialization>(spec, out var s) ? s : null;
        var vm = new DoctorListViewModel
        {
            Search     = search,
            Spec       = spec,
            PageNumber = pageNumber,
            Doctors    = PagedResult<Doctor>.Create(_doctors.Query(search, specFilter), pageNumber, 15)
        };
        return View(vm);
    }

    // GET /Doctors/Create
    public IActionResult Create() => View(new DoctorFormViewModel());

    // POST /Doctors/Create
    [HttpPost][ValidateAntiForgeryToken]
    public IActionResult Create(DoctorFormViewModel vm)
    {
        if (!ModelState.IsValid) return View(vm);
        var days = vm.AvailableDayInts.Select(d => (DayOfWeek)d).ToList();
        var doc  = _doctors.AddDoctor(vm.FirstName, vm.LastName, vm.Specialization,
            vm.PhoneNumber, vm.Email, vm.LicenseNumber, vm.ConsultationFee, days);
        TempData["Success"] = $"Dr. {doc.FullName} added (ID: {doc.Id}).";
        return RedirectToAction(nameof(Index));
    }

    // POST /Doctors/Toggle/5
    [HttpPost][ValidateAntiForgeryToken]
    public IActionResult Toggle(int id)
    {
        _doctors.ToggleAvailability(id);
        return RedirectToAction(nameof(Index));
    }
}
