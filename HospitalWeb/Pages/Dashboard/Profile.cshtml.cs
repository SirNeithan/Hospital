using HospitalManagement.Data;
using HospitalManagement.Core.Models;
using HospitalManagement.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace HospitalWeb.Pages.Dashboard;

[Authorize(Roles = "Patient")]
public class ProfileModel : PageModel
{
    private readonly HospitalDbContext _db;
    private readonly AuthService _auth;
    public ProfileModel(HospitalDbContext db, AuthService auth) { _db = db; _auth = auth; }

    public Patient? CurrentPatient { get; set; }
    public string? SuccessMessage { get; set; }
    public string? ErrorMessage { get; set; }

    // Profile fields
    [BindProperty] public string PhoneNumber { get; set; } = "";
    [BindProperty][EmailAddress] public string Email { get; set; } = "";
    [BindProperty] public string Address { get; set; } = "";
    [BindProperty] public string EmergencyContactName { get; set; } = "";
    [BindProperty] public string EmergencyContactPhone { get; set; } = "";

    // Password change fields
    [BindProperty][DataType(DataType.Password)] public string? CurrentPassword { get; set; }
    [BindProperty][DataType(DataType.Password)][MinLength(6)] public string? NewPassword { get; set; }
    [BindProperty][DataType(DataType.Password)][Compare("NewPassword")] public string? ConfirmNewPassword { get; set; }

    public void OnGet()
    {
        LoadPatient();
        if (CurrentPatient != null)
        {
            PhoneNumber           = CurrentPatient.PhoneNumber;
            Email                 = CurrentPatient.Email;
            Address               = CurrentPatient.Address;
            EmergencyContactName  = CurrentPatient.EmergencyContactName;
            EmergencyContactPhone = CurrentPatient.EmergencyContactPhone;
        }
    }

    public IActionResult OnPostUpdateProfile()
    {
        LoadPatient();
        if (CurrentPatient == null) return Page();

        CurrentPatient.PhoneNumber           = PhoneNumber;
        CurrentPatient.Email                 = Email;
        CurrentPatient.Address               = Address;
        CurrentPatient.EmergencyContactName  = EmergencyContactName;
        CurrentPatient.EmergencyContactPhone = EmergencyContactPhone;
        _db.SaveChanges();

        SuccessMessage = "Profile updated successfully.";
        return Page();
    }

    public IActionResult OnPostChangePassword()
    {
        LoadPatient();
        var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
        if (string.IsNullOrEmpty(CurrentPassword) || string.IsNullOrEmpty(NewPassword))
        {
            ErrorMessage = "Please fill in all password fields.";
            return Page();
        }
        var (success, message) = _auth.ChangePassword(userId, CurrentPassword, NewPassword);
        if (success) SuccessMessage = message;
        else ErrorMessage = message;
        return Page();
    }

    private void LoadPatient()
    {
        var claim = User.FindFirst("PatientId")?.Value;
        if (int.TryParse(claim, out int id)) CurrentPatient = _db.Patients.Find(id);
    }
}
