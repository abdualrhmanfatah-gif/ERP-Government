using ERP_Government.Application.Common.Interfaces;
using ERP_Government.Domain.Accounting.Entities;
using ERP_Government.Domain.Accounting.Enums;
using Microsoft.EntityFrameworkCore;

namespace ERP_Government.Application.Accounting.Common;

public static class AccountGroupHierarchyHelper
{
    public const byte MaxLevel = 5;

    public static byte ComputeLevel(AccountGroup? parent) => parent == null ? (byte)1 : (byte)(parent.Level + 1);

    public static bool ExceedsMaxDepth(byte newLevel) => newLevel > MaxLevel;

    public static string NormalizeCode(string code) => code.Trim().ToUpperInvariant();

    public static bool IsNormalBalanceCompatible(AccountGroupType type, NormalBalanceType balance) =>
        (type is AccountGroupType.Asset or AccountGroupType.Expense && balance == NormalBalanceType.Debit)
        || (type is AccountGroupType.Liability or AccountGroupType.Equity or AccountGroupType.Revenue && balance == NormalBalanceType.Credit);

    public static bool IsTypeInheritanceValid(AccountGroupType childType, AccountGroupType parentType) => childType == parentType;

    /// <summary>
    /// Detects cycle: checks if proposed parent chain contains movingNodeId.
    /// Walks up ancestors (max 5 steps) via DB queries.
    /// </summary>
    public static async Task<bool> WouldCreateCycleAsync(IApplicationDbContext context, int movingNodeId, int? proposedParentId, CancellationToken ct)
    {
        if (proposedParentId == null) return false;
        if (proposedParentId.Value == movingNodeId) return true;

        var currentId = proposedParentId.Value;
        var visited = new HashSet<int> { movingNodeId };
        var steps = 0;
        while (currentId != 0 && steps++ < MaxLevel + 1)
        {
            if (!visited.Add(currentId)) return true;
            if (currentId == movingNodeId) return true;
            var parent = await context.AccountGroups
                .AsNoTracking()
                .Where(x => x.Id == currentId)
                .Select(x => new { x.ParentId })
                .FirstOrDefaultAsync(ct);
            if (parent == null || parent.ParentId == null) break;
            currentId = parent.ParentId.Value;
        }
        return false;
    }

    /// <summary>
    /// Returns all descendant IDs (including self) via iterative BFS (depth ≤5).
    /// </summary>
    public static async Task<HashSet<int>> GetSubtreeIdsAsync(IApplicationDbContext context, int rootId, CancellationToken ct)
    {
        var result = new HashSet<int> { rootId };
        var queue = new Queue<int>();
        queue.Enqueue(rootId);
        var steps = 0;
        while (queue.Count > 0 && steps++ < 1000)
        {
            var current = queue.Dequeue();
            var children = await context.AccountGroups
                .AsNoTracking()
                .Where(x => x.ParentId == current)
                .Select(x => x.Id)
                .ToListAsync(ct);
            foreach (var child in children)
            {
                if (result.Add(child))
                    queue.Enqueue(child);
            }
        }
        return result;
    }

    /// <summary>
    /// Computes max depth within subtree (relative to subtree root Level).
    /// </summary>
    public static async Task<byte> GetMaxLevelInSubtreeAsync(IApplicationDbContext context, HashSet<int> subtreeIds, CancellationToken ct)
    {
        if (subtreeIds.Count == 0) return 0;
        var max = await context.AccountGroups
            .AsNoTracking()
            .Where(x => subtreeIds.Contains(x.Id))
            .MaxAsync(x => (byte?)x.Level, ct);
        return max ?? 0;
    }

    /// <summary>
    /// Builds ancestor path (breadcrumb) from immediate parent up to root (max 5).
    /// Returns list from root → immediate parent.
    /// </summary>
    public static async Task<List<AncestorRefDto>> BuildAncestorPathAsync(IApplicationDbContext context, int? parentId, CancellationToken ct)
    {
        var path = new List<AncestorRefDto>();
        var currentId = parentId;
        var steps = 0;
        var stack = new Stack<AncestorRefDto>();
        while (currentId != null && steps++ < MaxLevel)
        {
            var ancestor = await context.AccountGroups
                .AsNoTracking()
                .Where(x => x.Id == currentId.Value)
                .Select(x => new AncestorRefDto { Id = x.Id, Code = x.Code, Name = x.Name })
                .FirstOrDefaultAsync(ct);
            if (ancestor == null) break;
            stack.Push(ancestor);
            var parent = await context.AccountGroups
                .AsNoTracking()
                .Where(x => x.Id == currentId.Value)
                .Select(x => x.ParentId)
                .FirstOrDefaultAsync(ct);
            currentId = parent;
        }
        while (stack.Count > 0) path.Add(stack.Pop());
        return path;
    }

}
