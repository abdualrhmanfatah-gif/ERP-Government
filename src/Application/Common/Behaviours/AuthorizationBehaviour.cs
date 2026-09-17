using System.Reflection;
using ERP_Government.Application.Common.Exceptions;
using ERP_Government.Application.Common.Errors;
using ERP_Government.Application.Common.Interfaces;
using ERP_Government.Application.Common.Models;
using ERP_Government.Application.Common.Security;
using ERP_Government.Domain.Security.Entities;
using Microsoft.Extensions.Logging;

namespace ERP_Government.Application.Common.Behaviours;

public class AuthorizationBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> 
    where TRequest : notnull
{
    private const string SystemAdminRole = "SystemAdmin";

    private readonly IUser _user;
    private readonly IIdentityService _identityService;
    private readonly IApplicationDbContext _dbContext;
    private readonly IRequestContext _requestContext;
    private readonly ILogger<AuthorizationBehaviour<TRequest, TResponse>> _logger;

    public AuthorizationBehaviour(
        IUser user,
        IIdentityService identityService,
        IApplicationDbContext dbContext,
        IRequestContext requestContext,
        ILogger<AuthorizationBehaviour<TRequest, TResponse>> logger)
    {
        _user = user;
        _identityService = identityService;
        _dbContext = dbContext;
        _requestContext = requestContext;
        _logger = logger;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var authorizeAttributes = request.GetType().GetCustomAttributes<AuthorizeAttribute>();

        if (authorizeAttributes.Any())
        {
            // Must be authenticated user
            if (_user.Id == null)
            {
                await LogAuthorizationEvent("Unauthorized", null, success: false, "Unauthenticated", cancellationToken);
                throw new UnauthorizedAccessException();
            }

            // SystemAdmin role bypass — skip all permission checks
            if (_user.Roles?.Any(x => x == SystemAdminRole) ?? false)
            {
                await LogAuthorizationEvent("SystemAdminBypass", null, success: true, "SystemAdmin bypass", cancellationToken);
                return await next();
            }

            // Role-based authorization
            var authorizeAttributesWithRoles = authorizeAttributes.Where(a => !string.IsNullOrWhiteSpace(a.Roles));

            if (authorizeAttributesWithRoles.Any())
            {
                var authorized = false;

                foreach (var roles in authorizeAttributesWithRoles.Select(a => a.Roles.Split(',')))
                {
                    foreach (var role in roles)
                    {
                        var isInRole = _user.Roles?.Any(x => role == x)??false;
                        if (isInRole)
                        {
                            authorized = true;
                            break;
                        }
                    }
                }

                // Must be a member of at least one role in roles
                if (!authorized)
                {
                    await LogAuthorizationEvent("Forbidden", null, success: false, "Missing required role", cancellationToken);
                    return CreateForbiddenResult("ليس لديك الصلاحية المطلوبة للوصول إلى هذا المورد");
                }
            }

            // Policy-based authorization with fail-closed behavior
            var authorizeAttributesWithPolicies = authorizeAttributes.Where(a => !string.IsNullOrWhiteSpace(a.Policy));
            if (authorizeAttributesWithPolicies.Any())
            {
                foreach (var policy in authorizeAttributesWithPolicies.Select(a => a.Policy))
                {
                    bool authorized;
                    try
                    {
                        authorized = await _identityService.AuthorizeAsync(_user.Id.Value, policy);
                    }
                    catch (Exception ex)
                    {
                        // FAIL CLOSED: Permission service error → deny access
                        _logger.LogError(ex, "Authorization service error for policy '{Policy}' — failing closed", policy);
                        await LogAuthorizationEvent("Forbidden", policy, success: false, $"Service error: {ex.Message}", cancellationToken);
                        return CreateForbiddenResult("خطأ في خدمة التحقق من الصلاحيات");
                    }

                    if (!authorized)
                    {
                        await LogAuthorizationEvent("Forbidden", policy, success: false, $"Policy '{policy}' denied", cancellationToken);
                        return CreateForbiddenResult("ليس لديك صلاحية للوصول إلى هذا المورد");
                    }
                }
            }
        }

        // User is authorized / authorization not required
        return await next();
    }

    private TResponse CreateForbiddenResult(string message)
    {
        if (typeof(TResponse) == typeof(Result))
        {
            return (TResponse)(object)Result.Failure(
                ErrorCodes.Request.Forbidden,
                ErrorCategory.Authorization,
                message);
        }

        if (typeof(TResponse).IsGenericType && typeof(TResponse).GetGenericTypeDefinition() == typeof(Result<>))
        {
            var failureMethod = typeof(TResponse).GetMethod(
                "Failure",
                BindingFlags.Public | BindingFlags.Static,
                null,
                new[] { typeof(string), typeof(ErrorCategory), typeof(string), typeof(string) },
                null);
            if (failureMethod != null)
            {
                return (TResponse)failureMethod.Invoke(null, new object[] { ErrorCodes.Request.Forbidden, ErrorCategory.Authorization, message, string.Empty })!;
            }
        }

        throw new ForbiddenAccessException(message);
    }

    private async Task LogAuthorizationEvent(
        string action,
        string? permissionChecked,
        bool success,
        string? failureReason,
        CancellationToken cancellationToken)
    {
        var endpoint = _requestContext.Endpoint ?? "unknown";
        var method = _requestContext.Method ?? "unknown";
        var ipAddress = _requestContext.IpAddress;

        var log = new SecurityAuditLog
        {
            EventCategory = "Authorization",
            Action = action,
            UserId = _user.Id ?? 0,
            EntityName = $"{method} {endpoint}",
            Success = success,
            FailureReason = failureReason,
            IpAddress = ipAddress,
            Timestamp = DateTimeOffset.UtcNow
        };

        _dbContext.SecurityAuditLogs.Add(log);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
