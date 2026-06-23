using HospitalManagement.Core.Models;
using HospitalManagement.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HospitalWeb.Pages.Patients;

public class EditModel : PageModel
{
    private readonly PatientService _patients;

    public EditModel(PatientService patients) => _patients = patients;

    [BindProperty] public int Id { get; set; }
    [BindProperty] public string FirstName { get; set; } = "";
    [BindProperty] public string LastName { get; set; } = "";
    [BindProperty] public string PhoneNumber { get; set; } = "";
    [BindProperty] public string Email { get; set; } = "";
    [BindProperty] public string Address { get; set; } = "";
    [BindProperty] public string EmergencyContactName { get; set; } = "";
    [BindProperty] public string EmergencyContactPhone { get; set; } = "";

    public IActionResult OnGet(int id)
    {
        var p = _patients.GetById(id);
        if (p == null) return NotFound();
        Id = p.Id; FirstName = p.FirstName; LastName = p.LastName;
        PhoneNumber = p.PhoneNumber; Email = p.Email; Address = p.Address;
        EmergencyContactName = p.EmergencyContactName;
        EmergencyContactPhone = p.EmergencyContactPhone;
        return Page();
    }

    public IActionResult OnPost()
    {
        _patients.UpdatePatient(Id, p =>
        {
            p.FirstName = FirstName; p.LastName = LastName;
            p.PhoneNumber = PhoneNumber; p.Email = Email; p.Address = Address;
            p.EmergencyContactName = EmergencyContactName;
            p.EmergencyContactPhone = EmergencyContactPhone;
        });
        TempData["Success"] = "Patient updated.";
        return RedirectToPage("./Details", new { id = Id });
    }
}
