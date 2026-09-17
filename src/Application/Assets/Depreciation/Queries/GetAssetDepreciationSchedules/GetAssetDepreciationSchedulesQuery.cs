using ERP_Government.Application.Common.Models;
using ERP_Government.Domain.Assets.Entities;
using ERP_Government.Application.Common.Security;

namespace ERP_Government.Application.Assets.Depreciation.Queries.GetAssetDepreciationSchedules;

[Authorize(Policy = PermissionCodes.AssetDepreciationView)]
public record GetAssetDepreciationSchedulesQuery(int AssetId) : IRequest<PaginatedList<AssetDepreciationScheduleResponse>>;

public record AssetDepreciationScheduleResponse(
    int Id,
    DateOnly DepreciationDate,
    string FiscalYear,
    int PeriodNumber,
    decimal Amount,
    decimal AccumulatedDepreciation,
    string Status,
    int? JournalEntryId);

public class GetAssetDepreciationSchedulesQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetAssetDepreciationSchedulesQuery, PaginatedList<AssetDepreciationScheduleResponse>>
{
    public async Task<PaginatedList<AssetDepreciationScheduleResponse>> Handle(
        GetAssetDepreciationSchedulesQuery request, CancellationToken ct)
    {
        var query = context.DepreciationScheduleLines
            .Where(s => s.AssetId == request.AssetId)
            .OrderByDescending(s => s.DepreciationRun!.DepreciationDate);

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .Select(s => new AssetDepreciationScheduleResponse(
                s.Id,
                s.DepreciationRun!.DepreciationDate,
                s.DepreciationRun!.FiscalYear!.Name,
                s.PeriodNumber,
                s.Amount,
                s.ClosingAccumulatedDepreciation,
                s.DepreciationRun!.Status.ToString(),
                s.DepreciationRun!.JournalEntryId))
            .ToListAsync(ct);

        return new PaginatedList<AssetDepreciationScheduleResponse>(items, totalCount, 1, totalCount);
    }
}
