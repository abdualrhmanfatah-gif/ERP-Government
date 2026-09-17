using ERP_Government.Application.Common.Errors;
using ERP_Government.Application.Common.Models;
using ERP_Government.Domain.Inventory.Entities;
using ERP_Government.Application.Common.Security;

namespace ERP_Government.Application.Inventory.Items.Queries.GetItemById;

[Authorize(Policy = PermissionCodes.ItemsView)]
public record GetItemByIdQuery(int Id) : IRequest<Result<Item>>;

public class GetItemByIdQueryHandler(
    IApplicationDbContext context) : IRequestHandler<GetItemByIdQuery, Result<Item>>
{
    public async Task<Result<Item>> Handle(
        GetItemByIdQuery request,
        CancellationToken cancellationToken)
    {
        var entity = await context.Items
            .Include(i => i.Category)
            .Include(i => i.Unit)
            .FirstOrDefaultAsync(i => i.Id == request.Id, cancellationToken);

        if (entity is null)
            return Result<Item>.Failure(ErrorCodes.Inventory.ItemNotFound, ErrorCategory.NotFound, $"Item with ID {request.Id} not found.");

        return Result<Item>.Success(entity);
    }
}
