using ERP_Government.Application.Common.Security;
using ERP_Government.Domain.Banking.Enums;

namespace ERP_Government.Application.Banking.Commands.BankReconciliations;

// C-B008 — CompleteBankReconciliationCommand
[Authorize(Policy = PermissionCodes.BankReconciliationCreate)]
public class CompleteBankReconciliationCommand : IRequest<Result>
{
    public int Id { get; init; }
}

public class CompleteBankReconciliationCommandHandler(
    IApplicationDbContext context) : IRequestHandler<CompleteBankReconciliationCommand, Result>
{
    public async Task<Result> Handle(
        CompleteBankReconciliationCommand request,
        CancellationToken cancellationToken)
    {
        var entity = await context.BankReconciliations
            .FindAsync(request.Id, cancellationToken);

        if (entity is null)
            return Result.Failure(["Bank reconciliation not found."]);

        if (entity.Status == ReconciliationStatus.Completed)
            return Result.Success(); // Idempotent

        if (entity.Status != ReconciliationStatus.Draft)
            return Result.Failure(["Only draft reconciliations can be completed."]);

        // Check all reconciliation lines are matched
        var unmatchedCount = await context.BankReconciliationLines
            .Where(l => l.ReconciliationId == request.Id && l.Status != ReconciliationLineStatus.Matched)
            .CountAsync(cancellationToken);

        if (unmatchedCount > 0)
            return Result.Failure([$"Cannot complete: {unmatchedCount} line(s) are not matched."]);

        // Check difference is zero
        if (entity.Difference.HasValue && entity.Difference.Value != 0)
            return Result.Failure([$"Cannot complete: difference is not zero (actual: {entity.Difference.Value})."]);

        entity.Status = ReconciliationStatus.Completed;

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

public class CompleteBankReconciliationCommandValidator : AbstractValidator<CompleteBankReconciliationCommand>
{
    public CompleteBankReconciliationCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Bank reconciliation ID is required.");
    }
}
