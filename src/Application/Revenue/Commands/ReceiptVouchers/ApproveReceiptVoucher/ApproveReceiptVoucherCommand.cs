using ERP_Government.Application.Common.Security;
using ERP_Government.Application.Revenue.Common.Services;
using ERP_Government.Domain.Events.Revenue;
using ERP_Government.Domain.Revenue.Entities;
using ERP_Government.Domain.Revenue.Enums;
using ERP_Government.Domain.Security.Entities;

namespace ERP_Government.Application.Revenue.Commands.ReceiptVouchers.ApproveReceiptVoucher;

[Authorize(Policy = PermissionCodes.ReceiptVouchersApprove)]
public class ApproveReceiptVoucherCommand : IRequest<Result>
{
    public int Id { get; init; }
    public string? Reason { get; init; }
    public byte[] RowVersion { get; init; } = [];
}

public class ApproveReceiptVoucherCommandHandler(
    IApplicationDbContext context,
    IUser user) : IRequestHandler<ApproveReceiptVoucherCommand, Result>
{
    public async Task<Result> Handle(
        ApproveReceiptVoucherCommand request,
        CancellationToken cancellationToken)
    {
        if (user.Id is not int userId)
            return Result.Failure(["User identity is required."]);

        var voucher = await context.ReceiptVouchers
            .Include(v => v.Lines)
            .Include(v => v.Checks)
            .Include(v => v.CollectionOrder)
                .ThenInclude(o => o.RevenueClaim)
            .FirstOrDefaultAsync(v => v.Id == request.Id, cancellationToken);

        if (voucher is null)
            return Result.Failure(["Receipt voucher not found."]);

        if (voucher.Status != ReceiptVoucherStatus.Draft)
            return Result.Failure(["Only Draft receipt vouchers can be approved."]);

        voucher.Status = ReceiptVoucherStatus.Approved;
        voucher.ApprovedById = userId;
        voucher.ApprovedAt = DateTimeOffset.UtcNow;
        voucher.RowVersion = request.RowVersion;
        voucher.LastModified = DateTimeOffset.UtcNow;
        voucher.LastModifiedBy = userId.ToString();

        var totalAmount = voucher.Lines.Sum(l => l.Amount);

        if (voucher.PaymentMethod == PaymentMethod.Cash)
        {
            voucher.AddDomainEvent(new CashReceiptApprovedEvent
            {
                SourceEntityId = voucher.Id,
                OccurredAt = DateTimeOffset.UtcNow,
                VoucherNumber = voucher.VoucherNumber,
                ReceivedFrom = voucher.ReceivedFrom,
                TotalAmount = totalAmount,
                Lines = voucher.Lines.Select(l => new CashReceiptLineDetail(l.RevenueAccountId, l.Amount, l.Description)).ToList()
            });
        }
        else if (voucher.PaymentMethod == PaymentMethod.Check)
        {
            voucher.AddDomainEvent(new CheckReceiptApprovedEvent
            {
                SourceEntityId = voucher.Id,
                OccurredAt = DateTimeOffset.UtcNow,
                VoucherNumber = voucher.VoucherNumber,
                ReceivedFrom = voucher.ReceivedFrom,
                TotalAmount = totalAmount
            });
        }

        // Recalculate order & claim statuses
        var order = voucher.CollectionOrder;
        if (order is not null)
        {
            var orderVouchers = await context.ReceiptVouchers
                .Where(v => v.CollectionOrderId == order.Id)
                .Include(v => v.Lines)
                .Include(v => v.Checks)
                .ToListAsync(cancellationToken);

            var (orderCollected, _, orderOutstanding, _) = RevenueMetricsCalculator.CalculateOrderMetrics(order.AuthorizedAmount, orderVouchers);
            if (orderOutstanding <= 0)
                order.Status = CollectionOrderStatus.Collected;
            else if (orderCollected > 0)
                order.Status = CollectionOrderStatus.PartiallyCollected;

            var claim = order.RevenueClaim;
            if (claim is not null)
            {
                var claimOrders = await context.CollectionOrders
                    .Where(o => o.RevenueClaimId == claim.Id)
                    .Include(o => o.ReceiptVouchers)
                        .ThenInclude(v => v.Lines)
                    .Include(o => o.ReceiptVouchers)
                        .ThenInclude(v => v.Checks)
                    .ToListAsync(cancellationToken);

                var (claimCollected, _, claimOutstanding, _) = RevenueMetricsCalculator.CalculateClaimMetrics(claim.TotalAmount, claimOrders);
                if (claimOutstanding <= 0)
                    claim.Status = ClaimStatus.Settled;
                else if (claimCollected > 0)
                    claim.Status = ClaimStatus.PartiallySettled;
            }
        }

        context.DocumentStatusLogs.Add(new DocumentStatusLog
        {
            EntityName = nameof(ReceiptVoucher),
            DocumentId = voucher.Id,
            FromStatus = ReceiptVoucherStatus.Draft.ToString(),
            ToStatus = ReceiptVoucherStatus.Approved.ToString(),
            ChangedById = userId,
            ChangedAt = DateTimeOffset.UtcNow,
            Reason = request.Reason
        });

        await context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

public class ApproveReceiptVoucherCommandValidator : AbstractValidator<ApproveReceiptVoucherCommand>
{
    public ApproveReceiptVoucherCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0).WithMessage("Voucher ID is required.");
        RuleFor(x => x.RowVersion).NotEmpty().WithMessage("Row version is required for concurrency control.");
    }
}
