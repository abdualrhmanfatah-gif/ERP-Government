using ERP_Government.Application.Common.Errors;
using ERP_Government.Application.Common.Models;
using ERP_Government.Application.Common.Security;
using ERP_Government.Application.FinancialSettings.Common.DTOs;

namespace ERP_Government.Application.FinancialSettings.Queries.ClosingEntries;

// T-017-013 — GetClosingEntryByIdQuery
[Authorize(Policy = PermissionCodes.ClosingEntriesView)]
public class GetClosingEntryByIdQuery : IRequest<Result<ClosingEntryDto>>
{
    public int Id { get; init; }
}

public class GetClosingEntryByIdQueryHandler(
    IApplicationDbContext context,
    IMapper mapper) : IRequestHandler<GetClosingEntryByIdQuery, Result<ClosingEntryDto>>
{
    public async Task<Result<ClosingEntryDto>> Handle(
        GetClosingEntryByIdQuery request,
        CancellationToken cancellationToken)
    {
        var entry = await context.YearEndClosingEntries
            .Include(e => e.FiscalYear)
            .Include(e => e.ReversalOf)
            .FirstOrDefaultAsync(e => e.Id == request.Id, cancellationToken);

        if (entry is null)
            return Result<ClosingEntryDto>.Failure(ErrorCodes.FinancialSettings.ClosingEntryNotFound, ErrorCategory.NotFound, $"Closing entry with ID {request.Id} not found.");

        return Result<ClosingEntryDto>.Success(mapper.Map<ClosingEntryDto>(entry));
    }
}
