using HospitalManagement.Core;
using HospitalManagement.Core.Models;
using HospitalManagement.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HospitalWeb.Pages.Doctors;

[Authorize(Policy = "AdminOnly")]
public class IndexModel : PageModel
{
    private readonly DoctorService _doctors;
    public IndexModel(DoctorService doctors) => _doctors = doctors;

    [BindProperty(SupportsGet = true)] public string? Search { get; set; }
    [BindProperty(SupportsGet = true)] public string? Spec { get; set; }
    [BindProperty(SupportsGet = true)] public int PageNumber { get; set; } = 1;
    public const int PageSize = 15;

    public PagedResult<Doctor> Doctors { get; set; } = null!;

    public void OnGet()
    {
        Specialization? spec = Enum.TryParse<Specialization>(Spec, out var s) ? s : null;
        Doctors = PagedResult<Doctor>.Create(
            _doctors.Query(Search, spec),
            PageNumber, PageSize);
    }

    public IActionResult OnPostToggle(int id)
    {
        _doctors.ToggleAvailability(id);
        return RedirectToPage();
    }
}
