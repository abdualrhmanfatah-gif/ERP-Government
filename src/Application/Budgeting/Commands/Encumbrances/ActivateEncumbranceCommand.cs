using ERP_Government.Application.Common.Security;
using ERP_Government.Application.Parties.Common;
using ERP_Government.Domain.Budgeting.Enums;
using ERP_Government.Domain.Security.Entities;
using Microsoft.EntityFrameworkCore;

namespace ERP_Government.Application.Budgeting.Commands.Encumbrances;

[Authorize(Policy = PermissionCodes.EncumbrancesActivate)]
public record ActivateEncumbranceCommand(
    int Id,
    byte[] RowVersion) : IRequest<Result>;

public class ActivateEncumbranceCommandHandler(
    IApplicationDbContext context,
    IUser user,
    IDocumentStatusLogger statusLogger) : IRequestHandler<ActivateEncumbranceCommand, Result>
{
    public async Task<Result> Handle(ActivateEncumbranceCommand request, CancellationToken cancellationToken)
    {
        var entity = await context.Encumbrances.FindAsync(request.Id, cancellationToken);
        if (entity is null)
            return Result.Failure(["Encumbrance not found."]);

        if (entity.Status != EncumbranceStatus.Approved)
            return Result.Failure(["Only Approved encumbrances can be activated."]);

        if (!request.RowVersion.SequenceEqual(entity.RowVersion))
            return Result.Failure(["Concurrency conflict."]);

        entity.Status = EncumbranceStatus.Active;

        context.ApprovalHistory.Add(new ApprovalHistory
        {
            DocumentType = "Encumbrance",
            DocumentId = entity.Id,
            ApproverUserId = user.Id ?? 0,
            RequiredRole = "",
            Decision = "Approved -> Active",
            DecisionAt = DateTimeOffset.UtcNow
        });

        await statusLogger.LogAsync(
            "encumbrances",
            entity.Id,
            EncumbranceStatus.Approved.ToString(),
            EncumbranceStatus.Active.ToString(),
            user.Id ?? 0,
            null,
            cancellationToken);

        await context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
