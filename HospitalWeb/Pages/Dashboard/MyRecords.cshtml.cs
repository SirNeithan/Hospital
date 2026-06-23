using HospitalManagement.Data;
using HospitalManagement.Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HospitalWeb.Pages.Dashboard;

[Authorize(Roles = "Patient")]
public class MyRecordsModel : PageModel
{
    private readonly HospitalDbContext _db;
    public MyRecordsModel(HospitalDbContext db) => _db = db;

    public List<MedicalRecord> Records { get; set; } = new();
    public string PatientName { get; set; } = "";

    public void OnGet()
    {
        var claim = User.FindFirst("PatientId")?.Value;
        if (!int.TryParse(claim, out int patientId)) return;

        var patient = _db.Patients.Find(patientId);
        PatientName = patient?.FullName ?? "";

        Records = _db.MedicalRecords
            .Where(r => r.PatientId == patientId)
            .OrderByDescending(r => r.RecordDate)
            .ToList();
    }
}
