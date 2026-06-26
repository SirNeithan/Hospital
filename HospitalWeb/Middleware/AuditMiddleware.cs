using HospitalManagement.Core;
using HospitalManagement.Core.Models;
using HospitalManagement.Data;
using System.Diagnostics;

namespace HospitalWeb.Middleware;

/// <summary>
/// Middleware that writes an AuditLog record for every HTTP request.
/// Records: who made it, what path, HTTP method, response status, duration, IP.
/// Skips static files (css/js/images/favicon) to keep the log clean.
/// </summary>
public class AuditMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<AuditMiddleware> _logger;

    // Paths we deliberately skip logging (too noisy, no security value)
    private static readonly string[] _skipPrefixes =
    [
        "/css", "/js", "/lib", "/favicon", "/_framework", "/_blazor"
    ];

    public AuditMiddleware(RequestDelegate next, ILogger<AuditMiddleware> logger)
    {
        _next   = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext httpContext, IServiceScopeFactory scopeFactory)
    {
        var path = httpContext.Request.Path.Value ?? "";

        // Skip static assets — no need to audit every Bootstrap CSS load
        if (_skipPrefixes.Any(p => path.StartsWith(p, StringComparison.OrdinalIgnoreCase)))
        {
            await _next(httpContext);
            return;
        }

        var sw = Stopwatch.StartNew();

        // Let the rest of the pipeline run first so we capture the response status
        await _next(httpContext);

        sw.Stop();

        // Read identity from claims (populated by authentication middleware)
        var user       = httpContext.User;
        var userId     = user.GetUserId();
        var username   = user.Identity?.IsAuthenticated == true ? user.Identity.Name : null;
        var role       = user.GetRole();

        var entry = new AuditLog
        {
            UserId      = userId,
            Username    = username,
            Role        = role,
            Method      = httpContext.Request.Method,
            Path        = path,
            QueryString = httpContext.Request.QueryString.HasValue
                            ? httpContext.Request.QueryString.Value
                            : null,
            StatusCode  = httpContext.Response.StatusCode,
            DurationMs  = sw.ElapsedMilliseconds,
            IpAddress   = httpContext.Connection.RemoteIpAddress?.ToString(),
            UserAgent   = httpContext.Request.Headers.UserAgent.ToString()[..Math.Min(
                              httpContext.Request.Headers.UserAgent.ToString().Length, 500)],
            Timestamp   = DateTime.UtcNow
        };

        // Structured log — visible in console/dev tools immediately
        _logger.LogInformation(
            "[AUDIT] {Method} {Path}{Query} → {Status} | {Duration}ms | User: {User} ({Role}) | IP: {IP}",
            entry.Method, entry.Path,
            string.IsNullOrEmpty(entry.QueryString) ? "" : entry.QueryString,
            entry.StatusCode, entry.DurationMs,
            entry.Username ?? "anonymous", entry.Role ?? "-",
            entry.IpAddress ?? "unknown");

        // Persist to DB asynchronously using a fresh scope
        // (HttpContext's scoped DbContext is already disposed at this point
        //  if something upstream failed, so we use a new scope to be safe)
        try
        {
            using var scope = scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<HospitalDbContext>();
            db.AuditLogs.Add(entry);
            await db.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            // Never let audit failure crash the app — just log it
            _logger.LogError(ex, "[AUDIT] Failed to persist audit log entry for {Path}", path);
        }
    }
}

// Extension method so Program.cs reads cleanly
public static class AuditMiddlewareExtensions
{
    public static IApplicationBuilder UseAuditLogging(this IApplicationBuilder app)
        => app.UseMiddleware<AuditMiddleware>();
}
