using HospitalManagement.Core.Models;
using HospitalManagement.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace HospitalWeb.Pages.Auth;

public class RegisterModel : PageModel
{
    private readonly AuthService _auth;
    public RegisterModel(AuthService auth) => _auth = auth;

    [BindProperty][Required] public string Username { get; set; } = "";
    [BindProperty][Required][DataType(DataType.Password)][MinLength(6, ErrorMessage = "Password must be at least 6 characters.")] public string Password { get; set; } = "";
    [BindProperty][Required][DataType(DataType.Password)][Compare("Password", ErrorMessage = "Passwords do not match.")] public string ConfirmPassword { get; set; } = "";
    [BindProperty][Required] public string FirstName { get; set; } = "";
    [BindProperty][Required] public string LastName { get; set; } = "";
    [BindProperty][Required] public DateTime DateOfBirth { get; set; } = DateTime.Today.AddYears(-25);
    [BindProperty] public Gender Gender { get; set; }
    [BindProperty] public BloodType BloodType { get; set; } = BloodType.Unknown;
    [BindProperty][Required] public string PhoneNumber { get; set; } = "";
    [BindProperty][EmailAddress] public string Email { get; set; } = "";
    [BindProperty] public string Address { get; set; } = "";
    [BindProperty][Required] public string EmergencyContactName { get; set; } = "";
    [BindProperty][Required] public string EmergencyContactPhone { get; set; } = "";
    public string? ErrorMessage { get; set; }

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();

        var (success, message, user) = _auth.RegisterPatient(
            Username, Password,
            FirstName, LastName, DateOfBirth,
            Gender, BloodType,
            PhoneNumber, Email, Address,
            EmergencyContactName, EmergencyContactPhone);

        if (!success) { ErrorMessage = message; return Page(); }

        // Auto-login after registration
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user!.Id.ToString()),
            new(ClaimTypes.Name,           user.Username),
            new(ClaimTypes.Role,           user.Role.ToString()),
            new("FullName",                user.FullName),
            new("PatientId",               user.PatientId?.ToString() ?? ""),
        };
        var identity  = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

        TempData["Success"] = $"Welcome, {user.FullName}! Your account has been created.";
        return RedirectToPage("/Dashboard/Index");
    }
}
