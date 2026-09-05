using ERP_Government.Application.Common.Security;
using ERP_Government.Application.Parties.Common;
using ERP_Government.Domain.Budgeting.Enums;

namespace ERP_Government.Application.Budgeting.Commands.Appropriations;

[Authorize(Policy = PermissionCodes.AppropriationsSuspend)]
public record SuspendAppropriationCommand(
    int Id,
    byte[] RowVersion) : IRequest<Result>;

public class SuspendAppropriationCommandHandler(
    IApplicationDbContext context,
    IUser currentUser,
    IDocumentStatusLogger statusLogger) : IRequestHandler<SuspendAppropriationCommand, Result>
{
    public async Task<Result> Handle(
        SuspendAppropriationCommand request,
        CancellationToken cancellationToken)
    {
        var entity = await context.Appropriations
            .FindAsync(request.Id, cancellationToken);

        if (entity is null)
            return Result.Failure(["Appropriation not found."]);

        if (entity.Status != AppropriationStatus.Active)
            return Result.Failure(["Only Active appropriations can be suspended."]);

        if (!request.RowVersion.SequenceEqual(entity.RowVersion))
            return Result.Failure(["Concurrency conflict. The record has been modified by another user."]);

        entity.Status = AppropriationStatus.Suspended;

        RecordApproval(context, entity, AppropriationStatus.Active, AppropriationStatus.Suspended, currentUser.Id);

        await statusLogger.LogAsync(
            "appropriations",
            entity.Id,
            AppropriationStatus.Active.ToString(),
            AppropriationStatus.Suspended.ToString(),
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
