using HospitalManagement.Core.Models;

namespace HospitalWeb.ViewModels.Admin;

public class AuditLogViewModel
{
    public List<AuditLog> Logs           { get; set; } = new();
    public int            TotalCount     { get; set; }
    public int            CurrentPage    { get; set; } = 1;
    public int            TotalPages     { get; set; }
    public string?        FilterUser     { get; set; }
    public string?        FilterMethod   { get; set; }

    // Stats
    public int    TodayRequests    { get; set; }
    public int    UniqueUsersToday { get; set; }
    public double AvgDurationMs    { get; set; }
    public int    ErrorCount       { get; set; }

    public const int PageSize = 50;
}
