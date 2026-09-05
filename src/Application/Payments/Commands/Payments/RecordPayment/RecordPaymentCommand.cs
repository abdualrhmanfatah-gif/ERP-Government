using ERP_Government.Application.Common.Security;
using ERP_Government.Domain.Events.Common;
using ERP_Government.Domain.Payments.Entities;
using ERP_Government.Domain.Payments.Enums;

namespace ERP_Government.Application.Payments.Commands.Payments.RecordPayment;

[Authorize(Policy = PermissionCodes.PaymentsCreate)]
public class RecordPaymentCommand : IRequest<Result<Common.DTOs.PaymentDto>>
{
    public int DisbursementRequestId { get; init; }
    public PaymentMethod PaymentMethod { get; init; }
    public string? ReferenceNumber { get; init; }
    public string? Notes { get; init; }
}

public class RecordPaymentCommandHandler(
    IApplicationDbContext context,
    IUser user) : IRequestHandler<RecordPaymentCommand, Result<Common.DTOs.PaymentDto>>
{
    public async Task<Result<Common.DTOs.PaymentDto>> Handle(
        RecordPaymentCommand request,
        CancellationToken cancellationToken)
    {
        if (user.Id is not int userId)
            return Result<Common.DTOs.PaymentDto>.Failure(new[] { "User identity is required for this operation." });

        var disbursementRequest = await context.DisbursementRequests
            .Include(d => d.PaymentOrder)
            .FirstOrDefaultAsync(d => d.Id == request.DisbursementRequestId, cancellationToken);

        if (disbursementRequest is null)
            return Result<Common.DTOs.PaymentDto>.Failure(["Disbursement request not found."]);

        if (disbursementRequest.Status != DisbursementRequestStatus.Approved)
            return Result<Common.DTOs.PaymentDto>.Failure(["Disbursement request must be approved before payment execution."]);

        var existingPayment = await context.Payments
            .FirstOrDefaultAsync(p => p.DisbursementRequestId == request.DisbursementRequestId, cancellationToken);

        if (existingPayment is not null)
            return Result<Common.DTOs.PaymentDto>.Failure(["A payment has already been recorded for this disbursement request."]);

        var sequence = await context.DocumentSequences
            .FirstOrDefaultAsync(s => s.DocumentType == "Payment", cancellationToken);

        if (sequence is null)
            return Result<Common.DTOs.PaymentDto>.Failure(["Payment sequence not configured."]);

        var netTotal = disbursementRequest.PaymentOrder.AmountGross - disbursementRequest.PaymentOrder.DeductionAmount;

        var payment = new Payment
        {
            PaymentNumber = $"PAY-{sequence.CurrentNumber:D6}",
            DisbursementRequestId = request.DisbursementRequestId,
            PaymentOrderId = disbursementRequest.PaymentOrderId,
            PaymentMethod = request.PaymentMethod,
            Amount = netTotal,
            PaidById = userId,
            PaidAt = DateTimeOffset.UtcNow,
            ReferenceNumber = request.ReferenceNumber,
            Notes = request.Notes,
            Status = PaymentStatus.Completed,
            Created = DateTimeOffset.UtcNow,
            CreatedBy = userId.ToString(),
            LastModified = DateTimeOffset.UtcNow,
            LastModifiedBy = userId.ToString()
        };

        sequence.CurrentNumber++;

        disbursementRequest.Status = DisbursementRequestStatus.Disbursed;
        disbursementRequest.LastModified = DateTimeOffset.UtcNow;
        disbursementRequest.LastModifiedBy = userId.ToString();

        var paymentOrder = disbursementRequest.PaymentOrder;
        paymentOrder.Status = PaymentOrderStatus.Paid;
        paymentOrder.PaidAt = DateTimeOffset.UtcNow;
        paymentOrder.LastModified = DateTimeOffset.UtcNow;
        paymentOrder.LastModifiedBy = userId.ToString();

        payment.AddDomainEvent(new PaymentRecordedEvent(payment.Id));

        context.Payments.Add(payment);
        await context.SaveChangesAsync(cancellationToken);

        return Result<Common.DTOs.PaymentDto>.Success(new Common.DTOs.PaymentDto(
            payment.Id,
            payment.PaymentNumber,
            payment.DisbursementRequestId,
            disbursementRequest.RequestNumber,
            payment.PaymentOrderId,
            paymentOrder.PaymentOrderNumber,
            payment.PaymentMethod,
            payment.Amount,
            payment.PaidById,
            "",
            payment.PaidAt,
            payment.ReferenceNumber,
            payment.Notes,
            payment.Status,
            paymentOrder.BeneficiaryName));
    }
}

public class PaymentRecordedEvent : BaseEvent, IHasSourceEntity
{
    public int PaymentId { get; }
    public int SourceEntityId => PaymentId;
    public string SourceEntityType => "Payment";
    public DateTimeOffset OccurredAt { get; init; }

    public PaymentRecordedEvent(int paymentId)
    {
        PaymentId = paymentId;
        OccurredAt = DateTimeOffset.UtcNow;
    }
}

public class RecordPaymentCommandValidator : AbstractValidator<RecordPaymentCommand>
{
    public RecordPaymentCommandValidator()
    {
        RuleFor(x => x.DisbursementRequestId)
            .GreaterThan(0).WithMessage("Disbursement request is required.");
    }
}
