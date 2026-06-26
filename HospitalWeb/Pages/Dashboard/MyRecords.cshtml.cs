using HospitalManagement.Core;
using HospitalManagement.Data;
using HospitalManagement.Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HospitalWeb.Pages.Dashboard;

[Authorize(Policy = "PatientOnly")]
public class MyRecordsModel : PageModel
{
    private readonly HospitalDbContext _db;
    public MyRecordsModel(HospitalDbContext db) => _db = db;

    public List<MedicalRecord> Records { get; set; } = new();
    public string PatientName { get; set; } = "";

    public void OnGet()
    {
        var patientId = User.GetPatientId();
        if (patientId == null) return;

        PatientName = _db.Patients.Find(patientId.Value)?.FullName ?? "";
        Records = _db.MedicalRecords
            .Where(r => r.PatientId == patientId.Value)
            .OrderByDescending(r => r.RecordDate)
            .ToList();
    }
}
