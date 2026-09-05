using System.Text.Json;
using ERP_Government.Application.Common.Security;
using ERP_Government.Application.Security.Common;
using ERP_Government.Domain.Payments.Enums;
using ERP_Government.Domain.Security.Entities;
using ERP_Government.Domain.Security.Enums;

namespace ERP_Government.Application.Payments.Commands.DisbursementRequests.ApproveDisbursementRequest;

[Authorize(Policy = PermissionCodes.DisbursementRequestsApprove)]
public class ApproveDisbursementRequestCommand : IRequest<Result>
{
    public int Id { get; init; }
    public string? Reason { get; init; }
    public byte[] RowVersion { get; init; } = [];
}

public class ApproveDisbursementRequestCommandHandler(
    IApplicationDbContext context,
    IIdentityService identityService,
    IUser user) : IRequestHandler<ApproveDisbursementRequestCommand, Result>
{
    private static readonly HashSet<string> RequiredRoles = ["AccountsManager", "AuthorizingOfficer"];

    public async Task<Result> Handle(
        ApproveDisbursementRequestCommand request,
        CancellationToken cancellationToken)
    {
        if (user.Id is not int executingUserId)
            return Result.Failure(new[] { "User identity is required for this operation." });

        var entity = await context.DisbursementRequests
            .FindAsync(request.Id, cancellationToken);

        if (entity is null)
            return Result.Failure(["Disbursement request not found."]);

        if (entity.Status != DisbursementRequestStatus.PendingApproval)
            return Result.Failure(["Only pending disbursement requests can be approved."]);

        var existingApprovals = await context.ApprovalHistory
            .Where(a => a.DocumentType == "DisbursementRequest"
                && a.DocumentId == entity.Id
                && a.Action == ApprovalAction.Approve)
            .ToListAsync(cancellationToken);

        if (existingApprovals.Any(a => a.ApproverUserId == executingUserId))
            return Result.Failure(["A different approver is required."]);

        var step = existingApprovals.Count + 1;

        if (step == 1)
        {
            bool hasRequiredRole = false;
            string matchedRole = "";

            foreach (var role in RequiredRoles)
            {
                if (await identityService.IsInRoleAsync(executingUserId, role))
                {
                    hasRequiredRole = true;
                    matchedRole = role;
                    break;
                }
            }

            if (!hasRequiredRole)
                return Result.Failure(["First approver must hold AccountsManager or AuthorizingOfficer role."]);

            var history = new ApprovalHistory
            {
                DocumentType = "DisbursementRequest",
                DocumentId = entity.Id,
                ApprovalStep = step,
                Action = ApprovalAction.Approve,
                ApproverUserId = executingUserId,
                RequiredRole = matchedRole,
                Decision = "Approved",
                DecisionAt = DateTimeOffset.UtcNow,
                Reason = request.Reason,
                EvaluationSnapshot = JsonSerializer.Serialize(new { Step = step, DualSignature = true }),
                Created = DateTimeOffset.UtcNow,
                CreatedBy = executingUserId.ToString(),
                LastModified = DateTimeOffset.UtcNow,
                LastModifiedBy = executingUserId.ToString()
            };

            context.ApprovalHistory.Add(history);
            await context.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
        else
        {
            var history = new ApprovalHistory
            {
                DocumentType = "DisbursementRequest",
                DocumentId = entity.Id,
                ApprovalStep = step,
                Action = ApprovalAction.Approve,
                ApproverUserId = executingUserId,
                RequiredRole = "",
                Decision = "Approved",
                DecisionAt = DateTimeOffset.UtcNow,
                Reason = request.Reason,
                EvaluationSnapshot = JsonSerializer.Serialize(new { Step = step, DualSignature = true, Approved = true }),
                Created = DateTimeOffset.UtcNow,
                CreatedBy = executingUserId.ToString(),
                LastModified = DateTimeOffset.UtcNow,
                LastModifiedBy = executingUserId.ToString()
            };

            context.ApprovalHistory.Add(history);

            entity.Status = DisbursementRequestStatus.Approved;
            entity.LastModified = DateTimeOffset.UtcNow;
            entity.LastModifiedBy = executingUserId.ToString();

            await context.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
    }
}

public class ApproveDisbursementRequestCommandValidator : AbstractValidator<ApproveDisbursementRequestCommand>
{
    public ApproveDisbursementRequestCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Invalid disbursement request ID.");
    }
}
