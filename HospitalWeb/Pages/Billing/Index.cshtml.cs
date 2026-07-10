using Microsoft.AspNetCore.Mvc;
using HospitalManagement.Core;
using HospitalManagement.Core.Models;
using HospitalManagement.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HospitalWeb.Pages.Billing;

[Authorize(Policy = "AdminOnly")]
public class IndexModel : PageModel
{
    private readonly BillingService _billing;
    public IndexModel(BillingService billing) => _billing = billing;

    [BindProperty(SupportsGet = true)] public string Filter { get; set; } = "all";
    [BindProperty(SupportsGet = true)] public int PageNumber { get; set; } = 1;
    public const int PageSize = 15;

    public PagedResult<Bill> Bills { get; set; } = null!;
    public decimal TotalRevenue { get; set; }
    public decimal Outstanding { get; set; }

    public void OnGet()
    {
        Bills = PagedResult<Bill>.Create(
            _billing.Query(Filter),
            PageNumber, PageSize);

        TotalRevenue = _billing.GetTotalRevenue();
        Outstanding  = _billing.GetOutstandingBalance();
    }
}
