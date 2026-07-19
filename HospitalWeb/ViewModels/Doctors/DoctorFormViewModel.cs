using System.ComponentModel.DataAnnotations;
using HospitalManagement.Core.Models;

namespace HospitalWeb.ViewModels.Doctors;

public class DoctorFormViewModel
{
    [Required] public string FirstName { get; set; } = "";
    [Required] public string LastName  { get; set; } = "";
    public Specialization Specialization { get; set; }
    [Required] public string PhoneNumber    { get; set; } = "";
    [EmailAddress] public string Email      { get; set; } = "";
    [Required] public string LicenseNumber  { get; set; } = "";
    public decimal ConsultationFee { get; set; }
    public List<int> AvailableDayInts { get; set; } = new();
}
