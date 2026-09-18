using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ERP_Government.Web.Security;

/// <summary>
/// Validates authorization metadata across all registered endpoints at startup.
/// Startup fails when:
///   - An endpoint references an unregistered policy.
///   - A named policy lacks a permission requirement or authentication requirement.
///   - An API endpoint uses authentication-only authorization without a named permission policy
///     (unless the endpoint is in the explicit allowlist).
/// </summary>
public sealed class AuthorizationStartupValidator(ILogger<AuthorizationStartupValidator> logger)
{
    private static readonly HashSet<string> AuthenticationOnlyAllowlist = new(StringComparer.OrdinalIgnoreCase)
    {
        "/api/Users/login",
    };

    public void Validate(WebApplication app)
    {
        var policyProvider = app.Services.GetRequiredService<IAuthorizationPolicyProvider>();
        var endpoints = app.Services.GetRequiredService<EndpointDataSource>().Endpoints;

        var missingPolicies = new List<string>();
        var invalidPolicies = new List<string>();
        var anonymousApiEndpoints = new List<string>();
        var authOnlyApiEndpoints = new List<string>();
        var checkedPolicies = new HashSet<string>(StringComparer.Ordinal);

        foreach (var endpoint in endpoints)
        {
            var authorizeData = endpoint.Metadata.GetOrderedMetadata<IAuthorizeData>();
            var displayName = endpoint.DisplayName ?? endpoint.ToString() ?? "(unknown)";

            if (authorizeData.Count == 0)
            {
                if (IsApiRoute(endpoint))
                    anonymousApiEndpoints.Add(displayName);
                continue;
            }

            var hasNamedPolicy = false;
            foreach (var data in authorizeData)
            {
                if (string.IsNullOrWhiteSpace(data.Policy))
                    continue;

                hasNamedPolicy = true;

                if (!checkedPolicies.Add(data.Policy))
                    continue;

                var policy = policyProvider.GetPolicyAsync(data.Policy).GetAwaiter().GetResult();
                if (policy is null)
                {
                    missingPolicies.Add($"{data.Policy} ({displayName})");
                    continue;
                }

                if (!policy.Requirements.OfType<PermissionRequirement>().Any())
                    invalidPolicies.Add($"{data.Policy}: no permission requirement ({displayName})");

                if (!policy.Requirements.OfType<DenyAnonymousAuthorizationRequirement>().Any())
                    invalidPolicies.Add($"{data.Policy}: does not require authentication ({displayName})");
            }

            if (!hasNamedPolicy && IsApiRoute(endpoint))
            {
                var routePattern = GetRoutePattern(endpoint);
                if (!AuthenticationOnlyAllowlist.Contains(routePattern))
                    authOnlyApiEndpoints.Add(displayName);
            }
        }

        if (missingPolicies.Count > 0)
        {
            throw new InvalidOperationException(
                "Endpoints reference unregistered authorization policies: " + string.Join("; ", missingPolicies));
        }

        if (invalidPolicies.Count > 0)
        {
            throw new InvalidOperationException(
                "Authorization policies are missing required enforcement: " + string.Join("; ", invalidPolicies));
        }

        if (authOnlyApiEndpoints.Count > 0)
        {
            throw new InvalidOperationException(
                "API endpoints use authentication-only authorization without a named permission policy. " +
                "Add a named permission policy or add to AuthenticationOnlyAllowlist: " +
                string.Join("; ", authOnlyApiEndpoints));
        }

        if (anonymousApiEndpoints.Count > 0)
        {
            logger.LogWarning(
                "API endpoints registered without authorization metadata (verify intent): {Endpoints}",
                string.Join("; ", anonymousApiEndpoints));
        }

        logger.LogInformation(
            "Authorization validation PASSED: {PolicyCount} permission policies resolved across {EndpointCount} endpoints",
            checkedPolicies.Count,
            endpoints.Count);
    }

    private static bool IsApiRoute(Microsoft.AspNetCore.Http.Endpoint endpoint)
    {
        var pattern = GetRoutePattern(endpoint);
        return pattern.StartsWith("/api", StringComparison.OrdinalIgnoreCase);
    }

    private static string GetRoutePattern(Microsoft.AspNetCore.Http.Endpoint endpoint)
    {
        return (endpoint as RouteEndpoint)?.RoutePattern.RawText ?? string.Empty;
    }
}
