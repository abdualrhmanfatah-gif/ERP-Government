using ERP_Government.Application.Common.Security;
using ERP_Government.Application.Parties.Common;
using ERP_Government.Domain.Budgeting.Enums;
using ERP_Government.Domain.Security.Entities;
using Microsoft.EntityFrameworkCore;

namespace ERP_Government.Application.Budgeting.Commands.Encumbrances;

[Authorize(Policy = PermissionCodes.EncumbrancesCancel)]
public record CancelEncumbranceCommand(
    int Id,
    byte[] RowVersion,
    string? Reason) : IRequest<Result>;

public class CancelEncumbranceCommandHandler(
    IApplicationDbContext context,
    IUser user,
    IDocumentStatusLogger statusLogger) : IRequestHandler<CancelEncumbranceCommand, Result>
{
    private static readonly HashSet<EncumbranceStatus> NonTerminalStatuses =
        [EncumbranceStatus.Draft, EncumbranceStatus.PendingApproval, EncumbranceStatus.Approved,
         EncumbranceStatus.Active, EncumbranceStatus.Suspended];

    public async Task<Result> Handle(CancelEncumbranceCommand request, CancellationToken cancellationToken)
    {
        var entity = await context.Encumbrances.FindAsync(request.Id, cancellationToken);
        if (entity is null)
            return Result.Failure(["Encumbrance not found."]);

        if (!NonTerminalStatuses.Contains(entity.Status))
            return Result.Failure(["Cannot cancel a terminal encumbrance."]);

        if (!request.RowVersion.SequenceEqual(entity.RowVersion))
            return Result.Failure(["Concurrency conflict."]);

        var fromStatus = entity.Status;
        entity.Status = EncumbranceStatus.Cancelled;

        context.ApprovalHistory.Add(new ApprovalHistory
        {
            DocumentType = "Encumbrance",
            DocumentId = entity.Id,
            ApproverUserId = user.Id ?? 0,
            RequiredRole = "",
            Decision = $"{fromStatus} -> Cancelled",
            DecisionAt = DateTimeOffset.UtcNow,
            Reason = request.Reason
        });

        await statusLogger.LogAsync(
            "encumbrances",
            entity.Id,
            fromStatus.ToString(),
            EncumbranceStatus.Cancelled.ToString(),
            user.Id ?? 0,
            request.Reason,
            cancellationToken);

        await context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
