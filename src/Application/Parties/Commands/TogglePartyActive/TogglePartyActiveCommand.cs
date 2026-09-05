using ERP_Government.Application.Common.Security;

namespace ERP_Government.Application.Parties.Commands.TogglePartyActive;

[Authorize(Policy = PermissionCodes.PartiesUpdate)]
public record TogglePartyActiveCommand(int Id) : IRequest<Result>;

public class TogglePartyActiveCommandHandler(
    IApplicationDbContext context) : IRequestHandler<TogglePartyActiveCommand, Result>
{
    public async Task<Result> Handle(
        TogglePartyActiveCommand request,
        CancellationToken cancellationToken)
    {
        var entity = await context.Parties.FindAsync(request.Id, cancellationToken);
        if (entity is null)
            return Result.Failure(["Party not found."]);

        entity.IsActive = !entity.IsActive;
        entity.LastModified = DateTimeOffset.UtcNow;

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
