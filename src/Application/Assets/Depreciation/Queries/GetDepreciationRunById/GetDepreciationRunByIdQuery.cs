using ERP_Government.Application.Common.Errors;
using ERP_Government.Application.Common.Models;
using ERP_Government.Application.Common.Security;

namespace ERP_Government.Application.Assets.Depreciation.Queries.GetDepreciationRunById;

[Authorize(Policy = PermissionCodes.AssetDepreciationView)]
public record GetDepreciationRunByIdQuery(int Id) : IRequest<Result<DepreciationRunDetailResponse>>;

public record DepreciationRunScheduleResponse(
    int Id,
    int AssetId,
    string AssetCode,
    string AssetName,
    string Method,
    decimal Rate,
    int? PeriodNumber,
    int? TotalPeriods,
    decimal DepreciationBase,
    decimal ResidualValue,
    decimal OpeningBookValue,
    decimal OpeningAccumulatedDepreciation,
    decimal Amount,
    decimal ClosingAccumulatedDepreciation,
    decimal ClosingBookValue);

public record DepreciationRunDetailResponse(
    int Id,
    string RunNumber,
    int FiscalYearId,
    string FiscalYear,
    int FiscalPeriodId,
    int PeriodNumber,
    DateOnly DepreciationDate,
    string Status,
    decimal TotalDepreciation,
    string? Notes,
    int? JournalEntryId,
    string? MissedPeriodsPolicy,
    string RowVersion,
    IReadOnlyList<DepreciationRunScheduleResponse> ScheduleLines);

public class GetDepreciationRunByIdQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetDepreciationRunByIdQuery, Result<DepreciationRunDetailResponse>>
{
    public async Task<Result<DepreciationRunDetailResponse>> Handle(GetDepreciationRunByIdQuery request, CancellationToken ct)
    {
        var run = await context.DepreciationRuns
            .Include(r => r.FiscalYear)
            .Include(r => r.FiscalPeriod)
            .Include(r => r.ScheduleLines)
                .ThenInclude(s => s.Asset)
            .FirstOrDefaultAsync(r => r.Id == request.Id, ct);
        if (run is null)
            return Result<DepreciationRunDetailResponse>.Failure(
                ErrorCodes.Assets.DepreciationRunNotFound, ErrorCategory.NotFound, "عملية الإهلاك غير موجودة");

        var scheduleLines = run.ScheduleLines.OrderBy(s => s.Asset!.Code).Select(s => new DepreciationRunScheduleResponse(
            s.Id, s.AssetId, s.Asset!.Code, s.Asset.Name, s.Method, s.Rate, s.PeriodNumber, s.TotalPeriods,
            s.DepreciationBase, s.ResidualValue, s.OpeningBookValue, s.OpeningAccumulatedDepreciation,
            s.Amount, s.ClosingAccumulatedDepreciation, s.ClosingBookValue)).ToList();

        return Result<DepreciationRunDetailResponse>.Success(new DepreciationRunDetailResponse(
            run.Id, run.RunNumber, run.FiscalYearId, run.FiscalYear!.Name,
            run.FiscalPeriodId, run.FiscalPeriod!.PeriodNumber, run.DepreciationDate,
            run.Status.ToString(), run.TotalDepreciation, run.Notes, run.JournalEntryId,
            run.MissedPeriodsPolicy,
            Convert.ToBase64String(run.RowVersion), scheduleLines));
    }
}
