using HospitalManagement.Core.Models;
using HospitalManagement.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace HospitalWeb.Pages.Patients;

public class CreateModel : PageModel
{
    private readonly PatientService _patients;

    public CreateModel(PatientService patients) => _patients = patients;

    [BindProperty] [Required] public string FirstName { get; set; } = "";
    [BindProperty] [Required] public string LastName { get; set; } = "";
    [BindProperty] [Required] public DateTime DateOfBirth { get; set; } = DateTime.Today.AddYears(-30);
    [BindProperty] public Gender Gender { get; set; }
    [BindProperty] public BloodType BloodType { get; set; }
    [BindProperty] [Required] public string PhoneNumber { get; set; } = "";
    [BindProperty] public string Email { get; set; } = "";
    [BindProperty] public string Address { get; set; } = "";
    [BindProperty] [Required] public string EmergencyContactName { get; set; } = "";
    [BindProperty] [Required] public string EmergencyContactPhone { get; set; } = "";

    public void OnGet() { }

    public IActionResult OnPost()
    {
        if (!ModelState.IsValid) return Page();

        var p = _patients.RegisterPatient(FirstName, LastName, DateOfBirth, Gender, BloodType,
            PhoneNumber, Email, Address, EmergencyContactName, EmergencyContactPhone);

        TempData["Success"] = $"Patient {p.FullName} registered successfully (ID: {p.Id}).";
        return RedirectToPage("./Index");
    }
}
