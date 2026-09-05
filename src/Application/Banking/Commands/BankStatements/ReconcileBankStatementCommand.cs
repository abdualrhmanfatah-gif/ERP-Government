using ERP_Government.Application.Common.Security;
using ERP_Government.Domain.Banking.Enums;

namespace ERP_Government.Application.Banking.Commands.BankStatements;

// C-B005 — ReconcileBankStatementCommand
[Authorize(Policy = PermissionCodes.BankStatementsCreate)]
public class ReconcileBankStatementCommand : IRequest<Result>
{
    public int Id { get; init; }
}

public class ReconcileBankStatementCommandHandler(
    IApplicationDbContext context) : IRequestHandler<ReconcileBankStatementCommand, Result>
{
    public async Task<Result> Handle(
        ReconcileBankStatementCommand request,
        CancellationToken cancellationToken)
    {
        var entity = await context.BankStatements
            .FindAsync(request.Id, cancellationToken);

        if (entity is null)
            return Result.Failure(["Bank statement not found."]);

        if (entity.Status == BankStatementStatus.Reconciled)
            return Result.Success(); // Idempotent

        if (entity.Status != BankStatementStatus.Imported)
            return Result.Failure(["Only imported statements can be reconciled."]);

        entity.Status = BankStatementStatus.Reconciled;

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

public class ReconcileBankStatementCommandValidator : AbstractValidator<ReconcileBankStatementCommand>
{
    public ReconcileBankStatementCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Bank statement ID is required.");
    }
}
