using HospitalManagement.Data;
using HospitalManagement.Core.Models;
using HospitalManagement.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;

namespace HospitalWeb.Pages.Billing;

public class CreateModel : PageModel
{
    private readonly HospitalDbContext _db;
    private readonly BillingService _billing;

    public CreateModel(HospitalDbContext db, BillingService billing)
    { _db = db; _billing = billing; }

    public List<Patient> Patients { get; set; } = new();

    [BindProperty] public int PatientId { get; set; }
    [BindProperty] public string InsuranceProvider { get; set; } = "";
    [BindProperty] public string InsurancePolicyNumber { get; set; } = "";
    [BindProperty] public decimal InsuranceCoverage { get; set; }
    [BindProperty] public string ItemsJson { get; set; } = "[]";

    public void OnGet(int? patientId)
    {
        Patients = _db.Patients.ToList();
        if (patientId.HasValue) PatientId = patientId.Value;
    }

    public IActionResult OnPost()
    {
        Patients = _db.Patients.ToList();
        List<BillItem> items = new();
        try { items = JsonSerializer.Deserialize<List<BillItem>>(ItemsJson) ?? new(); } catch { }

        if (!items.Any()) { ModelState.AddModelError("", "Add at least one billing item."); return Page(); }

        var bill = _billing.GenerateBill(PatientId, items, InsuranceProvider, InsurancePolicyNumber, InsuranceCoverage);
        TempData["Success"] = $"Bill #{bill.Id} generated.";
        return RedirectToPage("./Details", new { id = bill.Id });
    }
}
