using ERP_Government.Application.Common.Errors;
using ERP_Government.Application.Common.Models;
using ERP_Government.Domain.Inventory.Entities;
using ERP_Government.Application.Common.Security;

namespace ERP_Government.Application.Inventory.Locations.Queries.GetLocationById;

[Authorize(Policy = PermissionCodes.LocationsView)]
public record GetLocationByIdQuery(int Id) : IRequest<Result<Location>>;

public class GetLocationByIdQueryHandler(
    IApplicationDbContext context) : IRequestHandler<GetLocationByIdQuery, Result<Location>>
{
    public async Task<Result<Location>> Handle(
        GetLocationByIdQuery request,
        CancellationToken cancellationToken)
    {
        var entity = await context.Locations
            .Include(l => l.ParentLocation)
            .FirstOrDefaultAsync(l => l.Id == request.Id, cancellationToken);

        if (entity is null)
            return Result<Location>.Failure(
                ErrorCodes.Inventory.LocationNotFound,
                ErrorCategory.NotFound,
                $"Location with ID {request.Id} not found.");

        return Result<Location>.Success(entity);
    }
}
