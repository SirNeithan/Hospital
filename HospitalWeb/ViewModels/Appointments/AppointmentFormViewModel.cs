using System.ComponentModel.DataAnnotations;
using HospitalManagement.Core.Models;

namespace HospitalWeb.ViewModels.Appointments;

public class AppointmentFormViewModel
{
    public List<Patient> Patients { get; set; } = new();
    public List<Doctor>  Doctors  { get; set; } = new();
    public string? ErrorMessage   { get; set; }

    public int PatientId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Please select a doctor.")]
    public int DoctorId { get; set; }

    [Required] public DateTime AppointmentDate { get; set; } = DateTime.Today.AddHours(9);
    [Required] public string   Reason          { get; set; } = "";
    public string Notes { get; set; } = "";
}
