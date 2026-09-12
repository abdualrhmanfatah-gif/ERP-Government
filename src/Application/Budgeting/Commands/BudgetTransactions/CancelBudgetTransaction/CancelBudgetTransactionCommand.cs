using ERP_Government.Application.Common.Security;
using ERP_Government.Application.Parties.Common;
using ERP_Government.Domain.Budgeting.Enums;
using ERP_Government.Domain.Security.Entities;

namespace ERP_Government.Application.Budgeting.Commands.BudgetTransactions.CancelBudgetTransaction;

[Authorize(Policy = PermissionCodes.BudgetTransactionsCancel)]
public record CancelBudgetTransactionCommand(
    int Id,
    byte[] RowVersion,
    string? Reason) : IRequest<Result>;

public class CancelBudgetTransactionCommandHandler(
    IApplicationDbContext context,
    IUser user,
    IDocumentStatusLogger statusLogger) : IRequestHandler<CancelBudgetTransactionCommand, Result>
{
    public async Task<Result> Handle(CancelBudgetTransactionCommand request, CancellationToken cancellationToken)
    {
        if (user.Id is not int userId)
            return Result.Failure(["User identity is required for this operation."]);

        var entity = await context.BudgetTransactions.FindAsync(request.Id, cancellationToken);
        if (entity is null)
            return Result.Failure(["Budget transaction not found."]);

        if (entity.Status != BudgetTransactionStatus.Submitted)
            return Result.Failure(["Only Submitted transactions can be cancelled."]);

        if (!request.RowVersion.SequenceEqual(entity.RowVersion))
            return Result.Failure(["Concurrency conflict."]);

        entity.Status = BudgetTransactionStatus.Rejected;

        context.ApprovalHistory.Add(new ApprovalHistory
        {
            DocumentType = "BudgetTransaction",
            DocumentId = entity.Id,
            ApproverUserId = userId,
            RequiredRole = "",
            Decision = "Submitted -> Rejected",
            DecisionAt = DateTimeOffset.UtcNow,
            Reason = request.Reason
        });

        await statusLogger.LogAsync(
            "budgettransactions",
            entity.Id,
            BudgetTransactionStatus.Submitted.ToString(),
            BudgetTransactionStatus.Rejected.ToString(),
            userId,
            request.Reason,
            cancellationToken);

        await context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
