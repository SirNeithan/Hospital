using System.Security.Claims;

namespace HospitalManagement.Core;

/// <summary>
/// Extension methods for ClaimsPrincipal to read hospital-specific claims
/// cleanly, without scattering magic strings across the codebase.
/// </summary>
public static class ClaimsPrincipalExtensions
{
    // ── Claim type constants ──────────────────────────────────────────────
    public const string FullNameClaim  = "FullName";
    public const string PatientIdClaim = "PatientId";
    public const string AuditIdClaim   = "AuditId";   // unique per login session

    // ── Typed accessors ───────────────────────────────────────────────────

    /// <summary>Returns the user's database Id, or null if not authenticated.</summary>
    public static int? GetUserId(this ClaimsPrincipal user)
    {
        var value = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(value, out var id) ? id : null;
    }

    /// <summary>Returns the linked PatientId for Patient-role users.</summary>
    public static int? GetPatientId(this ClaimsPrincipal user)
    {
        var value = user.FindFirst(PatientIdClaim)?.Value;
        return int.TryParse(value, out var id) && id > 0 ? id : null;
    }

    /// <summary>Returns the display name stored in the FullName claim.</summary>
    public static string GetFullName(this ClaimsPrincipal user)
        => user.FindFirst(FullNameClaim)?.Value ?? user.Identity?.Name ?? "Unknown";

    /// <summary>Returns the role string (Admin / Patient).</summary>
    public static string? GetRole(this ClaimsPrincipal user)
        => user.FindFirst(ClaimTypes.Role)?.Value;

    /// <summary>Returns the unique audit session Id stamped at login.</summary>
    public static string? GetAuditId(this ClaimsPrincipal user)
        => user.FindFirst(AuditIdClaim)?.Value;

    /// <summary>True if the current user is an Admin.</summary>
    public static bool IsAdmin(this ClaimsPrincipal user)
        => user.IsInRole("Admin");

    /// <summary>True if the current user is a Patient.</summary>
    public static bool IsPatient(this ClaimsPrincipal user)
        => user.IsInRole("Patient");
}
