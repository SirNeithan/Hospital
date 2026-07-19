using System.ComponentModel.DataAnnotations;
using HospitalManagement.Core.Models;

namespace HospitalWeb.ViewModels.Admin;

public class UserListViewModel
{
    public List<AppUser> Users        { get; set; } = new();
    public string?       ErrorMessage { get; set; }

    // New admin form fields
    [Required] public string NewUsername { get; set; } = "";
    [Required][MinLength(6)] public string NewPassword { get; set; } = "";
    [Required] public string NewFullName { get; set; } = "";
    [EmailAddress] public string NewEmail { get; set; } = "";
}
