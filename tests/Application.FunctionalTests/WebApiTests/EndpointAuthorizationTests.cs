using ERP_Government.Application.FunctionalTests.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace ERP_Government.WebApiTests;

/// <summary>
/// Structural authorization tests: every payment-domain endpoint must declare
/// its RequireAuthorization permission policy (T060 follow-up, spec 018).
/// Uses WebApiFactory directly (no database access — endpoint metadata only).
/// </summary>
[TestFixture]
public class EndpointAuthorizationTests
{
    private WebApiFactory _factory = null!;

    [OneTimeSetUp]
    public void SetUp()
    {
        _factory = new WebApiFactory(
            "Server=localhost;Database=EndpointAuthorizationStructuralOnly;Trusted_Connection=True;");
    }

    [OneTimeTearDown]
    public void TearDown()
    {
        _factory.Dispose();
    }

    [Test]
    public void DisbursementRequests_Endpoints_ShouldDeclarePermissionPolicies()
    {
        var expected = new Dictionary<string, (string[] Methods, string Policy)>
        {
            ["/api/DisbursementRequests"] = (["GET"], "DisbursementRequests.View"),
            ["/api/DisbursementRequests/{id:int}"] = (["GET"], "DisbursementRequests.View"),
            ["/api/DisbursementRequests/{id:int}/submit"] = (["PATCH"], "DisbursementRequests.Submit"),
            ["/api/DisbursementRequests/{id:int}/approve"] = (["PATCH"], "DisbursementRequests.Approve"),
            ["/api/DisbursementRequests/{id:int}/reject"] = (["PATCH"], "DisbursementRequests.Reject"),
            ["/api/DisbursementRequests/{id:int}/cancel"] = (["PATCH"], "DisbursementRequests.Cancel"),
        };

        // POST / and GET / share the same raw text; both must be present.
        AssertRoutes(_factory, expected, allowDuplicateRawText: true);
    }

    [Test]
    public void PaymentOrders_Endpoints_ShouldDeclarePermissionPolicies()
    {
        var expected = new Dictionary<string, (string[] Methods, string Policy)>
        {
            ["/api/PaymentOrders/{id:int}/submit"] = (["POST"], "PaymentOrders.Submit"),
            ["/api/PaymentOrders/{id:int}/approve"] = (["POST"], "PaymentOrders.Approve"),
            ["/api/PaymentOrders/{id:int}/reject"] = (["POST"], "PaymentOrders.Reject"),
            ["/api/PaymentOrders/{id:int}/cancel"] = (["POST"], "PaymentOrders.Cancel"),
            ["/api/PaymentOrders/{id:int}/send-to-treasury"] = (["POST"], "PaymentOrders.SendToTreasury"),
            ["/api/PaymentOrders/{id:int}/void"] = (["POST"], "PaymentOrders.Void"),
        };

        AssertRoutes(_factory, expected, allowDuplicateRawText: false);
    }

    private static void AssertRoutes(
        WebApiFactory factory,
        Dictionary<string, (string[] Methods, string Policy)> expected,
        bool allowDuplicateRawText)
    {
        var endpoints = factory
            .Services
            .GetRequiredService<EndpointDataSource>()
            .Endpoints
            .OfType<RouteEndpoint>()
            .Where(e => e.RoutePattern.RawText!.StartsWith("/api/DisbursementRequests")
                     || e.RoutePattern.RawText!.StartsWith("/api/PaymentOrders"))
            .ToList();

        var actual = endpoints
            .Select(e => new
            {
                Raw = e.RoutePattern.RawText!,
                Methods = e.Metadata.OfType<IHttpMethodMetadata>()
                    .SelectMany(m => m.HttpMethods).ToArray(),
                Policies = e.Metadata.OfType<IAuthorizeData>()
                    .Select(a => a.Policy).Where(p => p is not null).ToArray()
            })
            .ToList();

        foreach (var (raw, (methods, policy)) in expected)
        {
            var matches = actual
                .Where(a => a.Raw == raw && a.Methods.Intersect(methods).Any())
                .ToList();

            matches.Count.ShouldBe(1, $"Route {string.Join('/', methods)} {raw} should exist exactly once.");
            matches[0].Policies.ShouldContain(policy,
                $"Route {string.Join('/', methods)} {raw} must require policy {policy}.");
        }
    }
}
