using HospitalWeb.Data;
using HospitalWeb.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HospitalWeb.Pages.Dashboard;

[Authorize(Roles = "Patient")]
public class DoctorsModel : PageModel
{
    private readonly HospitalDbContext _db;
    public DoctorsModel(HospitalDbContext db) => _db = db;

    [BindProperty(SupportsGet = true)] public string? Specialization { get; set; }
    [BindProperty(SupportsGet = true)] public string? Search { get; set; }
    public List<Doctor> Doctors { get; set; } = new();
    public List<string> Specializations { get; set; } = new();

    public void OnGet()
    {
        Specializations = Enum.GetNames<Specialization>().ToList();

        var query = _db.Doctors.Where(d => d.IsAvailable).AsQueryable();

        if (!string.IsNullOrWhiteSpace(Specialization) &&
            Enum.TryParse<Specialization>(Specialization, out var spec))
            query = query.Where(d => d.Specialization == spec);

        if (!string.IsNullOrWhiteSpace(Search))
        {
            var s = Search.ToLower();
            query = query.Where(d =>
                d.FirstName.ToLower().Contains(s) ||
                d.LastName.ToLower().Contains(s));
        }

        Doctors = query.OrderBy(d => d.Specialization).ThenBy(d => d.LastName).ToList();
    }
}
