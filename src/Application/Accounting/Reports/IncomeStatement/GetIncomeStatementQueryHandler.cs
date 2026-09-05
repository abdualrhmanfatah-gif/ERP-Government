using ERP_Government.Application.Accounting.Reports.Common;
using ERP_Government.Domain.Accounting.Enums;
using Microsoft.EntityFrameworkCore;

namespace ERP_Government.Application.Accounting.Reports.IncomeStatement;

public class GetIncomeStatementQueryHandler(
    IApplicationDbContext context,
    ReportAuditService auditService) : IRequestHandler<GetIncomeStatementQuery, IncomeStatementDto>
{
    public async Task<IncomeStatementDto> Handle(
        GetIncomeStatementQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            var startDate = DateOnly.TryParse(request.StartDate, out var s) ? s : DateOnly.FromDateTime(DateTime.Today.AddMonths(-1));
            var endDate = DateOnly.TryParse(request.EndDate, out var e) ? e : DateOnly.FromDateTime(DateTime.Today);

            var revenueLines = await context.JournalEntryLines
                .Include(ml => ml.JournalEntry)
                .Include(ml => ml.Account)
                    .ThenInclude(a => a.AccountGroup)
                .Where(ml => ml.JournalEntry.EntryStatus == EntryStatus.Posted
                          && ml.Account.AccountGroup.Type == AccountGroupType.Revenue
                          && ml.JournalEntry.DocumentDate >= startDate
                          && ml.JournalEntry.DocumentDate <= endDate)
                .ToListAsync(cancellationToken);

            var expenseLines = await context.JournalEntryLines
                .Include(ml => ml.JournalEntry)
                .Include(ml => ml.Account)
                    .ThenInclude(a => a.AccountGroup)
                .Where(ml => ml.JournalEntry.EntryStatus == EntryStatus.Posted
                          && ml.Account.AccountGroup.Type == AccountGroupType.Expense
                          && ml.JournalEntry.DocumentDate >= startDate
                          && ml.JournalEntry.DocumentDate <= endDate)
                .ToListAsync(cancellationToken);

            var revenue = BuildGroup(revenueLines, AccountGroupType.Revenue);
            var expenses = BuildGroup(expenseLines, AccountGroupType.Expense);

            var result = new IncomeStatementDto
            {
                StartDate = startDate,
                EndDate = endDate,
                Currency = "YER",
                Revenue = revenue,
                Expenses = expenses,
                NetIncome = revenue.Total - expenses.Total,
                GeneratedAt = DateTimeOffset.UtcNow,
            };

            await auditService.LogAsync("IncomeStatement", request, "Screen", true, cancellationToken: cancellationToken);
            return result;
        }
        catch (Exception ex)
        {
            await auditService.LogAsync("IncomeStatement", request, "Screen", false, ex.Message, cancellationToken);
            throw;
        }
    }

    private static IncomeStatementGroup BuildGroup(
        List<Domain.Accounting.Entities.JournalEntryLine> lines,
        AccountGroupType groupType)
    {
        var grouped = lines
            .GroupBy(ml => ml.Account.AccountGroup)
            .OrderBy(g => g.Key.Code)
            .Select(g => new ReportSection
            {
                Title = g.Key.Name,
                TitleEn = g.Key.Name,
                Lines = g.Select(ml => new ReportLine
                {
                    AccountCode = ml.Account.Code,
                    AccountName = ml.Account.Name,
                    Debit = ml.Debit,
                    Credit = ml.Credit,
                    Balance = groupType == AccountGroupType.Revenue
                        ? ml.Credit - ml.Debit
                        : ml.Debit - ml.Credit,
                }).ToList(),
                Total = g.Sum(ml => groupType == AccountGroupType.Revenue
                    ? ml.Credit - ml.Debit
                    : ml.Debit - ml.Credit),
            })
            .ToList();

        return new IncomeStatementGroup
        {
            Sections = grouped,
            Total = grouped.Sum(s => s.Total),
        };
    }
}
