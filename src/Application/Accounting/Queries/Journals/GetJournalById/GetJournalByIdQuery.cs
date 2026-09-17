using ERP_Government.Application.Accounting.Common;
using ERP_Government.Application.Common.Errors;
using ERP_Government.Application.Common.Models;
using ERP_Government.Application.Common.Security;

namespace ERP_Government.Application.Accounting.Queries.Journals.GetJournalById;

[Authorize(Policy = PermissionCodes.JournalsRead)]
public class GetJournalByIdQuery : IRequest<Result<JournalDto>>
{
    public int Id { get; init; }
}

public class GetJournalByIdQueryHandler(
    IApplicationDbContext context,
    IMapper mapper) : IRequestHandler<GetJournalByIdQuery, Result<JournalDto>>
{
    public async Task<Result<JournalDto>> Handle(
        GetJournalByIdQuery request,
        CancellationToken cancellationToken)
    {
        var entity = await context.Journals
            .FindAsync(request.Id, cancellationToken);

        if (entity is null)
            return Result<JournalDto>.Failure(ErrorCodes.Accounting.JournalNotFound, ErrorCategory.NotFound, $"Journal with ID {request.Id} not found.");

        return Result<JournalDto>.Success(mapper.Map<JournalDto>(entity));
    }
}
