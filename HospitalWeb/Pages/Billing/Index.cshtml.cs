using HospitalManagement.Core.Models;
using HospitalManagement.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HospitalWeb.Pages.Billing;

[Authorize(Policy = "AdminOnly")]
public class IndexModel : PageModel
{
    private readonly BillingService _billing;
    public IndexModel(BillingService billing) => _billing = billing;

    [BindProperty(SupportsGet = true)] public string Filter { get; set; } = "all";

    public List<Bill> Bills { get; set; } = new();
    public decimal TotalRevenue { get; set; }
    public decimal Outstanding { get; set; }

    public void OnGet()
    {
        Bills = Filter switch
        {
            "unpaid" => _billing.GetUnpaid(),
            _        => _billing.GetAll().OrderByDescending(b => b.BillDate).ToList()
        };
        TotalRevenue = _billing.GetTotalRevenue();
        Outstanding = _billing.GetOutstandingBalance();
    }
}
