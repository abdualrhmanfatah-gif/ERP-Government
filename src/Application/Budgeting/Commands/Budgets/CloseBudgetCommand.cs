using ERP_Government.Application.Common.Security;
using ERP_Government.Application.Parties.Common;
using ERP_Government.Domain.Budgeting.Enums;
using Microsoft.EntityFrameworkCore;

namespace ERP_Government.Application.Budgeting.Commands.Budgets;

[Authorize(Policy = PermissionCodes.BudgetsClose)]
public record CloseBudgetCommand(
    int Id,
    byte[] RowVersion) : IRequest<Result>;

public class CloseBudgetCommandHandler(
    IApplicationDbContext context,
    IUser currentUser,
    IDocumentStatusLogger statusLogger) : IRequestHandler<CloseBudgetCommand, Result>
{
    public async Task<Result> Handle(
        CloseBudgetCommand request,
        CancellationToken cancellationToken)
    {
        if (currentUser.Id is not int userId)
            return Result.Failure(new[] { "User identity is required for this operation." });

        var entity = await context.Budgets
            .FindAsync(request.Id, cancellationToken);

        if (entity is null)
            return Result.Failure(["Budget not found."]);

        if (entity.Status != BudgetStatus.Active && entity.Status != BudgetStatus.Suspended)
            return Result.Failure(["Only Active or Suspended budgets can be closed."]);

        if (!request.RowVersion.SequenceEqual(entity.RowVersion))
            return Result.Failure(["Concurrency conflict. The record has been modified by another user."]);

        var hasPostedTransactions = await context.BudgetTransactions
            .AnyAsync(x => x.BudgetId == entity.Id
                && x.Status == BudgetTransactionStatus.Posted, cancellationToken);

        if (hasPostedTransactions)
            return Result.Failure(["Cannot close budget with posted transactions. Reverse or lapse them first."]);

        var hasActiveEncumbrances = await context.EncumbranceLines
            .AnyAsync(x => x.BudgetItem.BudgetId == entity.Id
                && (x.Encumbrance.Status == EncumbranceStatus.Active
                    || x.Encumbrance.Status == EncumbranceStatus.PartiallyReleased
                    || x.Encumbrance.Status == EncumbranceStatus.PartiallyLiquidated), cancellationToken);

        if (hasActiveEncumbrances)
            return Result.Failure(["Cannot close budget with active encumbrances. Cancel or reverse them first."]);

        var previousStatus = entity.Status;
        entity.Status = BudgetStatus.Closed;

        RecordApproval(context, entity, previousStatus, BudgetStatus.Closed, userId);

        await statusLogger.LogAsync(
            "budgets",
            entity.Id,
            previousStatus.ToString(),
            BudgetStatus.Closed.ToString(),
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
