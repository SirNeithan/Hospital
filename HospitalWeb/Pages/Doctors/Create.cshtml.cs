using HospitalWeb.Models;
using HospitalWeb.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace HospitalWeb.Pages.Doctors;

public class CreateModel : PageModel
{
    private readonly DoctorService _doctors;
    public CreateModel(DoctorService doctors) => _doctors = doctors;

    [BindProperty][Required] public string FirstName { get; set; } = "";
    [BindProperty][Required] public string LastName { get; set; } = "";
    [BindProperty] public Specialization Specialization { get; set; }
    [BindProperty][Required] public string PhoneNumber { get; set; } = "";
    [BindProperty] public string Email { get; set; } = "";
    [BindProperty][Required] public string LicenseNumber { get; set; } = "";
    [BindProperty] public decimal ConsultationFee { get; set; }
    [BindProperty] public List<int> AvailableDayInts { get; set; } = new();

    public void OnGet() { }

    public IActionResult OnPost()
    {
        if (!ModelState.IsValid) return Page();
        var days = AvailableDayInts.Select(d => (DayOfWeek)d).ToList();
        var doc = _doctors.AddDoctor(FirstName, LastName, Specialization, PhoneNumber, Email, LicenseNumber, ConsultationFee, days);
        TempData["Success"] = $"Dr. {doc.FullName} added (ID: {doc.Id}).";
        return RedirectToPage("./Index");
    }
}
