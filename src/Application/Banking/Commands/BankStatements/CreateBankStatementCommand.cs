using ERP_Government.Application.Common.Security;
using ERP_Government.Domain.Banking.Enums;

namespace ERP_Government.Application.Banking.Commands.BankStatements;

// C-B001 — CreateBankStatementCommand
[Authorize(Policy = PermissionCodes.BankStatementsCreate)]
public class CreateBankStatementCommand : IRequest<Result>
{
    public string Name { get; init; } = string.Empty;
    public int BankAccountId { get; init; }
    public int JournalId { get; init; }
    public DateOnly StatementDate { get; init; }
    public decimal BalanceStart { get; init; }
    public decimal BalanceEnd { get; init; }
    public string? ImportSource { get; init; }
}

public class CreateBankStatementCommandHandler(
    IApplicationDbContext context) : IRequestHandler<CreateBankStatementCommand, Result>
{
    public async Task<Result> Handle(
        CreateBankStatementCommand request,
        CancellationToken cancellationToken)
    {
        var bankAccount = await context.BankAccounts
            .FindAsync(request.BankAccountId, cancellationToken);

        if (bankAccount is null)
            return Result.Failure(["Bank account not found."]);

        var journal = await context.Journals
            .FindAsync(request.JournalId, cancellationToken);

        if (journal is null)
            return Result.Failure(["Journal not found."]);

        var entity = new BankStatement
        {
            Name = request.Name,
            BankAccountId = request.BankAccountId,
            JournalId = request.JournalId,
            StatementDate = request.StatementDate,
            BalanceStart = request.BalanceStart,
            BalanceEnd = request.BalanceEnd,
            BalanceEndComputed = request.BalanceStart,
            ImportSource = request.ImportSource,
            Status = BankStatementStatus.Draft
        };

        context.BankStatements.Add(entity);
        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

public class CreateBankStatementCommandValidator : AbstractValidator<CreateBankStatementCommand>
{
    public CreateBankStatementCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(200).WithMessage("Name must not exceed 200 characters.");

        RuleFor(x => x.BankAccountId)
            .GreaterThan(0).WithMessage("Bank account is required.");

        RuleFor(x => x.JournalId)
            .GreaterThan(0).WithMessage("Journal is required.");

        RuleFor(x => x.StatementDate)
            .NotEmpty().WithMessage("Statement date is required.");
    }
}
