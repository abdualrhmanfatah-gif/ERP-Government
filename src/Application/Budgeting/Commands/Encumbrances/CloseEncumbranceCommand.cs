using ERP_Government.Application.Common.Security;
using ERP_Government.Application.Parties.Common;
using ERP_Government.Domain.Budgeting.Enums;
using ERP_Government.Domain.Security.Entities;
using Microsoft.EntityFrameworkCore;

namespace ERP_Government.Application.Budgeting.Commands.Encumbrances;

[Authorize(Policy = PermissionCodes.EncumbrancesClose)]
public record CloseEncumbranceCommand(
    int Id,
    byte[] RowVersion,
    string? Reason) : IRequest<Result>;

public class CloseEncumbranceCommandHandler(
    IApplicationDbContext context,
    IUser user,
    IDocumentStatusLogger statusLogger) : IRequestHandler<CloseEncumbranceCommand, Result>
{
    public async Task<Result> Handle(CloseEncumbranceCommand request, CancellationToken cancellationToken)
    {
        if (user.Id is not int userId)
            return Result.Failure(["User identity is required for this operation."]);

        var entity = await context.Encumbrances.FindAsync(request.Id, cancellationToken);
        if (entity is null)
            return Result.Failure(["Encumbrance not found."]);

        if (entity.Status is not EncumbranceStatus.Active and not EncumbranceStatus.Suspended)
            return Result.Failure(["Only Active or Suspended encumbrances can be closed."]);

        if (!request.RowVersion.SequenceEqual(entity.RowVersion))
            return Result.Failure(["Concurrency conflict."]);

        var fromStatus = entity.Status;
        entity.Status = EncumbranceStatus.Closed;

        context.ApprovalHistory.Add(new ApprovalHistory
        {
            DocumentType = "Encumbrance",
            DocumentId = entity.Id,
            ApproverUserId = userId,
            RequiredRole = "",
            Decision = $"{fromStatus} -> Closed",
            DecisionAt = DateTimeOffset.UtcNow,
            Reason = request.Reason
        });

        await statusLogger.LogAsync(
            "encumbrances",
            entity.Id,
            fromStatus.ToString(),
            EncumbranceStatus.Closed.ToString(),
            userId,
            request.Reason,
            cancellationToken);

        await context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
