using ERP_Government.Application.Common.Security;

namespace ERP_Government.Application.Inventory.ItemCategories.Commands.ToggleItemCategoryActive;

[Authorize(Policy = PermissionCodes.ItemCategoriesUpdate)]
public record ToggleItemCategoryActiveCommand(int Id) : IRequest<Result>;

public class ToggleItemCategoryActiveCommandHandler(
    IApplicationDbContext context) : IRequestHandler<ToggleItemCategoryActiveCommand, Result>
{
    public async Task<Result> Handle(
        ToggleItemCategoryActiveCommand request,
        CancellationToken cancellationToken)
    {
        var entity = await context.ItemCategories.FindAsync(request.Id, cancellationToken);
        if (entity is null)
            return Result.Failure(["Category not found."]);

        entity.IsActive = !entity.IsActive;
        entity.LastModified = DateTimeOffset.UtcNow;

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
