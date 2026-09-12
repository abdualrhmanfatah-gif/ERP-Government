using ERP_Government.Application.Common.Interfaces;
using ERP_Government.Domain.Procurement.Enums;

namespace ERP_Government.Application.Procurement.Queries.SupplierInvoices.GetSupplierInvoices;

public class GetSupplierInvoicesQueryHandler(
    IApplicationDbContext context) : IRequestHandler<GetSupplierInvoicesQuery, Result<PaginatedList<SupplierInvoiceListItem>>>
{
    public async Task<Result<PaginatedList<SupplierInvoiceListItem>>> Handle(
        GetSupplierInvoicesQuery request,
        CancellationToken cancellationToken)
    {
        IQueryable<Domain.Procurement.Entities.SupplierInvoice> query = context.SupplierInvoices;

        if (request.PurchaseOrderId.HasValue)
            query = query.Where(i => i.PurchaseOrderId == request.PurchaseOrderId.Value);

        if (request.Status.HasValue)
            query = query.Where(i => i.Status == request.Status.Value);

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToLower();
            query = query.Where(i =>
                i.InvoiceNumber.ToLower().Contains(search) ||
                i.SupplierInvoiceNumber.ToLower().Contains(search));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(i => i.Created)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(i => new SupplierInvoiceListItem(
                i.Id,
                i.InvoiceNumber,
                i.SupplierInvoiceNumber,
                i.PurchaseOrderId,
                i.PurchaseOrder.PONumber,
                i.SupplierPartyId,
                i.InvoiceDate,
                i.Status,
                i.GrandTotal,
                i.Created))
            .ToListAsync(cancellationToken);

        return Result<PaginatedList<SupplierInvoiceListItem>>.Success(
            new PaginatedList<SupplierInvoiceListItem>(items, totalCount, request.Page, request.PageSize));
    }
}
