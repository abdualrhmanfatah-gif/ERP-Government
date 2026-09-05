using ERP_Government.Application.Common.Security;
using ERP_Government.Domain.Payments.Enums;
using ERP_Government.Domain.Security.Entities;
using ERP_Government.Domain.Security.Enums;

namespace ERP_Government.Application.Payments.Commands.DisbursementRequests.CancelDisbursementRequest;

[Authorize(Policy = PermissionCodes.DisbursementRequestsCancel)]
public class CancelDisbursementRequestCommand : IRequest<Result>
{
    public int Id { get; init; }
    public string Reason { get; init; } = string.Empty;
    public byte[] RowVersion { get; init; } = [];
}

public class CancelDisbursementRequestCommandHandler(
    IApplicationDbContext context,
    IUser user) : IRequestHandler<CancelDisbursementRequestCommand, Result>
{
    public async Task<Result> Handle(
        CancelDisbursementRequestCommand request,
        CancellationToken cancellationToken)
    {
        if (user.Id is not int userId)
            return Result.Failure(new[] { "User identity is required for this operation." });

        var entity = await context.DisbursementRequests
            .FindAsync(request.Id, cancellationToken);

        if (entity is null)
            return Result.Failure(["Disbursement request not found."]);

        if (entity.Status != DisbursementRequestStatus.Draft
            && entity.Status != DisbursementRequestStatus.Approved)
            return Result.Failure(["Only draft or approved disbursement requests can be cancelled."]);

        if (entity.Status == DisbursementRequestStatus.Approved)
        {
            var hasPayment = await context.Payments
                .AnyAsync(p => p.DisbursementRequestId == entity.Id, cancellationToken);

            if (hasPayment)
                return Result.Failure(["Cannot cancel a disbursement request that has already been paid."]);
        }

        var history = new ApprovalHistory
        {
            DocumentType = "DisbursementRequest",
            DocumentId = entity.Id,
            ApprovalStep = 0,
            Action = ApprovalAction.Cancel,
            ApproverUserId = userId,
            RequiredRole = "",
            Decision = "Cancelled",
            DecisionAt = DateTimeOffset.UtcNow,
            Reason = request.Reason,
            Created = DateTimeOffset.UtcNow,
            CreatedBy = userId.ToString(),
            LastModified = DateTimeOffset.UtcNow,
            LastModifiedBy = userId.ToString()
        };

        context.ApprovalHistory.Add(history);

        entity.Status = DisbursementRequestStatus.Cancelled;
        entity.PaymentOrderId = 0;
        entity.LastModified = DateTimeOffset.UtcNow;
        entity.LastModifiedBy = userId.ToString();

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

public class CancelDisbursementRequestCommandValidator : AbstractValidator<CancelDisbursementRequestCommand>
{
    public CancelDisbursementRequestCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Invalid disbursement request ID.");
        RuleFor(x => x.Reason)
            .NotEmpty().WithMessage("Cancellation reason is required.");
    }
}
