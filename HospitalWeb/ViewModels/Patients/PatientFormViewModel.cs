using System.ComponentModel.DataAnnotations;
using HospitalManagement.Core.Models;

namespace HospitalWeb.ViewModels.Patients;

/// <summary>Used for both Create and Edit patient forms.</summary>
public class PatientFormViewModel
{
    public int Id { get; set; }  // 0 on create

    [Required] public string FirstName { get; set; } = "";
    [Required] public string LastName  { get; set; } = "";

    [Required] public DateTime DateOfBirth { get; set; } = DateTime.Today.AddYears(-30);
    public Gender    Gender    { get; set; }
    public BloodType BloodType { get; set; }

    [Required] public string PhoneNumber { get; set; } = "";
    [EmailAddress] public string Email   { get; set; } = "";
    public string Address { get; set; } = "";

    [Required] public string EmergencyContactName  { get; set; } = "";
    [Required] public string EmergencyContactPhone { get; set; } = "";
}
