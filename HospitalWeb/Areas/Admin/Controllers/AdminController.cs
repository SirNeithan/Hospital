using HospitalManagement.Core;
using HospitalManagement.Core.Models;
using HospitalManagement.Data;
using HospitalManagement.Services;
using HospitalWeb.ViewModels.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HospitalWeb.Areas.Admin.Controllers;

[Authorize(Policy = "AdminOnly")]
[Area("Admin")]
public class AdminController : Controller
{
    private readonly AuthService       _auth;
    private readonly HospitalDbContext _db;

    public AdminController(AuthService auth, HospitalDbContext db)
    { _auth = auth; _db = db; }

    // GET /Admin/Users
    public IActionResult Users()
    {
        var vm = new UserListViewModel { Users = _auth.GetAll() };
        return View(vm);
    }

    // POST /Admin/Users/Create
    [HttpPost][ValidateAntiForgeryToken]
    public IActionResult CreateUser(UserListViewModel vm)
    {
        if (!ModelState.IsValid)
        {
            vm.Users = _auth.GetAll();
            return View("Users", vm);
        }
        var (success, message, _) = _auth.CreateAdmin(
            vm.NewUsername, vm.NewPassword, vm.NewFullName, vm.NewEmail);
        if (!success)
        {
            vm.ErrorMessage = message;
            vm.Users = _auth.GetAll();
            return View("Users", vm);
        }
        TempData["Success"] = $"Admin account '{vm.NewUsername}' created.";
        return RedirectToAction(nameof(Users), "Admin", new { area = "Admin" });
    }

    // POST /Admin/Users/Toggle/5
    [HttpPost][ValidateAntiForgeryToken]
    public IActionResult ToggleUser(int id)
    {
        var selfId = User.GetUserId();
        if (id == selfId)
        {
            TempData["Error"] = "You cannot deactivate your own account.";
            return RedirectToAction(nameof(Users), "Admin", new { area = "Admin" });
        }
        _auth.ToggleActive(id);
        return RedirectToAction(nameof(Users), "Admin", new { area = "Admin" });
    }

    // GET /Admin/AuditLogs
    public IActionResult AuditLogs(string? filterUser, string? filterMethod, int currentPage = 1)
    {
        const int pageSize = 50;
        var today = DateTime.UtcNow.Date;

        var query = _db.AuditLogs.AsQueryable();
        if (!string.IsNullOrWhiteSpace(filterUser))
            query = query.Where(l => l.Username != null && l.Username.Contains(filterUser));
        if (!string.IsNullOrWhiteSpace(filterMethod))
            query = query.Where(l => l.Method == filterMethod.ToUpper());

        var totalCount = query.Count();
        var logs = query.OrderByDescending(l => l.Timestamp)
            .Skip((currentPage - 1) * pageSize).Take(pageSize).ToList();

        var todayLogs = _db.AuditLogs.Where(l => l.Timestamp >= today).ToList();

        var vm = new AuditLogViewModel
        {
            Logs           = logs,
            TotalCount     = totalCount,
            CurrentPage    = currentPage,
            TotalPages     = (int)Math.Ceiling(totalCount / (double)pageSize),
            FilterUser     = filterUser,
            FilterMethod   = filterMethod,
            TodayRequests    = todayLogs.Count,
            UniqueUsersToday = todayLogs.Where(l => l.Username != null).Select(l => l.Username).Distinct().Count(),
            AvgDurationMs    = todayLogs.Any() ? todayLogs.Average(l => l.DurationMs) : 0,
            ErrorCount       = todayLogs.Count(l => l.StatusCode >= 400)
        };
        return View(vm);
    }

    // POST /Admin/AuditLogs/Clear
    [HttpPost][ValidateAntiForgeryToken]
    public IActionResult ClearAuditLogs()
    {
        var cutoff = DateTime.UtcNow.AddDays(-7);
        var old = _db.AuditLogs.Where(l => l.Timestamp < cutoff).ToList();
        _db.AuditLogs.RemoveRange(old);
        _db.SaveChanges();
        TempData["Success"] = $"Cleared {old.Count} audit entries older than 7 days.";
        return RedirectToAction(nameof(AuditLogs), "Admin", new { area = "Admin" });
    }
}
