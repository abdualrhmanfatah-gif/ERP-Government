using ERP_Government.Application.Budgeting.Common;
using ERP_Government.Application.Common.Security;

namespace ERP_Government.Application.Budgeting.Queries.Encumbrances;

[Authorize(Policy = PermissionCodes.EncumbrancesView)]
public record GetEncumbranceByIdQuery(int Id) : IRequest<EncumbranceDetailDto?>;

public class GetEncumbranceByIdQueryHandler(
    IApplicationDbContext context,
    IMapper mapper) : IRequestHandler<GetEncumbranceByIdQuery, EncumbranceDetailDto?>
{
    public async Task<EncumbranceDetailDto?> Handle(
        GetEncumbranceByIdQuery request,
        CancellationToken cancellationToken)
    {
        var entity = await context.Encumbrances
            .FirstOrDefaultAsync(e => e.Id == request.Id, cancellationToken);

        if (entity is null)
            return null;

        var dto = mapper.Map<EncumbranceDetailDto>(entity);

        dto.IsReversed = await context.Encumbrances
            .AnyAsync(r => r.ReversalOfId == entity.Id, cancellationToken);

        var lines = await context.EncumbranceLines
            .Where(l => l.EncumbranceId == entity.Id)
            .Select(l => new EncumbranceLineDto(
                l.Id, l.BudgetItemId, l.Amount, l.LiquidatedAmount, l.CancelledAmount, l.Description))
            .ToListAsync(cancellationToken);

        dto.Lines = lines;

        return dto;
    }
}
