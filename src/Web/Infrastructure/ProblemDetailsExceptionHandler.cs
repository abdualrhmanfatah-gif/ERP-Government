using ERP_Government.Application.Common.Exceptions;
using ERP_Government.Application.Common.Models;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace ERP_Government.Web.Infrastructure;

/// <summary>
/// Converts well-known application exceptions and Result failures into RFC 9457-compliant
/// ProblemDetails responses with stable code, traceId, and field errors.
/// </summary>
public class ProblemDetailsExceptionHandler(IWebHostEnvironment env, ILogger<ProblemDetailsExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var traceId = httpContext.TraceIdentifier;
        var instance = httpContext.Request.Path.Value ?? "/";

        var (statusCode, problemDetails) = exception switch
        {
            ValidationException ve => HandleValidation(ve, instance, traceId),
            ERP_Government.Application.Common.Exceptions.NotFoundException ne => HandleNotFound(ne, instance, traceId),
            UnauthorizedAccessException => HandleUnauthorized(instance, traceId),
            ForbiddenAccessException fe => HandleForbidden(fe, instance, traceId),
            OperationCanceledException => HandleCancelled(httpContext, traceId),
            _ => HandleUnexpected(exception, httpContext, instance, traceId)
        };

        LogDiagnostic(httpContext, exception, statusCode, traceId);

        httpContext.Response.StatusCode = statusCode;
        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);
        return true;
    }

    public static int MapCategoryToStatus(ErrorCategory? category, bool isAuthenticated = true)
    {
        return category switch
        {
            ErrorCategory.Validation => StatusCodes.Status400BadRequest,
            ErrorCategory.Authorization when isAuthenticated => StatusCodes.Status403Forbidden,
            ErrorCategory.Authorization => StatusCodes.Status401Unauthorized,
            ErrorCategory.NotFound => StatusCodes.Status404NotFound,
            ErrorCategory.Conflict => StatusCodes.Status409Conflict,
            ErrorCategory.BusinessRule => StatusCodes.Status400BadRequest,
            ErrorCategory.Internal => StatusCodes.Status500InternalServerError,
            _ => StatusCodes.Status500InternalServerError,
        };
    }

    private static (int statusCode, ProblemDetails problemDetails) HandleValidation(ValidationException ve, string instance, string traceId)
    {
        var canonicalized = FieldPathCanonicalizer.CanonicalizeKeys(ve.Errors);
        var errorsDict = new Dictionary<string, string[]>(canonicalized);
        var problemDetails = new ValidationProblemDetails(errorsDict)
        {
            Status = StatusCodes.Status400BadRequest,
            Type = "about:blank",
            Title = "بيانات غير صالحة",
            Detail = "راجع الحقول المحددة ثم أعد المحاولة.",
            Instance = instance
        };
        problemDetails.Extensions["code"] = Application.Common.Errors.ErrorCodes.Request.ValidationFailed;
        problemDetails.Extensions["traceId"] = traceId;
        return (StatusCodes.Status400BadRequest, problemDetails);
    }

    private static (int statusCode, ProblemDetails problemDetails) HandleNotFound(ERP_Government.Application.Common.Exceptions.NotFoundException ne, string instance, string traceId)
    {
        var problemDetails = new ProblemDetails
        {
            Status = StatusCodes.Status404NotFound,
            Type = "about:blank",
            Title = "المورد غير موجود",
            Detail = ne.Message,
            Instance = instance
        };
        problemDetails.Extensions["code"] = Application.Common.Errors.ErrorCodes.Request.NotFound;
        problemDetails.Extensions["traceId"] = traceId;
        return (StatusCodes.Status404NotFound, problemDetails);
    }

    private static (int statusCode, ProblemDetails problemDetails) HandleUnauthorized(string instance, string traceId)
    {
        var problemDetails = new ProblemDetails
        {
            Status = StatusCodes.Status401Unauthorized,
            Type = "about:blank",
            Title = "غير مصرح",
            Detail = "يرجى تسجيل الدخول للمتابعة.",
            Instance = instance
        };
        problemDetails.Extensions["code"] = Application.Common.Errors.ErrorCodes.Request.Unauthorized;
        problemDetails.Extensions["traceId"] = traceId;
        return (StatusCodes.Status401Unauthorized, problemDetails);
    }

    private static (int statusCode, ProblemDetails problemDetails) HandleForbidden(ForbiddenAccessException fe, string instance, string traceId)
    {
        var problemDetails = new ProblemDetails
        {
            Status = StatusCodes.Status403Forbidden,
            Type = "about:blank",
            Title = "ممنوع",
            Detail = fe.Message,
            Instance = instance
        };
        problemDetails.Extensions["code"] = Application.Common.Errors.ErrorCodes.Request.Forbidden;
        problemDetails.Extensions["traceId"] = traceId;
        return (StatusCodes.Status403Forbidden, problemDetails);
    }

    private static (int statusCode, ProblemDetails problemDetails) HandleCancelled(HttpContext httpContext, string traceId)
    {
        var problemDetails = new ProblemDetails
        {
            Status = StatusCodes.Status499ClientClosedRequest,
            Type = "about:blank",
            Title = "تم إلغاء الطلب",
            Instance = httpContext.Request.Path.Value ?? "/"
        };
        problemDetails.Extensions["code"] = Application.Common.Errors.ErrorCodes.Request.InternalError;
        problemDetails.Extensions["traceId"] = traceId;
        return (StatusCodes.Status499ClientClosedRequest, problemDetails);
    }

    private (int statusCode, ProblemDetails problemDetails) HandleUnexpected(Exception exception, HttpContext httpContext, string instance, string traceId)
    {
        var problemDetails = new ProblemDetails
        {
            Status = StatusCodes.Status500InternalServerError,
            Type = "about:blank",
            Title = "حدث خطأ غير متوقع",
            Detail = env.IsDevelopment() ? exception.Message : "حدث خطأ داخلي. يرجى المحاولة لاحقاً.",
            Instance = instance
        };
        problemDetails.Extensions["code"] = Application.Common.Errors.ErrorCodes.Request.InternalError;
        problemDetails.Extensions["traceId"] = traceId;
        return (StatusCodes.Status500InternalServerError, problemDetails);
    }

    private void LogDiagnostic(HttpContext httpContext, Exception exception, int statusCode, string traceId)
    {
        var path = httpContext.Request.Path.Value ?? "/";
        var method = httpContext.Request.Method;

        if (statusCode == StatusCodes.Status499ClientClosedRequest)
        {
            logger.LogDebug("Request cancelled by client: {Method} {Path} {TraceId}", method, path, traceId);
            return;
        }

        if (statusCode is >= 400 and < 500)
        {
            switch (statusCode)
            {
                case StatusCodes.Status401Unauthorized:
                    logger.LogInformation("Authentication required: {Method} {Path} {TraceId}", method, path, traceId);
                    break;
                case StatusCodes.Status403Forbidden:
                    logger.LogWarning("Authorization denied: {Method} {Path} {TraceId}", method, path, traceId);
                    break;
                case StatusCodes.Status404NotFound:
                    logger.LogWarning("Resource not found: {Method} {Path} {TraceId}", method, path, traceId);
                    break;
                case StatusCodes.Status400BadRequest:
                    logger.LogWarning("Validation failed: {Method} {Path} {TraceId}", method, path, traceId);
                    break;
                case StatusCodes.Status409Conflict:
                    logger.LogWarning("Conflict: {Method} {Path} {TraceId}", method, path, traceId);
                    break;
                default:
                    logger.LogWarning(exception, "Client error {StatusCode}: {Method} {Path} {TraceId}", statusCode, method, path, traceId);
                    break;
            }
            return;
        }

        logger.LogError(exception, "Unhandled server fault {StatusCode}: {Method} {Path} {TraceId}", statusCode, method, path, traceId);
    }
}
