using System.ComponentModel.DataAnnotations;
using HospitalManagement.Core.Models;

namespace HospitalWeb.ViewModels.Dashboard;

public class BookAppointmentViewModel
{
    public List<Doctor> Doctors      { get; set; } = new();
    public string?      ErrorMessage { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Please select a doctor.")]
    public int DoctorId { get; set; }

    [Required(ErrorMessage = "Please choose a date and time.")]
    public DateTime AppointmentDate { get; set; } = DateTime.Today.AddDays(1).AddHours(9);

    [Required(ErrorMessage = "Please provide a reason for the visit.")]
    public string Reason { get; set; } = "";

    public string Notes { get; set; } = "";
}
