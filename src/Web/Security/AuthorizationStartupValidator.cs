using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ERP_Government.Web.Security;

/// <summary>
/// Validates authorization metadata across all registered endpoints at startup.
/// Startup fails when an endpoint references an unregistered policy or a named policy
/// lacks a permission requirement or authentication requirement.
/// </summary>
public sealed class AuthorizationStartupValidator(ILogger<AuthorizationStartupValidator> logger)
{
    public void Validate(WebApplication app)
    {
        var policyProvider = app.Services.GetRequiredService<IAuthorizationPolicyProvider>();
        var endpoints = app.Services.GetRequiredService<EndpointDataSource>().Endpoints;

        var missingPolicies = new List<string>();
        var invalidPolicies = new List<string>();
        var anonymousApiEndpoints = new List<string>();
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

            foreach (var data in authorizeData)
            {
                if (string.IsNullOrWhiteSpace(data.Policy))
                    continue;

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
        var pattern = (endpoint as RouteEndpoint)?.RoutePattern.RawText ?? string.Empty;
        return pattern.StartsWith("/api", StringComparison.OrdinalIgnoreCase);
    }
}
