using ERP_Government.Domain.Inventory.Entities;

namespace ERP_Government.Application.Inventory.ItemCategories.Queries.GetItemCategoryById;

public record GetItemCategoryByIdQuery(int Id) : IRequest<ItemCategory?>;

public class GetItemCategoryByIdQueryHandler(
    IApplicationDbContext context) : IRequestHandler<GetItemCategoryByIdQuery, ItemCategory?>
{
    public async Task<ItemCategory?> Handle(
        GetItemCategoryByIdQuery request,
        CancellationToken cancellationToken)
    {
        return await context.ItemCategories
            .Include(c => c.ParentItemCategory)
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);
    }
}
