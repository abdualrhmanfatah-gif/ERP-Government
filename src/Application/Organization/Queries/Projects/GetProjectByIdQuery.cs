using ERP_Government.Application.Common.Security;
using ERP_Government.Application.Organization.Common.DTOs;

namespace ERP_Government.Application.Organization.Queries.Projects;

// Q-O008 — GetProjectByIdQuery
[Authorize(Policy = PermissionCodes.ProjectsView)]
public class GetProjectByIdQuery : IRequest<ProjectDto>
{
    public int Id { get; init; }
}

public class GetProjectByIdQueryHandler(
    IApplicationDbContext context,
    IMapper mapper) : IRequestHandler<GetProjectByIdQuery, ProjectDto>
{
    public async Task<ProjectDto> Handle(
        GetProjectByIdQuery request,
        CancellationToken cancellationToken)
    {
        var entity = await context.Projects
            .Include(x => x.CostCenter)
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        return entity is null
            ? throw new ERP_Government.Application.Common.Exceptions.NotFoundException(nameof(Project), request.Id)
            : mapper.Map<ProjectDto>(entity);
    }
}
