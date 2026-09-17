using System.Net;
using System.Net.Http.Json;
using ERP_Government.Application.Common.Models;
using ERP_Government.Application.FunctionalTests.Infrastructure;
using ERP_Government.Web.Infrastructure;
using NUnit.Framework;

namespace ERP_Government.Application.FunctionalTests.Common;

public class HttpStatusClassificationTests
{
    [Test]
    public void Validation_ShouldMapTo400()
    {
        var status = HttpErrorMapper.Map(ErrorCategory.Validation);
        status.ShouldBe(400);
    }

    [Test]
    public void Authorization_Forbidden_ShouldMapTo403()
    {
        var status = HttpErrorMapper.Map(ErrorCategory.Authorization, isAuthenticated: true);
        status.ShouldBe(403);
    }

    [Test]
    public void Authorization_Unauthenticated_ShouldMapTo401()
    {
        var status = HttpErrorMapper.Map(ErrorCategory.Authorization, isAuthenticated: false);
        status.ShouldBe(401);
    }

    [Test]
    public void NotFound_ShouldMapTo404()
    {
        var status = HttpErrorMapper.Map(ErrorCategory.NotFound);
        status.ShouldBe(404);
    }

    [Test]
    public void Conflict_ShouldMapTo409()
    {
        var status = HttpErrorMapper.Map(ErrorCategory.Conflict);
        status.ShouldBe(409);
    }

    [Test]
    public void BusinessRule_ShouldMapTo400()
    {
        var status = HttpErrorMapper.Map(ErrorCategory.BusinessRule);
        status.ShouldBe(400);
    }

    [Test]
    public void Internal_ShouldMapTo500()
    {
        var status = HttpErrorMapper.Map(ErrorCategory.Internal);
        status.ShouldBe(500);
    }

    [Test]
    public void Null_ShouldMapTo500()
    {
        var status = HttpErrorMapper.Map(null);
        status.ShouldBe(500);
    }
}
