using ERP_Government.Application.Common.Security;
using ERP_Government.Application.Parties.Common;
using ERP_Government.Domain.Budgeting.Enums;

namespace ERP_Government.Application.Budgeting.Commands.Budgets;

[Authorize(Policy = PermissionCodes.BudgetsSuspend)]
public record SuspendBudgetCommand(
    int Id,
    byte[] RowVersion) : IRequest<Result>;

public class SuspendBudgetCommandHandler(
    IApplicationDbContext context,
    IUser currentUser,
    IDocumentStatusLogger statusLogger) : IRequestHandler<SuspendBudgetCommand, Result>
{
    public async Task<Result> Handle(
        SuspendBudgetCommand request,
        CancellationToken cancellationToken)
    {
        if (currentUser.Id is not int userId)
            return Result.Failure(new[] { "User identity is required for this operation." });

        var entity = await context.Budgets
            .FindAsync(request.Id, cancellationToken);

        if (entity is null)
            return Result.Failure(["Budget not found."]);

        if (entity.Status != BudgetStatus.Active)
            return Result.Failure(["Only Active budgets can be suspended."]);

        if (!request.RowVersion.SequenceEqual(entity.RowVersion))
            return Result.Failure(["Concurrency conflict. The record has been modified by another user."]);

        entity.Status = BudgetStatus.Suspended;

        RecordApproval(context, entity, BudgetStatus.Active, BudgetStatus.Suspended, userId);

        await statusLogger.LogAsync(
            "budgets",
            entity.Id,
            BudgetStatus.Active.ToString(),
            BudgetStatus.Suspended.ToString(),
            userId,
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
        int userId)
    {
        context.ApprovalHistory.Add(new ERP_Government.Domain.Security.Entities.ApprovalHistory
        {
            DocumentType = "Budget",
            DocumentId = budget.Id,
            ApproverUserId = userId,
            RequiredRole = string.Empty,
            Decision = $"{from} -> {to}",
            DecisionAt = DateTimeOffset.UtcNow
        });
    }
}
