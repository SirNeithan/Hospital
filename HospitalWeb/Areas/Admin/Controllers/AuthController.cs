using HospitalManagement.Core;
using HospitalManagement.Core.Models;
using HospitalManagement.Services;
using HospitalWeb.ViewModels.Auth;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HospitalWeb.Areas.Admin.Controllers;

[Area("Admin")]
public class AuthController : Controller
{
    private readonly AuthService _auth;
    public AuthController(AuthService auth) => _auth = auth;

    // GET /Admin/Auth/Login
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectAfterLogin();
        ViewBag.ReturnUrl = returnUrl;
        return View(new LoginViewModel());
    }

    // POST /Admin/Auth/Login
    [HttpPost][ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel vm, string? returnUrl = null)
    {
        if (!ModelState.IsValid) return View(vm);

        var user = _auth.Login(vm.Username, vm.Password);
        if (user == null)
        {
            vm.ErrorMessage = "Invalid username or password.";
            return View(vm);
        }

        await SignInUser(user, vm.RememberMe);

        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            return Redirect(returnUrl);

        return RedirectAfterLogin(user.Role);
    }

    // GET /Admin/Auth/Register
    public IActionResult Register() => View(new RegisterViewModel());

    // POST /Admin/Auth/Register
    [HttpPost][ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel vm)
    {
        if (!ModelState.IsValid) return View(vm);

        var (success, message, user) = _auth.RegisterPatient(
            vm.Username, vm.Password,
            vm.FirstName, vm.LastName, vm.DateOfBirth,
            vm.Gender, vm.BloodType,
            vm.PhoneNumber, vm.Email, vm.Address,
            vm.EmergencyContactName, vm.EmergencyContactPhone);

        if (!success) { vm.ErrorMessage = message; return View(vm); }

        await SignInUser(user!, rememberMe: false);
        TempData["Success"] = $"Welcome, {user!.FullName}! Your account has been created.";
        // Redirect to Portal area Dashboard
        return RedirectToAction("Index", "Dashboard", new { area = "Portal" });
    }

    // GET /Admin/Auth/Logout
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Login", "Auth", new { area = "Admin" });
    }

    // GET /Admin/Auth/AccessDenied
    public IActionResult AccessDenied() => View();

    // ── Helpers ──────────────────────────────────────────────────────────

    private async Task SignInUser(AppUser user, bool rememberMe)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier,               user.Id.ToString()),
            new(ClaimTypes.Name,                         user.Username),
            new(ClaimTypes.Role,                         user.Role.ToString()),
            new(ClaimTypes.Email,                        user.Email),
            new(ClaimsPrincipalExtensions.FullNameClaim, user.FullName),
            new(ClaimsPrincipalExtensions.PatientIdClaim,user.PatientId?.ToString() ?? ""),
            new(ClaimsPrincipalExtensions.AuditIdClaim,  Guid.NewGuid().ToString("N")),
        };
        var identity  = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);
        var props = new AuthenticationProperties
        {
            IsPersistent = rememberMe,
            ExpiresUtc   = rememberMe
                ? DateTimeOffset.UtcNow.AddDays(7)
                : DateTimeOffset.UtcNow.AddHours(8)
        };
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, props);
    }

    private IActionResult RedirectAfterLogin(UserRole role = UserRole.Patient) =>
        role == UserRole.Admin
            ? RedirectToAction("Index", "Home", new { area = "Hospital" })
            : RedirectToAction("Index", "Dashboard", new { area = "Portal" });

    private IActionResult RedirectAfterLogin() =>
        User.IsAdmin()
            ? RedirectToAction("Index", "Home",      new { area = "Hospital" })
            : RedirectToAction("Index", "Dashboard", new { area = "Portal" });
}
