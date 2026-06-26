using HospitalManagement.Core;
using HospitalManagement.Core.Models;
using HospitalManagement.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace HospitalWeb.Pages.Auth;

public class LoginModel : PageModel
{
    private readonly AuthService _auth;
    public LoginModel(AuthService auth) => _auth = auth;

    [BindProperty][Required] public string Username { get; set; } = "";
    [BindProperty][Required][DataType(DataType.Password)] public string Password { get; set; } = "";
    [BindProperty] public bool RememberMe { get; set; }
    public string? ErrorMessage { get; set; }

    public IActionResult OnGet(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectBasedOnRole();
        ViewData["ReturnUrl"] = returnUrl;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
    {
        if (!ModelState.IsValid) return Page();

        var user = _auth.Login(Username, Password);
        if (user == null)
        {
            ErrorMessage = "Invalid username or password.";
            return Page();
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier,          user.Id.ToString()),
            new(ClaimTypes.Name,                    user.Username),
            new(ClaimTypes.Role,                    user.Role.ToString()),
            new(ClaimTypes.Email,                   user.Email),
            new(ClaimsPrincipalExtensions.FullNameClaim,  user.FullName),
            new(ClaimsPrincipalExtensions.PatientIdClaim, user.PatientId?.ToString() ?? ""),
            // Unique session token — used by audit trail to correlate all
            // requests from the same login session
            new(ClaimsPrincipalExtensions.AuditIdClaim, Guid.NewGuid().ToString("N")),
        };

        var identity  = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);
        var props     = new AuthenticationProperties
        {
            IsPersistent = RememberMe,
            ExpiresUtc   = RememberMe
                ? DateTimeOffset.UtcNow.AddDays(7)
                : DateTimeOffset.UtcNow.AddHours(8)
        };

        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, props);

        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            return Redirect(returnUrl);

        return user.Role == UserRole.Admin
            ? RedirectToPage("/Index")
            : RedirectToPage("/Dashboard/Index");
    }

    private IActionResult RedirectBasedOnRole() =>
        User.IsAdmin()
            ? RedirectToPage("/Index")
            : RedirectToPage("/Dashboard/Index");
}
