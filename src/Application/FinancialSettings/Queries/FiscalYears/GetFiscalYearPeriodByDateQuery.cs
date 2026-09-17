using MediatR;
using Microsoft.EntityFrameworkCore;
using ERP_Government.Application.Common.Errors;
using ERP_Government.Application.Common.Interfaces;
using ERP_Government.Application.Common.Models;
using ERP_Government.Application.Common.Security;

namespace ERP_Government.Application.FinancialSettings.Queries.FiscalYears;

[Authorize(Policy = PermissionCodes.FiscalYearsView)]
public class GetFiscalYearPeriodByDateQuery : IRequest<Result<GetFiscalYearPeriodByDateResult>>
{
    public DateTime Date { get; set; }
}

public class GetFiscalYearPeriodByDateResult
{
    public int FiscalYearId { get; set; }
    public string? FiscalYearCode { get; set; }
    public string? FiscalYearName { get; set; }
    public int FiscalPeriodId { get; set; }
    public string? FiscalPeriodName { get; set; }
}

public class GetFiscalYearPeriodByDateQueryHandler(
    IApplicationDbContext context)
    : IRequestHandler<GetFiscalYearPeriodByDateQuery, Result<GetFiscalYearPeriodByDateResult>>
{
    public async Task<Result<GetFiscalYearPeriodByDateResult>> Handle(
        GetFiscalYearPeriodByDateQuery request,
        CancellationToken cancellationToken)
    {
        var date = DateOnly.FromDateTime(request.Date);

        var fiscalYear = await context.FiscalYears
            .Where(fy => fy.IsActive && fy.StartDate <= date && fy.EndDate >= date)
            .FirstOrDefaultAsync(cancellationToken);

        if (fiscalYear is null)
            return Result<GetFiscalYearPeriodByDateResult>.Failure(ErrorCodes.FinancialSettings.FiscalYearNotFound, ErrorCategory.NotFound, $"No active fiscal year found for date {date}.");

        var fiscalPeriod = await context.FiscalPeriods
            .Where(fp => fp.FiscalYearId == fiscalYear.Id
                      && fp.StartDate <= date && fp.EndDate >= date)
            .FirstOrDefaultAsync(cancellationToken);

        return Result<GetFiscalYearPeriodByDateResult>.Success(new GetFiscalYearPeriodByDateResult
        {
            FiscalYearId = fiscalYear.Id,
            FiscalYearCode = fiscalYear.YearNumber.ToString(),
            FiscalYearName = fiscalYear.Name,
            FiscalPeriodId = fiscalPeriod?.Id ?? 0,
            FiscalPeriodName = fiscalPeriod?.Name,
        });
    }
}
