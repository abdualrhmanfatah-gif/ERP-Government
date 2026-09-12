using ERP_Government.Domain.Inventory.Entities;

namespace ERP_Government.Application.Inventory.Items.Queries.GetItemById;

public record GetItemByIdQuery(int Id) : IRequest<Item?>;

public class GetItemByIdQueryHandler(
    IApplicationDbContext context) : IRequestHandler<GetItemByIdQuery, Item?>
{
    public async Task<Item?> Handle(
        GetItemByIdQuery request,
        CancellationToken cancellationToken)
    {
        return await context.Items
            .Include(i => i.Category)
            .Include(i => i.Unit)
            .FirstOrDefaultAsync(i => i.Id == request.Id, cancellationToken);
    }
}
