using ERP_Government.Application.Accounting.Common;
using ERP_Government.Application.Common.Errors;
using ERP_Government.Application.Common.Models;
using ERP_Government.Application.Common.Security;

namespace ERP_Government.Application.Accounting.Queries.RecurringEntries.GetRecurringEntryById;

[Authorize(Policy = PermissionCodes.RecurringEntriesRead)]
public class GetRecurringEntryByIdQuery : IRequest<Result<RecurringEntryDto>>
{
    public int Id { get; init; }
}

public class GetRecurringEntryByIdQueryHandler(
    IApplicationDbContext context,
    IMapper mapper) : IRequestHandler<GetRecurringEntryByIdQuery, Result<RecurringEntryDto>>
{
    public async Task<Result<RecurringEntryDto>> Handle(
        GetRecurringEntryByIdQuery request,
        CancellationToken cancellationToken)
    {
        var entity = await context.RecurringEntries
            .Include(x => x.Template)
            .Include(x => x.Journal)
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (entity is null)
            return Result<RecurringEntryDto>.Failure(ErrorCodes.Accounting.RecurringEntryNotFound, ErrorCategory.NotFound, $"Recurring entry with ID {request.Id} not found.");

        return Result<RecurringEntryDto>.Success(mapper.Map<RecurringEntryDto>(entity));
    }
}
