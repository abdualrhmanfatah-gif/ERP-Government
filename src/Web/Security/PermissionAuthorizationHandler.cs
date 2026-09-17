using System.Security.Claims;

using ERP_Government.Application.Common.Interfaces;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;

namespace ERP_Government.Web.Security;

/// <summary>
/// Requires an authenticated principal holding the named permission.
/// </summary>
public sealed class PermissionRequirement(string permission) : IAuthorizationRequirement
{
    public string Permission { get; } = permission;
}

/// <summary>
/// Evaluates <see cref="PermissionRequirement"/> against the current user using the
/// permission claim issued at login and the application identity service as fallback
/// (covers user-level permission overrides).
/// </summary>
public sealed class PermissionAuthorizationHandler(IHttpContextAccessor httpContextAccessor)
    : AuthorizationHandler<PermissionRequirement>
{
    private const string SystemAdminRole = "SystemAdmin";
    private const string PermissionClaimType = "permission";

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        var user = context.User;
        if (user.Identity?.IsAuthenticated != true)
            return;

        if (user.IsInRole(SystemAdminRole))
        {
            context.Succeed(requirement);
            return;
        }

        if (user.HasClaim(PermissionClaimType, requirement.Permission))
        {
            context.Succeed(requirement);
            return;
        }

        var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdClaim, out var userId))
            return;

        var httpContext = context.Resource as HttpContext ?? httpContextAccessor.HttpContext;
        var identityService = httpContext?.RequestServices.GetService(typeof(IIdentityService)) as IIdentityService;
        if (identityService is null)
            return;

        if (await identityService.AuthorizeAsync(userId, requirement.Permission))
            context.Succeed(requirement);
    }
}
