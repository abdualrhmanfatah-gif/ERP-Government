using ERP_Government.Application.Common.Interfaces;
using ERP_Government.Domain.Procurement.Enums;
using Microsoft.EntityFrameworkCore;

namespace ERP_Government.Application.Procurement.Queries.PurchaseOrders.GetPurchaseOrders;

public class GetPurchaseOrdersQueryHandler(
    IApplicationDbContext context) : IRequestHandler<GetPurchaseOrdersQuery, Result<PaginatedList<PurchaseOrderListItem>>>
{
    public async Task<Result<PaginatedList<PurchaseOrderListItem>>> Handle(
        GetPurchaseOrdersQuery request,
        CancellationToken cancellationToken)
    {
        IQueryable<Domain.Procurement.Entities.PurchaseOrder> baseQuery = context.PurchaseOrders;

        if (request.PurchaseRequestId.HasValue)
            baseQuery = baseQuery.Where(po => po.PurchaseRequestId == request.PurchaseRequestId.Value);

        if (request.QuotationId.HasValue)
            baseQuery = baseQuery.Where(po => po.QuotationId == request.QuotationId.Value);

        if (request.SupplierPartyId.HasValue)
            baseQuery = baseQuery.Where(po => po.SupplierPartyId == request.SupplierPartyId.Value);

        if (request.Status.HasValue)
            baseQuery = baseQuery.Where(po => po.Status == request.Status.Value);

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToLower();
            baseQuery = baseQuery.Where(po =>
                po.PONumber.ToLower().Contains(search));
        }

        if (request.ExpectedDeliveryDateFrom.HasValue)
            baseQuery = baseQuery.Where(po => po.ExpectedDeliveryDate >= request.ExpectedDeliveryDateFrom.Value);

        if (request.ExpectedDeliveryDateTo.HasValue)
            baseQuery = baseQuery.Where(po => po.ExpectedDeliveryDate <= request.ExpectedDeliveryDateTo.Value);

        var totalCount = await baseQuery.CountAsync(cancellationToken);

        var items = await (from po in baseQuery
                           join p in context.Parties on po.SupplierPartyId equals p.Id into partyGroup
                           from p in partyGroup.DefaultIfEmpty()
                           orderby po.Created descending
                           select new PurchaseOrderListItem(
                               po.Id,
                               po.PONumber,
                               po.PurchaseRequestId ?? 0,
                               po.SupplierPartyId,
                               p != null ? p.NameAr : null,
                               po.Status,
                               po.GrandTotal,
                               po.ExpectedDeliveryDate,
                               po.Created))
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        return Result<PaginatedList<PurchaseOrderListItem>>.Success(
            new PaginatedList<PurchaseOrderListItem>(items, totalCount, request.Page, request.PageSize));
    }
}
