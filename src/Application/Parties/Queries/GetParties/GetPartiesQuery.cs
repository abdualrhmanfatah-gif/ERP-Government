using ERP_Government.Domain.Parties.Entities;
using ERP_Government.Domain.Parties.Enums;

namespace ERP_Government.Application.Parties.Queries.GetParties;

public record GetPartiesQuery(
    PartyType? PartyType = null,
    bool? IsActive = null,
    string? Search = null) : IRequest<IReadOnlyList<Party>>;

public class GetPartiesQueryHandler(
    IApplicationDbContext context) : IRequestHandler<GetPartiesQuery, IReadOnlyList<Party>>
{
    public async Task<IReadOnlyList<Party>> Handle(
        GetPartiesQuery request,
        CancellationToken cancellationToken)
    {
        IQueryable<Party> query = context.Parties;

        if (request.PartyType.HasValue)
            query = query.Where(p => p.PartyType == request.PartyType.Value);

        if (request.IsActive.HasValue)
            query = query.Where(p => p.IsActive == request.IsActive.Value);

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToLower();
            query = query.Where(p =>
                p.NameAr.ToLower().Contains(search) ||
                (p.NameEn != null && p.NameEn.ToLower().Contains(search)) ||
                (p.TaxNumber != null && p.TaxNumber.ToLower().Contains(search)));
        }

        return await query
            .OrderBy(p => p.NameAr)
            .ToListAsync(cancellationToken);
    }
}
