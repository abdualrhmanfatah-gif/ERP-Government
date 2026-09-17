using ERP_Government.Application.Accounting.Common;
using ERP_Government.Application.Common.Errors;
using ERP_Government.Application.Common.Models;
using ERP_Government.Application.Common.Security;

namespace ERP_Government.Application.Accounting.Queries.JournalEntries.GetJournalEntryById;

[Authorize(Policy = PermissionCodes.JournalEntriesRead)]
public class GetJournalEntryByIdQuery : IRequest<Result<JournalEntryDto>>
{
    public int Id { get; init; }
}

public class GetJournalEntryByIdQueryHandler(
    IApplicationDbContext context,
    IMapper mapper) : IRequestHandler<GetJournalEntryByIdQuery, Result<JournalEntryDto>>
{
    public async Task<Result<JournalEntryDto>> Handle(
        GetJournalEntryByIdQuery request,
        CancellationToken cancellationToken)
    {
        var entity = await context.JournalEntries
            .Include(x => x.Journal)
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (entity is null)
            return Result<JournalEntryDto>.Failure(ErrorCodes.Accounting.JournalEntryNotFound, ErrorCategory.NotFound, $"Journal entry with ID {request.Id} not found.");

        var dto = mapper.Map<JournalEntryDto>(entity);

        var lines = await context.JournalEntryLines
            .Where(x => x.JournalEntryId == request.Id)
            .Include(x => x.Account)
            .OrderBy(x => x.Sequence)
            .ToListAsync(cancellationToken);

        dto.Lines = mapper.Map<List<JournalEntryLineDto>>(lines);

        return Result<JournalEntryDto>.Success(dto);
    }
}
