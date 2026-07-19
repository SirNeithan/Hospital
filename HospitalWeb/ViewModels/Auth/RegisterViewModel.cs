using System.ComponentModel.DataAnnotations;
using HospitalManagement.Core.Models;

namespace HospitalWeb.ViewModels.Auth;

/// <summary>Carries new patient registration data from the view to the controller.</summary>
public class RegisterViewModel
{
    // ── Account credentials ───────────────────────────────────────────────
    [Required] public string Username { get; set; } = "";

    [Required][MinLength(6, ErrorMessage = "Password must be at least 6 characters.")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = "";

    [Required][DataType(DataType.Password)]
    [Compare("Password", ErrorMessage = "Passwords do not match.")]
    public string ConfirmPassword { get; set; } = "";

    // ── Personal information ──────────────────────────────────────────────
    [Required] public string FirstName { get; set; } = "";
    [Required] public string LastName  { get; set; } = "";
    [Required] public DateTime DateOfBirth { get; set; } = DateTime.Today.AddYears(-25);
    public Gender    Gender    { get; set; }
    public BloodType BloodType { get; set; } = BloodType.Unknown;

    [Required] public string PhoneNumber { get; set; } = "";
    [EmailAddress] public string Email   { get; set; } = "";
    public string Address { get; set; } = "";

    // ── Emergency contact ─────────────────────────────────────────────────
    [Required] public string EmergencyContactName  { get; set; } = "";
    [Required] public string EmergencyContactPhone { get; set; } = "";

    public string? ErrorMessage { get; set; }
}
