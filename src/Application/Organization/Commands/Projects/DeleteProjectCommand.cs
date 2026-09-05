using ERP_Government.Application.Common.Security;

namespace ERP_Government.Application.Organization.Commands.Projects;

// C-O011 — DeleteProjectCommand
[Authorize(Policy = PermissionCodes.ProjectsDelete)]
public class DeleteProjectCommand : IRequest<Result>
{
    public int Id { get; init; }
}

public class DeleteProjectCommandHandler(
    IApplicationDbContext context) : IRequestHandler<DeleteProjectCommand, Result>
{
    public async Task<Result> Handle(
        DeleteProjectCommand request,
        CancellationToken cancellationToken)
    {
        var entity = await context.Projects
            .FindAsync(request.Id, cancellationToken);

        if (entity is null)
            return Result.Failure(["Project not found."]);

        context.Projects.Remove(entity);
        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
