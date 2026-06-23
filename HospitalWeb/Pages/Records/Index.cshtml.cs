using HospitalWeb.Models;
using HospitalWeb.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HospitalWeb.Pages.Records;

[Authorize(Roles = "Admin")]
public class IndexModel : PageModel
{
    private readonly MedicalRecordService _records;
    public IndexModel(MedicalRecordService records) => _records = records;

    public List<MedicalRecord> Records { get; set; } = new();

    public void OnGet() => Records = _records.GetAll().OrderByDescending(r => r.RecordDate).ToList();
}
