using ERP_Government.Application.Common.Security;
using ERP_Government.Application.Parties.Common;
using ERP_Government.Domain.Budgeting.Enums;

namespace ERP_Government.Application.Budgeting.Commands.Budgets;

[Authorize(Policy = PermissionCodes.BudgetsApprove)]
public record RejectBudgetCommand(
    int Id,
    byte[] RowVersion,
    string? Reason,
    string? Notes) : IRequest<Result>;

public class RejectBudgetCommandHandler(
    IApplicationDbContext context,
    IUser currentUser,
    IDocumentStatusLogger statusLogger) : IRequestHandler<RejectBudgetCommand, Result>
{
    public async Task<Result> Handle(
        RejectBudgetCommand request,
        CancellationToken cancellationToken)
    {
        if (currentUser.Id is not int userId)
            return Result.Failure(new[] { "User identity is required for this operation." });

        var entity = await context.Budgets
            .FindAsync(request.Id, cancellationToken);

        if (entity is null)
            return Result.Failure(["Budget not found."]);

        if (entity.Status != BudgetStatus.Submitted)
            return Result.Failure(["Only Submitted budgets can be rejected."]);

        if (!request.RowVersion.SequenceEqual(entity.RowVersion))
            return Result.Failure(["Concurrency conflict. The record has been modified by another user."]);

        entity.Status = BudgetStatus.Draft;

        RecordApproval(context, entity, BudgetStatus.Submitted, BudgetStatus.Draft, userId, request.Reason, request.Notes);

        await statusLogger.LogAsync(
            "budgets",
            entity.Id,
            BudgetStatus.Submitted.ToString(),
            BudgetStatus.Draft.ToString(),
            userId,
            request.Reason,
            cancellationToken);

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    private static void RecordApproval(
        IApplicationDbContext context,
        Domain.Budgeting.Entities.Budget budget,
        BudgetStatus from,
        BudgetStatus to,
        int userId,
        string? reason,
        string? notes)
    {
        context.ApprovalHistory.Add(new ERP_Government.Domain.Security.Entities.ApprovalHistory
        {
            DocumentType = "Budget",
            DocumentId = budget.Id,
            ApproverUserId = userId,
            RequiredRole = string.Empty,
            Decision = $"{from} -> {to}",
            DecisionAt = DateTimeOffset.UtcNow,
            Reason = reason ?? notes
        });
    }
}
