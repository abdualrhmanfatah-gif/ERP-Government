using ERP_Government.Application.Common.Errors;
using ERP_Government.Application.Common.Interfaces;
using ERP_Government.Application.Common.Models;
using ERP_Government.Application.FinancialSettings.Common.Services;
using ERP_Government.Domain.Assets.Entities;
using ERP_Government.Domain.Assets.Enums;
using ERP_Government.Domain.Assets.Services;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ERP_Government.Application.Common.Security;

namespace ERP_Government.Application.Assets.Depreciation.Run.Commands;

[Authorize(Policy = PermissionCodes.AssetDepreciationRun)]
public record RunDepreciationCommand(
    int FiscalYearId,
    int FiscalPeriodId,
    DateOnly DepreciationDate,
    string MissedPeriodsPolicy = "CurrentPeriodOnly",
    string? Notes = null) : IRequest<Result<RunDepreciationResult>>;

public record RunDepreciationResult(
    int RunId,
    string RunNumber,
    int AssetsProcessed,
    decimal TotalDepreciation);

public class RunDepreciationCommandHandler(
    IApplicationDbContext context,
    IDocumentSequenceService sequenceService)
    : IRequestHandler<RunDepreciationCommand, Result<RunDepreciationResult>>
{
    public async Task<Result<RunDepreciationResult>> Handle(RunDepreciationCommand request, CancellationToken ct)
    {
        var period = await context.FiscalPeriods.FindAsync([request.FiscalPeriodId], ct);
        if (period is null || period.FiscalYearId != request.FiscalYearId)
            return Failure(ErrorCodes.FinancialSettings.FiscalPeriodNotFound, "الفترة المالية غير موجودة أو لا تنتمي إلى السنة المحددة");

        if (request.DepreciationDate < period.StartDate || request.DepreciationDate > period.EndDate)
            return Failure(ErrorCodes.FinancialSettings.FiscalPeriodNotFound, "تاريخ الإهلاك يجب أن يقع داخل الفترة المالية");

        var duplicate = await context.DepreciationRuns
            .AnyAsync(r => r.FiscalYearId == request.FiscalYearId && r.FiscalPeriodId == request.FiscalPeriodId, ct);
        if (duplicate)
            return Failure(ErrorCodes.Assets.DuplicateDepreciationRun, "توجد عملية إهلاك لهذه الفترة المالية بالفعل");

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

        var strategy = context.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(async (CancellationToken ct) =>
        {
            await using var transaction = await context.Database.BeginTransactionAsync(ct);
            var runNumber = await sequenceService.GenerateNextNumberAsync("DepreciationRun", ct);
            var run = new DepreciationRun
            {
                RunNumber = runNumber,
                FiscalYearId = request.FiscalYearId,
                FiscalPeriodId = request.FiscalPeriodId,
                DepreciationDate = request.DepreciationDate,
                MissedPeriodsPolicy = request.MissedPeriodsPolicy,
                Status = DepreciationRunStatus.Draft,
                Notes = request.Notes
            };

            context.DepreciationRuns.Add(run);
            await context.SaveChangesAsync(ct);

            var previousCounts = await context.DepreciationScheduleLines
                .GroupBy(s => s.AssetId)
                .Select(g => new { AssetId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.AssetId, x => x.Count, ct);

            foreach (var asset in eligibleAssets)
            {
                var periodNumber = previousCounts.GetValueOrDefault(asset.Id) + 1;
                var ratePerPeriod = DepreciationCalculator.CalculateRatePerPeriod(asset, asset.DepreciationStartDate ?? asset.PurchaseDate ?? asset.ActivationDate ?? request.DepreciationDate, request.DepreciationDate);
                var line = DepreciationCalculator.Calculate(asset, ratePerPeriod, periodNumber, null);
                line.DepreciationRunId = run.Id;
                line.FiscalYearId = request.FiscalYearId;
                line.FiscalPeriodId = request.FiscalPeriodId;
                line.DepreciationDate = request.DepreciationDate;
                run.ScheduleLines.Add(line);
            }

            run.TotalDepreciation = run.ScheduleLines.Sum(s => s.Amount);
            await context.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);

            return Result<RunDepreciationResult>.Success(
                new RunDepreciationResult(run.Id, run.RunNumber, run.ScheduleLines.Count, run.TotalDepreciation));
        }, ct);
    }

    private static Result<RunDepreciationResult> Failure(string code, string message) =>
        Result<RunDepreciationResult>.Failure(code, ErrorCategory.Validation, message);
}

public class RunDepreciationCommandValidator : AbstractValidator<RunDepreciationCommand>
{
    public RunDepreciationCommandValidator()
    {
        RuleFor(x => x.FiscalYearId).GreaterThan(0);
        RuleFor(x => x.FiscalPeriodId).GreaterThan(0);
        RuleFor(x => x.DepreciationDate).NotEmpty();
        RuleFor(x => x.MissedPeriodsPolicy).Must(p => p is "CurrentPeriodOnly" or "CatchUp");
        RuleFor(x => x.Notes).MaximumLength(500);
    }
}
