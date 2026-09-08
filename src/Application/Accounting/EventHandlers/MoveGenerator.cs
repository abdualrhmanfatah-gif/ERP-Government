using ERP_Government.Application.Common.Interfaces;
using ERP_Government.Domain.Accounting.Entities;
using ERP_Government.Domain.Accounting.Enums;
using ERP_Government.Domain.Events.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ERP_Government.Application.Accounting.EventHandlers;

/// <summary>
/// Generates JournalEntry (journal entry header) shells from PostingRule matches.
/// Each PostingRule match produces one JournalEntry (DEP-026 — SourceEventId link removed).
/// JournalEntryLine generation is FEATURE-027 responsibility.
/// </summary>
public class JournalEntryGenerator
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<JournalEntryGenerator> _logger;

    public JournalEntryGenerator(IApplicationDbContext context, ILogger<JournalEntryGenerator> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Creates a JournalEntry shell for the given PostingRule.
    /// </summary>
    public async Task<JournalEntry> GenerateJournalEntryAsync(
        EventType eventType,
        PostingRule postingRule,
        IHasSourceEntity sourceEvent,
        DateOnly documentDate,
        CancellationToken cancellationToken = default)
    {
        var entryNumber = await GenerateEntryNumberAsync(cancellationToken);

        var journalEntry = new JournalEntry
        {
            EntryNumber = entryNumber,
            DocumentDate = documentDate,
            PostingDate = documentDate,
            EntryStatus = EntryStatus.Draft,
            JournalId = postingRule.JournalId,
            IsSystemGenerated = true,
            Narration = $"Auto-generated from {eventType} event"
        };

        _context.JournalEntries.Add(journalEntry);
        // SaveChangesAsync removed — persistence handled by OutboxProcessorService transaction scope.
        // Nested SaveChanges would re-trigger DispatchDomainEventsInterceptor, causing infinite recursion.

        _logger.LogDebug("JournalEntry generated: EntryNumber={EntryNumber}, JournalId={JournalId}",
            journalEntry.EntryNumber, journalEntry.JournalId);

        return journalEntry;
    }

    private async Task<string> GenerateEntryNumberAsync(CancellationToken cancellationToken)
    {
        var year = DateTime.Today.Year;
        var prefix = $"AUTO-{year}-";

        var last = await _context.JournalEntries
            .Where(m => m.EntryNumber.StartsWith(prefix))
            .OrderByDescending(m => m.EntryNumber)
            .Select(m => m.EntryNumber)
            .FirstOrDefaultAsync(cancellationToken);

        if (last is null)
            return $"{prefix}000001";

        var seq = int.Parse(last.Substring(prefix.Length));
        return $"{prefix}{(seq + 1):D6}";
    }
}
