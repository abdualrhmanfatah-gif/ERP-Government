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
            // Live aggregation from posted JournalEntryLines (DEP-026 — AccountBalances removed)
            var lines = await context.JournalEntryLines
                .Include(x => x.JournalEntry)
                .Include(x => x.Account)
                    .ThenInclude(a => a.AccountGroup)
                .Where(x => x.JournalEntry.EntryStatus == EntryStatus.Posted
                         && x.JournalEntry.FiscalYearId == request.FiscalYearId
                         && x.JournalEntry.PeriodId == request.FiscalPeriodId)
                .ToListAsync(cancellationToken);

            var fiscalYearName = await context.FiscalYears
                .Where(f => f.Id == request.FiscalYearId)
                .Select(f => f.Name)
                .FirstOrDefaultAsync(cancellationToken) ?? string.Empty;
            var periodName = await context.FiscalPeriods
                .Where(p => p.Id == request.FiscalPeriodId)
                .Select(p => p.Name)
                .FirstOrDefaultAsync(cancellationToken) ?? string.Empty;

            var perAccount = lines
                .GroupBy(x => x.Account)
                .ToList();

            var currencyCode = "YER";

            var sections = perAccount
                .GroupBy(x => x.Key.AccountGroup)
                .OrderBy(g => g.Key.Code)
                .Select(g => new ReportSection
                {
                    Title = g.Key.Name,
                    TitleEn = g.Key.Name,
                    Lines = g.Select(x => new ReportLine
                    {
                        AccountCode = x.Key.Code,
                        AccountName = x.Key.Name,
                        Debit = x.Sum(l => l.Debit),
                        Credit = x.Sum(l => l.Credit),
                        Balance = x.Sum(l => l.Debit) - x.Sum(l => l.Credit),
                    }).ToList(),
                    Total = g.Sum(x => x.Sum(l => l.Debit) - x.Sum(l => l.Credit)),
                })
                .ToList();

            var totalDebit = perAccount.Sum(x => x.Sum(l => l.Debit));
            var totalCredit = perAccount.Sum(x => x.Sum(l => l.Credit));

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
