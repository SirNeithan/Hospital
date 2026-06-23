using HospitalManagement.Data;
using HospitalManagement.Core.Models;
using HospitalManagement.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HospitalWeb.Pages.Patients;

public class AdmitModel : PageModel
{
    private readonly HospitalDbContext _db;
    private readonly PatientService _patients;

    public AdmitModel(HospitalDbContext db, PatientService patients)
    { _db = db; _patients = patients; }

    public Patient? Patient { get; set; }
    public List<Room> AvailableRooms { get; set; } = new();
    [BindProperty] public int RoomId { get; set; }

    public IActionResult OnGet(int id)
    {
        Patient = _patients.GetById(id);
        if (Patient == null) return NotFound();
        AvailableRooms = _db.Rooms
            .Where(r => r.CurrentOccupancy < r.Capacity && r.Status != RoomStatus.UnderMaintenance)
            .ToList();
        return Page();
    }

    public IActionResult OnPost(int id)
    {
        bool ok = _patients.AdmitPatient(id, RoomId);
        TempData[ok ? "Success" : "Error"] = ok ? "Patient admitted successfully." : "Could not admit patient.";
        return RedirectToPage("./Details", new { id });
    }
}
