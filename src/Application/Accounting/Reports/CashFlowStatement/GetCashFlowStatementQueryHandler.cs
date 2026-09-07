using ERP_Government.Application.Accounting.Reports.Common;
using ERP_Government.Domain.Accounting.Enums;
using Microsoft.EntityFrameworkCore;

namespace ERP_Government.Application.Accounting.Reports.CashFlowStatement;

public class GetCashFlowStatementQueryHandler(
    IApplicationDbContext context,
    ReportAuditService auditService) : IRequestHandler<GetCashFlowStatementQuery, CashFlowStatementDto>
{
    public async Task<CashFlowStatementDto> Handle(
        GetCashFlowStatementQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            var startDate = DateOnly.TryParse(request.StartDate, out var s) ? s : DateOnly.FromDateTime(DateTime.Today.AddMonths(-1));
            var endDate = DateOnly.TryParse(request.EndDate, out var e) ? e : DateOnly.FromDateTime(DateTime.Today);

            // Get all posted JournalEntryLines in period with account groups
            var journalEntryLines = await context.JournalEntryLines
                .Include(ml => ml.JournalEntry)
                .Include(ml => ml.Account)
                    .ThenInclude(a => a.AccountGroup)
                .Where(ml => ml.JournalEntry.EntryStatus == EntryStatus.Posted
                          && ml.JournalEntry.DocumentDate >= startDate
                          && ml.JournalEntry.DocumentDate <= endDate)
                .ToListAsync(cancellationToken);

            // Opening Cash: GL cash account balances as of startDate - 1 day
            var openingDate = startDate.AddDays(-1);
            var cashAccounts = await context.Accounts
                .Where(a => a.IsActive && (a.IsReconcilable || a.AccountGroup.Type == AccountGroupType.Asset))
                .ToListAsync(cancellationToken);

            var openingBalances = await context.JournalEntryLines
                .Include(ml => ml.JournalEntry)
                .Include(ml => ml.Account)
                .Where(ml => ml.JournalEntry.EntryStatus == EntryStatus.Posted
                          && ml.JournalEntry.DocumentDate <= openingDate
                          && cashAccounts.Select(c => c.Id).Contains(ml.AccountId))
                .ToListAsync(cancellationToken);

            var openingCash = openingBalances.Sum(ml => ml.Debit - ml.Credit);

            // Classify by account group type (cash accounts excluded — they are the statement's subject)
            var operating = BuildSection("الأنشطة التشغيلية", "Operating Activities",
                journalEntryLines, CashFlowSectionType.Operating);
            var investing = BuildSection("أنشطة الاستثمار", "Investing Activities",
                journalEntryLines, CashFlowSectionType.Investing);
            var financing = BuildSection("أنشطة التمويل", "Financing Activities",
                journalEntryLines, CashFlowSectionType.Financing);

            var netChange = operating.Total + investing.Total + financing.Total;
            var closingCash = openingCash + netChange;

            // Reconcile
            var glCashBalance = await context.JournalEntryLines
                .Include(ml => ml.JournalEntry)
                .Include(ml => ml.Account)
                .Where(ml => ml.JournalEntry.EntryStatus == EntryStatus.Posted
                          && ml.JournalEntry.DocumentDate <= endDate
                          && cashAccounts.Select(c => c.Id).Contains(ml.AccountId))
                .SumAsync(ml => ml.Debit - ml.Credit, cancellationToken);

            var reconciled = Math.Abs(closingCash - glCashBalance) <= 0.0001m;

            var result = new CashFlowStatementDto
            {
                StartDate = startDate,
                EndDate = endDate,
                Currency = "YER",
                Operating = operating,
                Investing = investing,
                Financing = financing,
                NetChange = netChange,
                OpeningCash = openingCash,
                ClosingCash = closingCash,
                Reconciled = reconciled,
                GeneratedAt = DateTimeOffset.UtcNow,
            };

            await auditService.LogAsync("CashFlowStatement", request, "Screen", true, cancellationToken: cancellationToken);
            return result;
        }
        catch (Exception ex)
        {
            await auditService.LogAsync("CashFlowStatement", request, "Screen", false, ex.Message, cancellationToken);
            throw;
        }
    }

    private static CashFlowSection BuildSection(
        string titleAr,
        string title,
        List<Domain.Accounting.Entities.JournalEntryLine> journalEntryLines,
        CashFlowSectionType sectionType)
    {
        IReadOnlyList<AccountGroupType> sectionTypes = sectionType switch
        {
            CashFlowSectionType.Operating => [AccountGroupType.Revenue, AccountGroupType.Expense],
            CashFlowSectionType.Investing => [AccountGroupType.Asset],
            CashFlowSectionType.Financing => [AccountGroupType.Equity, AccountGroupType.Liability],
            _ => [],
        };

        var sectionLines = journalEntryLines
            .Where(ml => !ml.Account.IsReconcilable
                      && sectionTypes.Contains(ml.Account.AccountGroup.Type))
            .ToList();

        var items = sectionLines
            .GroupBy(ml => ml.Account.AccountGroup)
            .OrderBy(g => g.Key.Code)
            .Select(g => new CashFlowLineItem
            {
                Description = g.Key.Name,
                Amount = g.Sum(ml => ml.Credit - ml.Debit),
            })
            .ToList();

        return new CashFlowSection
        {
            Title = title,
            TitleAr = titleAr,
            Items = items,
            Total = items.Sum(i => i.Amount),
        };
    }
}
