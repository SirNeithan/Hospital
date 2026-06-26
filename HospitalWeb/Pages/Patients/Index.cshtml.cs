using HospitalManagement.Core.Models;
using HospitalManagement.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HospitalWeb.Pages.Patients;

[Authorize(Policy = "AdminOnly")]
public class IndexModel : PageModel
{
    private readonly PatientService _patients;

    public IndexModel(PatientService patients) => _patients = patients;

    [BindProperty(SupportsGet = true)]
    public string? Search { get; set; }

    public List<Patient> Patients { get; set; } = new();

    public void OnGet()
    {
        Patients = string.IsNullOrWhiteSpace(Search)
            ? _patients.GetAll()
            : _patients.Search(Search);
    }
}
