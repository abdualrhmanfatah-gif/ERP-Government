using ERP_Government.Domain.Assets.Enums;
using ERP_Government.Application.Common.Security;

namespace ERP_Government.Application.Assets.PhysicalCounts.Queries;

public record AssetCountResponse(
    int Id,
    string DocumentNumber,
    DateOnly CountDate,
    string CountType,
    string Status,
    string ResolvedScopeLabel,
    int TotalAssets,
    int FoundCount,
    int NotFoundCount,
    int NotExaminedCount,
    DateTimeOffset Created);

[Authorize(Policy = PermissionCodes.AssetCountsView)]
public record GetAssetCountsQuery(
    string? Search,
    string? Status,
    int Page = 1,
    int PageSize = 20) : IRequest<PaginatedList<AssetCountResponse>>;

public class GetAssetCountsQueryHandler(
    IApplicationDbContext context) : IRequestHandler<GetAssetCountsQuery, PaginatedList<AssetCountResponse>>
{
    public async Task<PaginatedList<AssetCountResponse>> Handle(
        GetAssetCountsQuery request,
        CancellationToken cancellationToken)
    {
        var query = context.AssetPhysicalCounts.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.Search))
            query = query.Where(c => c.CountNumber.Contains(request.Search));

        if (!string.IsNullOrWhiteSpace(request.Status)
            && Enum.TryParse<CountStatus>(request.Status, ignoreCase: true, out var status))
            query = query.Where(c => c.Status == status);

        var totalCount = await query.CountAsync(cancellationToken);

        var counts = await query
            .OrderByDescending(c => c.CountDate)
            .ThenByDescending(c => c.Id)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        var countIds = counts.Select(c => c.Id).ToList();
        var lineStats = await context.AssetPhysicalCountDetails
            .Where(l => countIds.Contains(l.AssetPhysicalCountId))
            .GroupBy(l => l.AssetPhysicalCountId)
            .Select(g => new
            {
                CountId = g.Key,
                Total = g.Count(),
                Found = g.Count(l => l.IsFound == CountFoundState.Found),
                NotFound = g.Count(l => l.IsFound == CountFoundState.NotFound),
                NotExamined = g.Count(l => l.IsFound == CountFoundState.NotExamined)
            })
            .ToDictionaryAsync(x => x.CountId, cancellationToken);

        var items = counts.Select(c =>
        {
            lineStats.TryGetValue(c.Id, out var stats);
            return new AssetCountResponse(
                c.Id,
                c.CountNumber,
                c.CountDate,
                c.CountType,
                c.Status.ToString(),
                c.ResolvedScopeLabel,
                stats?.Total ?? 0,
                stats?.Found ?? 0,
                stats?.NotFound ?? 0,
                stats?.NotExamined ?? 0,
                c.Created);
        }).ToList();

        return new PaginatedList<AssetCountResponse>(items, totalCount, request.Page, request.PageSize);
    }
}
