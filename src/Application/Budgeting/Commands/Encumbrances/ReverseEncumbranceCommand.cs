using ERP_Government.Application.Common.Security;
using ERP_Government.Application.Parties.Common;
using ERP_Government.Domain.Budgeting.Enums;
using ERP_Government.Domain.Security.Entities;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace ERP_Government.Application.Budgeting.Commands.Encumbrances;

[Authorize(Policy = PermissionCodes.EncumbrancesReverse)]
public record ReverseEncumbranceCommand(
    int Id,
    byte[] RowVersion,
    string? Reason) : IRequest<Result>;

public class ReverseEncumbranceCommandHandler(
    IApplicationDbContext context,
    IUser user,
    IDocumentStatusLogger statusLogger) : IRequestHandler<ReverseEncumbranceCommand, Result>
{
    public async Task<Result> Handle(ReverseEncumbranceCommand request, CancellationToken cancellationToken)
    {
        if (user.Id is not int userId)
            return Result.Failure(["User identity is required for this operation."]);

        var entity = await context.Encumbrances.FindAsync(request.Id, cancellationToken);
        if (entity is null)
            return Result.Failure(["Encumbrance not found."]);

        if (entity.Status is not EncumbranceStatus.Active and not EncumbranceStatus.Suspended)
            return Result.Failure(["Only Active or Suspended encumbrances can be reversed."]);

        if (!request.RowVersion.SequenceEqual(entity.RowVersion))
            return Result.Failure(["Concurrency conflict."]);

        if (string.IsNullOrWhiteSpace(request.Reason))
            return Result.Failure(["Reversal reason is required."]);

        var fromStatus = entity.Status;
        entity.Status = EncumbranceStatus.Reversed;

        context.ApprovalHistory.Add(new ApprovalHistory
        {
            DocumentType = "Encumbrance",
            DocumentId = entity.Id,
            ApproverUserId = userId,
            RequiredRole = "",
            Decision = $"{fromStatus} -> Reversed",
            DecisionAt = DateTimeOffset.UtcNow,
            Reason = request.Reason
        });

        await statusLogger.LogAsync(
            "encumbrances",
            entity.Id,
            fromStatus.ToString(),
            EncumbranceStatus.Reversed.ToString(),
            userId,
            request.Reason,
            cancellationToken);

        await context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
