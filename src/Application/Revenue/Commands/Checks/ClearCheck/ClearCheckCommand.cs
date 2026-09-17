using ERP_Government.Application.Common.Security;
using ERP_Government.Application.Revenue.Common.Services;
using ERP_Government.Domain.Events.Revenue;
using ERP_Government.Domain.Revenue.Entities;
using ERP_Government.Domain.Revenue.Enums;
using ERP_Government.Domain.Security.Entities;

namespace ERP_Government.Application.Revenue.Commands.Checks.ClearCheck;

[Authorize(Policy = PermissionCodes.ChecksClear)]
public class ClearCheckCommand : IRequest<Result>
{
    public int Id { get; init; }
    public DateTimeOffset ClearedAt { get; init; }
    public byte[] RowVersion { get; init; } = [];
}

public class ClearCheckCommandHandler(
    IApplicationDbContext context,
    IUser user) : IRequestHandler<ClearCheckCommand, Result>
{
    public async Task<Result> Handle(
        ClearCheckCommand request,
        CancellationToken cancellationToken)
    {
        if (user.Id is not int userId)
            return Result.Failure(["User identity is required."]);

        var check = await context.Checks
            .Include(c => c.ReceiptVoucher)
                .ThenInclude(v => v.Lines)
            .Include(c => c.ReceiptVoucher)
                .ThenInclude(v => v.CollectionOrder)
                    .ThenInclude(o => o.RevenueClaim)
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

        if (check is null)
            return Result.Failure(["Check not found."]);

        if (check.Status != CheckStatus.UnderCollection)
            return Result.Failure(["Only checks in UnderCollection status can be cleared."]);

        check.Status = CheckStatus.Cleared;
        check.ClearedAt = request.ClearedAt;
        check.RowVersion = request.RowVersion;
        check.LastModified = DateTimeOffset.UtcNow;
        check.LastModifiedBy = userId.ToString();

        var voucher = check.ReceiptVoucher;
        var revenueLines = voucher?.Lines
            .Select(l => new CheckClearedRevenueLineDetail(l.RevenueAccountId, l.Amount, l.Description))
            .ToList() ?? [];

        check.AddDomainEvent(new CheckClearedEvent
        {
            SourceEntityId = check.Id,
            OccurredAt = request.ClearedAt,
            BankName = check.BankName,
            CheckNumber = check.CheckNumber,
            Amount = check.Amount,
            RevenueLines = revenueLines
        });

        // Recalculate order & claim statuses
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
            }
        }

        context.DocumentStatusLogs.Add(new DocumentStatusLog
        {
            EntityName = nameof(Check),
            DocumentId = check.Id,
            FromStatus = CheckStatus.UnderCollection.ToString(),
            ToStatus = CheckStatus.Cleared.ToString(),
            ChangedById = userId,
            ChangedAt = DateTimeOffset.UtcNow
        });

        await context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

public class ClearCheckCommandValidator : AbstractValidator<ClearCheckCommand>
{
    public ClearCheckCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0).WithMessage("Check ID is required.");
        RuleFor(x => x.ClearedAt).NotEmpty().WithMessage("Clearing date is required.");
        RuleFor(x => x.RowVersion).NotEmpty().WithMessage("Row version is required for concurrency control.");
    }
}
