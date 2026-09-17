using ERP_Government.Application.Common.Errors;
using ERP_Government.Application.Common.Models;
using ERP_Government.Application.Common.Security;
using ERP_Government.Application.FinancialSettings.Common.DTOs;

namespace ERP_Government.Application.FinancialSettings.Queries.DocumentSequences.GetDocumentSequenceById;

[Authorize(Policy = PermissionCodes.DocumentSequencesView)]
public class GetDocumentSequenceByIdQuery : IRequest<Result<DocumentSequenceDto>>
{
    public int Id { get; init; }
}

public class GetDocumentSequenceByIdQueryHandler(
    IApplicationDbContext context,
    IMapper mapper) : IRequestHandler<GetDocumentSequenceByIdQuery, Result<DocumentSequenceDto>>
{
    public async Task<Result<DocumentSequenceDto>> Handle(
        GetDocumentSequenceByIdQuery request,
        CancellationToken cancellationToken)
    {
        var entity = await context.DocumentSequences
            .FindAsync(request.Id, cancellationToken);

        if (entity is null)
            return Result<DocumentSequenceDto>.Failure(ErrorCodes.FinancialSettings.DocumentSequenceNotFound, ErrorCategory.NotFound, $"Document sequence with ID {request.Id} not found.");

        return Result<DocumentSequenceDto>.Success(mapper.Map<DocumentSequenceDto>(entity));
    }
}
