using HospitalManagement.Core;
using HospitalManagement.Core.Models;

namespace HospitalWeb.ViewModels.Patients;

public class PatientListViewModel
{
    public PagedResult<Patient> Patients { get; set; } = null!;
    public string? Search { get; set; }
    public int PageNumber { get; set; } = 1;
}
