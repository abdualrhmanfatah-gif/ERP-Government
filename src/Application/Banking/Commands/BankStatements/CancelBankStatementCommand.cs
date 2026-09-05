using ERP_Government.Application.Common.Security;
using ERP_Government.Domain.Banking.Enums;

namespace ERP_Government.Application.Banking.Commands.BankStatements;

// C-B006 — CancelBankStatementCommand
[Authorize(Policy = PermissionCodes.BankStatementsCreate)]
public class CancelBankStatementCommand : IRequest<Result>
{
    public int Id { get; init; }
}

public class CancelBankStatementCommandHandler(
    IApplicationDbContext context) : IRequestHandler<CancelBankStatementCommand, Result>
{
    public async Task<Result> Handle(
        CancelBankStatementCommand request,
        CancellationToken cancellationToken)
    {
        var entity = await context.BankStatements
            .FindAsync(request.Id, cancellationToken);

        if (entity is null)
            return Result.Failure(["Bank statement not found."]);

        if (entity.Status == BankStatementStatus.Cancelled)
            return Result.Success(); // Idempotent

        if (entity.Status != BankStatementStatus.Draft)
            return Result.Failure(["Only draft statements can be cancelled."]);

        entity.Status = BankStatementStatus.Cancelled;

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

public class CancelBankStatementCommandValidator : AbstractValidator<CancelBankStatementCommand>
{
    public CancelBankStatementCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Bank statement ID is required.");
    }
}
