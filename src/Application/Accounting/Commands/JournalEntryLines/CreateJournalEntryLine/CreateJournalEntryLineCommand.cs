using ERP_Government.Application.Common.Security;
using ERP_Government.Domain.Accounting.Entities;
using ERP_Government.Domain.Accounting.Enums;

namespace ERP_Government.Application.Accounting.Commands.JournalEntryLines.CreateJournalEntryLine;

[Authorize(Policy = PermissionCodes.JournalEntriesUpdateLines)]
public class CreateJournalEntryLineCommand : IRequest<Result<long>>
{
    public int JournalEntryId { get; init; }
    public int AccountId { get; init; }
    public string? Description { get; init; }
    public int CurrencyId { get; init; }
    public decimal ExchangeRate { get; init; } = 1;
    public decimal Debit { get; init; }
    public decimal Credit { get; init; }
    public int? CostCenterId { get; init; }
}

public class CreateJournalEntryLineCommandHandler(
    IApplicationDbContext context) : IRequestHandler<CreateJournalEntryLineCommand, Result<long>>
{
    public async Task<Result<long>> Handle(
        CreateJournalEntryLineCommand request,
        CancellationToken cancellationToken)
    {
        // Validate JournalEntry exists and is in Draft status
        var journalEntry = await context.JournalEntries
            .FindAsync(request.JournalEntryId, cancellationToken);

        if (journalEntry is null)
            return Result<long>.Failure(["Journal entry not found."]);

        if (journalEntry.EntryStatus != EntryStatus.Draft)
            return Result<long>.Failure(["Can only add lines to journal entries in Draft status."]);

        // Validate Account exists and is postable
        var account = await context.Accounts
            .FindAsync(request.AccountId, cancellationToken);

        if (account is null)
            return Result<long>.Failure(["Account not found."]);

        if (!account.IsPostable)
            return Result<long>.Failure(["Account is not postable."]);

        if (!account.IsActive)
            return Result<long>.Failure(["Account is not active."]);

        // Validate debit/credit XOR
        if (request.Debit > 0 && request.Credit > 0)
            return Result<long>.Failure(["A line cannot have both debit and credit amounts."]);

        if (request.Debit == 0 && request.Credit == 0)
            return Result<long>.Failure(["A line must have either a debit or credit amount."]);

        // Validate amounts are non-negative
        if (request.Debit < 0 || request.Credit < 0)
            return Result<long>.Failure(["Amounts cannot be negative."]);

        // Get next sequence number
        var maxSequence = await context.JournalEntryLines
            .Where(x => x.JournalEntryId == request.JournalEntryId)
            .MaxAsync(x => (int?)x.Sequence, cancellationToken) ?? 0;

        var entity = new JournalEntryLine
        {
            JournalEntryId = request.JournalEntryId,
            Sequence = maxSequence + 1,
            AccountId = request.AccountId,
            Description = request.Description,
            CurrencyId = request.CurrencyId,
            ExchangeRate = request.ExchangeRate,
            Debit = request.Debit,
            Credit = request.Credit,
            CostCenterId = request.CostCenterId
        };

        context.JournalEntryLines.Add(entity);
        await context.SaveChangesAsync(cancellationToken);

        return Result<long>.Success(entity.Id);
    }
}

public class CreateJournalEntryLineCommandValidator : AbstractValidator<CreateJournalEntryLineCommand>
{
    public CreateJournalEntryLineCommandValidator()
    {
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
    }
}
