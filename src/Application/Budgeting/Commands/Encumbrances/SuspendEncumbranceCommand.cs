using ERP_Government.Application.Common.Security;
using ERP_Government.Application.Parties.Common;
using ERP_Government.Domain.Budgeting.Enums;
using ERP_Government.Domain.Security.Entities;
using Microsoft.EntityFrameworkCore;

namespace ERP_Government.Application.Budgeting.Commands.Encumbrances;

[Authorize(Policy = PermissionCodes.EncumbrancesSuspend)]
public record SuspendEncumbranceCommand(
    int Id,
    byte[] RowVersion,
    string? Reason) : IRequest<Result>;

public class SuspendEncumbranceCommandHandler(
    IApplicationDbContext context,
    IUser user,
    IDocumentStatusLogger statusLogger) : IRequestHandler<SuspendEncumbranceCommand, Result>
{
    public async Task<Result> Handle(SuspendEncumbranceCommand request, CancellationToken cancellationToken)
    {
        if (user.Id is not int userId)
            return Result.Failure(["User identity is required for this operation."]);

        var entity = await context.Encumbrances.FindAsync(request.Id, cancellationToken);
        if (entity is null)
            return Result.Failure(["Encumbrance not found."]);

        if (entity.Status != EncumbranceStatus.Active)
            return Result.Failure(["Only Active encumbrances can be suspended."]);

        if (!request.RowVersion.SequenceEqual(entity.RowVersion))
            return Result.Failure(["Concurrency conflict."]);

        entity.Status = EncumbranceStatus.Suspended;

        context.ApprovalHistory.Add(new ApprovalHistory
        {
            DocumentType = "Encumbrance",
            DocumentId = entity.Id,
            ApproverUserId = userId,
            RequiredRole = "",
            Decision = "Active -> Suspended",
            DecisionAt = DateTimeOffset.UtcNow,
            Reason = request.Reason
        });

        await statusLogger.LogAsync(
            "encumbrances",
            entity.Id,
            EncumbranceStatus.Active.ToString(),
            EncumbranceStatus.Suspended.ToString(),
            userId,
            request.Reason,
            cancellationToken);

        await context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
