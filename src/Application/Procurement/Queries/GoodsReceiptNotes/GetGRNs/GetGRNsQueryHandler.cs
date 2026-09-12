using ERP_Government.Application.Common.Interfaces;
using ERP_Government.Domain.Procurement.Enums;

namespace ERP_Government.Application.Procurement.Queries.GoodsReceiptNotes.GetGRNs;

public class GetGRNsQueryHandler(
    IApplicationDbContext context) : IRequestHandler<GetGRNsQuery, Result<PaginatedList<GRNListItem>>>
{
    public async Task<Result<PaginatedList<GRNListItem>>> Handle(
        GetGRNsQuery request,
        CancellationToken cancellationToken)
    {
        IQueryable<Domain.Procurement.Entities.GoodsReceiptNote> query = context.GoodsReceiptNotes;

        if (request.PurchaseOrderId.HasValue)
            query = query.Where(g => g.PurchaseOrderId == request.PurchaseOrderId.Value);

        if (request.Status.HasValue)
            query = query.Where(g => g.Status == request.Status.Value);

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToLower();
            query = query.Where(g => g.GRNNumber.ToLower().Contains(search));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(g => g.Created)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(g => new GRNListItem(
                g.Id,
                g.GRNNumber,
                g.PurchaseOrderId,
                g.PurchaseOrder.PONumber,
                g.SupplierPartyId,
                g.Status,
                g.GRNDate,
                g.Created))
            .ToListAsync(cancellationToken);

        return Result<PaginatedList<GRNListItem>>.Success(
            new PaginatedList<GRNListItem>(items, totalCount, request.Page, request.PageSize));
    }
}
