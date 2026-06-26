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

    public List<Doctor> Doctors { get; set; } = new();

    public void OnGet()
    {
        Doctors = string.IsNullOrWhiteSpace(Search)
            ? _doctors.GetAll()
            : _doctors.Search(Search);
    }

    public IActionResult OnPostToggle(int id)
    {
        _doctors.ToggleAvailability(id);
        return RedirectToPage();
    }
}
