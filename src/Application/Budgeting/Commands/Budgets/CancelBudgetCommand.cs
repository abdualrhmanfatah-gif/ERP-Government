using ERP_Government.Application.Common.Security;
using ERP_Government.Application.Parties.Common;
using ERP_Government.Domain.Budgeting.Enums;

namespace ERP_Government.Application.Budgeting.Commands.Budgets;

[Authorize(Policy = PermissionCodes.BudgetsCancel)]
public record CancelBudgetCommand(
    int Id,
    byte[] RowVersion) : IRequest<Result>;

public class CancelBudgetCommandHandler(
    IApplicationDbContext context,
    IUser currentUser,
    IDocumentStatusLogger statusLogger) : IRequestHandler<CancelBudgetCommand, Result>
{
    public async Task<Result> Handle(
        CancelBudgetCommand request,
        CancellationToken cancellationToken)
    {
        var entity = await context.Budgets
            .FindAsync(request.Id, cancellationToken);

        if (entity is null)
            return Result.Failure(["Budget not found."]);

        if (entity.Status != BudgetStatus.Draft
            && entity.Status != BudgetStatus.Submitted
            && entity.Status != BudgetStatus.Suspended)
            return Result.Failure(["Only Draft, Submitted, or Suspended budgets can be cancelled."]);

        if (!request.RowVersion.SequenceEqual(entity.RowVersion))
            return Result.Failure(["Concurrency conflict. The record has been modified by another user."]);

        var previousStatus = entity.Status;
        entity.Status = BudgetStatus.Cancelled;

        RecordApproval(context, entity, previousStatus, BudgetStatus.Cancelled, currentUser.Id);

        await statusLogger.LogAsync(
            "budgets",
            entity.Id,
            previousStatus.ToString(),
            BudgetStatus.Cancelled.ToString(),
            currentUser.Id ?? 0,
            null,
            cancellationToken);

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    private static void RecordApproval(
        IApplicationDbContext context,
        Domain.Budgeting.Entities.Budget budget,
        BudgetStatus from,
        BudgetStatus to,
        int? userId)
    {
        context.ApprovalHistory.Add(new ERP_Government.Domain.Security.Entities.ApprovalHistory
        {
            DocumentType = "Budget",
            DocumentId = budget.Id,
            ApproverUserId = userId ?? 0,
            RequiredRole = string.Empty,
            Decision = $"{from} -> {to}",
            DecisionAt = DateTimeOffset.UtcNow
        });
    }
}
