using ERP_Government.Application.Common.Security;

namespace ERP_Government.Application.Organization.Commands.OrganizationalUnits;

// C-O003 — DeleteOrganizationalUnitCommand
[Authorize(Policy = PermissionCodes.OrgUnitsDelete)]
public class DeleteOrganizationalUnitCommand : IRequest<Result>
{
    public int Id { get; init; }
}

public class DeleteOrganizationalUnitCommandHandler(
    IApplicationDbContext context) : IRequestHandler<DeleteOrganizationalUnitCommand, Result>
{
    public async Task<Result> Handle(
        DeleteOrganizationalUnitCommand request,
        CancellationToken cancellationToken)
    {
        var entity = await context.OrganizationalUnits
            .FindAsync(request.Id, cancellationToken);

        if (entity is null)
            return Result.Failure(["Organizational unit not found."]);

        // Reject deletion when children exist (ASM-004 accepted)
        var hasChildren = await context.OrganizationalUnits
            .AnyAsync(x => x.ParentId == request.Id, cancellationToken);

        if (hasChildren)
            return Result.Failure(["Cannot delete organizational unit that has child units."]);

        // Reject deletion when employees are assigned
        var hasEmployees = await context.Employees
            .AnyAsync(x => x.OrganizationalUnitId == request.Id, cancellationToken);

        if (hasEmployees)
            return Result.Failure(["Cannot delete organizational unit that has assigned employees."]);

        context.OrganizationalUnits.Remove(entity);
        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
