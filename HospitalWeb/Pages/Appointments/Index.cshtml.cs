using HospitalManagement.Core;
using HospitalManagement.Core.Models;
using HospitalManagement.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HospitalWeb.Pages.Appointments;

[Authorize(Policy = "AdminOnly")]
public class IndexModel : PageModel
{
    private readonly AppointmentService _appts;
    public IndexModel(AppointmentService appts) => _appts = appts;

    [BindProperty(SupportsGet = true)] public string Filter { get; set; } = "upcoming";
    [BindProperty(SupportsGet = true)] public int PageNumber { get; set; } = 1;
    public const int PageSize = 15;

    public PagedResult<Appointment> Appointments { get; set; } = null!;

    public void OnGet()
    {
        Appointments = PagedResult<Appointment>.Create(
            _appts.Query(Filter),
            PageNumber, PageSize);
    }

    public IActionResult OnPostCancel(int id)
    {
        _appts.Cancel(id);
        return RedirectToPage();
    }
}
