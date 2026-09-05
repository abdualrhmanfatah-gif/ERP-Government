using ERP_Government.Application.Common.Interfaces;
using ERP_Government.Domain.Accounting.Entities;
using Microsoft.EntityFrameworkCore;

namespace ERP_Government.Application.Accounting.EventHandlers;

/// <summary>
/// Matches domain events against active PostingRules and returns them in Priority order.
/// </summary>
public class PostingRuleMatcher
{
    private readonly IApplicationDbContext _context;

    public PostingRuleMatcher(IApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Returns all active PostingRules matching the given EventType, ordered by Priority ascending.
    /// </summary>
    public async Task<IReadOnlyList<PostingRule>> MatchAsync(string eventType, CancellationToken cancellationToken = default)
    {
        return await _context.PostingRules
            .Where(r => r.IsActive && r.EventType == eventType)
            .OrderBy(r => r.Priority)
            .ToListAsync(cancellationToken);
    }
}
