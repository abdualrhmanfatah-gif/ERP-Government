using ERP_Government.Domain.Inventory.Entities;

namespace ERP_Government.Application.Inventory.ItemCategories.Queries.GetItemCategories;

public record GetItemCategoriesQuery(
    string? Search = null,
    bool? IsActive = null) : IRequest<IReadOnlyList<ItemCategory>>;

public class GetItemCategoriesQueryHandler(
    IApplicationDbContext context) : IRequestHandler<GetItemCategoriesQuery, IReadOnlyList<ItemCategory>>
{
    public async Task<IReadOnlyList<ItemCategory>> Handle(
        GetItemCategoriesQuery request,
        CancellationToken cancellationToken)
    {
        IQueryable<ItemCategory> query = context.ItemCategories
            .Include(c => c.ParentItemCategory);

        if (request.IsActive.HasValue)
            query = query.Where(c => c.IsActive == request.IsActive.Value);

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToLower();
            query = query.Where(c =>
                c.Code.ToLower().Contains(search) ||
                c.Name.ToLower().Contains(search));
        }

        return await query
            .OrderBy(c => c.Code)
            .ToListAsync(cancellationToken);
    }
}
