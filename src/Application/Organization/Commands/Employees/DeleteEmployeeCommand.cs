using ERP_Government.Application.Common.Security;

namespace ERP_Government.Application.Organization.Commands.Employees;

// C-O005 — DeleteEmployeeCommand (soft-delete per FR-015)
[Authorize(Policy = PermissionCodes.EmployeesDelete)]
public class DeleteEmployeeCommand : IRequest<Result>
{
    public int Id { get; init; }
}

public class DeleteEmployeeCommandHandler(
    IApplicationDbContext context) : IRequestHandler<DeleteEmployeeCommand, Result>
{
    public async Task<Result> Handle(
        DeleteEmployeeCommand request,
        CancellationToken cancellationToken)
    {
        var entity = await context.Employees
            .FindAsync(request.Id, cancellationToken);

        if (entity is null)
            return Result.Failure(["Employee not found."]);

        // Soft-delete: IsActive=false (FR-015, ASM-005 — does NOT affect linked projects)
        entity.IsActive = false;

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
