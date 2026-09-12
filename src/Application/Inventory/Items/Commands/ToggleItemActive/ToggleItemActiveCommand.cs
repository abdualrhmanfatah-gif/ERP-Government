using ERP_Government.Application.Common.Security;

namespace ERP_Government.Application.Inventory.Items.Commands.ToggleItemActive;

[Authorize(Policy = PermissionCodes.ItemsUpdate)]
public record ToggleItemActiveCommand(int Id) : IRequest<Result>;

public class ToggleItemActiveCommandHandler(
    IApplicationDbContext context) : IRequestHandler<ToggleItemActiveCommand, Result>
{
    public async Task<Result> Handle(
        ToggleItemActiveCommand request,
        CancellationToken cancellationToken)
    {
        var entity = await context.Items.FindAsync(request.Id, cancellationToken);
        if (entity is null)
            return Result.Failure(["Item not found."]);

        entity.IsActive = !entity.IsActive;
        entity.LastModified = DateTimeOffset.UtcNow;

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
