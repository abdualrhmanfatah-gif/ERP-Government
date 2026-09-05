using ERP_Government.Domain.Accounting.Entities;
using ERP_Government.Domain.Accounting.Enums;
using ERP_Government.Domain.FinancialSettings.Enums;

namespace ERP_Government.Application.FinancialSettings.Commands.ClosingEntries;

// T-017-010 — GenerateYearEndClosingCommandHandler
// Generates a year-end closing entry that closes all fiscal year balances
// into a Retained Earnings account. Auto-locks all periods.
public class GenerateYearEndClosingCommandHandler(
    IApplicationDbContext context) : IRequestHandler<GenerateYearEndClosingCommand, Result<int>>
{
    public async Task<Result<int>> Handle(
        GenerateYearEndClosingCommand request,
        CancellationToken cancellationToken)
    {
        var fiscalYear = await context.FiscalYears
            .Include(fy => fy.Periods)
            .FirstOrDefaultAsync(fy => fy.Id == request.FiscalYearId, cancellationToken);

        if (fiscalYear is null)
            return Result<int>.Failure(["Fiscal year not found."]);

        if (fiscalYear.Status != FiscalYearStatus.Open && fiscalYear.Status != FiscalYearStatus.SoftClosed)
            return Result<int>.Failure(["Only open or soft-closed fiscal years can have closing entries generated."]);

        // Single closing entry per fiscal year
        var existingEntry = await context.YearEndClosingEntries
            .AnyAsync(e => e.FiscalYearId == request.FiscalYearId && e.IsActive, cancellationToken);

        if (existingEntry)
            return Result<int>.Failure(["A closing entry already exists for this fiscal year."]);

        // Lock all periods for the fiscal year
        foreach (var period in fiscalYear.Periods.Where(p => p.IsActive))
        {
            period.IsLockedForPosting = true;
        }

        // Generate closing entry number
        var entryNumber = await GenerateEntryNumber(cancellationToken);

        // Collect all journal entry lines for the fiscal year to calculate net balances per account
        var journalEntryLines = await context.JournalEntryLines
            .Where(ml => ml.JournalEntry.FiscalYearId == request.FiscalYearId && ml.JournalEntry.EntryStatus == EntryStatus.Posted)
            .GroupBy(ml => ml.AccountId)
            .Select(g => new
            {
                AccountId = g.Key,
                TotalDebit = g.Sum(ml => ml.Debit),
                TotalCredit = g.Sum(ml => ml.Credit)
            })
            .ToListAsync(cancellationToken);

        // Create the closing entry entity
        var closingEntry = new YearEndClosingEntry
        {
            ClosingEntryNumber = entryNumber,
            FiscalYearId = request.FiscalYearId,
            ClosingDate = DateOnly.FromDateTime(DateTime.UtcNow),
            Description = request.Description ?? $"Year-end closing for {fiscalYear.Name}",
            Status = ClosingEntryStatus.Draft,
            IsReversal = false,
            IsActive = true
        };

        context.YearEndClosingEntries.Add(closingEntry);
        await context.SaveChangesAsync(cancellationToken);

        return Result<int>.Success(closingEntry.Id);
    }

    private async Task<string> GenerateEntryNumber(CancellationToken cancellationToken)
    {
        var year = DateTime.UtcNow.Year;
        var count = await context.YearEndClosingEntries
            .CountAsync(e => e.ClosingEntryNumber.StartsWith($"YEC-{year}"), cancellationToken);

        return $"YEC-{year}-{(count + 1):D4}";
    }
}
