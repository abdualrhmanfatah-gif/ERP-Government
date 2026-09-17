using ERP_Government.Application.Common.Errors;
using ERP_Government.Application.Common.Interfaces;
using ERP_Government.Application.Common.Models;
using ERP_Government.Domain.Assets.Entities;
using ERP_Government.Domain.Assets.Services;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ERP_Government.Application.Common.Security;

namespace ERP_Government.Application.Assets.Depreciation.Run.Commands;

[Authorize(Policy = PermissionCodes.AssetDepreciationView)]
public record PreviewDepreciationCommand(
    int FiscalYearId,
    int FiscalPeriodId,
    DateOnly DepreciationDate,
    string MissedPeriodsPolicy = "CurrentPeriodOnly") : IRequest<Result<PreviewDepreciationResult>>;

public record PreviewDepreciationResult(
    int TotalAssets,
    decimal TotalDepreciation,
    IReadOnlyList<PreviewScheduleLine> Lines);

public record PreviewScheduleLine(
    int AssetId,
    string AssetCode,
    string? AssetName,
    int Days,
    decimal Rate,
    decimal Amount,
    decimal OriginalValue,
    decimal AccumulatedDepreciation,
    decimal OpeningBookValue,
    decimal ClosingBookValue,
    string AssetStatus);

public class PreviewDepreciationCommandHandler(IApplicationDbContext context)
    : IRequestHandler<PreviewDepreciationCommand, Result<PreviewDepreciationResult>>
{
    public async Task<Result<PreviewDepreciationResult>> Handle(PreviewDepreciationCommand request, CancellationToken ct)
    {
        var period = await context.FiscalPeriods.FindAsync([request.FiscalPeriodId], ct);
        if (period is null || period.FiscalYearId != request.FiscalYearId)
            return Failure(ErrorCodes.FinancialSettings.FiscalPeriodNotFound, "الفترة المالية غير موجودة أو لا تنتمي إلى السنة المحددة");

        if (request.DepreciationDate < period.StartDate || request.DepreciationDate > period.EndDate)
            return Failure(ErrorCodes.FinancialSettings.FiscalPeriodNotFound, "تاريخ الإهلاك يجب أن يقع داخل الفترة المالية");

        var assets = await context.Assets
            .Include(a => a.AssetGroup)
            .Where(a => a.Status == "Active"
                && a.IsActive
                && a.AssetGroup != null
                && a.AssetGroup.IsActive
                && a.AssetGroup.IsDepreciable)
            .OrderBy(a => a.Id)
            .ToListAsync(ct);

        var eligibleAssets = assets.Where(DepreciationEligibilityFilter.IsEligible).ToList();
        if (eligibleAssets.Count == 0)
            return Failure(ErrorCodes.Assets.NoEligibleAssets, "لا توجد أصول مستحقة للإهلاك");

        var previousCounts = await context.DepreciationScheduleLines
            .GroupBy(s => s.AssetId)
            .Select(g => new { AssetId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.AssetId, x => x.Count, ct);

        var lines = new List<PreviewScheduleLine>();
        foreach (var asset in eligibleAssets)
        {
            var periodNumber = previousCounts.GetValueOrDefault(asset.Id) + 1;
            var lastDepreciatedDate = asset.LastDepreciationDate ?? asset.DepreciationStartDate ?? asset.PurchaseDate ?? request.DepreciationDate;
            var days = Math.Max(0, request.DepreciationDate.DayNumber - lastDepreciatedDate.DayNumber);
            var ratePerPeriod = DepreciationCalculator.CalculateRatePerPeriod(asset, asset.DepreciationStartDate ?? asset.PurchaseDate ?? asset.ActivationDate ?? request.DepreciationDate, request.DepreciationDate);
            var calculated = DepreciationCalculator.Calculate(asset, ratePerPeriod, periodNumber, null);
            lines.Add(new PreviewScheduleLine(
                asset.Id,
                asset.Code,
                asset.Name,
                days,
                calculated.Rate,
                calculated.Amount,
                asset.OriginalValue,
                calculated.OpeningAccumulatedDepreciation,
                calculated.OpeningBookValue,
                calculated.ClosingBookValue,
                asset.Status));
        }

        return Result<PreviewDepreciationResult>.Success(
            new PreviewDepreciationResult(lines.Count, lines.Sum(l => l.Amount), lines));
    }

    private static Result<PreviewDepreciationResult> Failure(string code, string message) =>
        Result<PreviewDepreciationResult>.Failure(code, ErrorCategory.Validation, message);
}

public class PreviewDepreciationCommandValidator : AbstractValidator<PreviewDepreciationCommand>
{
    public PreviewDepreciationCommandValidator()
    {
        RuleFor(x => x.FiscalYearId).GreaterThan(0);
        RuleFor(x => x.FiscalPeriodId).GreaterThan(0);
        RuleFor(x => x.DepreciationDate).NotEmpty();
        RuleFor(x => x.MissedPeriodsPolicy).Must(p => p is "CurrentPeriodOnly" or "CatchUp");
    }
}
