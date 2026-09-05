using ERP_Government.Application.Common.Security;
using ERP_Government.Application.FinancialSettings.Common.Services;
using ERP_Government.Domain.Accounting.Entities;
using ERP_Government.Domain.Accounting.Enums;
using ERP_Government.Domain.FinancialSettings.Enums;

namespace ERP_Government.Application.Accounting.Commands.JournalEntries.CreateJournalEntry;

[Authorize(Policy = PermissionCodes.JournalEntriesCreate)]
public class CreateJournalEntryCommand : IRequest<Result<int>>
{
    public string? Ref { get; init; }
    public DateOnly DocumentDate { get; init; }
    public MoveEntryType? EntryType { get; init; }
    public int? JournalId { get; init; }
    public int PeriodId { get; init; }
    public int FiscalYearId { get; init; }
    public string? Narration { get; init; }
}

public class CreateJournalEntryCommandHandler(
    IApplicationDbContext context,
    IDocumentSequenceService documentSequenceService) : IRequestHandler<CreateJournalEntryCommand, Result<int>>
{
    public async Task<Result<int>> Handle(
        CreateJournalEntryCommand request,
        CancellationToken cancellationToken)
    {
        // Validate Journal exists (if provided)
        if (request.JournalId.HasValue)
        {
            var journal = await context.Journals
                .FindAsync(request.JournalId.Value, cancellationToken);

            if (journal is null)
                return Result<int>.Failure(["Journal not found."]);
        }

        // Validate Period exists
        var period = await context.FiscalPeriods
            .FindAsync(request.PeriodId, cancellationToken);

        if (period is null)
            return Result<int>.Failure(["Fiscal period not found."]);

        // Validate FiscalYear exists
        var fiscalYear = await context.FiscalYears
            .FindAsync(request.FiscalYearId, cancellationToken);

        if (fiscalYear is null)
            return Result<int>.Failure(["Fiscal year not found."]);

        // Early fiscal control: period must be unlocked, year must be Open, date within period
        if (period.IsLockedForPosting)
            return Result<int>.Failure(["Fiscal period is locked for posting."]);

        if (fiscalYear.Status != FiscalYearStatus.Open)
            return Result<int>.Failure(["Fiscal year is not Open."]);

        if (request.DocumentDate < period.StartDate || request.DocumentDate > period.EndDate)
            return Result<int>.Failure(["Document date must be within the fiscal period range."]);

        // Generate entry number via JRN sequence (atomic)
        var entryNumber = await documentSequenceService.GenerateNextNumberAsync("JournalEntry", cancellationToken);
        if (entryNumber.StartsWith("Error:", StringComparison.OrdinalIgnoreCase))
            return Result<int>.Failure([entryNumber]);

        var entity = new JournalEntry
        {
            EntryNumber = entryNumber,
            Ref = request.Ref,
            DocumentDate = request.DocumentDate,
            EntryType = request.EntryType,
            EntryStatus = EntryStatus.Draft,
            JournalId = request.JournalId,
            PeriodId = request.PeriodId,
            FiscalYearId = request.FiscalYearId,
            Narration = request.Narration,
            IsSystemGenerated = false
        };

        context.JournalEntries.Add(entity);
        await context.SaveChangesAsync(cancellationToken);

        return Result<int>.Success(entity.Id);
    }
}

public class CreateJournalEntryCommandValidator : AbstractValidator<CreateJournalEntryCommand>
{
    public CreateJournalEntryCommandValidator()
    {
        RuleFor(x => x.DocumentDate)
            .NotEmpty().WithMessage("Document date is required.");

        RuleFor(x => x.PeriodId)
            .GreaterThan(0).WithMessage("Fiscal period is required.");

        RuleFor(x => x.FiscalYearId)
            .GreaterThan(0).WithMessage("Fiscal year is required.");

        RuleFor(x => x.EntryType)
            .IsInEnum().When(x => x.EntryType.HasValue)
            .WithMessage("Invalid entry type.");
    }
}
