using ERP_Government.Application.Common.Security;
using ERP_Government.Domain.Accounting.Entities;
using ERP_Government.Domain.Accounting.Enums;

namespace ERP_Government.Application.Accounting.Commands.JournalEntryLines.UpdateJournalEntryLine;

[Authorize(Policy = PermissionCodes.JournalEntriesUpdateLines)]
public class UpdateJournalEntryLineCommand : IRequest<Result>
{
    public long Id { get; init; }
    public int JournalEntryId { get; init; }
    public int AccountId { get; init; }
    public string? Description { get; init; }
    public int CurrencyId { get; init; }
    public decimal ExchangeRate { get; init; } = 1;
    public decimal Debit { get; init; }
    public decimal Credit { get; init; }
    public int? CostCenterId { get; init; }
    public byte[] RowVersion { get; init; } = [];
}

public class UpdateJournalEntryLineCommandHandler(
    IApplicationDbContext context) : IRequestHandler<UpdateJournalEntryLineCommand, Result>
{
    public async Task<Result> Handle(
        UpdateJournalEntryLineCommand request,
        CancellationToken cancellationToken)
    {
        // Validate JournalEntry exists and is in Draft status
        var journalEntry = await context.JournalEntries
            .FindAsync(request.JournalEntryId, cancellationToken);

        if (journalEntry is null)
            return Result.Failure(["Journal entry not found."]);

        if (journalEntry.EntryStatus != EntryStatus.Draft)
            return Result.Failure(["Can only edit lines in journal entries with Draft status."]);

        // Validate JournalEntryLine exists
        var entity = await context.JournalEntryLines
            .FindAsync(request.Id, cancellationToken);

        if (entity is null)
            return Result.Failure(["Journal entry line not found."]);

        if (entity.JournalEntryId != request.JournalEntryId)
            return Result.Failure(["Journal entry line does not belong to the specified journal entry."]);

        // Validate Account exists and is postable
        var account = await context.Accounts
            .FindAsync(request.AccountId, cancellationToken);

        if (account is null)
            return Result.Failure(["Account not found."]);

        if (!account.IsPostable)
            return Result.Failure(["Account is not postable."]);

        if (!account.IsActive)
            return Result.Failure(["Account is not active."]);

        // Validate debit/credit XOR
        if (request.Debit > 0 && request.Credit > 0)
            return Result.Failure(["A line cannot have both debit and credit amounts."]);

        if (request.Debit == 0 && request.Credit == 0)
            return Result.Failure(["A line must have either a debit or credit amount."]);

        if (request.Debit < 0 || request.Credit < 0)
            return Result.Failure(["Amounts cannot be negative."]);

        // Update entity
        entity.AccountId = request.AccountId;
        entity.Description = request.Description;
        entity.CurrencyId = request.CurrencyId;
        entity.ExchangeRate = request.ExchangeRate;
        entity.Debit = request.Debit;
        entity.Credit = request.Credit;
        entity.CostCenterId = request.CostCenterId;

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

public class UpdateJournalEntryLineCommandValidator : AbstractValidator<UpdateJournalEntryLineCommand>
{
    public UpdateJournalEntryLineCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Invalid journal entry line ID.");

        RuleFor(x => x.JournalEntryId)
            .GreaterThan(0).WithMessage("Journal entry is required.");

        RuleFor(x => x.AccountId)
            .GreaterThan(0).WithMessage("Account is required.");

        RuleFor(x => x.CurrencyId)
            .GreaterThan(0).WithMessage("Currency is required.");

        RuleFor(x => x.ExchangeRate)
            .GreaterThan(0).WithMessage("Exchange rate must be greater than zero.");

        RuleFor(x => x.Debit)
            .GreaterThanOrEqualTo(0).WithMessage("Debit cannot be negative.");

        RuleFor(x => x.Credit)
            .GreaterThanOrEqualTo(0).WithMessage("Credit cannot be negative.");

        RuleFor(x => x.RowVersion)
            .NotEmpty().WithMessage("RowVersion is required for concurrency control.");
    }
}
