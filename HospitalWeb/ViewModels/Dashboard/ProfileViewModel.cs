using System.ComponentModel.DataAnnotations;
using HospitalManagement.Core.Models;

namespace HospitalWeb.ViewModels.Dashboard;

public class ProfileViewModel
{
    public Patient? CurrentPatient { get; set; }
    public string?  SuccessMessage { get; set; }
    public string?  ErrorMessage   { get; set; }

    // Contact info
    public string PhoneNumber           { get; set; } = "";
    [EmailAddress] public string Email  { get; set; } = "";
    public string Address               { get; set; } = "";
    public string EmergencyContactName  { get; set; } = "";
    public string EmergencyContactPhone { get; set; } = "";

    // Password change
    [DataType(DataType.Password)] public string? CurrentPassword  { get; set; }
    [DataType(DataType.Password)][MinLength(6)] public string? NewPassword { get; set; }
    [DataType(DataType.Password)][Compare("NewPassword")] public string? ConfirmNewPassword { get; set; }
}
