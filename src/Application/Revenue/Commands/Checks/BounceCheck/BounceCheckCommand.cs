using ERP_Government.Application.Common.Security;
using ERP_Government.Application.Revenue.Common.Services;
using ERP_Government.Domain.Events.Revenue;
using ERP_Government.Domain.Revenue.Entities;
using ERP_Government.Domain.Revenue.Enums;
using ERP_Government.Domain.Security.Entities;

namespace ERP_Government.Application.Revenue.Commands.Checks.BounceCheck;

[Authorize(Policy = PermissionCodes.ChecksBounce)]
public class BounceCheckCommand : IRequest<Result>
{
    public int Id { get; init; }
    public DateTimeOffset BouncedAt { get; init; }
    public string? Reason { get; init; }
    public byte[] RowVersion { get; init; } = [];
}

public class BounceCheckCommandHandler(
    IApplicationDbContext context,
    IUser user) : IRequestHandler<BounceCheckCommand, Result>
{
    public async Task<Result> Handle(
        BounceCheckCommand request,
        CancellationToken cancellationToken)
    {
        if (user.Id is not int userId)
            return Result.Failure(["User identity is required."]);

        var check = await context.Checks
            .Include(c => c.ReceiptVoucher)
                .ThenInclude(v => v.CollectionOrder)
                    .ThenInclude(o => o.RevenueClaim)
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

        if (check is null)
            return Result.Failure(["Check not found."]);

        if (check.Status != CheckStatus.UnderCollection)
            return Result.Failure(["Only checks in UnderCollection status can be bounced."]);

        check.Status = CheckStatus.Bounced;
        check.BouncedAt = request.BouncedAt;
        check.RowVersion = request.RowVersion;
        check.LastModified = DateTimeOffset.UtcNow;
        check.LastModifiedBy = userId.ToString();

        check.AddDomainEvent(new CheckBouncedEvent
        {
            SourceEntityId = check.Id,
            OccurredAt = request.BouncedAt,
            BankName = check.BankName,
            CheckNumber = check.CheckNumber,
            Amount = check.Amount,
            Reason = request.Reason
        });

        // Recalculate order & claim statuses (re-evaluates available amount)
        var voucher = check.ReceiptVoucher;
        if (voucher?.CollectionOrder is not null)
        {
            var order = voucher.CollectionOrder;
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
            else
                order.Status = CollectionOrderStatus.Approved;

            if (order.RevenueClaim is not null)
            {
                var claim = order.RevenueClaim;
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
                else
                    claim.Status = ClaimStatus.Open;
            }
        }

        context.DocumentStatusLogs.Add(new DocumentStatusLog
        {
            EntityName = nameof(Check),
            DocumentId = check.Id,
            FromStatus = CheckStatus.UnderCollection.ToString(),
            ToStatus = CheckStatus.Bounced.ToString(),
            ChangedById = userId,
            ChangedAt = DateTimeOffset.UtcNow,
            Reason = request.Reason
        });

        await context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

public class BounceCheckCommandValidator : AbstractValidator<BounceCheckCommand>
{
    public BounceCheckCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0).WithMessage("Check ID is required.");
        RuleFor(x => x.BouncedAt).NotEmpty().WithMessage("Bounced date is required.");
        RuleFor(x => x.RowVersion).NotEmpty().WithMessage("Row version is required for concurrency control.");
    }
}
