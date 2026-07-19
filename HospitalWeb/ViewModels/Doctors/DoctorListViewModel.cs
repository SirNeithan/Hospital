using HospitalManagement.Core;
using HospitalManagement.Core.Models;

namespace HospitalWeb.ViewModels.Doctors;

public class DoctorListViewModel
{
    public PagedResult<Doctor> Doctors { get; set; } = null!;
    public string? Search { get; set; }
    public string? Spec   { get; set; }
    public int PageNumber { get; set; } = 1;
}
