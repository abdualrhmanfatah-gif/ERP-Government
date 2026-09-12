using ERP_Government.Application.Procurement.Queries.PurchaseRequests.GetPurchaseRequests;
using ERP_Government.Domain.Procurement.Enums;

namespace ERP_Government.Application.Procurement.Queries.Quotations.GetQuotations;

public class GetQuotationsQueryHandler(
    IApplicationDbContext context) : IRequestHandler<GetQuotationsQuery, Result<PaginatedList<QuotationListItem>>>
{
    public async Task<Result<PaginatedList<QuotationListItem>>> Handle(
        GetQuotationsQuery request,
        CancellationToken cancellationToken)
    {
        IQueryable<Domain.Procurement.Entities.Quotation> query = context.Quotations;

        if (request.SupplierPartyId.HasValue)
            query = query.Where(q => q.SupplierPartyId == request.SupplierPartyId.Value);

        if (request.Status.HasValue)
            query = query.Where(q => q.Status == request.Status.Value);

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToLower();
            query = query.Where(q =>
                q.QuotationNumber.ToLower().Contains(search));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(q => q.Created)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(q => new QuotationListItem(
                q.Id,
                q.QuotationNumber,
                q.SupplierPartyId,
                q.QuotationDate,
                q.Status,
                q.GrandTotal,
                q.Created))
            .ToListAsync(cancellationToken);

        return Result<PaginatedList<QuotationListItem>>.Success(
            new PaginatedList<QuotationListItem>(items, totalCount, request.Page, request.PageSize));
    }
}
