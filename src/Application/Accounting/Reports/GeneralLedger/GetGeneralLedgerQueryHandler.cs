using ERP_Government.Application.Accounting.Reports.Common;
using ERP_Government.Domain.Accounting.Enums;
using Microsoft.EntityFrameworkCore;

namespace ERP_Government.Application.Accounting.Reports.GeneralLedger;

public class GetGeneralLedgerQueryHandler(
    IApplicationDbContext context,
    ReportAuditService auditService) : IRequestHandler<GetGeneralLedgerQuery, GeneralLedgerDto>
{
    public async Task<GeneralLedgerDto> Handle(
        GetGeneralLedgerQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            var startDate = DateOnly.TryParse(request.StartDate, out var s) ? s : (DateOnly?)null;
            var endDate = DateOnly.TryParse(request.EndDate, out var e) ? e : (DateOnly?)null;

            // If FiscalPeriodId is provided, resolve its date range
            if (request.FiscalPeriodId.HasValue && (!startDate.HasValue || !endDate.HasValue))
            {
                var period = await context.FiscalPeriods
                    .FirstOrDefaultAsync(p => p.Id == request.FiscalPeriodId.Value, cancellationToken);
                if (period != null)
                {
                    startDate ??= period.StartDate;
                    endDate ??= period.EndDate;
                }
            }

            var query = context.JournalEntryLines
                .Include(ml => ml.JournalEntry)
                .Include(ml => ml.Account)
                .Where(ml => ml.JournalEntry.EntryStatus == EntryStatus.Posted);

            if (request.AccountId.HasValue)
                query = query.Where(ml => ml.AccountId == request.AccountId.Value);

            if (!string.IsNullOrEmpty(request.AccountCode))
                query = query.Where(ml => ml.Account.Code == request.AccountCode);

            if (startDate.HasValue)
                query = query.Where(ml => ml.JournalEntry.DocumentDate >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(ml => ml.JournalEntry.DocumentDate <= endDate.Value);

            var totalCount = await query.CountAsync(cancellationToken);

            var page = request.Page ?? 1;
            var pageSize = request.PageSize ?? 500;

            var allLines = await query
                .OrderBy(ml => ml.JournalEntry.DocumentDate)
                .ThenBy(ml => ml.JournalEntry.EntryNumber)
                .ThenBy(ml => ml.Sequence)
                .ToListAsync(cancellationToken);

            // Compute running balance per account from all filtered lines
            var runningBalances = new Dictionary<int, decimal>();
            foreach (var line in allLines)
            {
                if (!runningBalances.ContainsKey(line.AccountId))
                    runningBalances[line.AccountId] = 0;
                runningBalances[line.AccountId] += line.Debit - line.Credit;
            }

            // Apply pagination
            var pagedLines = allLines
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(ml => new GeneralLedgerLine
                {
                    DocumentDate = ml.JournalEntry.DocumentDate,
                    EntryNumber = ml.JournalEntry.EntryNumber,
                    Reference = ml.JournalEntry.Ref ?? string.Empty,
                    Narration = ml.JournalEntry.Narration ?? string.Empty,
                    AccountCode = ml.Account.Code,
                    AccountName = ml.Account.Name,
                    Debit = ml.Debit,
                    Credit = ml.Credit,
                    RunningBalance = runningBalances[ml.AccountId],
                })
                .ToList();

            var result = new GeneralLedgerDto
            {
                Currency = "YER",
                TotalLines = totalCount,
                Page = page,
                PageSize = pageSize,
                Lines = pagedLines,
                Totals = new GeneralLedgerTotals
                {
                    Debit = pagedLines.Sum(l => l.Debit),
                    Credit = pagedLines.Sum(l => l.Credit),
                },
                GeneratedAt = DateTimeOffset.UtcNow,
            };

            await auditService.LogAsync("GeneralLedger", request, "Screen", true, cancellationToken: cancellationToken);
            return result;
        }
        catch (Exception ex)
        {
            await auditService.LogAsync("GeneralLedger", request, "Screen", false, ex.Message, cancellationToken);
            throw;
        }
    }
}
