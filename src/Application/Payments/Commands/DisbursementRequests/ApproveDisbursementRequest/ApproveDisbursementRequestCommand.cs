using System.Text.Json;
using ERP_Government.Application.Common.Security;
using ERP_Government.Application.FinancialSettings.Common.Services;
using ERP_Government.Application.Parties.Common;
using ERP_Government.Application.Security.Common;
using ERP_Government.Domain.Payments.Entities;
using ERP_Government.Domain.Payments.Enums;
using ERP_Government.Domain.Security.Entities;
using ERP_Government.Domain.Security.Enums;
using Microsoft.EntityFrameworkCore;

namespace ERP_Government.Application.Payments.Commands.DisbursementRequests.ApproveDisbursementRequest;

[Authorize(Policy = PermissionCodes.DisbursementRequestsApprove)]
public class ApproveDisbursementRequestCommand : IRequest<Result>
{
    public int Id { get; init; }
    public decimal? ApprovedAmount { get; init; }
    public string? Reason { get; init; }
    public byte[] RowVersion { get; init; } = [];
}

public class ApproveDisbursementRequestCommandHandler(
    IApplicationDbContext context,
    IDocumentStatusLogger statusLogger,
    IUser user) : IRequestHandler<ApproveDisbursementRequestCommand, Result>
{
    public async Task<Result> Handle(
        ApproveDisbursementRequestCommand request,
        CancellationToken cancellationToken)
    {
        if (user.Id is not int executingUserId)
            return Result.Failure(["User identity is required for this operation."]);

        var entity = await context.DisbursementRequests
            .FindAsync(request.Id, cancellationToken);

        if (entity is null)
            return Result.Failure(["Disbursement request not found."]);

        if (entity.Status != DisbursementRequestStatus.PendingApproval)
            return Result.Failure(["Only pending disbursement requests can be approved."]);

        // Resolve approver role name for audit trail
        var approverUser = await context.Users.FindAsync(executingUserId, cancellationToken);
        var matchedRole = approverUser?.Role?.Code ?? "Unknown";

        var existingApprovals = await context.ApprovalHistory
            .Where(a => a.DocumentType == "DisbursementRequest"
                && a.DocumentId == entity.Id
                && a.Action == ApprovalAction.Approve)
            .ToListAsync(cancellationToken);

        // Validate approved amount
        if (request.ApprovedAmount.HasValue && request.ApprovedAmount.Value > entity.RequestedAmount)
            return Result.Failure(["Approved amount cannot exceed the requested amount."]);

        if (request.ApprovedAmount.HasValue && request.ApprovedAmount.Value <= 0)
            return Result.Failure(["Approved amount must be greater than zero."]);

        decimal approvedAmount = request.ApprovedAmount ?? entity.RequestedAmount;

        var step = existingApprovals.Count + 1;

        // Record approval in ApprovalHistory
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
            EvaluationSnapshot = JsonSerializer.Serialize(new
            {
                Step = step,
                DualSignature = true,
                ApprovedAmount = approvedAmount
            }),
            Created = DateTimeOffset.UtcNow,
            CreatedBy = executingUserId.ToString(),
            LastModified = DateTimeOffset.UtcNow,
            LastModifiedBy = executingUserId.ToString()
        };

        context.ApprovalHistory.Add(history);

        await statusLogger.LogAsync(
            "disbursementrequests",
            entity.Id,
            DisbursementRequestStatus.PendingApproval.ToString(),
            step == 2 ? DisbursementRequestStatus.Approved.ToString() : DisbursementRequestStatus.PendingApproval.ToString(),
            executingUserId,
            step == 2 ? "Final approval — order generated" : $"Signature {step}",
            cancellationToken);

        // Step 2+: finalize — mark approved (payment order created separately)
        if (step >= 2 && entity.Status == DisbursementRequestStatus.PendingApproval)
        {
            // Validate different user from first signer
            var firstApproval = existingApprovals.FirstOrDefault();
            if (firstApproval != null && firstApproval.ApproverUserId == executingUserId)
                return Result.Failure(["Second signature must be by a different user."]);

            // Validate same amount across signatures
            if (firstApproval != null)
            {
                var firstSnapshot = JsonSerializer.Deserialize<JsonElement>(firstApproval.EvaluationSnapshot ?? "{}");
                var firstAmount = firstSnapshot.TryGetProperty("ApprovedAmount", out var amtProp) ? amtProp.GetDecimal() : entity.RequestedAmount;
                if (firstAmount != approvedAmount)
                    return Result.Failure(["Signatures on different amounts cannot finalize. Reauthorization required."]);
            }

            entity.Status = DisbursementRequestStatus.Approved;
        }

        entity.LastModified = DateTimeOffset.UtcNow;
        entity.LastModifiedBy = executingUserId.ToString();

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

public class ApproveDisbursementRequestCommandValidator : AbstractValidator<ApproveDisbursementRequestCommand>
{
    public ApproveDisbursementRequestCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Invalid disbursement request ID.");

        RuleFor(x => x.ApprovedAmount)
            .GreaterThan(0).When(x => x.ApprovedAmount.HasValue)
            .WithMessage("Approved amount must be greater than zero.");
    }
}
