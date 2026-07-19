using HospitalManagement.Core;
using HospitalManagement.Core.Models;

namespace HospitalWeb.ViewModels.Appointments;

public class AppointmentListViewModel
{
    public PagedResult<Appointment> Appointments { get; set; } = null!;
    public string Filter     { get; set; } = "upcoming";
    public int    PageNumber { get; set; } = 1;
}
