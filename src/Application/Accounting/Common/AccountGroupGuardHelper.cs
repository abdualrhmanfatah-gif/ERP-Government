using ERP_Government.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ERP_Government.Application.Accounting.Common;

public static class AccountGroupGuardHelper
{
    public static async Task<bool> IsCodeUniquePermanentAsync(IApplicationDbContext context, string code, int? excludeId, CancellationToken ct)
    {
        var normalized = AccountGroupHierarchyHelper.NormalizeCode(code);
        return !await context.AccountGroups
            .AsNoTracking()
            .AnyAsync(x => x.Code.ToUpper() == normalized && (excludeId == null || x.Id != excludeId.Value), ct);
    }

    /// <summary>
    /// Deep deactivate guard: true if any active descendant or any active account in entire subtree.
    /// Returns tuple (hasActiveDescendant, hasActiveAccount).
    /// </summary>
    public static async Task<(bool HasActiveDescendant, bool HasActiveAccount)> CheckDeepDeactivateGuardAsync(
        IApplicationDbContext context, HashSet<int> subtreeIds, int rootId, CancellationToken ct)
    {
        var hasActiveDescendant = await context.AccountGroups
            .AsNoTracking()
            .AnyAsync(x => subtreeIds.Contains(x.Id) && x.Id != rootId && x.IsActive, ct);

        var hasActiveAccount = await context.Accounts
            .AsNoTracking()
            .AnyAsync(x => subtreeIds.Contains(x.AccountGroupId) && x.IsActive, ct);

        return (hasActiveDescendant, hasActiveAccount);
    }

    /// <summary>
    /// Posted-entries guard: true if any JournalEntryLine belonging to accounts in subtree has posted JournalEntry.
    /// Posted = JournalEntry.PostedAt != null or EntryStatus == Posted.
    /// </summary>
    public static async Task<bool> HasPostedEntriesAsync(IApplicationDbContext context, HashSet<int> subtreeIds, CancellationToken ct)
    {
        // AccountIds in subtree
        var accountIds = await context.Accounts
            .AsNoTracking()
            .Where(a => subtreeIds.Contains(a.AccountGroupId))
            .Select(a => a.Id)
            .ToListAsync(ct);
        if (accountIds.Count == 0) return false;

        // Check JournalEntryLines with posted JournalEntry
        return await context.JournalEntryLines
            .AsNoTracking()
            .AnyAsync(ml => accountIds.Contains(ml.AccountId) && ml.JournalEntry.PostedAt != null, ct);
    }
}
