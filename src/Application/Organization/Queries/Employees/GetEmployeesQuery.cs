using ERP_Government.Application.Common.Security;
using ERP_Government.Application.Organization.Common.DTOs;

namespace ERP_Government.Application.Organization.Queries.Employees;

// Q-O002 — GetEmployeesQuery
[Authorize(Policy = PermissionCodes.EmployeesView)]
public class GetEmployeesQuery : IRequest<List<EmployeeDto>>
{
    public bool? IsActive { get; init; }
    public int? OrganizationalUnitId { get; init; }
}

public class GetEmployeesQueryHandler(
    IApplicationDbContext context,
    IMapper mapper) : IRequestHandler<GetEmployeesQuery, List<EmployeeDto>>
{
    public async Task<List<EmployeeDto>> Handle(
        GetEmployeesQuery request,
        CancellationToken cancellationToken)
    {
        var query = context.Employees
            .Include(x => x.OrganizationalUnit)
            .AsQueryable();

        if (request.IsActive.HasValue)
            query = query.Where(x => x.IsActive == request.IsActive.Value);

        if (request.OrganizationalUnitId.HasValue)
            query = query.Where(x => x.OrganizationalUnitId == request.OrganizationalUnitId.Value);

        var items = await query
            .OrderBy(x => x.EmployeeNumber)
            .ToListAsync(cancellationToken);

        return mapper.Map<List<EmployeeDto>>(items);
    }
}
