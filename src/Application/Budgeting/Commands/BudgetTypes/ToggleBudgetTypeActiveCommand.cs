using ERP_Government.Application.Common.Security;

namespace ERP_Government.Application.Budgeting.Commands.BudgetTypes;

[Authorize(Policy = PermissionCodes.BudgetTypesToggleActive)]
public record ToggleBudgetTypeActiveCommand(
    int Id,
    byte[] RowVersion) : IRequest<Result>;

public class ToggleBudgetTypeActiveCommandHandler(
    IApplicationDbContext context) : IRequestHandler<ToggleBudgetTypeActiveCommand, Result>
{
    public async Task<Result> Handle(
        ToggleBudgetTypeActiveCommand request,
        CancellationToken cancellationToken)
    {
        var entity = await context.BudgetTypes
            .FindAsync(request.Id, cancellationToken);

        if (entity is null)
            return Result.Failure(["Budget type not found."]);

        if (!request.RowVersion.SequenceEqual(entity.RowVersion))
            return Result.Failure(["Concurrency conflict. The record has been modified by another user."]);

        entity.IsActive = !entity.IsActive;

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
