using ERP_Government.Application.Budgeting.Common;
using ERP_Government.Application.Common.Security;
using ERP_Government.Application.Parties.Common;
using ERP_Government.Domain.Budgeting.Enums;

namespace ERP_Government.Application.Budgeting.Commands.Appropriations;

[Authorize(Policy = PermissionCodes.AppropriationsActivate)]
public record ActivateAppropriationCommand(
    int Id,
    byte[] RowVersion) : IRequest<Result>;

public class ActivateAppropriationCommandHandler(
    IApplicationDbContext context,
    IUser currentUser,
    IBudgetAvailabilityService availabilityService,
    IDocumentStatusLogger statusLogger) : IRequestHandler<ActivateAppropriationCommand, Result>
{
    public async Task<Result> Handle(
        ActivateAppropriationCommand request,
        CancellationToken cancellationToken)
    {
        var entity = await context.Appropriations
            .FindAsync(request.Id, cancellationToken);

        if (entity is null)
            return Result.Failure(["Appropriation not found."]);

        if (entity.Status != AppropriationStatus.Approved)
            return Result.Failure(["Only Approved appropriations can be activated."]);

        if (!request.RowVersion.SequenceEqual(entity.RowVersion))
            return Result.Failure(["Concurrency conflict. The record has been modified by another user."]);

        // Check availability (only for Original and Supplement types)
        if (entity.AppropriationType == AppropriationType.Original || entity.AppropriationType == AppropriationType.Supplement)
        {
            var available = await availabilityService.GetAvailableForAppropriationAsync(entity.BudgetItemId);

            // Get the BudgetType's ControlMethod
            var budgetItem = await context.BudgetItems
                .Include(x => x.Budget)
                    .ThenInclude(x => x.BudgetType)
                .FirstOrDefaultAsync(x => x.Id == entity.BudgetItemId, cancellationToken);

            var controlMethod = budgetItem?.Budget?.BudgetType?.ControlMethod ?? BudgetControlMethod.None;

            var (allowed, warning) = availabilityService.EvaluateControlMethod(controlMethod, entity.Amount, available);

            if (!allowed)
                return Result.Failure([warning ?? "Amount exceeds available budget."]);
        }

        entity.Status = AppropriationStatus.Active;

        RecordApproval(context, entity, AppropriationStatus.Approved, AppropriationStatus.Active, currentUser.Id);

        await statusLogger.LogAsync(
            "appropriations",
            entity.Id,
            AppropriationStatus.Approved.ToString(),
            AppropriationStatus.Active.ToString(),
            currentUser.Id ?? 0,
            null,
            cancellationToken);

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
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
