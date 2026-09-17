using ERP_Government.Application.Common.Errors;
using ERP_Government.Application.Common.Models;
using ERP_Government.Domain.Inventory.Entities;
using ERP_Government.Application.Common.Security;

namespace ERP_Government.Application.Inventory.ItemCategories.Queries.GetItemCategoryById;

[Authorize(Policy = PermissionCodes.ItemCategoriesView)]
public record GetItemCategoryByIdQuery(int Id) : IRequest<Result<ItemCategory>>;

public class GetItemCategoryByIdQueryHandler(
    IApplicationDbContext context) : IRequestHandler<GetItemCategoryByIdQuery, Result<ItemCategory>>
{
    public async Task<Result<ItemCategory>> Handle(
        GetItemCategoryByIdQuery request,
        CancellationToken cancellationToken)
    {
        var entity = await context.ItemCategories
            .Include(c => c.ParentItemCategory)
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

        if (entity is null)
            return Result<ItemCategory>.Failure(ErrorCodes.Inventory.ItemCategoryNotFound, ErrorCategory.NotFound, $"Item category with ID {request.Id} not found.");

        return Result<ItemCategory>.Success(entity);
    }
}
