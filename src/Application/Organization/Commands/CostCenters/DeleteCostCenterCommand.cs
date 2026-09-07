using ERP_Government.Application.Common.Security;

namespace ERP_Government.Application.Organization.Commands.CostCenters;

// C-O008 — DeleteCostCenterCommand
[Authorize(Policy = PermissionCodes.CostCentersDelete)]
public class DeleteCostCenterCommand : IRequest<Result>
{
    public int Id { get; init; }
}

public class DeleteCostCenterCommandHandler(
    IApplicationDbContext context) : IRequestHandler<DeleteCostCenterCommand, Result>
{
    public async Task<Result> Handle(
        DeleteCostCenterCommand request,
        CancellationToken cancellationToken)
    {
        var entity = await context.CostCenters
            .FindAsync(request.Id, cancellationToken);

        if (entity is null)
            return Result.Failure(["Cost center not found."]);

        // Reject when projects reference it
        var hasProjects = await context.Projects
            .AnyAsync(x => x.CostCenterId == request.Id, cancellationToken);

        if (hasProjects)
            return Result.Failure(["Cannot delete cost center that has associated projects."]);

        // Accounts no longer reference cost centers directly (CostCenterAccounts join table removed — DEP-026)

        context.CostCenters.Remove(entity);
        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
