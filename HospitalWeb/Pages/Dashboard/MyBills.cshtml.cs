using HospitalManagement.Core.Models;
using HospitalManagement.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HospitalWeb.Pages.Dashboard;

[Authorize(Roles = "Patient")]
public class MyBillsModel : PageModel
{
    private readonly BillingService _billing;
    public MyBillsModel(BillingService billing) => _billing = billing;

    public List<Bill> Bills { get; set; } = new();
    public decimal TotalPaid { get; set; }
    public decimal TotalOutstanding { get; set; }

    public void OnGet()
    {
        var claim = User.FindFirst("PatientId")?.Value;
        if (!int.TryParse(claim, out int patientId)) return;

        Bills = _billing.GetByPatient(patientId);
        TotalPaid        = Bills.Sum(b => b.AmountPaid);
        TotalOutstanding = Bills
            .Where(b => b.Status != BillStatus.Paid && b.Status != BillStatus.Cancelled)
            .Sum(b => b.Balance);
    }
}
