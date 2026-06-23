using HospitalManagement.Core.Models;
using HospitalManagement.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace HospitalWeb.Pages.Admin;

[Authorize(Roles = "Admin")]
public class UsersModel : PageModel
{
    private readonly AuthService _auth;
    public UsersModel(AuthService auth) => _auth = auth;

    public List<AppUser> Users { get; set; } = new();
    public string? ErrorMessage { get; set; }

    [BindProperty][Required] public string NewUsername { get; set; } = "";
    [BindProperty][Required][MinLength(6)] public string NewPassword { get; set; } = "";
    [BindProperty][Required] public string NewFullName { get; set; } = "";
    [BindProperty][EmailAddress] public string NewEmail { get; set; } = "";

    public void OnGet() => Users = _auth.GetAll();

    public IActionResult OnPostCreate()
    {
        Users = _auth.GetAll();
        if (!ModelState.IsValid) return Page();

        var (success, message, _) = _auth.CreateAdmin(NewUsername, NewPassword, NewFullName, NewEmail);
        if (!success) { ErrorMessage = message; return Page(); }
        TempData["Success"] = $"Admin account '{NewUsername}' created.";
        return RedirectToPage();
    }

    public IActionResult OnPostToggle(int id)
    {
        // Prevent admins from deactivating themselves
        var selfId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
        if (id == selfId)
        {
            TempData["Error"] = "You cannot deactivate your own account.";
            return RedirectToPage();
        }
        _auth.ToggleActive(id);
        return RedirectToPage();
    }
}
