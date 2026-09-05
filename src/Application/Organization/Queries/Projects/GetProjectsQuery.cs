using ERP_Government.Application.Common.Security;
using ERP_Government.Application.Organization.Common.DTOs;

namespace ERP_Government.Application.Organization.Queries.Projects;

// Q-O007 — GetProjectsQuery
[Authorize(Policy = PermissionCodes.ProjectsView)]
public class GetProjectsQuery : IRequest<List<ProjectDto>>
{
    public bool? IsActive { get; init; }
}

public class GetProjectsQueryHandler(
    IApplicationDbContext context,
    IMapper mapper) : IRequestHandler<GetProjectsQuery, List<ProjectDto>>
{
    public async Task<List<ProjectDto>> Handle(
        GetProjectsQuery request,
        CancellationToken cancellationToken)
    {
        var query = context.Projects
            .Include(x => x.CostCenter)
            .AsQueryable();

        if (request.IsActive.HasValue)
            query = query.Where(x => x.IsActive == request.IsActive.Value);

        var items = await query
            .OrderBy(x => x.Code)
            .ToListAsync(cancellationToken);

        return mapper.Map<List<ProjectDto>>(items);
    }
}
