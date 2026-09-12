using ERP_Government.Application.Common.Security;

namespace ERP_Government.Application.Inventory.Units.Commands.ToggleUnitActive;

[Authorize(Policy = PermissionCodes.UnitsUpdate)]
public record ToggleUnitActiveCommand(int Id) : IRequest<Result>;

public class ToggleUnitActiveCommandHandler(
    IApplicationDbContext context) : IRequestHandler<ToggleUnitActiveCommand, Result>
{
    public async Task<Result> Handle(
        ToggleUnitActiveCommand request,
        CancellationToken cancellationToken)
    {
        var entity = await context.Units.FindAsync(request.Id, cancellationToken);
        if (entity is null)
            return Result.Failure(["Unit not found."]);

        entity.IsActive = !entity.IsActive;
        entity.LastModified = DateTimeOffset.UtcNow;

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
