using ERP_Government.Application.Inventory.Items.Enums;
using ERP_Government.Domain.Inventory.Entities;
using ERP_Government.Application.Common.Security;

namespace ERP_Government.Application.Inventory.Items.Queries.GetItems;

[Authorize(Policy = PermissionCodes.ItemsView)]
public record GetItemsQuery(
    string? Search = null,
    int? CategoryId = null,
    int? UnitId = null,
    ItemType? ItemType = null,
    bool? IsActive = null,
    bool? UnderReorderLevel = null,
    int Page = 1,
    int PageSize = 20) : IRequest<PaginatedList<Item>>;

public class GetItemsQueryHandler(
    IApplicationDbContext context) : IRequestHandler<GetItemsQuery, PaginatedList<Item>>
{
    public async Task<PaginatedList<Item>> Handle(
        GetItemsQuery request,
        CancellationToken cancellationToken)
    {
        IQueryable<Item> query = context.Items
            .Include(i => i.Category)
            .Include(i => i.Unit);

        if (request.CategoryId.HasValue)
            query = query.Where(i => i.CategoryId == request.CategoryId.Value);

        if (request.UnitId.HasValue)
            query = query.Where(i => i.UnitId == request.UnitId.Value);

        if (request.ItemType.HasValue)
        {
            var typeName = request.ItemType.Value.ToString();
            query = query.Where(i => i.ItemType == typeName);
        }

        if (request.IsActive.HasValue)
            query = query.Where(i => i.IsActive == request.IsActive.Value);

        if (request.UnderReorderLevel == true)
            query = query.Where(i => i.AvailableQuantity.HasValue && i.ReorderLevel.HasValue && i.AvailableQuantity <= i.ReorderLevel);

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToLower();
            query = query.Where(i =>
                i.Code.ToLower().Contains(search) ||
                i.Name.ToLower().Contains(search) ||
                (i.Barcode != null && i.Barcode.ToLower().Contains(search)));
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderBy(i => i.Code)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        return new PaginatedList<Item>(items, totalCount, request.Page, request.PageSize);
    }
}
