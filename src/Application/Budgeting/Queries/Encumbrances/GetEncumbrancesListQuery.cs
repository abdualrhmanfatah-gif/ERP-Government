using ERP_Government.Application.Budgeting.Common;
using ERP_Government.Application.Common.Security;
using ERP_Government.Domain.Budgeting.Enums;

namespace ERP_Government.Application.Budgeting.Queries.Encumbrances;

[Authorize(Policy = PermissionCodes.EncumbrancesView)]
public record GetEncumbrancesListQuery(
    int? AppropriationId = null,
    EncumbranceType? Type = null,
    EncumbranceStatus? Status = null,
    DateOnly? DateFrom = null,
    DateOnly? DateTo = null,
    bool? IsReversed = null) : IRequest<List<EncumbranceListItemDto>>;

public class GetEncumbrancesListQueryHandler(
    IApplicationDbContext context,
    IMapper mapper) : IRequestHandler<GetEncumbrancesListQuery, List<EncumbranceListItemDto>>
{
    public async Task<List<EncumbranceListItemDto>> Handle(
        GetEncumbrancesListQuery request,
        CancellationToken cancellationToken)
    {
        var query = context.Encumbrances.AsQueryable();

        if (request.AppropriationId.HasValue)
            query = query.Where(e => e.AppropriationId == request.AppropriationId.Value);

        if (request.Type.HasValue)
            query = query.Where(e => e.EncumbranceType == request.Type.Value);

        if (request.Status.HasValue)
            query = query.Where(e => e.Status == request.Status.Value);

        if (request.DateFrom.HasValue)
            query = query.Where(e => e.EncumbranceDate >= request.DateFrom.Value);

        if (request.DateTo.HasValue)
            query = query.Where(e => e.EncumbranceDate <= request.DateTo.Value);

        if (request.IsReversed.HasValue && request.IsReversed.Value)
            query = query.Where(e => context.Encumbrances.Any(r => r.ReversalOfId == e.Id));

        var items = await query
            .OrderByDescending(e => e.Created)
            .ToListAsync(cancellationToken);

        var dtos = mapper.Map<List<EncumbranceListItemDto>>(items);

        if (request.IsReversed.HasValue)
        {
            var reversedIds = await context.Encumbrances
                .Where(r => r.ReversalOfId != null)
                .Select(r => r.ReversalOfId!.Value)
                .ToListAsync(cancellationToken);

            foreach (var dto in dtos)
                dto.IsReversed = reversedIds.Contains(dto.Id);

            if (!request.IsReversed.Value)
                dtos = dtos.Where(d => !d.IsReversed).ToList();
        }

        return dtos;
    }
}
