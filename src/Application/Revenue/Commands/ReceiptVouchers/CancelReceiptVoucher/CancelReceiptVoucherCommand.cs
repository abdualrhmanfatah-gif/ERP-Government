using ERP_Government.Application.Common.Security;
using ERP_Government.Application.Revenue.Common.Services;
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
            return Result.Failure(["User identity is required."]);

        var voucher = await context.ReceiptVouchers
            .Include(v => v.CollectionOrder)
                .ThenInclude(o => o.RevenueClaim)
            .FirstOrDefaultAsync(v => v.Id == request.Id, cancellationToken);

        if (voucher is null)
            return Result.Failure(["Receipt voucher not found."]);

        if (voucher.Status != ReceiptVoucherStatus.Draft)
            return Result.Failure(["Only Draft receipt vouchers can be cancelled."]);

        var previousStatus = voucher.Status;
        voucher.Status = ReceiptVoucherStatus.Cancelled;
        voucher.CancellationReason = request.Reason;
        voucher.RowVersion = request.RowVersion;
        voucher.LastModified = DateTimeOffset.UtcNow;
        voucher.LastModifiedBy = userId.ToString();

        context.DocumentStatusLogs.Add(new DocumentStatusLog
        {
            EntityName = nameof(ReceiptVoucher),
            DocumentId = voucher.Id,
            FromStatus = previousStatus.ToString(),
            ToStatus = ReceiptVoucherStatus.Cancelled.ToString(),
            ChangedById = userId,
            ChangedAt = DateTimeOffset.UtcNow,
            Reason = request.Reason
        });

        await context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

public class CancelReceiptVoucherCommandValidator : AbstractValidator<CancelReceiptVoucherCommand>
{
    public CancelReceiptVoucherCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0).WithMessage("Voucher ID is required.");
        RuleFor(x => x.Reason).NotEmpty().WithMessage("Cancellation reason is required.");
        RuleFor(x => x.RowVersion).NotEmpty().WithMessage("Row version is required for concurrency control.");
    }
}
