using ERP_Government.Application.Common.Errors;
using ERP_Government.Application.Common.Models;
using Microsoft.AspNetCore.Mvc;

namespace ERP_Government.Web.Infrastructure;

/// <summary>
/// Extension methods for converting Result failures to IResult responses.
/// Use in endpoint handlers to return ProblemDetails responses for Result failures.
/// </summary>
public static class ResultExtensions
{
    public static IResult ToProblemDetails<T>(this Result<T> result, HttpContext? httpContext = null)
    {
        if (result.Succeeded)
            return Results.Ok(result.Value);

        return CreateProblemResult(result.Code, result.Category, result.Message, result.Errors, httpContext);
    }

    public static IResult ToProblemDetails(this Result result, HttpContext? httpContext = null)
    {
        if (result.Succeeded)
            return Results.Ok();

        return CreateProblemResult(result.Code, result.Category, result.Message, result.Errors, httpContext);
    }

    private static IResult CreateProblemResult(
        string? code,
        ErrorCategory? category,
        string? message,
        string[] errors,
        HttpContext? httpContext)
    {
        var statusCode = ProblemDetailsExceptionHandler.MapCategoryToStatus(category);
        var traceId = httpContext?.TraceIdentifier;

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Type = "about:blank",
            Title = GetTitle(category),
            Detail = message ?? string.Join(", ", errors)
        };

        if (code != null)
            problemDetails.Extensions["code"] = code;

        if (traceId != null)
            problemDetails.Extensions["traceId"] = traceId;

        if (errors.Length > 0)
            problemDetails.Extensions["errors"] = errors;

        return Results.Json(problemDetails, statusCode: statusCode);
    }

    private static string GetTitle(ErrorCategory? category) => category switch
    {
        ErrorCategory.Validation => "بيانات غير صالحة",
        ErrorCategory.Authorization => "غير مصرح",
        ErrorCategory.NotFound => "المورد غير موجود",
        ErrorCategory.Conflict => "تعارض",
        ErrorCategory.BusinessRule => "خطأ في قاعدة العمل",
        ErrorCategory.Internal => "حدث خطأ غير متوقع",
        _ => "خطأ"
    };
}
