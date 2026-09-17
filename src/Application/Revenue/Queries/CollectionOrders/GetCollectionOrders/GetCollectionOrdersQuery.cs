using ERP_Government.Application.Common.Security;
using ERP_Government.Application.Revenue.Common.DTOs;
using ERP_Government.Application.Revenue.Common.Services;
using ERP_Government.Domain.Revenue.Enums;

namespace ERP_Government.Application.Revenue.Queries.CollectionOrders.GetCollectionOrders;

[Authorize(Policy = PermissionCodes.CollectionOrdersView)]
public class GetCollectionOrdersQuery : IRequest<List<CollectionOrderDto>>
{
    public int? RevenueClaimId { get; init; }
    public CollectionOrderStatus? Status { get; init; }
}

public class GetCollectionOrdersQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetCollectionOrdersQuery, List<CollectionOrderDto>>
{
    public async Task<List<CollectionOrderDto>> Handle(
        GetCollectionOrdersQuery request,
        CancellationToken cancellationToken)
    {
        var query = context.CollectionOrders
            .AsNoTracking()
            .Include(o => o.RevenueClaim)
            .Include(o => o.ReceiptVouchers)
                .ThenInclude(v => v.Lines)
            .Include(o => o.ReceiptVouchers)
                .ThenInclude(v => v.Checks)
            .AsQueryable();

        if (request.RevenueClaimId.HasValue)
            query = query.Where(o => o.RevenueClaimId == request.RevenueClaimId.Value);

        if (request.Status.HasValue)
            query = query.Where(o => o.Status == request.Status.Value);

        var orders = await query.OrderByDescending(o => o.Created).ToListAsync(cancellationToken);

        return orders.Select(o =>
        {
            var (collected, underColl, outstanding, available) = RevenueMetricsCalculator.CalculateOrderMetrics(o.AuthorizedAmount, o.ReceiptVouchers);
            return new CollectionOrderDto
            {
                Id = o.Id,
                RevenueClaimId = o.RevenueClaimId,
                RevenueClaimNumber = o.RevenueClaim.ClaimNumber,
                OrderNumber = o.OrderNumber,
                OrderDate = o.OrderDate,
                AuthorizedAmount = o.AuthorizedAmount,
                CollectedAmount = collected,
                UnderCollectionAmount = underColl,
                OutstandingAmount = outstanding,
                AvailableAmount = available,
                Notes = o.Notes,
                Status = o.Status,
                StatusName = o.Status.ToString(),
                RowVersion = o.RowVersion,
                Created = o.Created,
                CreatedBy = o.CreatedBy
            };
        }).ToList();
    }
}
