using HospitalWeb.Data;
using HospitalWeb.Models;
using HospitalWeb.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace HospitalWeb.Pages.Records;

public class CreateModel : PageModel
{
    private readonly HospitalDbContext _db;
    private readonly MedicalRecordService _records;

    public CreateModel(HospitalDbContext db, MedicalRecordService records)
    { _db = db; _records = records; }

    public List<Patient> Patients { get; set; } = new();
    public List<Doctor> Doctors { get; set; } = new();
    public List<Appointment> Appointments { get; set; } = new();

    [BindProperty] public int PatientId { get; set; }
    [BindProperty] public int DoctorId { get; set; }
    [BindProperty] public int? AppointmentId { get; set; }
    [BindProperty][Required] public string Diagnosis { get; set; } = "";
    [BindProperty][Required] public string Symptoms { get; set; } = "";
    [BindProperty][Required] public string TreatmentPlan { get; set; } = "";
    [BindProperty] public string LabResults { get; set; } = "";
    [BindProperty] public string Allergies { get; set; } = "";
    [BindProperty] public string Notes { get; set; } = "";
    [BindProperty] public string PrescriptionsJson { get; set; } = "[]";

    public void OnGet(int? patientId)
    {
        Patients     = _db.Patients.ToList();
        Doctors      = _db.Doctors.ToList();
        Appointments = _db.Appointments.Where(a => a.Status != AppointmentStatus.Cancelled).ToList();
        if (patientId.HasValue) PatientId = patientId.Value;
    }

    public IActionResult OnPost()
    {
        Patients     = _db.Patients.ToList();
        Doctors      = _db.Doctors.ToList();
        Appointments = _db.Appointments.Where(a => a.Status != AppointmentStatus.Cancelled).ToList();
        if (!ModelState.IsValid) return Page();

        List<Prescription> prescriptions = new();
        try { prescriptions = JsonSerializer.Deserialize<List<Prescription>>(PrescriptionsJson) ?? new(); } catch { }

        var record = _records.CreateRecord(PatientId, DoctorId,
            AppointmentId == 0 ? null : AppointmentId,
            Diagnosis, Symptoms, TreatmentPlan, prescriptions, LabResults, Allergies, Notes);

        TempData["Success"] = $"Medical record #{record.Id} created.";
        return RedirectToPage("./Index");
    }
}
