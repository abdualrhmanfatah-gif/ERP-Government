using ERP_Government.Application.Common.Security;
using ERP_Government.Domain.Banking.Enums;

namespace ERP_Government.Application.Banking.Commands.BankReconciliations;

// C-B003 — CreateBankReconciliationCommand
[Authorize(Policy = PermissionCodes.BankReconciliationCreate)]
public class CreateBankReconciliationCommand : IRequest<Result>
{
    public int BankAccountId { get; init; }
    public int StatementId { get; init; }
    public DateOnly ReconciliationDate { get; init; }
    public decimal BookBalance { get; init; }
    public decimal StatementBalance { get; init; }
    public int PreparedById { get; init; }
}

public class CreateBankReconciliationCommandHandler(
    IApplicationDbContext context) : IRequestHandler<CreateBankReconciliationCommand, Result>
{
    public async Task<Result> Handle(
        CreateBankReconciliationCommand request,
        CancellationToken cancellationToken)
    {
        var bankAccount = await context.BankAccounts
            .FindAsync(request.BankAccountId, cancellationToken);

        if (bankAccount is null)
            return Result.Failure(["Bank account not found."]);

        var statement = await context.BankStatements
            .FindAsync(request.StatementId, cancellationToken);

        if (statement is null)
            return Result.Failure(["Bank statement not found."]);

        var entity = new BankReconciliation
        {
            BankAccountId = request.BankAccountId,
            StatementId = request.StatementId,
            ReconciliationDate = request.ReconciliationDate,
            BookBalance = request.BookBalance,
            StatementBalance = request.StatementBalance,
            AdjustedBalance = request.BookBalance,
            Difference = request.StatementBalance - request.BookBalance,
            Status = ReconciliationStatus.Draft,
            PreparedById = request.PreparedById
        };

        context.BankReconciliations.Add(entity);
        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

public class CreateBankReconciliationCommandValidator : AbstractValidator<CreateBankReconciliationCommand>
{
    public CreateBankReconciliationCommandValidator()
    {
        RuleFor(x => x.BankAccountId)
            .GreaterThan(0).WithMessage("Bank account is required.");

        RuleFor(x => x.StatementId)
            .GreaterThan(0).WithMessage("Statement is required.");

        RuleFor(x => x.ReconciliationDate)
            .NotEmpty().WithMessage("Reconciliation date is required.");

        RuleFor(x => x.PreparedById)
            .GreaterThan(0).WithMessage("Prepared by is required.");
    }
}
