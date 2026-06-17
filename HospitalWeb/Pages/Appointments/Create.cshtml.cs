using HospitalWeb.Data;
using HospitalWeb.Models;
using HospitalWeb.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace HospitalWeb.Pages.Appointments;

public class CreateModel : PageModel
{
    private readonly HospitalDbContext _db;
    private readonly AppointmentService _appts;

    public CreateModel(HospitalDbContext db, AppointmentService appts)
    { _db = db; _appts = appts; }

    public List<Patient> Patients { get; set; } = new();
    public List<Doctor> Doctors { get; set; } = new();
    public string? ErrorMessage { get; set; }

    [BindProperty] public int PatientId { get; set; }
    [BindProperty] public int DoctorId { get; set; }
    [BindProperty][Required] public DateTime AppointmentDate { get; set; } = DateTime.Today.AddHours(9);
    [BindProperty][Required] public string Reason { get; set; } = "";
    [BindProperty] public string Notes { get; set; } = "";

    public void OnGet(int? patientId)
    {
        Patients = _db.Patients.ToList();
        Doctors  = _db.Doctors.Where(d => d.IsAvailable).ToList();
        if (patientId.HasValue) PatientId = patientId.Value;
    }

    public IActionResult OnPost()
    {
        Patients = _db.Patients.ToList();
        Doctors  = _db.Doctors.Where(d => d.IsAvailable).ToList();
        if (!ModelState.IsValid) return Page();

        var (success, message, appt) = _appts.ScheduleAppointment(PatientId, DoctorId, AppointmentDate, Reason, Notes);
        if (!success) { ErrorMessage = message; return Page(); }
        TempData["Success"] = $"Appointment #{appt!.Id} scheduled.";
        return RedirectToPage("./Index");
    }
}
