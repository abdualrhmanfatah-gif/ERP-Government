using ERP_Government.Application.Common.Security;
using ERP_Government.Application.Parties.Common;
using ERP_Government.Domain.Budgeting.Enums;
using Microsoft.EntityFrameworkCore;

namespace ERP_Government.Application.Budgeting.Commands.Budgets;

[Authorize(Policy = PermissionCodes.BudgetsActivate)]
public record ActivateBudgetCommand(
    int Id,
    byte[] RowVersion) : IRequest<Result>;

public class ActivateBudgetCommandHandler(
    IApplicationDbContext context,
    IUser currentUser,
    IDocumentStatusLogger statusLogger) : IRequestHandler<ActivateBudgetCommand, Result>
{
    public async Task<Result> Handle(
        ActivateBudgetCommand request,
        CancellationToken cancellationToken)
    {
        if (currentUser.Id is not int userId)
            return Result.Failure(new[] { "User identity is required for this operation." });

        var entity = await context.Budgets
            .FindAsync(request.Id, cancellationToken);

        if (entity is null)
            return Result.Failure(["Budget not found."]);

        if (entity.Status != BudgetStatus.Approved)
            return Result.Failure(["Only Approved budgets can be activated."]);

        if (!request.RowVersion.SequenceEqual(entity.RowVersion))
            return Result.Failure(["Concurrency conflict. The record has been modified by another user."]);

        var allocations = await context.BudgetItemAllocations
            .Where(x => x.BudgetId == entity.Id)
            .ToListAsync(cancellationToken);

        foreach (var allocation in allocations)
        {
            if (!allocation.ApprovedAmount.HasValue)
                allocation.ApprovedAmount = allocation.ProposedAmount;
        }

        entity.Status = BudgetStatus.Active;

        RecordApproval(context, entity, BudgetStatus.Approved, BudgetStatus.Active, userId);

        await statusLogger.LogAsync(
            "budgets",
            entity.Id,
            BudgetStatus.Approved.ToString(),
            BudgetStatus.Active.ToString(),
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
