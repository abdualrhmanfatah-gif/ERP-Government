using ERP_Government.Application.Common.Errors;
using ERP_Government.Application.Common.Models;
using ERP_Government.Application.Common.Security;
using ERP_Government.Application.Organization.Common.DTOs;

namespace ERP_Government.Application.Organization.Queries.CostCenters;

// Q-O006 — GetCostCenterByIdQuery
[Authorize(Policy = PermissionCodes.CostCentersView)]
public class GetCostCenterByIdQuery : IRequest<Result<CostCenterDto>>
{
    public int Id { get; init; }
}

public class GetCostCenterByIdQueryHandler(
    IApplicationDbContext context,
    IMapper mapper) : IRequestHandler<GetCostCenterByIdQuery, Result<CostCenterDto>>
{
    public async Task<Result<CostCenterDto>> Handle(
        GetCostCenterByIdQuery request,
        CancellationToken cancellationToken)
    {
        var entity = await context.CostCenters
            .Include(x => x.OrganizationUnit)
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (entity is null)
            return Result<CostCenterDto>.Failure(ErrorCodes.Organization.CostCenterNotFound, ErrorCategory.NotFound, $"Cost center with ID {request.Id} not found.");

        return Result<CostCenterDto>.Success(mapper.Map<CostCenterDto>(entity));
    }
}
