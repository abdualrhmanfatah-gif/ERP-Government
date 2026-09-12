using ERP_Government.Application.Common.Interfaces;
using ERP_Government.Domain.Accounting.Entities;
using ERP_Government.Domain.Accounting.Enums;
using ERP_Government.Domain.Events.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ERP_Government.Application.Accounting.EventHandlers;

/// <summary>
/// Generates JournalEntry + JournalEntryLines from PostingRule matches.
/// Each PostingRule match produces one JournalEntry with lines from PostingRuleLines.
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
    /// Creates a JournalEntry with lines for the given PostingRule.
    /// </summary>
    public async Task<JournalEntry> GenerateJournalEntryAsync(
        EventType eventType,
        PostingRule postingRule,
        IHasSourceEntity sourceEvent,
        DateOnly documentDate,
        CancellationToken cancellationToken = default)
    {
        var entryNumber = await GenerateEntryNumberAsync(cancellationToken);

        var period = await _context.FiscalPeriods
            .Where(p => p.IsActive && p.StartDate <= documentDate && p.EndDate >= documentDate)
            .FirstOrDefaultAsync(cancellationToken);

        if (period is null)
        {
            throw new InvalidOperationException(
                $"No active fiscal period found for date {documentDate:yyyy-MM-dd}. Cannot generate journal entry.");
        }

        var journalEntry = new JournalEntry
        {
            EntryNumber = entryNumber,
            DocumentDate = documentDate,
            PostingDate = documentDate,
            EntryStatus = EntryStatus.Draft,
            JournalId = postingRule.JournalId,
            PeriodId = period.Id,
            FiscalYearId = period.FiscalYearId,
            IsSystemGenerated = true,
            Narration = $"Auto-generated from {eventType} event"
        };

        _context.JournalEntries.Add(journalEntry);

        // Load PostingRuleLines and create JournalEntryLines
        var ruleLines = await _context.PostingRuleLines
            .Where(l => l.PostingRuleId == postingRule.Id && l.IsActive)
            .OrderBy(l => l.Sequence)
            .ToListAsync(cancellationToken);

        if (ruleLines.Count > 0)
        {
            var amount = ExtractAmount(sourceEvent);
            var (bankAccountId, accountId, costCenterId, currencyId) = ExtractDimensions(sourceEvent);

            int sequence = 1;
            foreach (var ruleLine in ruleLines)
            {
                var resolvedAccountId = ruleLine.FixedAccountId
                    ?? (ruleLine.AccountSource == AccountSource.FromEventDimension ? accountId : null);

                if (resolvedAccountId is null)
                {
                    _logger.LogWarning("Could not resolve account for PostingRuleLine {LineId}, skipping", ruleLine.Id);
                    continue;
                }

                var journalEntryLine = new JournalEntryLine
                {
                    // JournalEntry navigation is managed by adding to Lines collection below
                    Sequence = sequence++,
                    AccountId = resolvedAccountId.Value,
                    Description = $"{eventType} — {ruleLine.DebitOrCredit}",
                    Debit = ruleLine.DebitOrCredit == DebitOrCredit.Debit ? amount : 0,
                    Credit = ruleLine.DebitOrCredit == DebitOrCredit.Credit ? amount : 0,
                    CurrencyId = currencyId,
                    ExchangeRate = 1,
                    CostCenterId = costCenterId,
                    PaymentOrderId = sourceEvent is Domain.Events.Payments.PaymentOrderExecuted p ? p.PaymentOrderId : null
                };

                journalEntry.Lines.Add(journalEntryLine);
            }
        }

        _logger.LogDebug("JournalEntry generated: EntryNumber={EntryNumber}, JournalId={JournalId}, Lines={LineCount}",
            journalEntry.EntryNumber, journalEntry.JournalId, ruleLines.Count);

        return journalEntry;
    }

    private static decimal ExtractAmount(IHasSourceEntity sourceEvent)
    {
        return sourceEvent switch
        {
            Domain.Events.Payments.PaymentOrderExecuted e => e.Amount,
            _ => 0
        };
    }

    private static (int? BankAccountId, int? AccountId, int? CostCenterId, int CurrencyId) ExtractDimensions(IHasSourceEntity sourceEvent)
    {
        return sourceEvent switch
        {
            Domain.Events.Payments.PaymentOrderExecuted e => (e.BankAccountId, e.AccountId, e.CostCenterId, e.CurrencyId),
            _ => (null, null, null, 1)
        };
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
