using ERP_Government.Application.Common.Security;
using ERP_Government.Domain.Revenue.Entities;
using ERP_Government.Domain.Revenue.Enums;
using ERP_Government.Domain.Security.Entities;

namespace ERP_Government.Application.Revenue.Commands.ReceiptVouchers.CancelReceiptVoucher;

[Authorize(Policy = PermissionCodes.ReceiptVouchersCancel)]
public class CancelReceiptVoucherCommand : IRequest<Result>
{
    public int Id { get; init; }
    public string Reason { get; init; } = string.Empty;
    public byte[] RowVersion { get; init; } = [];
}

public class CancelReceiptVoucherCommandHandler(
    IApplicationDbContext context,
    IUser user) : IRequestHandler<CancelReceiptVoucherCommand, Result>
{
    public async Task<Result> Handle(
        CancelReceiptVoucherCommand request,
        CancellationToken cancellationToken)
    {
        if (user.Id is not int userId)
            return Result.Failure(new[] { "User identity is required for this operation." });

        var voucher = await context.ReceiptVouchers.FindAsync(request.Id, cancellationToken);
        if (voucher is null)
            return Result.Failure(new[] { "Receipt voucher not found."});

        if (voucher.Status is not (ReceiptVoucherStatus.Draft or ReceiptVoucherStatus.PendingReview))
            return Result.Failure(new[] { "Only Draft or PendingReview vouchers can be cancelled."});

        if (voucher.DepositSlipId.HasValue)
            return Result.Failure(new[] { "Cannot cancel a voucher that is linked to a deposit slip."});

        var fromStatus = voucher.Status;

        voucher.Status = ReceiptVoucherStatus.Cancelled;
        voucher.CancellationReason = request.Reason;
        voucher.RowVersion = request.RowVersion;

        context.DocumentStatusLogs.Add(new DocumentStatusLog
        {
            EntityName = nameof(ReceiptVoucher),
            DocumentId = voucher.Id,
            FromStatus = fromStatus.ToString(),
            ToStatus = ReceiptVoucherStatus.Cancelled.ToString(),
            ChangedById = userId,
            ChangedAt = DateTimeOffset.UtcNow,
            Reason = request.Reason
        });

        try
        {
            await context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            return Result.Failure(new[] { "Voucher was modified by another user. Please refresh and try again."});
        }

        return Result.Success();
    }
}

public class CancelReceiptVoucherCommandValidator : AbstractValidator<CancelReceiptVoucherCommand>
{
    public CancelReceiptVoucherCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Voucher ID is required.");

        RuleFor(x => x.Reason)
            .NotEmpty().WithMessage("Cancellation reason is required.")
            .MaximumLength(500).WithMessage("Cancellation reason must not exceed 500 characters.");

        RuleFor(x => x.RowVersion)
            .NotEmpty().WithMessage("Row version is required for concurrency control.");
    }
}
