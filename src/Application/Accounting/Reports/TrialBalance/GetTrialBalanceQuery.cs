using ERP_Government.Application.Accounting.Reports.Common;
using ERP_Government.Application.Common.Security;
using ERP_Government.Domain.Accounting.Enums;
using Microsoft.EntityFrameworkCore;

namespace ERP_Government.Application.Accounting.Reports.TrialBalance;

[Authorize(Policy = PermissionCodes.ViewTrialBalance)]
public class GetTrialBalanceQuery : IRequest<TrialBalanceDto>
{
    public int FiscalYearId { get; init; }

    public int FiscalPeriodId { get; init; }
}

public class GetTrialBalanceQueryHandler(
    IApplicationDbContext context,
    ReportAuditService auditService) : IRequestHandler<GetTrialBalanceQuery, TrialBalanceDto>
{
    public async Task<TrialBalanceDto> Handle(
        GetTrialBalanceQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            var balances = await context.AccountBalances
                .Include(x => x.Account)
                    .ThenInclude(a => a.AccountGroup)
                .Include(x => x.FiscalYear)
                .Include(x => x.FiscalPeriod)
                .Include(x => x.Currency)
                .Where(x => x.FiscalYearId == request.FiscalYearId
                         && x.FiscalPeriodId == request.FiscalPeriodId)
                .OrderBy(x => x.Account.Code)
                .ToListAsync(cancellationToken);

            var currencyCode = balances.FirstOrDefault()?.Currency?.Code ?? "YER";
            var fiscalYearName = balances.FirstOrDefault()?.FiscalYear?.Name ?? string.Empty;
            var periodName = balances.FirstOrDefault()?.FiscalPeriod?.Name ?? string.Empty;

            var sections = balances
                .GroupBy(x => x.Account.AccountGroup)
                .OrderBy(g => g.Key.Code)
                .Select(g => new ReportSection
                {
                    Title = g.Key.Name,
                    TitleEn = g.Key.Name,
                    Lines = g.Select(x => new ReportLine
                    {
                        AccountCode = x.Account.Code,
                        AccountName = x.Account.Name,
                        Debit = x.ClosingDebit,
                        Credit = x.ClosingCredit,
                        Balance = x.ClosingDebit - x.ClosingCredit,
                    }).ToList(),
                    Total = g.Sum(x => x.ClosingDebit - x.ClosingCredit),
                })
                .ToList();

            var totalDebit = balances.Sum(x => x.ClosingDebit);
            var totalCredit = balances.Sum(x => x.ClosingCredit);

            var result = new TrialBalanceDto
            {
                FiscalYearId = request.FiscalYearId,
                FiscalYearName = fiscalYearName,
                FiscalPeriodId = request.FiscalPeriodId,
                PeriodName = periodName,
                Sections = sections,
                TotalDebit = totalDebit,
                TotalCredit = totalCredit,
                IsBalanced = totalDebit == totalCredit,
                Currency = currencyCode,
                GeneratedAt = DateTimeOffset.UtcNow,
            };

            await auditService.LogAsync("TrialBalance", request, "Screen", true, cancellationToken: cancellationToken);
            return result;
        }
        catch (Exception ex)
        {
            await auditService.LogAsync("TrialBalance", request, "Screen", false, ex.Message, cancellationToken);
            throw;
        }
    }
}

public class GetTrialBalanceQueryValidator : AbstractValidator<GetTrialBalanceQuery>
{
    public GetTrialBalanceQueryValidator()
    {
        RuleFor(x => x.FiscalYearId)
            .GreaterThan(0)
            .WithMessage("Invalid fiscal year ID.");

        RuleFor(x => x.FiscalPeriodId)
            .GreaterThan(0)
            .WithMessage("Invalid fiscal period ID.");
    }
}
