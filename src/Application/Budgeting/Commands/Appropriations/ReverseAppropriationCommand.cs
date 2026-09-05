using ERP_Government.Application.Common.Security;
using ERP_Government.Application.Parties.Common;
using ERP_Government.Domain.Budgeting.Enums;

namespace ERP_Government.Application.Budgeting.Commands.Appropriations;

[Authorize(Policy = PermissionCodes.AppropriationsReverse)]
public record ReverseAppropriationCommand(
    int Id,
    byte[] RowVersion) : IRequest<Result<int>>;

public class ReverseAppropriationCommandHandler(
    IApplicationDbContext context,
    IUser currentUser,
    IDocumentStatusLogger statusLogger) : IRequestHandler<ReverseAppropriationCommand, Result<int>>
{
    public async Task<Result<int>> Handle(
        ReverseAppropriationCommand request,
        CancellationToken cancellationToken)
    {
        var entity = await context.Appropriations
            .FindAsync(request.Id, cancellationToken);

        if (entity is null)
            return Result<int>.Failure(["Appropriation not found."]);

        if (entity.Status != AppropriationStatus.Active)
            return Result<int>.Failure(["Only Active appropriations can be reversed."]);

        if (!request.RowVersion.SequenceEqual(entity.RowVersion))
            return Result<int>.Failure(["Concurrency conflict. The record has been modified by another user."]);

        // Create new Adjustment row with negative Amount
        var reversal = new Domain.Budgeting.Entities.Appropriation
        {
            BudgetId = entity.BudgetId,
            BudgetItemId = entity.BudgetItemId,
            AppropriationType = AppropriationType.Adjustment,
            DocumentType = entity.DocumentType,
            DocumentId = entity.DocumentId,
            Amount = -entity.Amount,
            Status = AppropriationStatus.Active
        };

        context.Appropriations.Add(reversal);

        // Mark original as Reversed
        entity.Status = AppropriationStatus.Reversed;

        RecordApproval(context, entity, AppropriationStatus.Active, AppropriationStatus.Reversed, currentUser.Id);

        await statusLogger.LogAsync(
            "appropriations",
            entity.Id,
            AppropriationStatus.Active.ToString(),
            AppropriationStatus.Reversed.ToString(),
            currentUser.Id ?? 0,
            null,
            cancellationToken);

        await context.SaveChangesAsync(cancellationToken);

        return Result<int>.Success(reversal.Id);
    }

    private static void RecordApproval(
        IApplicationDbContext context,
        Domain.Budgeting.Entities.Appropriation appropriation,
        AppropriationStatus from,
        AppropriationStatus to,
        int? userId)
    {
        context.ApprovalHistory.Add(new ERP_Government.Domain.Security.Entities.ApprovalHistory
        {
            DocumentType = "Appropriation",
            DocumentId = appropriation.Id,
            ApproverUserId = userId ?? 0,
            RequiredRole = string.Empty,
            Decision = $"{from} -> {to}",
            DecisionAt = DateTimeOffset.UtcNow
        });
    }
}
