using ERP_Government.Application.Common.Security;
using ERP_Government.Application.FinancialSettings.Common.DTOs;

namespace ERP_Government.Application.FinancialSettings.Queries.DocumentSequences.GetDocumentSequenceById;

[Authorize(Policy = PermissionCodes.DocumentSequencesView)]
public class GetDocumentSequenceByIdQuery : IRequest<DocumentSequenceDto?>
{
    public int Id { get; init; }
}

public class GetDocumentSequenceByIdQueryHandler(
    IApplicationDbContext context,
    IMapper mapper) : IRequestHandler<GetDocumentSequenceByIdQuery, DocumentSequenceDto?>
{
    public async Task<DocumentSequenceDto?> Handle(
        GetDocumentSequenceByIdQuery request,
        CancellationToken cancellationToken)
    {
        var entity = await context.DocumentSequences
            .FindAsync(request.Id, cancellationToken);

        return entity is null ? null : mapper.Map<DocumentSequenceDto>(entity);
    }
}
