using ERP_Government.Application.Common.Interfaces;
using ERP_Government.Domain.Assets.Entities;
using Microsoft.EntityFrameworkCore;

namespace ERP_Government.Application.Assets.AssetGroups.Common;

public static class AssetGroupValidationHelper
{
    public static async Task<bool> WouldCreateCycleAsync(
        IApplicationDbContext context,
        int parentId,
        int childId,
        CancellationToken ct)
    {
        var visited = new HashSet<int> { childId };
        var current = parentId;

        while (current != 0)
        {
            if (visited.Contains(current))
                return true;

            visited.Add(current);
            var group = await context.AssetGroups.FindAsync([current], ct);
            if (group?.ParentAssetGroupId is null or 0)
                break;
            current = group.ParentAssetGroupId.Value;
        }

        return false;
    }

    public static async Task<int> GetDepthAsync(
        IApplicationDbContext context,
        int groupId,
        CancellationToken ct)
    {
        var depth = 1;
        var current = groupId;

        while (depth < 10)
        {
            var group = await context.AssetGroups.FindAsync([current], ct);
            if (group?.ParentAssetGroupId is null or 0)
                break;
            current = group.ParentAssetGroupId.Value;
            depth++;
        }

        return depth;
    }
}
