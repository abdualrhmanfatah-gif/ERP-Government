using ERP_Government.Application.Common.Interfaces;
using ERP_Government.Application.Common.Security;
using ERP_Government.Application.Parties.Common;
using ERP_Government.Domain.Payments.Enums;
using Microsoft.EntityFrameworkCore;

namespace ERP_Government.Application.Payments.Commands.PaymentOrders.VoidPaymentOrder;

[Authorize(Policy = PermissionCodes.PaymentOrdersVoid)]
public class VoidPaymentOrderCommand : IRequest<Result>
{
    public int Id { get; init; }
    public string VoidReason { get; init; } = string.Empty;
    public byte[] RowVersion { get; init; } = [];
}

public class VoidPaymentOrderCommandHandler(
    IApplicationDbContext context,
    IDocumentStatusLogger statusLogger,
    IUser user) : IRequestHandler<VoidPaymentOrderCommand, Result>
{
    public async Task<Result> Handle(
        VoidPaymentOrderCommand request,
        CancellationToken cancellationToken)
    {
        if (user.Id is not int userId)
            return Result.Failure(["User identity is required for this operation."]);

        var entity = await context.PaymentOrders
            .FindAsync(request.Id, cancellationToken);

        if (entity is null)
            return Result.Failure(["Payment order not found."]);

        if (entity.Status != PaymentOrderStatus.Approved && entity.Status != PaymentOrderStatus.SentToTreasury)
            return Result.Failure(["Only approved or sent-to-treasury payment orders can be voided."]);

        if (string.IsNullOrWhiteSpace(request.VoidReason))
            return Result.Failure(["Void reason is required."]);

        var hasPayments = await context.Payments
            .AnyAsync(p => p.PaymentOrderId == entity.Id && p.Status == PaymentStatus.Completed, cancellationToken);
        if (hasPayments)
            return Result.Failure(["Cannot void a payment order with completed payments."]);

        var previousStatus = entity.Status.ToString();
        entity.Status = PaymentOrderStatus.Voided;

        // ADR-001 D-2: link is one-directional (PaymentOrder.DisbursementRequestId)
        var linkedRequests = entity.DisbursementRequestId.HasValue
            ? await context.DisbursementRequests
                .Where(r => r.Id == entity.DisbursementRequestId.Value
                    && (r.Status == DisbursementRequestStatus.Draft
                        || r.Status == DisbursementRequestStatus.PendingApproval
                        || (r.Status == DisbursementRequestStatus.Approved
                            && !context.Payments.Any(p => p.PaymentOrderId == entity.Id && p.Status == PaymentStatus.Completed))))
                .ToListAsync(cancellationToken)
            : [];

        foreach (var request_ in linkedRequests)
        {
            request_.Status = DisbursementRequestStatus.Invalidated;
            request_.AccrualJournalEntryId = null;

            context.ApprovalHistory.Add(new Domain.Security.Entities.ApprovalHistory
            {
                DocumentType = "DisbursementRequest",
                DocumentId = request_.Id,
                ApprovalStep = 0,
                Action = Domain.Security.Enums.ApprovalAction.Cancel,
                ApproverUserId = userId,
                Decision = "Invalidated",
                DecisionAt = DateTimeOffset.UtcNow,
                EvaluationSnapshot = $"Linked order voided. Reason: {request.VoidReason}",
                Created = DateTimeOffset.UtcNow,
                CreatedBy = userId.ToString(),
                LastModified = DateTimeOffset.UtcNow,
                LastModifiedBy = userId.ToString()
            });
        }

        await statusLogger.LogAsync(
            "paymentorders",
            entity.Id,
            previousStatus,
            PaymentOrderStatus.Voided.ToString(),
            userId,
            request.VoidReason,
            cancellationToken);

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

public class VoidPaymentOrderCommandValidator : AbstractValidator<VoidPaymentOrderCommand>
{
    public VoidPaymentOrderCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Invalid payment order ID.");

        RuleFor(x => x.VoidReason)
            .NotEmpty().WithMessage("Void reason is required.");
    }
}
