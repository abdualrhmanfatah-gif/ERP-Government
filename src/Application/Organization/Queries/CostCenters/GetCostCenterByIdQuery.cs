using ERP_Government.Application.Common.Security;
using ERP_Government.Application.Organization.Common.DTOs;

namespace ERP_Government.Application.Organization.Queries.CostCenters;

// Q-O006 — GetCostCenterByIdQuery
[Authorize(Policy = PermissionCodes.CostCentersView)]
public class GetCostCenterByIdQuery : IRequest<CostCenterDto>
{
    public int Id { get; init; }
}

public class GetCostCenterByIdQueryHandler(
    IApplicationDbContext context,
    IMapper mapper) : IRequestHandler<GetCostCenterByIdQuery, CostCenterDto>
{
    public async Task<CostCenterDto> Handle(
        GetCostCenterByIdQuery request,
        CancellationToken cancellationToken)
    {
        var entity = await context.CostCenters
            .Include(x => x.OrganizationUnit)
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        return entity is null
            ? throw new ERP_Government.Application.Common.Exceptions.NotFoundException(nameof(CostCenter), request.Id)
            : mapper.Map<CostCenterDto>(entity);
    }
}
