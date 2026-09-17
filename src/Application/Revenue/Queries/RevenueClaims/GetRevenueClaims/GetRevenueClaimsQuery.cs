using ERP_Government.Application.Common.Security;
using ERP_Government.Application.Revenue.Common.DTOs;
using ERP_Government.Application.Revenue.Common.Services;
using ERP_Government.Domain.Revenue.Enums;

namespace ERP_Government.Application.Revenue.Queries.RevenueClaims.GetRevenueClaims;

[Authorize(Policy = PermissionCodes.RevenueClaimsView)]
public class GetRevenueClaimsQuery : IRequest<List<RevenueClaimDto>>
{
    public int? PartyId { get; init; }
    public ClaimStatus? Status { get; init; }
}

public class GetRevenueClaimsQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetRevenueClaimsQuery, List<RevenueClaimDto>>
{
    public async Task<List<RevenueClaimDto>> Handle(
        GetRevenueClaimsQuery request,
        CancellationToken cancellationToken)
    {
        var query = context.RevenueClaims
            .AsNoTracking()
            .Include(c => c.Party)
            .Include(c => c.CollectionOrders)
                .ThenInclude(o => o.ReceiptVouchers)
                    .ThenInclude(v => v.Lines)
            .Include(c => c.CollectionOrders)
                .ThenInclude(o => o.ReceiptVouchers)
                    .ThenInclude(v => v.Checks)
            .AsQueryable();

        if (request.PartyId.HasValue)
            query = query.Where(c => c.PartyId == request.PartyId.Value);

        if (request.Status.HasValue)
            query = query.Where(c => c.Status == request.Status.Value);

        var claims = await query.OrderByDescending(c => c.Created).ToListAsync(cancellationToken);

        return claims.Select(c =>
        {
            var (collected, underColl, outstanding, available) = RevenueMetricsCalculator.CalculateClaimMetrics(c.TotalAmount, c.CollectionOrders);
            return new RevenueClaimDto
            {
                Id = c.Id,
                ClaimNumber = c.ClaimNumber,
                ClaimDate = c.ClaimDate,
                PartyId = c.PartyId,
                PartyName = c.Party?.NameAr ?? string.Empty,
                TotalAmount = c.TotalAmount,
                CollectedAmount = collected,
                UnderCollectionAmount = underColl,
                OutstandingAmount = outstanding,
                AvailableAmount = available,
                Notes = c.Notes,
                Status = c.Status,
                StatusName = c.Status.ToString(),
                RowVersion = c.RowVersion,
                Created = c.Created,
                CreatedBy = c.CreatedBy
            };
        }).ToList();
    }
}
