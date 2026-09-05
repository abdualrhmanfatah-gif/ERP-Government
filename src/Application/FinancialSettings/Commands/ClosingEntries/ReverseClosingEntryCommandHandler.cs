using ERP_Government.Domain.FinancialSettings.Enums;

namespace ERP_Government.Application.FinancialSettings.Commands.ClosingEntries;

// T-017-030 — ReverseClosingEntryCommandHandler
// Creates a reversal entry with IsReversal=true and ReversalOfId,
// then reopens the fiscal year and unlocks all periods.
public class ReverseClosingEntryCommandHandler(
    IApplicationDbContext context) : IRequestHandler<ReverseClosingEntryCommand, Result<int>>
{
    public async Task<Result<int>> Handle(
        ReverseClosingEntryCommand request,
        CancellationToken cancellationToken)
    {
        var original = await context.YearEndClosingEntries
            .Include(e => e.FiscalYear)
            .FirstOrDefaultAsync(e => e.Id == request.Id, cancellationToken);

        if (original is null)
            return Result<int>.Failure(["Closing entry not found."]);

        if (original.Status != ClosingEntryStatus.Posted)
            return Result<int>.Failure(["Only posted closing entries can be reversed."]);

        if (original.IsReversal)
            return Result<int>.Failure(["A reversal entry cannot be reversed again."]);

        // Reopen fiscal year and unlock periods
        var fiscalYear = original.FiscalYear;
        fiscalYear.Status = FiscalYearStatus.Open;
        fiscalYear.IsClosed = false;

        var periods = await context.FiscalPeriods
            .Where(p => p.FiscalYearId == fiscalYear.Id && p.IsActive)
            .ToListAsync(cancellationToken);

        foreach (var period in periods)
        {
            period.IsLockedForPosting = false;
        }

        // Generate reversal entry number
        var entryNumber = await GenerateEntryNumber(cancellationToken);

        var reversalEntry = new YearEndClosingEntry
        {
            ClosingEntryNumber = entryNumber,
            FiscalYearId = original.FiscalYearId,
            ClosingDate = DateOnly.FromDateTime(DateTime.UtcNow),
            Description = request.Reason ?? $"Reversal of {original.ClosingEntryNumber}",
            Status = ClosingEntryStatus.Posted,
            IsReversal = true,
            ReversalOfId = original.Id,
            IsActive = true
        };

        context.YearEndClosingEntries.Add(reversalEntry);
        await context.SaveChangesAsync(cancellationToken);

        return Result<int>.Success(reversalEntry.Id);
    }

    private async Task<string> GenerateEntryNumber(CancellationToken cancellationToken)
    {
        var year = DateTime.UtcNow.Year;
        var count = await context.YearEndClosingEntries
            .CountAsync(e => e.ClosingEntryNumber.StartsWith($"YEC-{year}"), cancellationToken);

        return $"YEC-{year}-{(count + 1):D4}";
    }
}
