using System.Net;
using System.Net.Http.Json;
using ERP_Government.Application.Common.Errors;
using ERP_Government.Application.FunctionalTests.Infrastructure;
using NUnit.Framework;

namespace ERP_Government.Application.FunctionalTests.Common;

public class ProblemDetailsContractTests : TestBase
{
    private HttpClient _client = null!;

    [SetUp]
    public override async Task SetUp()
    {
        await base.SetUp();
        _client = FunctionalTestSetup.WebApplicationFactory!.CreateClient();
    }

    [TearDown]
    public void TearDown()
    {
        _client.Dispose();
    }

    [Test]
    public async Task ValidationError_ShouldReturnProblemDetailsWith400()
    {
        var response = await _client.PostAsJsonAsync("/api/Parties", new { });

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        response.Content.Headers.ContentType?.MediaType.ShouldBe("application/problem+json");

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetailsResponse>();
        problem.ShouldNotBeNull();
        problem!.Type.ShouldBe("about:blank");
        problem.Status.ShouldBe(400);
        problem.Code.ShouldBe(ErrorCodes.Request.ValidationFailed);
        problem.TraceId.ShouldNotBeNullOrWhiteSpace();
    }

    [Test]
    public async Task NotFound_ShouldReturnProblemDetailsWith404()
    {
        var response = await _client.GetAsync("/api/Parties/999999");

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        response.Content.Headers.ContentType?.MediaType.ShouldBe("application/problem+json");

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetailsResponse>();
        problem.ShouldNotBeNull();
        problem!.Status.ShouldBe(404);
        problem.Code.ShouldBe(ErrorCodes.Request.NotFound);
        problem.TraceId.ShouldNotBeNullOrWhiteSpace();
    }

    [Test]
    public async Task Unauthorized_ShouldReturnProblemDetailsWith401()
    {
        var response = await _client.GetAsync("/api/Parties");

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
        response.Content.Headers.ContentType?.MediaType.ShouldBe("application/problem+json");

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetailsResponse>();
        problem.ShouldNotBeNull();
        problem!.Status.ShouldBe(401);
    }
}

public class ProblemDetailsResponse
{
    public string Type { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public int Status { get; set; }
    public string Detail { get; set; } = string.Empty;
    public string Instance { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string TraceId { get; set; } = string.Empty;
    public Dictionary<string, string[]>? Errors { get; set; }
}
