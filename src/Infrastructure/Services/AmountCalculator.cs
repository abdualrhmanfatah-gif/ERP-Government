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
    /// Debit/Credit nature: debit lines scale by (line.Debit / totalDebit),
    /// credit lines scale by (line.Credit / totalCredit).
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
                result.Add((line.Debit, line.Credit));
            }
            return result;
        }

        // Amount override — proportional recalculation preserving D/C nature
        var totalOriginalDebit = activeLines.Sum(l => l.Debit);
        var totalOriginalCredit = activeLines.Sum(l => l.Credit);

        foreach (var line in activeLines)
        {
            decimal debit = 0;
            decimal credit = 0;

            if (line.Debit > 0 && totalOriginalDebit > 0)
            {
                debit = entryAmount.Value * (line.Debit / totalOriginalDebit);
            }
            else if (line.Credit > 0 && totalOriginalCredit > 0)
            {
                credit = entryAmount.Value * (line.Credit / totalOriginalCredit);
            }

            result.Add((debit, credit));
        }

        return result;
    }
}
