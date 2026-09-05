using ERP_Government.Application.Common.Security;
using ERP_Government.Application.Parties.Common;
using ERP_Government.Application.Security.Common;
using ERP_Government.Domain.Budgeting.Enums;

namespace ERP_Government.Application.Budgeting.Commands.Budgets;

[Authorize(Policy = PermissionCodes.BudgetsApprove)]
public record ApproveBudgetCommand(
    int Id,
    byte[] RowVersion) : IRequest<Result>;

public class ApproveBudgetCommandHandler(
    IApplicationDbContext context,
    IUser currentUser,
    IDocumentStatusLogger statusLogger,
    IAttachmentGateService attachmentGate) : IRequestHandler<ApproveBudgetCommand, Result>
{
    public async Task<Result> Handle(
        ApproveBudgetCommand request,
        CancellationToken cancellationToken)
    {
        var entity = await context.Budgets
            .FindAsync(request.Id, cancellationToken);

        if (entity is null)
            return Result.Failure(["Budget not found."]);

        if (entity.Status != BudgetStatus.Submitted)
            return Result.Failure(["Only Submitted budgets can be approved."]);

        if (!request.RowVersion.SequenceEqual(entity.RowVersion))
            return Result.Failure(["Concurrency conflict. The record has been modified by another user."]);

        var missingAttachments = await attachmentGate.CheckMandatoryAttachmentsAsync(
            "Budget", entity.Id, cancellationToken);

        if (missingAttachments.Count > 0)
            return Result.Failure([$"Cannot approve: missing mandatory attachments ({string.Join(", ", missingAttachments)})."]);

        entity.Status = BudgetStatus.Approved;

        RecordApproval(context, entity, BudgetStatus.Submitted, BudgetStatus.Approved, currentUser.Id);

        await statusLogger.LogAsync(
            "budgets",
            entity.Id,
            BudgetStatus.Submitted.ToString(),
            BudgetStatus.Approved.ToString(),
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
