using ERP_Government.Application.Common.Security;
using ERP_Government.Application.Parties.Common;
using ERP_Government.Domain.Budgeting.Enums;

namespace ERP_Government.Application.Budgeting.Commands.Appropriations;

[Authorize(Policy = PermissionCodes.AppropriationsClose)]
public record CloseAppropriationCommand(
    int Id,
    byte[] RowVersion) : IRequest<Result>;

public class CloseAppropriationCommandHandler(
    IApplicationDbContext context,
    IUser currentUser,
    IDocumentStatusLogger statusLogger) : IRequestHandler<CloseAppropriationCommand, Result>
{
    public async Task<Result> Handle(
        CloseAppropriationCommand request,
        CancellationToken cancellationToken)
    {
        var entity = await context.Appropriations
            .FindAsync(request.Id, cancellationToken);

        if (entity is null)
            return Result.Failure(["Appropriation not found."]);

        if (entity.Status != AppropriationStatus.Active && entity.Status != AppropriationStatus.Suspended)
            return Result.Failure(["Only Active or Suspended appropriations can be closed."]);

        if (!request.RowVersion.SequenceEqual(entity.RowVersion))
            return Result.Failure(["Concurrency conflict. The record has been modified by another user."]);

        var previousStatus = entity.Status;
        entity.Status = AppropriationStatus.Closed;

        RecordApproval(context, entity, previousStatus, AppropriationStatus.Closed, currentUser.Id);

        await statusLogger.LogAsync(
            "appropriations",
            entity.Id,
            previousStatus.ToString(),
            AppropriationStatus.Closed.ToString(),
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
