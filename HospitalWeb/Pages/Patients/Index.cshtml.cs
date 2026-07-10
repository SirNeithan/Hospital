using Microsoft.AspNetCore.Mvc;
using HospitalManagement.Core;
using HospitalManagement.Core.Models;
using HospitalManagement.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HospitalWeb.Pages.Patients;

[Authorize(Policy = "AdminOnly")]
public class IndexModel : PageModel
{
    private readonly PatientService _patients;
    public IndexModel(PatientService patients) => _patients = patients;

    [BindProperty(SupportsGet = true)] public string? Search { get; set; }
    [BindProperty(SupportsGet = true)] public int PageNumber { get; set; } = 1;
    public const int PageSize = 15;

    public PagedResult<Patient> Patients { get; set; } = null!;

    public void OnGet()
    {
        Patients = PagedResult<Patient>.Create(
            _patients.Query(Search),
            PageNumber, PageSize);
    }
}
