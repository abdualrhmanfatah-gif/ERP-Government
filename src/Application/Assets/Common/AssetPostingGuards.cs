using ERP_Government.Application.Common.Errors;

namespace ERP_Government.Application.Assets.Common;

/// <summary>
/// Synchronous posting gates shared by asset financial documents (Constitution IV):
/// the fiscal period covering the effect date must be open/unlocked, and the accounts
/// the consumer will post to must be postable before the event is emitted.
/// </summary>
public static class AssetPostingGuards
{
    public static async Task<Result<FiscalPeriod>> ResolveOpenPeriodAsync(
        IApplicationDbContext context,
        DateOnly effectDate,
        CancellationToken ct)
    {
        var period = await context.FiscalPeriods
            .Where(p => p.StartDate <= effectDate && p.EndDate >= effectDate)
            .OrderBy(p => p.PeriodNumber)
            .FirstOrDefaultAsync(ct);

        if (period is null)
            return Result<FiscalPeriod>.Failure(
                ErrorCodes.FinancialSettings.FiscalPeriodNotFound,
                ErrorCategory.Validation,
                "لا توجد فترة مالية تغطي تاريخ الأثر");

        if (period.IsLockedForPosting || !period.IsActive)
            return Result<FiscalPeriod>.Failure(
                ErrorCodes.Accounting.PeriodClosed,
                ErrorCategory.Validation,
                "الفترة المالية مغلقة أو غير مفعّلة للترحيل");

        return Result<FiscalPeriod>.Success(period);
    }

    public static async Task<bool> AreAccountsPostableAsync(
        IApplicationDbContext context,
        IEnumerable<int> accountIds,
        CancellationToken ct)
    {
        var distinct = accountIds.Distinct().ToList();
        var postableCount = await context.Accounts
            .CountAsync(a => distinct.Contains(a.Id) && a.IsPostable && a.IsActive, ct);

        return postableCount == distinct.Count;
    }
}
