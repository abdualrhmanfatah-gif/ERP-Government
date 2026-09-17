using ERP_Government.Application.Common.Errors;
using ERP_Government.Application.Common.Models;
using Unit = ERP_Government.Domain.Inventory.Entities.Unit;
using ERP_Government.Application.Common.Security;

namespace ERP_Government.Application.Inventory.Units.Queries.GetUnitById;

[Authorize(Policy = PermissionCodes.UnitsView)]
public record GetUnitByIdQuery(int Id) : IRequest<Result<Unit>>;

public class GetUnitByIdQueryHandler(
    IApplicationDbContext context) : IRequestHandler<GetUnitByIdQuery, Result<Unit>>
{
    public async Task<Result<Unit>> Handle(
        GetUnitByIdQuery request,
        CancellationToken cancellationToken)
    {
        var entity = await context.Units
            .Include(u => u.BaseUnit)
            .FirstOrDefaultAsync(u => u.Id == request.Id, cancellationToken);

        if (entity is null)
            return Result<Unit>.Failure(ErrorCodes.Inventory.UnitNotFound, ErrorCategory.NotFound, $"Unit with ID {request.Id} not found.");

        return Result<Unit>.Success(entity);
    }
}
