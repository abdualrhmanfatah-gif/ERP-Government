using ERP_Government.Application.Common.Security;
using ERP_Government.Domain.Banking.Enums;

namespace ERP_Government.Application.Banking.Commands.BankReconciliations;

// C-B009 — RejectBankReconciliationCommand
[Authorize(Policy = PermissionCodes.BankReconciliationApprove)]
public class RejectBankReconciliationCommand : IRequest<Result>
{
    public int Id { get; init; }
    public string? Reason { get; init; }
}

public class RejectBankReconciliationCommandHandler(
    IApplicationDbContext context) : IRequestHandler<RejectBankReconciliationCommand, Result>
{
    public async Task<Result> Handle(
        RejectBankReconciliationCommand request,
        CancellationToken cancellationToken)
    {
        var entity = await context.BankReconciliations
            .FindAsync(request.Id, cancellationToken);

        if (entity is null)
            return Result.Failure(["Bank reconciliation not found."]);

        if (entity.Status == ReconciliationStatus.Rejected)
            return Result.Success(); // Idempotent

        if (entity.Status != ReconciliationStatus.Completed)
            return Result.Failure(["Only completed reconciliations can be rejected."]);

        entity.Status = ReconciliationStatus.Rejected;
        entity.RejectionReason = request.Reason;

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

public class RejectBankReconciliationCommandValidator : AbstractValidator<RejectBankReconciliationCommand>
{
    public RejectBankReconciliationCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Bank reconciliation ID is required.");

        RuleFor(x => x.Reason)
            .NotEmpty().WithMessage("Rejection reason is required.")
            .MaximumLength(500).WithMessage("Rejection reason must not exceed 500 characters.");
    }
}
