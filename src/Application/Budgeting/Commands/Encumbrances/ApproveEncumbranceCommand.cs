using ERP_Government.Application.Common.Security;
using ERP_Government.Application.Parties.Common;
using ERP_Government.Application.Security.Common;
using ERP_Government.Domain.Budgeting.Enums;
using ERP_Government.Domain.Security.Entities;
using Microsoft.EntityFrameworkCore;

namespace ERP_Government.Application.Budgeting.Commands.Encumbrances;

[Authorize(Policy = PermissionCodes.EncumbrancesApprove)]
public record ApproveEncumbranceCommand(
    int Id,
    byte[] RowVersion,
    string? Reason) : IRequest<Result>;

public class ApproveEncumbranceCommandHandler(
    IApplicationDbContext context,
    IUser user,
    IDocumentStatusLogger statusLogger,
    IAttachmentGateService attachmentGate) : IRequestHandler<ApproveEncumbranceCommand, Result>
{
    public async Task<Result> Handle(ApproveEncumbranceCommand request, CancellationToken cancellationToken)
    {
        var entity = await context.Encumbrances.FindAsync(request.Id, cancellationToken);
        if (entity is null)
            return Result.Failure(["Encumbrance not found."]);

        if (entity.Status != EncumbranceStatus.PendingApproval)
            return Result.Failure(["Only PendingApproval encumbrances can be approved."]);

        if (!request.RowVersion.SequenceEqual(entity.RowVersion))
            return Result.Failure(["Concurrency conflict."]);

        var missingAttachments = await attachmentGate.CheckMandatoryAttachmentsAsync(
            "Encumbrance", entity.Id, cancellationToken);

        if (missingAttachments.Count > 0)
            return Result.Failure([$"Cannot approve: missing mandatory attachments ({string.Join(", ", missingAttachments)})."]);

        entity.Status = EncumbranceStatus.Approved;

        context.ApprovalHistory.Add(new ApprovalHistory
        {
            DocumentType = "Encumbrance",
            DocumentId = entity.Id,
            ApproverUserId = user.Id ?? 0,
            RequiredRole = "",
            Decision = "PendingApproval -> Approved",
            DecisionAt = DateTimeOffset.UtcNow,
            Reason = request.Reason
        });

        await statusLogger.LogAsync(
            "encumbrances",
            entity.Id,
            EncumbranceStatus.PendingApproval.ToString(),
            EncumbranceStatus.Approved.ToString(),
            user.Id ?? 0,
            request.Reason,
            cancellationToken);

        await context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
