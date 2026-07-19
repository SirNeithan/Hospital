using HospitalManagement.Core;
using HospitalManagement.Core.Models;
using HospitalManagement.Data;
using HospitalManagement.Services;
using HospitalWeb.ViewModels.Billing;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace HospitalWeb.Areas.Hospital.Controllers;

[Area("Hospital")]
[Authorize(Policy = "AdminOnly")]
public class BillingController : Controller
{
    private readonly BillingService    _billing;
    private readonly HospitalDbContext _db;

    public BillingController(BillingService billing, HospitalDbContext db)
    { _billing = billing; _db = db; }

    // GET /Billing
    public IActionResult Index(string filter = "all", int pageNumber = 1)
    {
        var vm = new BillingListViewModel
        {
            Filter       = filter,
            PageNumber   = pageNumber,
            Bills        = PagedResult<Bill>.Create(_billing.Query(filter), pageNumber, 15),
            TotalRevenue = _billing.GetTotalRevenue(),
            Outstanding  = _billing.GetOutstandingBalance()
        };
        return View(vm);
    }

    // GET /Billing/Details/5
    public IActionResult Details(int id)
    {
        var bill = _billing.GetById(id);
        if (bill == null) return NotFound();
        return View(bill);
    }

    // GET /Billing/Create
    public IActionResult Create(int? patientId)
    {
        ViewBag.Patients  = _db.Patients.OrderBy(p => p.LastName).ToList();
        ViewBag.PatientId = patientId ?? 0;
        return View();
    }

    // POST /Billing/Create
    [HttpPost][ValidateAntiForgeryToken]
    public IActionResult Create(int patientId, string itemsJson,
        string insuranceProvider, string insurancePolicyNumber, decimal insuranceCoverage)
    {
        ViewBag.Patients = _db.Patients.OrderBy(p => p.LastName).ToList();
        List<BillItem> items = new();
        try { items = JsonSerializer.Deserialize<List<BillItem>>(itemsJson) ?? new(); } catch { }
        if (!items.Any())
        {
            ModelState.AddModelError("", "Add at least one billing item.");
            ViewBag.PatientId = patientId;
            return View();
        }
        var bill = _billing.GenerateBill(patientId, items, insuranceProvider, insurancePolicyNumber, insuranceCoverage);
        TempData["Success"] = $"Bill #{bill.Id} generated.";
        return RedirectToAction(nameof(Details), new { id = bill.Id });
    }

    // GET /Billing/Pay/5
    public IActionResult Pay(int id)
    {
        var bill = _billing.GetById(id);
        if (bill == null) return NotFound();
        ViewBag.Amount = bill.Balance;
        return View(bill);
    }

    // POST /Billing/Pay/5
    [HttpPost][ValidateAntiForgeryToken]
    public IActionResult Pay(int id, decimal amount)
    {
        var bill = _billing.GetById(id);
        if (bill == null) return NotFound();
        var (success, message) = _billing.RecordPayment(id, amount);
        if (!success) { ViewBag.ErrorMessage = message; ViewBag.Amount = amount; return View(bill); }
        TempData["Success"] = message;
        return RedirectToAction(nameof(Details), new { id });
    }
}
