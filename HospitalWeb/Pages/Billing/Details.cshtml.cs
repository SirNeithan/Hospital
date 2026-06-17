using HospitalWeb.Models;
using HospitalWeb.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HospitalWeb.Pages.Billing;

public class DetailsModel : PageModel
{
    private readonly BillingService _billing;
    public DetailsModel(BillingService billing) => _billing = billing;

    public Bill? Bill { get; set; }

    public IActionResult OnGet(int id)
    {
        Bill = _billing.GetById(id);
        if (Bill == null) return NotFound();
        return Page();
    }
}
