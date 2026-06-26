using HospitalManagement.Core.Models;
using HospitalManagement.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HospitalWeb.Pages.Admin;

[Authorize(Policy = "AdminOnly")]
public class AuditLogsModel : PageModel
{
    private readonly HospitalDbContext _db;
    public AuditLogsModel(HospitalDbContext db) => _db = db;

    [BindProperty(SupportsGet = true)] public string? FilterUser { get; set; }
    [BindProperty(SupportsGet = true)] public string? FilterMethod { get; set; }
    [BindProperty(SupportsGet = true)] public int CurrentPage { get; set; } = 1;
    public const int PageSize = 50;

    public List<AuditLog> Logs { get; set; } = new();
    public int TotalCount { get; set; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);

    // Summary stats
    public int TodayRequests { get; set; }
    public int UniqueUsersToday { get; set; }
    public double AvgDurationMs { get; set; }
    public int ErrorCount { get; set; }

    public void OnGet()
    {
        var today = DateTime.UtcNow.Date;
        var query = _db.AuditLogs.AsQueryable();

        if (!string.IsNullOrWhiteSpace(FilterUser))
            query = query.Where(l => l.Username != null &&
                                     l.Username.Contains(FilterUser));

        if (!string.IsNullOrWhiteSpace(FilterMethod))
            query = query.Where(l => l.Method == FilterMethod.ToUpper());

        TotalCount = query.Count();
        Logs = query
            .OrderByDescending(l => l.Timestamp)
            .Skip((CurrentPage - 1) * PageSize)
            .Take(PageSize)
            .ToList();

        // Stats (always from full data, not filtered)
        var todayLogs = _db.AuditLogs.Where(l => l.Timestamp >= today).ToList();
        TodayRequests    = todayLogs.Count;
        UniqueUsersToday = todayLogs.Where(l => l.Username != null)
                                    .Select(l => l.Username).Distinct().Count();
        AvgDurationMs    = todayLogs.Any() ? todayLogs.Average(l => l.DurationMs) : 0;
        ErrorCount       = todayLogs.Count(l => l.StatusCode >= 400);
    }

    public IActionResult OnPostClear()
    {
        // Keep last 7 days, purge the rest
        var cutoff = DateTime.UtcNow.AddDays(-7);
        var old = _db.AuditLogs.Where(l => l.Timestamp < cutoff).ToList();
        _db.AuditLogs.RemoveRange(old);
        _db.SaveChanges();
        TempData["Success"] = $"Cleared {old.Count} audit log entries older than 7 days.";
        return RedirectToPage();
    }
}
