using ERP_Government.Domain.Procurement.Enums;
using ERP_Government.Application.Common.Security;

namespace ERP_Government.Application.Procurement.Queries.PurchaseRequests.GetPurchaseRequests;

[Authorize(Policy = PermissionCodes.PurchaseRequestsView)]
public record GetPurchaseRequestsQuery(
    PurchaseRequestStatus? Status = null,
    PurchaseRequestPriority? Priority = null,
    string? Search = null,
    int Page = 1,
    int PageSize = 20) : IRequest<Result<PaginatedList<PurchaseRequestListItem>>>;

public record PurchaseRequestListItem(
    int Id,
    string RequestNumber,
    DateTime RequestDate,
    string RequesterName,
    PurchaseRequestPriority Priority,
    PurchaseRequestStatus Status,
    decimal? TotalEstimatedCost,
    int LineCount,
    DateTimeOffset Created);

public class GetPurchaseRequestsQueryHandler(
    IApplicationDbContext context) : IRequestHandler<GetPurchaseRequestsQuery, Result<PaginatedList<PurchaseRequestListItem>>>
{
    public async Task<Result<PaginatedList<PurchaseRequestListItem>>> Handle(
        GetPurchaseRequestsQuery request,
        CancellationToken cancellationToken)
    {
        IQueryable<Domain.Procurement.Entities.PurchaseRequest> query = context.PurchaseRequests;

        if (request.Status.HasValue)
            query = query.Where(p => p.Status == request.Status.Value);

        if (request.Priority.HasValue)
            query = query.Where(p => p.Priority == request.Priority.Value);

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToLower();
            query = query.Where(p =>
                p.RequestNumber.ToLower().Contains(search) ||
                (p.Notes != null && p.Notes.ToLower().Contains(search)));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(p => p.Created)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(p => new PurchaseRequestListItem(
                p.Id,
                p.RequestNumber,
                p.RequestDate,
                p.RequesterName,
                p.Priority,
                p.Status,
                p.TotalEstimatedCost,
                p.Details.Count,
                p.Created))
            .ToListAsync(cancellationToken);

        return Result<PaginatedList<PurchaseRequestListItem>>.Success(
            new PaginatedList<PurchaseRequestListItem>(items, totalCount, request.Page, request.PageSize));
    }
}
