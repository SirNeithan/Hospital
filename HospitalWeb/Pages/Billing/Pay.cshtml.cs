using HospitalWeb.Models;
using HospitalWeb.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HospitalWeb.Pages.Billing;

public class PayModel : PageModel
{
    private readonly BillingService _billing;
    public PayModel(BillingService billing) => _billing = billing;

    public Bill? Bill { get; set; }
    [BindProperty] public decimal Amount { get; set; }
    public string? ErrorMessage { get; set; }

    public IActionResult OnGet(int id)
    {
        Bill = _billing.GetById(id);
        if (Bill == null) return NotFound();
        Amount = Bill.Balance;
        return Page();
    }

    public IActionResult OnPost(int id)
    {
        Bill = _billing.GetById(id);
        if (Bill == null) return NotFound();
        var (success, message) = _billing.RecordPayment(id, Amount);
        if (!success) { ErrorMessage = message; return Page(); }
        TempData["Success"] = message;
        return RedirectToPage("./Details", new { id });
    }
}
