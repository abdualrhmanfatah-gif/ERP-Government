using ERP_Government.Application.Common.Security;

namespace ERP_Government.Application.Budgeting.Commands.BudgetClassifications;

[Authorize(Policy = PermissionCodes.BudgetClassificationsToggleActive)]
public record ToggleBudgetClassificationActiveCommand(
    int Id,
    byte[] RowVersion) : IRequest<Result>;

public class ToggleBudgetClassificationActiveCommandHandler(
    IApplicationDbContext context) : IRequestHandler<ToggleBudgetClassificationActiveCommand, Result>
{
    public async Task<Result> Handle(
        ToggleBudgetClassificationActiveCommand request,
        CancellationToken cancellationToken)
    {
        var entity = await context.BudgetClassifications
            .FindAsync(request.Id, cancellationToken);

        if (entity is null)
            return Result.Failure(["Budget classification not found."]);

        if (!request.RowVersion.SequenceEqual(entity.RowVersion))
            return Result.Failure(["Concurrency conflict. The record has been modified by another user."]);

        entity.IsActive = !entity.IsActive;

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
