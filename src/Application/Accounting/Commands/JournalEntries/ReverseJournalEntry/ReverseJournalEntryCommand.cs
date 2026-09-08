using ERP_Government.Application.Accounting.Common;
using ERP_Government.Application.Common.Security;
using ERP_Government.Application.FinancialSettings.Common.Services;
using ERP_Government.Domain.Accounting.Entities;
using ERP_Government.Domain.Accounting.Enums;
using ERP_Government.Domain.FinancialSettings.Enums;

namespace ERP_Government.Application.Accounting.Commands.JournalEntries.ReverseJournalEntry;

[Authorize(Policy = PermissionCodes.JournalEntriesReverse)]
public class ReverseJournalEntryCommand : IRequest<Result<int>>
{
    public int Id { get; init; }
    public string ReversalReason { get; init; } = string.Empty;
    public byte[] RowVersion { get; init; } = [];
}

public class ReverseJournalEntryCommandHandler(
    IApplicationDbContext context,
    IDocumentSequenceService documentSequenceService) : IRequestHandler<ReverseJournalEntryCommand, Result<int>>
{
    public async Task<Result<int>> Handle(
        ReverseJournalEntryCommand request,
        CancellationToken cancellationToken)
    {
        var original = await context.JournalEntries
            .FindAsync(request.Id, cancellationToken);

        if (original is null)
            return Result<int>.Failure(["Journal entry not found."]);

        // Validate lifecycle: only Posted entries can be reversed
        if (original.EntryStatus != EntryStatus.Posted)
            return Result<int>.Failure(["Only posted journal entries can be reversed."]);

        // Validate not already reversed
        var alreadyReversed = await context.JournalEntries
            .AnyAsync(x => x.ReversalOfId == request.Id, cancellationToken);

        if (alreadyReversed)
            return Result<int>.Failure(["Journal entry has already been reversed."]);

        // Fiscal period control per Q1 A+C: reversal's period must be unlocked and year Open
        var period = await context.FiscalPeriods.FindAsync(original.PeriodId, cancellationToken);
        if (period is null)
            return Result<int>.Failure(["Fiscal period not found."]);
        if (period.IsLockedForPosting)
            return Result<int>.Failure(["Fiscal period is locked for posting."]);

        var fiscalYear = await context.FiscalYears.FindAsync(original.FiscalYearId, cancellationToken);
        if (fiscalYear is null)
            return Result<int>.Failure(["Fiscal year not found."]);
        if (fiscalYear.Status != FiscalYearStatus.Open)
            return Result<int>.Failure(["Fiscal year is not Open."]);

        // FR-020 (revised): finalized-period blocking handled by IsLockedForPosting check above (AccountBalances removed — DEP-026)

        // Get original lines
        var originalLines = await context.JournalEntryLines
            .Where(x => x.JournalEntryId == request.Id)
            .ToListAsync(cancellationToken);

        // Generate entry number via JRN sequence (atomic)
        var entryNumber = await documentSequenceService.GenerateNextNumberAsync("JournalEntry", cancellationToken);
        if (entryNumber.StartsWith("Error:", StringComparison.OrdinalIgnoreCase))
            return Result<int>.Failure([entryNumber]);

        // Create reversal journal entry — DocumentDate = original's date to stay within period range
        var reversalEntry = new JournalEntry
        {
            EntryNumber = entryNumber,
            Ref = $"Reversal of {original.EntryNumber}",
            DocumentDate = original.DocumentDate,
            PostingDate = original.DocumentDate,
            EntryType = original.EntryType,
            EntryStatus = EntryStatus.Posted,
            JournalId = original.JournalId,
            PeriodId = original.PeriodId,
            FiscalYearId = original.FiscalYearId,
            Narration = $"Reversal: {request.ReversalReason}",
            ReversalOfId = original.Id,
            ReversalReason = request.ReversalReason,
            PostedAt = DateTimeOffset.UtcNow,
            IsSystemGenerated = true
        };

        context.JournalEntries.Add(reversalEntry);
        await context.SaveChangesAsync(cancellationToken);

        // Create reversal lines (swap debit/credit)
        foreach (var line in originalLines)
        {
            var reversalLine = new JournalEntryLine
            {
                JournalEntryId = reversalEntry.Id,
                Sequence = line.Sequence,
                AccountId = line.AccountId,
                Description = $"Reversal: {line.Description}",
                CurrencyId = line.CurrencyId,
                ExchangeRate = line.ExchangeRate,
                Debit = line.Credit, // Swap
                Credit = line.Debit, // Swap
                CostCenterId = line.CostCenterId
            };

            context.JournalEntryLines.Add(reversalLine);
        }

        // Mark original as reversed
        original.EntryStatus = EntryStatus.Reversed;

        // FR-001 (revised): AccountBalances removed (DEP-026) — reversal lines themselves adjust live aggregation

        await context.SaveChangesAsync(cancellationToken);

        return Result<int>.Success(reversalEntry.Id);
    }
}

public class ReverseJournalEntryCommandValidator : AbstractValidator<ReverseJournalEntryCommand>
{
    public ReverseJournalEntryCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Invalid journal entry ID.");

        RuleFor(x => x.ReversalReason)
            .NotEmpty().WithMessage("Reversal reason is required.")
            .MaximumLength(500).WithMessage("Reversal reason must not exceed 500 characters.");

        RuleFor(x => x.RowVersion)
            .NotEmpty().WithMessage("RowVersion is required for concurrency control.");
    }
}
