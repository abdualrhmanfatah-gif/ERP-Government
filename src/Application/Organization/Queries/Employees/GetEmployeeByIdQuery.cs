using ERP_Government.Application.Common.Errors;
using ERP_Government.Application.Common.Models;
using ERP_Government.Application.Common.Security;
using ERP_Government.Application.Organization.Common.DTOs;

namespace ERP_Government.Application.Organization.Queries.Employees;

// Q-O004 — GetEmployeeByIdQuery
[Authorize(Policy = PermissionCodes.EmployeesView)]
public class GetEmployeeByIdQuery : IRequest<Result<EmployeeDto>>
{
    public int Id { get; init; }
}

public class GetEmployeeByIdQueryHandler(
    IApplicationDbContext context,
    IMapper mapper) : IRequestHandler<GetEmployeeByIdQuery, Result<EmployeeDto>>
{
    public async Task<Result<EmployeeDto>> Handle(
        GetEmployeeByIdQuery request,
        CancellationToken cancellationToken)
    {
        var entity = await context.Employees
            .Include(x => x.OrganizationalUnit)
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (entity is null)
            return Result<EmployeeDto>.Failure(ErrorCodes.Organization.EmployeeNotFound, ErrorCategory.NotFound, $"Employee with ID {request.Id} not found.");

        return Result<EmployeeDto>.Success(mapper.Map<EmployeeDto>(entity));
    }
}
