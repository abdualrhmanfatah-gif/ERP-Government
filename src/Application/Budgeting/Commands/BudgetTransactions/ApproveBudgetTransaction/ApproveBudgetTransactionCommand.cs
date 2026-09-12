using ERP_Government.Application.Common.Security;
using ERP_Government.Application.Parties.Common;
using ERP_Government.Domain.Budgeting.Enums;
using ERP_Government.Domain.Security.Entities;

namespace ERP_Government.Application.Budgeting.Commands.BudgetTransactions.ApproveBudgetTransaction;

[Authorize(Policy = PermissionCodes.BudgetTransactionsApprove)]
public record ApproveBudgetTransactionCommand(
    int Id,
    byte[] RowVersion,
    string? Reason) : IRequest<Result>;

public class ApproveBudgetTransactionCommandHandler(
    IApplicationDbContext context,
    IUser user,
    IDocumentStatusLogger statusLogger) : IRequestHandler<ApproveBudgetTransactionCommand, Result>
{
    public async Task<Result> Handle(ApproveBudgetTransactionCommand request, CancellationToken cancellationToken)
    {
        if (user.Id is not int userId)
            return Result.Failure(["User identity is required for this operation."]);

        var entity = await context.BudgetTransactions.FindAsync(request.Id, cancellationToken);
        if (entity is null)
            return Result.Failure(["Budget transaction not found."]);

        if (entity.Status != BudgetTransactionStatus.Submitted)
            return Result.Failure(["Only Submitted transactions can be approved."]);

        if (!request.RowVersion.SequenceEqual(entity.RowVersion))
            return Result.Failure(["Concurrency conflict."]);

        entity.Status = BudgetTransactionStatus.Approved;
        entity.ApprovedAt = DateTimeOffset.UtcNow;
        entity.ApprovedBy = userId.ToString();

        context.ApprovalHistory.Add(new ApprovalHistory
        {
            DocumentType = "BudgetTransaction",
            DocumentId = entity.Id,
            ApproverUserId = userId,
            RequiredRole = "",
            Decision = "Submitted -> Approved",
            DecisionAt = DateTimeOffset.UtcNow,
            Reason = request.Reason
        });

        await statusLogger.LogAsync(
            "budgettransactions",
            entity.Id,
            BudgetTransactionStatus.Submitted.ToString(),
            BudgetTransactionStatus.Approved.ToString(),
            userId,
            request.Reason,
            cancellationToken);

        await context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
