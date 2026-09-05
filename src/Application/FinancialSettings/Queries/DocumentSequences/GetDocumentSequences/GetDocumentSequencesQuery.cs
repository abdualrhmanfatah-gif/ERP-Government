using ERP_Government.Application.Common.Security;
using ERP_Government.Application.FinancialSettings.Common.DTOs;

namespace ERP_Government.Application.FinancialSettings.Queries.DocumentSequences.GetDocumentSequences;

[Authorize(Policy = PermissionCodes.DocumentSequencesView)]
public class GetDocumentSequencesQuery : IRequest<List<DocumentSequenceDto>>
{
    public bool? IsActive { get; init; }
}

public class GetDocumentSequencesQueryHandler(
    IApplicationDbContext context,
    IMapper mapper) : IRequestHandler<GetDocumentSequencesQuery, List<DocumentSequenceDto>>
{
    public async Task<List<DocumentSequenceDto>> Handle(
        GetDocumentSequencesQuery request,
        CancellationToken cancellationToken)
    {
        var query = context.DocumentSequences.AsQueryable();

        if (request.IsActive.HasValue)
            query = query.Where(x => x.IsActive == request.IsActive.Value);

        var items = await query
            .OrderBy(x => x.Name)
            .ToListAsync(cancellationToken);

        return mapper.Map<List<DocumentSequenceDto>>(items);
    }
}
