using ERP_Government.Application.Common.Models;
using ERP_Government.Domain.Assets.Entities;
using ERP_Government.Domain.Assets.Enums;
using ERP_Government.Application.Common.Security;

namespace ERP_Government.Application.Assets.Depreciation.Queries.GetDepreciationRuns;

[Authorize(Policy = PermissionCodes.AssetDepreciationView)]
public record GetDepreciationRunsQuery(
    int? FiscalYearId = null,
    string? Status = null,
    int Page = 1,
    int PageSize = 20) : IRequest<PaginatedList<DepreciationRunResponse>>;

public record DepreciationRunResponse(
    int Id,
    string RunNumber,
    string FiscalYear,
    int PeriodNumber,
    DateOnly DepreciationDate,
    int AssetsCount,
    decimal TotalDepreciation,
    string Status,
    int? JournalEntryId,
    string? MissedPeriodsPolicy);

public class GetDepreciationRunsQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetDepreciationRunsQuery, PaginatedList<DepreciationRunResponse>>
{
    public async Task<PaginatedList<DepreciationRunResponse>> Handle(GetDepreciationRunsQuery request, CancellationToken ct)
    {
        IQueryable<DepreciationRun> query = context.DepreciationRuns;
        if (request.FiscalYearId.HasValue)
            query = query.Where(r => r.FiscalYearId == request.FiscalYearId.Value);
        if (!string.IsNullOrWhiteSpace(request.Status)
            && Enum.TryParse<DepreciationRunStatus>(request.Status, true, out var status))
            query = query.Where(r => r.Status == status);

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(r => r.DepreciationDate)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(r => new DepreciationRunResponse(
                r.Id,
                r.RunNumber,
                r.FiscalYear!.Name,
                r.FiscalPeriod!.PeriodNumber,
                r.DepreciationDate,
                r.ScheduleLines.Count,
                r.TotalDepreciation,
                r.Status.ToString(),
                r.JournalEntryId,
                r.MissedPeriodsPolicy))
            .ToListAsync(ct);

        return new PaginatedList<DepreciationRunResponse>(items, totalCount, request.Page, request.PageSize);
    }
}
