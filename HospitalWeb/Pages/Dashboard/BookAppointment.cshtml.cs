using HospitalManagement.Data;
using HospitalManagement.Core.Models;
using HospitalManagement.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace HospitalWeb.Pages.Dashboard;

[Authorize(Roles = "Patient")]
public class BookAppointmentModel : PageModel
{
    private readonly HospitalDbContext _db;
    private readonly AppointmentService _appts;

    public BookAppointmentModel(HospitalDbContext db, AppointmentService appts)
    { _db = db; _appts = appts; }

    public List<Doctor> Doctors { get; set; } = new();
    public string? ErrorMessage { get; set; }

    // Range(1, int.MaxValue) ensures 0 (unselected) fails validation
    [BindProperty]
    [Range(1, int.MaxValue, ErrorMessage = "Please select a doctor.")]
    public int DoctorId { get; set; }

    [BindProperty]
    [Required(ErrorMessage = "Please choose a date and time.")]
    public DateTime AppointmentDate { get; set; } = DateTime.Today.AddDays(1).AddHours(9);

    [BindProperty]
    [Required(ErrorMessage = "Please provide a reason for the visit.")]
    public string Reason { get; set; } = "";

    [BindProperty]
    public string Notes { get; set; } = "";

    public void OnGet(int? doctorId = null)
    {
        LoadDoctors();
        if (doctorId.HasValue) DoctorId = doctorId.Value;
    }

    public IActionResult OnPost()
    {
        LoadDoctors();
        if (!ModelState.IsValid) return Page();

        var patientId = GetPatientId();
        if (patientId == null) return RedirectToPage("/Auth/Login");

        // Ensure we treat the submitted datetime as local time (not UTC)
        // to match how the DB stores it (legacy timestamp behavior is on)
        var localDate = DateTime.SpecifyKind(AppointmentDate, DateTimeKind.Local);

        var (success, message, appt) = _appts.ScheduleAppointment(
            patientId.Value, DoctorId, localDate, Reason, Notes ?? "");

        if (!success) { ErrorMessage = message; return Page(); }

        TempData["Success"] = $"Appointment booked for {appt!.AppointmentDate:MMM dd, yyyy} at {appt.AppointmentDate:HH:mm}.";
        return RedirectToPage("/Dashboard/Index");
    }

    private void LoadDoctors() =>
        Doctors = _db.Doctors.Where(d => d.IsAvailable).OrderBy(d => d.Specialization).ToList();

    private int? GetPatientId()
    {
        var claim = User.FindFirst("PatientId")?.Value;
        return int.TryParse(claim, out int id) && id > 0 ? id : null;
    }
}
