using ERP_Government.Application.Common.Security;
using ERP_Government.Application.Parties.Common;
using ERP_Government.Domain.Budgeting.Enums;
using ERP_Government.Domain.Security.Entities;
using Microsoft.EntityFrameworkCore;

namespace ERP_Government.Application.Budgeting.Commands.Encumbrances;

[Authorize(Policy = PermissionCodes.EncumbrancesSubmit)]
public record SubmitEncumbranceCommand(
    int Id,
    byte[] RowVersion) : IRequest<Result>;

public class SubmitEncumbranceCommandHandler(
    IApplicationDbContext context,
    IUser user,
    IDocumentStatusLogger statusLogger) : IRequestHandler<SubmitEncumbranceCommand, Result>
{
    public async Task<Result> Handle(SubmitEncumbranceCommand request, CancellationToken cancellationToken)
    {
        var entity = await context.Encumbrances.FindAsync(request.Id, cancellationToken);
        if (entity is null)
            return Result.Failure(["Encumbrance not found."]);

        if (entity.Status != EncumbranceStatus.Draft)
            return Result.Failure(["Only Draft encumbrances can be submitted."]);

        if (!request.RowVersion.SequenceEqual(entity.RowVersion))
            return Result.Failure(["Concurrency conflict."]);

        entity.Status = EncumbranceStatus.PendingApproval;

        context.ApprovalHistory.Add(new ApprovalHistory
        {
            DocumentType = "Encumbrance",
            DocumentId = entity.Id,
            ApproverUserId = user.Id ?? 0,
            RequiredRole = "",
            Decision = "Draft -> PendingApproval",
            DecisionAt = DateTimeOffset.UtcNow
        });

        await statusLogger.LogAsync(
            "encumbrances",
            entity.Id,
            EncumbranceStatus.Draft.ToString(),
            EncumbranceStatus.PendingApproval.ToString(),
            user.Id ?? 0,
            null,
            cancellationToken);

        await context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
