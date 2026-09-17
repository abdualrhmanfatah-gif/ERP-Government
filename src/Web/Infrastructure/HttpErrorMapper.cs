using ERP_Government.Application.Common.Models;

namespace ERP_Government.Web.Infrastructure;

/// <summary>
/// Maps ErrorCategory to HTTP status codes per Constitution XIII.
/// </summary>
public static class HttpErrorMapper
{
    public static int Map(ErrorCategory? category, bool isAuthenticated = true)
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
}
