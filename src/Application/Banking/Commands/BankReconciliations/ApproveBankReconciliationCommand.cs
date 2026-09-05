using ERP_Government.Application.Common.Security;
using ERP_Government.Domain.Banking.Enums;
using ERP_Government.Domain.Events.Banking;

namespace ERP_Government.Application.Banking.Commands.BankReconciliations;

// C-B004 — ApproveBankReconciliationCommand
[Authorize(Policy = PermissionCodes.BankReconciliationApprove)]
public class ApproveBankReconciliationCommand : IRequest<Result>
{
    public int Id { get; init; }
    public int ApprovedById { get; init; }
}

public class ApproveBankReconciliationCommandHandler(
    IApplicationDbContext context) : IRequestHandler<ApproveBankReconciliationCommand, Result>
{
    public async Task<Result> Handle(
        ApproveBankReconciliationCommand request,
        CancellationToken cancellationToken)
    {
        var entity = await context.BankReconciliations
            .FindAsync(request.Id, cancellationToken);

        if (entity is null)
            return Result.Failure(["Bank reconciliation not found."]);

        if (entity.Status == ReconciliationStatus.Approved)
            return Result.Success(); // Idempotent

        if (entity.Status != ReconciliationStatus.Completed)
            return Result.Failure(["Only completed reconciliations can be approved."]);

        entity.Status = ReconciliationStatus.Approved;
        entity.ApprovedById = request.ApprovedById;

        entity.AddDomainEvent(new BankReconciliationPosted
        {
            SourceEntityId = entity.Id,
            OccurredAt = DateTimeOffset.UtcNow,
            BankAccountId = entity.BankAccountId,
            ReconciledAmount = entity.AdjustedBalance
        });

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

public class ApproveBankReconciliationCommandValidator : AbstractValidator<ApproveBankReconciliationCommand>
{
    public ApproveBankReconciliationCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Bank reconciliation ID is required.");

        RuleFor(x => x.ApprovedById)
            .GreaterThan(0).WithMessage("Approver is required.");
    }
}
