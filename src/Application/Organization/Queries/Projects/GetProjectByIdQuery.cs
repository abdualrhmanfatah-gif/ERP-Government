using ERP_Government.Application.Common.Errors;
using ERP_Government.Application.Common.Models;
using ERP_Government.Application.Common.Security;
using ERP_Government.Application.Organization.Common.DTOs;

namespace ERP_Government.Application.Organization.Queries.Projects;

// Q-O008 — GetProjectByIdQuery
[Authorize(Policy = PermissionCodes.ProjectsView)]
public class GetProjectByIdQuery : IRequest<Result<ProjectDto>>
{
    public int Id { get; init; }
}

public class GetProjectByIdQueryHandler(
    IApplicationDbContext context,
    IMapper mapper) : IRequestHandler<GetProjectByIdQuery, Result<ProjectDto>>
{
    public async Task<Result<ProjectDto>> Handle(
        GetProjectByIdQuery request,
        CancellationToken cancellationToken)
    {
        var entity = await context.Projects
            .Include(x => x.CostCenter)
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (entity is null)
            return Result<ProjectDto>.Failure(ErrorCodes.Organization.ProjectNotFound, ErrorCategory.NotFound, $"Project with ID {request.Id} not found.");

        return Result<ProjectDto>.Success(mapper.Map<ProjectDto>(entity));
    }
}
