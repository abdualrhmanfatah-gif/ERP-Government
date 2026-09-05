using ERP_Government.Domain.Accounting.Entities;

namespace ERP_Government.Infrastructure.Services;

/// <summary>
/// Calculates JournalEntryLine amounts from template lines and RecurringEntry amount.
/// Handles proportional recalculation when Amount override is set.
/// </summary>
public static class AmountCalculator
{
    /// <summary>
    /// Calculates Debit/Credit amounts for each template line.
    /// If entryAmount is null, uses template amounts as-is.
    /// If entryAmount is set, recalculates proportionally preserving each line's
    /// Debit/Credit nature: debit lines scale by (line.DebitAmount / totalDebit),
    /// credit lines scale by (line.CreditAmount / totalCredit).
    /// </summary>
    /// <returns>List of (Debit, Credit) tuples, one per active template line.</returns>
    public static List<(decimal Debit, decimal Credit)> CalculateAmounts(
        IReadOnlyList<JournalEntryTemplateLine> templateLines,
        decimal? entryAmount)
    {
        var activeLines = templateLines.ToList();
        var result = new List<(decimal Debit, decimal Credit)>(activeLines.Count);

        if (entryAmount is null)
        {
            // Use template amounts as-is
            foreach (var line in activeLines)
            {
                result.Add((line.DebitAmount, line.CreditAmount));
            }
            return result;
        }

        // Amount override — proportional recalculation preserving D/C nature
        var totalOriginalDebit = activeLines.Sum(l => l.DebitAmount);
        var totalOriginalCredit = activeLines.Sum(l => l.CreditAmount);

        foreach (var line in activeLines)
        {
            decimal debit = 0;
            decimal credit = 0;

            if (line.DebitAmount > 0 && totalOriginalDebit > 0)
            {
                debit = entryAmount.Value * (line.DebitAmount / totalOriginalDebit);
            }
            else if (line.CreditAmount > 0 && totalOriginalCredit > 0)
            {
                credit = entryAmount.Value * (line.CreditAmount / totalOriginalCredit);
            }

            result.Add((debit, credit));
        }

        return result;
    }
}
