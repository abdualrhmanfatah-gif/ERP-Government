using ERP_Government.Application.Common.Interfaces;
using ERP_Government.Application.Common.Security;
using ERP_Government.Domain.Budgeting.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ERP_Government.Application.Budgeting.Queries.FinancialControl.GetYearClosingRuns;

[Authorize(Policy = PermissionCodes.FinancialControlLapseYear)]
public record GetYearClosingRunsQuery(int? FiscalYearId = null)
    : IRequest<List<YearClosingRunResult>>;

public record YearClosingRunResult(
    int Id,
    int FiscalYearId,
    string FiscalYearName,
    DateTimeOffset StartedAt,
    int RunById,
    YearClosingRunType RunType,
    decimal LapsedAppropriationTotal,
    decimal LapsedEncumbranceTotal,
    YearClosingRunStatus Status);

public class GetYearClosingRunsQueryHandler(
    IApplicationDbContext context) : IRequestHandler<GetYearClosingRunsQuery, List<YearClosingRunResult>>
{
    public async Task<List<YearClosingRunResult>> Handle(
        GetYearClosingRunsQuery request,
        CancellationToken cancellationToken)
    {
        var query = context.YearClosingRuns
            .Include(r => r.FiscalYear)
            .AsQueryable();

        if (request.FiscalYearId.HasValue)
            query = query.Where(r => r.FiscalYearId == request.FiscalYearId.Value);

        return await query
            .OrderByDescending(r => r.StartedAt)
            .Select(r => new YearClosingRunResult(
                r.Id,
                r.FiscalYearId,
                r.FiscalYear.Name,
                r.StartedAt,
                r.RunById,
                r.RunType,
                r.LapsedAppropriationTotal,
                r.LapsedEncumbranceTotal,
                r.Status))
            .ToListAsync(cancellationToken);
    }
}
