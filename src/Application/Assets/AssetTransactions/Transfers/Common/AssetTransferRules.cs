using ERP_Government.Application.Common.Errors;
using ERP_Government.Application.Common.Interfaces;
using ERP_Government.Application.Common.Models;
using Microsoft.EntityFrameworkCore;

namespace ERP_Government.Application.Assets.AssetTransactions.Transfers.Common;

public static class AssetTransferRules
{
    public static async Task<Result> ValidateDestinationAsync(
        IApplicationDbContext context,
        int? currentLocationId,
        int? currentEmployeeId,
        int? toLocationId,
        int? toEmployeeId,
        CancellationToken ct)
    {
        if (!toLocationId.HasValue && !toEmployeeId.HasValue)
            return Result.Failure(
                ErrorCodes.Assets.InvalidTransferDestination,
                ErrorCategory.Validation,
                "يجب تحديد وجهة النقل: موقع أو حارس جديد على الأقل");

        var effectiveLocationId = toLocationId ?? currentLocationId;
        var effectiveEmployeeId = toEmployeeId ?? currentEmployeeId;

        if (effectiveLocationId == currentLocationId && effectiveEmployeeId == currentEmployeeId)
            return Result.Failure(
                ErrorCodes.Assets.InvalidTransferDestination,
                ErrorCategory.Validation,
                "لا يوجد تغيير في وجهة النقل — حدد موقعاً أو حارساً مختلفاً");

        if (toLocationId.HasValue)
        {
            var location = await context.Locations
                .Where(l => l.Id == toLocationId.Value)
                .Select(l => new { l.IsActive })
                .FirstOrDefaultAsync(ct);

            if (location is null)
                return Result.Failure(ErrorCodes.Inventory.LocationNotFound, ErrorCategory.NotFound, "الموقع غير موجود");

            if (!location.IsActive)
                return Result.Failure(ErrorCodes.Inventory.LocationInactive, ErrorCategory.Validation, "الموقع غير نشط");
        }

        if (toEmployeeId.HasValue)
        {
            var employee = await context.Employees
                .Where(e => e.Id == toEmployeeId.Value)
                .Select(e => new { e.IsActive })
                .FirstOrDefaultAsync(ct);

            if (employee is null)
                return Result.Failure(ErrorCodes.Organization.EmployeeNotFound, ErrorCategory.NotFound, "الموظف غير موجود");

            if (!employee.IsActive)
                return Result.Failure(
                    ErrorCodes.Organization.EmployeeNotFound,
                    ErrorCategory.Validation,
                    "لا يمكن تعيين موظف غير نشط كحارس للوجهة");
        }

        return Result.Success();
    }

    public static Task<int?> ResolveDepartmentIdAsync(IApplicationDbContext context, int? employeeId, CancellationToken ct)
    {
        if (!employeeId.HasValue)
            return Task.FromResult<int?>(null);

        return context.Employees
            .Where(e => e.Id == employeeId.Value)
            .Select(e => (int?)e.OrganizationalUnitId)
            .FirstOrDefaultAsync(ct);
    }

    public static bool RowVersionMatches(byte[] current, byte[]? provided) =>
        provided is { Length: > 0 } && current.SequenceEqual(provided);

    public static Result<T> ConcurrencyConflict<T>() =>
        Result<T>.Failure(
            ErrorCodes.Request.ConcurrencyConflict,
            ErrorCategory.Conflict,
            "تم تعديل البيانات من مستخدم آخر — أعد التحميل ثم حاول مجدداً");

    public static Result<T> SourceChanged<T>() =>
        Result<T>.Failure(
            ErrorCodes.Assets.TransferSourceChanged,
            ErrorCategory.Conflict,
            "تغيّرت بيانات الأصل منذ إنشاء مسودة النقل — أعد تحميل المسودة ثم حاول مجدداً");
}
