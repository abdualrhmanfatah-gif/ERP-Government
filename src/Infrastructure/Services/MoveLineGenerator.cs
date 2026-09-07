using ERP_Government.Domain.Accounting.Entities;

namespace ERP_Government.Infrastructure.Services;

/// <summary>
/// Generates JournalEntryLines from JournalEntryTemplateLines for recurring entry processing.
/// Handles dimension override (CostCenterId) from RecurringEntry.
/// </summary>
public static class JournalEntryLineGenerator
{
    /// <summary>
    /// Creates JournalEntryLines from template lines with calculated amounts and dimension overrides.
    /// </summary>
    public static List<JournalEntryLine> GenerateLines(
        JournalEntry journalEntry,
        IReadOnlyList<JournalEntryTemplateLine> templateLines,
        List<(decimal Debit, decimal Credit)> calculatedAmounts,
        RecurringEntry entry)
    {
        var journalEntryLines = new List<JournalEntryLine>();
        var activeLines = templateLines.ToList();

        for (int i = 0; i < activeLines.Count; i++)
        {
            var templateLine = activeLines[i];
            var (debit, credit) = calculatedAmounts[i];

            var journalEntryLine = new JournalEntryLine
            {
                JournalEntryId = journalEntry.Id,
                Sequence = templateLine.Sequence,
                AccountId = templateLine.AccountId,
                Description = templateLine.Description,
                Debit = debit,
                Credit = credit,
                // Dimension override: RecurringEntry values take precedence over template defaults
                CostCenterId = entry.CostCenterId ?? templateLine.CostCenterId,
                CurrencyId = templateLine.CurrencyId, // Required on template line
                ExchangeRate = 1 // Multi-currency deferred
            };

            journalEntryLines.Add(journalEntryLine);
        }

        return journalEntryLines;
    }
}
