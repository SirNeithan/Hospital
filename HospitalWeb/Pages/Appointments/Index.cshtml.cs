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

    public List<Appointment> Appointments { get; set; } = new();

    public void OnGet()
    {
        Appointments = Filter switch
        {
            "today"    => _appts.GetToday(),
            "all"      => _appts.GetAll().OrderByDescending(a => a.AppointmentDate).ToList(),
            _          => _appts.GetUpcoming(30)
        };
    }

    public IActionResult OnPostCancel(int id)
    {
        _appts.Cancel(id);
        return RedirectToPage();
    }
}
