using ERP_Government.Application.Common.Security;

namespace ERP_Government.Application.Budgeting.Commands.Funds;

[Authorize(Policy = PermissionCodes.FundsToggleActive)]
public record ToggleFundActiveCommand(
    int Id,
    byte[] RowVersion) : IRequest<Result>;

public class ToggleFundActiveCommandHandler(
    IApplicationDbContext context) : IRequestHandler<ToggleFundActiveCommand, Result>
{
    public async Task<Result> Handle(
        ToggleFundActiveCommand request,
        CancellationToken cancellationToken)
    {
        var entity = await context.Funds
            .FindAsync(request.Id, cancellationToken);

        if (entity is null)
            return Result.Failure(["Fund not found."]);

        if (!request.RowVersion.SequenceEqual(entity.RowVersion))
            return Result.Failure(["Concurrency conflict. The record has been modified by another user."]);

        entity.IsActive = !entity.IsActive;

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
