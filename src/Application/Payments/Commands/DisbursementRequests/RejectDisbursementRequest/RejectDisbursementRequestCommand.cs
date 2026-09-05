using ERP_Government.Application.Common.Security;
using ERP_Government.Domain.Payments.Enums;
using ERP_Government.Domain.Security.Entities;
using ERP_Government.Domain.Security.Enums;

namespace ERP_Government.Application.Payments.Commands.DisbursementRequests.RejectDisbursementRequest;

[Authorize(Policy = PermissionCodes.DisbursementRequestsReject)]
public class RejectDisbursementRequestCommand : IRequest<Result>
{
    public int Id { get; init; }
    public string Reason { get; init; } = string.Empty;
    public byte[] RowVersion { get; init; } = [];
}

public class RejectDisbursementRequestCommandHandler(
    IApplicationDbContext context,
    IUser user) : IRequestHandler<RejectDisbursementRequestCommand, Result>
{
    public async Task<Result> Handle(
        RejectDisbursementRequestCommand request,
        CancellationToken cancellationToken)
    {
        if (user.Id is not int userId)
            return Result.Failure(new[] { "User identity is required for this operation." });

        var entity = await context.DisbursementRequests
            .FindAsync(request.Id, cancellationToken);

        if (entity is null)
            return Result.Failure(["Disbursement request not found."]);

        if (entity.Status != DisbursementRequestStatus.PendingApproval)
            return Result.Failure(["Only pending disbursement requests can be rejected."]);

        var history = new ApprovalHistory
        {
            DocumentType = "DisbursementRequest",
            DocumentId = entity.Id,
            ApprovalStep = 1,
            Action = ApprovalAction.Reject,
            ApproverUserId = userId,
            RequiredRole = "",
            Decision = "Rejected",
            DecisionAt = DateTimeOffset.UtcNow,
            Reason = request.Reason,
            Created = DateTimeOffset.UtcNow,
            CreatedBy = userId.ToString(),
            LastModified = DateTimeOffset.UtcNow,
            LastModifiedBy = userId.ToString()
        };

        context.ApprovalHistory.Add(history);

        entity.Status = DisbursementRequestStatus.Rejected;
        entity.LastModified = DateTimeOffset.UtcNow;
        entity.LastModifiedBy = userId.ToString();

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

public class RejectDisbursementRequestCommandValidator : AbstractValidator<RejectDisbursementRequestCommand>
{
    public RejectDisbursementRequestCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Invalid disbursement request ID.");
        RuleFor(x => x.Reason)
            .NotEmpty().WithMessage("Rejection reason is required.");
    }
}
