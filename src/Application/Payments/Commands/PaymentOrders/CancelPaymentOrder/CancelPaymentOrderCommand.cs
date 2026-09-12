using ERP_Government.Application.Common.Interfaces;
using ERP_Government.Application.Common.Security;
using ERP_Government.Application.Parties.Common;
using ERP_Government.Domain.Payments.Enums;
using ERP_Government.Domain.Security.Entities;
using ERP_Government.Domain.Security.Enums;
using Microsoft.EntityFrameworkCore;

namespace ERP_Government.Application.Payments.Commands.PaymentOrders.CancelPaymentOrder;

[Authorize(Policy = PermissionCodes.PaymentOrdersCancel)]
public class CancelPaymentOrderCommand : IRequest<Result>
{
    public int Id { get; init; }
    public string CancellationReason { get; init; } = string.Empty;
    public byte[] RowVersion { get; init; } = [];
}

public class CancelPaymentOrderCommandHandler(
    IApplicationDbContext context,
    IDocumentStatusLogger statusLogger,
    IUser user) : IRequestHandler<CancelPaymentOrderCommand, Result>
{
    public async Task<Result> Handle(
        CancelPaymentOrderCommand request,
        CancellationToken cancellationToken)
    {
        if (user.Id is not int userId)
            return Result.Failure(["User identity is required for this operation."]);

        var entity = await context.PaymentOrders
            .FindAsync(request.Id, cancellationToken);

        if (entity is null)
            return Result.Failure(["Payment order not found."]);

        if (entity.Status != PaymentOrderStatus.Draft && entity.Status != PaymentOrderStatus.Submitted)
            return Result.Failure(["Only draft or submitted payment orders can be cancelled."]);

        if (string.IsNullOrWhiteSpace(request.CancellationReason))
            return Result.Failure(["Cancellation reason is required."]);

        var previousStatus = entity.Status.ToString();
        entity.Status = PaymentOrderStatus.Cancelled;

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

        foreach (var req in linkedRequests)
        {
            req.Status = DisbursementRequestStatus.Invalidated;
            req.AccrualJournalEntryId = null;

            context.ApprovalHistory.Add(new ApprovalHistory
            {
                DocumentType = "DisbursementRequest",
                DocumentId = req.Id,
                ApprovalStep = 0,
                Action = ApprovalAction.Cancel,
                ApproverUserId = userId,
                Decision = "Invalidated",
                DecisionAt = DateTimeOffset.UtcNow,
                EvaluationSnapshot = $"Linked order cancelled. Reason: {request.CancellationReason}",
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
            PaymentOrderStatus.Cancelled.ToString(),
            userId,
            request.CancellationReason,
            cancellationToken);

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

public class CancelPaymentOrderCommandValidator : AbstractValidator<CancelPaymentOrderCommand>
{
    public CancelPaymentOrderCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Invalid payment order ID.");

        RuleFor(x => x.CancellationReason)
            .NotEmpty().WithMessage("Cancellation reason is required.");
    }
}
