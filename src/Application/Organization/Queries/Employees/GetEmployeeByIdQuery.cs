using ERP_Government.Application.Common.Security;
using ERP_Government.Application.Organization.Common.DTOs;

namespace ERP_Government.Application.Organization.Queries.Employees;

// Q-O004 — GetEmployeeByIdQuery
[Authorize(Policy = PermissionCodes.EmployeesView)]
public class GetEmployeeByIdQuery : IRequest<EmployeeDto>
{
    public int Id { get; init; }
}

public class GetEmployeeByIdQueryHandler(
    IApplicationDbContext context,
    IMapper mapper) : IRequestHandler<GetEmployeeByIdQuery, EmployeeDto>
{
    public async Task<EmployeeDto> Handle(
        GetEmployeeByIdQuery request,
        CancellationToken cancellationToken)
    {
        var entity = await context.Employees
            .Include(x => x.OrganizationalUnit)
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        return entity is null
            ? throw new ERP_Government.Application.Common.Exceptions.NotFoundException(nameof(Employee), request.Id)
            : mapper.Map<EmployeeDto>(entity);
    }
}
