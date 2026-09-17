using ERP_Government.Application.Common.Models;
using NUnit.Framework;

namespace ERP_Government.Application.UnitTests.Common;

public class ResultTests
{
    [Test]
    public void Failure_WithCodeCategoryMessage_ShouldReturnStructuredFailure()
    {
        var result = Result.Failure("REQ.VALIDATION", ErrorCategory.Validation, "Invalid input", "Name");

        Assert.That(result.Succeeded, Is.False);
        Assert.That(result.Code, Is.EqualTo("REQ.VALIDATION"));
        Assert.That(result.Category, Is.EqualTo(ErrorCategory.Validation));
        Assert.That(result.Message, Is.EqualTo("Invalid input"));
        Assert.That(result.Target, Is.EqualTo("Name"));
        Assert.That(result.Errors, Is.Empty);
    }

    [Test]
    public void Failure_WithoutTarget_ShouldReturnNullTarget()
    {
        var result = Result.Failure("BUDGET.NOT_FOUND", ErrorCategory.NotFound, "Budget not found");

        Assert.That(result.Succeeded, Is.False);
        Assert.That(result.Code, Is.EqualTo("BUDGET.NOT_FOUND"));
        Assert.That(result.Category, Is.EqualTo(ErrorCategory.NotFound));
        Assert.That(result.Message, Is.EqualTo("Budget not found"));
        Assert.That(result.Target, Is.Null);
    }

    [Test]
    public void Failure_WithErrors_ShouldReturnStringArray()
    {
        var result = Result.Failure(new[] { "Error 1", "Error 2" });

        Assert.That(result.Succeeded, Is.False);
        Assert.That(result.Errors, Has.Length.EqualTo(2));
        Assert.That(result.Errors, Does.Contain("Error 1"));
        Assert.That(result.Errors, Does.Contain("Error 2"));
    }

    [Test]
    public void Success_ShouldReturnSucceededTrue()
    {
        var result = Result.Success();

        Assert.That(result.Succeeded, Is.True);
        Assert.That(result.Errors, Is.Empty);
    }

    [Test]
    public void Success_WithValue_ShouldReturnValue()
    {
        var result = Result<int>.Success(42);

        Assert.That(result.Succeeded, Is.True);
        Assert.That(result.Value, Is.EqualTo(42));
        Assert.That(result.Errors, Is.Empty);
    }

    [Test]
    public void Failure_Generic_WithCodeCategoryMessage_ShouldReturnStructuredFailure()
    {
        var result = Result<int>.Failure("PARTY.DUPLICATE", ErrorCategory.Conflict, "Party already exists");

        Assert.That(result.Succeeded, Is.False);
        Assert.That(result.Code, Is.EqualTo("PARTY.DUPLICATE"));
        Assert.That(result.Category, Is.EqualTo(ErrorCategory.Conflict));
        Assert.That(result.Message, Is.EqualTo("Party already exists"));
        Assert.That(result.Value, Is.Default);
    }

    [Test]
    public void Failure_Generic_WithErrors_ShouldReturnStringArray()
    {
        var result = Result<string>.Failure(new[] { "Name is required", "Email is invalid" });

        Assert.That(result.Succeeded, Is.False);
        Assert.That(result.Errors, Has.Length.EqualTo(2));
    }

    [Test]
    public void Failure_ShouldPreserveBackwardCompat_WithStringArray()
    {
        var result = Result.Failure(new[] { "Legacy error 1", "Legacy error 2" });

        Assert.That(result.Succeeded, Is.False);
        Assert.That(result.Code, Is.Null);
        Assert.That(result.Category, Is.Null);
        Assert.That(result.Message, Is.Null);
        Assert.That(result.Errors, Has.Length.EqualTo(2));
    }
}
