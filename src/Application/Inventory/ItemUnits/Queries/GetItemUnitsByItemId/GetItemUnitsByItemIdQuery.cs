using ERP_Government.Domain.Inventory.Entities;

namespace ERP_Government.Application.Inventory.ItemUnits.Queries.GetItemUnitsByItemId;

public record GetItemUnitsByItemIdQuery(int ItemId) : IRequest<IReadOnlyList<ItemUnit>>;

public class GetItemUnitsByItemIdQueryHandler(
    IApplicationDbContext context) : IRequestHandler<GetItemUnitsByItemIdQuery, IReadOnlyList<ItemUnit>>
{
    public async Task<IReadOnlyList<ItemUnit>> Handle(
        GetItemUnitsByItemIdQuery request,
        CancellationToken cancellationToken)
    {
        return await context.ItemUnits
            .Include(iu => iu.Unit)
            .Where(iu => iu.ItemId == request.ItemId)
            .OrderByDescending(iu => iu.IsBase)
            .ThenBy(iu => iu.Unit!.Name)
            .ToListAsync(cancellationToken);
    }
}
