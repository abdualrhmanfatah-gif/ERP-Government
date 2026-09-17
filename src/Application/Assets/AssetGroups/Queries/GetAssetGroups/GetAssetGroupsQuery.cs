using ERP_Government.Application.Assets.AssetGroups.Common;
using ERP_Government.Domain.Assets.Entities;
using ERP_Government.Application.Common.Security;

namespace ERP_Government.Application.Assets.AssetGroups.Queries.GetAssetGroups;

[Authorize(Policy = PermissionCodes.AssetGroupsView)]
public record GetAssetGroupsQuery(
    string? Search = null,
    bool? IsActive = null,
    int? ParentId = null) : IRequest<List<AssetGroupResponse>>;

public class GetAssetGroupsQueryHandler(
    IApplicationDbContext context) : IRequestHandler<GetAssetGroupsQuery, List<AssetGroupResponse>>
{
    public async Task<List<AssetGroupResponse>> Handle(
        GetAssetGroupsQuery request,
        CancellationToken cancellationToken)
    {
        IQueryable<AssetGroup> query = context.AssetGroups
            .Include(g => g.ParentAssetGroup);

        if (request.IsActive.HasValue)
            query = query.Where(g => g.IsActive == request.IsActive.Value);

        if (request.ParentId.HasValue)
            query = query.Where(g => g.ParentAssetGroupId == request.ParentId.Value);

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToLower();
            query = query.Where(g =>
                g.Code.ToLower().Contains(search) ||
                g.Name.ToLower().Contains(search));
        }

        var groups = await query
            .OrderBy(g => g.Code)
            .ToListAsync(cancellationToken);

        var groupIds = groups.Select(g => g.Id).ToHashSet();
        var childCounts = await context.AssetGroups
            .Where(g => g.ParentAssetGroupId.HasValue && groupIds.Contains(g.ParentAssetGroupId!.Value))
            .GroupBy(g => g.ParentAssetGroupId!.Value)
            .Select(g => new { ParentId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.ParentId, x => x.Count, cancellationToken);

        return groups.Select(g => g.ToResponse(childCounts.GetValueOrDefault(g.Id, 0) > 0)).ToList();
    }
}
