using HospitalManagement.Core;
using HospitalManagement.Data;
using Microsoft.AspNetCore.Authorization;

namespace HospitalWeb.Authorization;

/// <summary>
/// A custom authorization requirement that verifies the authenticated user
/// still exists AND is still active in the database.
/// This catches cases where an admin deactivated an account after the user
/// already logged in (the cookie would still be valid otherwise).
/// </summary>
public class ActiveUserRequirement : IAuthorizationRequirement { }

public class ActiveUserHandler : AuthorizationHandler<ActiveUserRequirement>
{
    // Use IServiceScopeFactory because AuthorizationHandlers are singletons
    // but DbContext is scoped — we must create our own scope.
    private readonly IServiceScopeFactory _scopeFactory;

    public ActiveUserHandler(IServiceScopeFactory scopeFactory)
        => _scopeFactory = scopeFactory;

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        ActiveUserRequirement requirement)
    {
        var userId = context.User.GetUserId();
        if (userId == null)
        {
            context.Fail();
            return;
        }

        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<HospitalDbContext>();
        var user = await db.AppUsers.FindAsync(userId.Value);

        if (user is { IsActive: true })
            context.Succeed(requirement);
        else
            context.Fail();
    }
}
