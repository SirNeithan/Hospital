namespace HospitalManagement.Core.Models;

public class AuditLog
{
    public int Id { get; set; }

    // Who made the request (null = anonymous)
    public int? UserId { get; set; }
    public string? Username { get; set; }
    public string? Role { get; set; }

    // What they did
    public string Method { get; set; } = string.Empty;   // GET, POST, etc.
    public string Path { get; set; } = string.Empty;
    public string? QueryString { get; set; }
    public int StatusCode { get; set; }
    public long DurationMs { get; set; }

    // Context
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
