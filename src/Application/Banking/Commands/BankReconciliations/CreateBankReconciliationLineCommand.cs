using ERP_Government.Application.Common.Security;
using ERP_Government.Domain.Banking.Enums;

namespace ERP_Government.Application.Banking.Commands.BankReconciliations;

// C-B010 — CreateBankReconciliationLineCommand
[Authorize(Policy = PermissionCodes.BankReconciliationCreate)]
public class CreateBankReconciliationLineCommand : IRequest<Result>
{
    public int ReconciliationId { get; init; }
    public ReconciliationLineType LineType { get; init; }
    public int? BankStatementLineId { get; init; }
    public long? JournalEntryLineId { get; init; }
    public decimal Amount { get; init; }
    public string? Description { get; init; }
}

public class CreateBankReconciliationLineCommandHandler(
    IApplicationDbContext context) : IRequestHandler<CreateBankReconciliationLineCommand, Result>
{
    public async Task<Result> Handle(
        CreateBankReconciliationLineCommand request,
        CancellationToken cancellationToken)
    {
        var reconciliation = await context.BankReconciliations
            .FindAsync(request.ReconciliationId, cancellationToken);

        if (reconciliation is null)
            return Result.Failure(["Bank reconciliation not found."]);

        if (reconciliation.Status != ReconciliationStatus.Draft)
            return Result.Failure(["Lines can only be added to draft reconciliations."]);

        var entity = new Domain.Banking.Entities.BankReconciliationLine
        {
            ReconciliationId = request.ReconciliationId,
            LineType = request.LineType,
            BankStatementLineId = request.BankStatementLineId,
            JournalEntryLineId = request.JournalEntryLineId,
            Amount = request.Amount,
            Description = request.Description,
            Status = ReconciliationLineStatus.Pending
        };

        context.BankReconciliationLines.Add(entity);
        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

public class CreateBankReconciliationLineCommandValidator : AbstractValidator<CreateBankReconciliationLineCommand>
{
    public CreateBankReconciliationLineCommandValidator()
    {
        RuleFor(x => x.ReconciliationId)
            .GreaterThan(0).WithMessage("Reconciliation ID is required.");

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Amount must be greater than zero.");

        RuleFor(x => x.LineType)
            .IsInEnum().WithMessage("Invalid line type.");

        RuleFor(x => x)
            .Must(x => x.LineType == ReconciliationLineType.Statement || x.JournalEntryLineId.HasValue)
            .WithMessage("Book-type lines must reference a JournalEntryLine.");
    }
}
