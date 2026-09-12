using Unit = ERP_Government.Domain.Inventory.Entities.Unit;

namespace ERP_Government.Application.Inventory.Units.Queries.GetUnitById;

public record GetUnitByIdQuery(int Id) : IRequest<Unit?>;

public class GetUnitByIdQueryHandler(
    IApplicationDbContext context) : IRequestHandler<GetUnitByIdQuery, Unit?>
{
    public async Task<Unit?> Handle(
        GetUnitByIdQuery request,
        CancellationToken cancellationToken)
    {
        return await context.Units
            .Include(u => u.BaseUnit)
            .FirstOrDefaultAsync(u => u.Id == request.Id, cancellationToken);
    }
}
