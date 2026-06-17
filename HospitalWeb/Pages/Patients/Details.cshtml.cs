using HospitalWeb.Data;
using HospitalWeb.Models;
using HospitalWeb.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HospitalWeb.Pages.Patients;

public class DetailsModel : PageModel
{
    private readonly HospitalDbContext _db;
    private readonly PatientService _patients;
    private readonly AppointmentService _appointments;
    private readonly MedicalRecordService _records;
    private readonly BillingService _billing;

    public DetailsModel(HospitalDbContext db, PatientService patients,
        AppointmentService appointments, MedicalRecordService records, BillingService billing)
    { _db = db; _patients = patients; _appointments = appointments; _records = records; _billing = billing; }

    public Patient? Patient { get; set; }
    public Room? AssignedRoom { get; set; }
    public List<Appointment> Appointments { get; set; } = new();
    public List<MedicalRecord> Records { get; set; } = new();
    public List<Bill> Bills { get; set; } = new();

    public IActionResult OnGet(int id)
    {
        Patient = _patients.GetById(id);
        if (Patient == null) return NotFound();
        if (Patient.AssignedRoomId.HasValue)
            AssignedRoom = _db.Rooms.Find(Patient.AssignedRoomId.Value);
        Appointments = _appointments.GetByPatient(id);
        Records      = _records.GetByPatient(id);
        Bills        = _billing.GetByPatient(id);
        return Page();
    }

    public IActionResult OnPostDischarge(int id)
    {
        _patients.DischargePatient(id);
        TempData["Success"] = "Patient discharged successfully.";
        return RedirectToPage(new { id });
    }
}
