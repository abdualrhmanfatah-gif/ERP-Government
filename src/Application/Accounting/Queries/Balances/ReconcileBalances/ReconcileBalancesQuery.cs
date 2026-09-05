using ERP_Government.Application.Accounting.Common;
using ERP_Government.Application.Common.Security;
using ERP_Government.Domain.Accounting.Entities;
using ERP_Government.Domain.Accounting.Enums;

namespace ERP_Government.Application.Accounting.Queries.Balances.ReconcileBalances;

[Authorize(Policy = PermissionCodes.BalancesRead)]
public class ReconcileBalancesQuery : IRequest<ReconciliationResultDto>
{
    public int FiscalYearId { get; init; }
    public int FiscalPeriodId { get; init; }
}

public class ReconcileBalancesQueryHandler(
    IApplicationDbContext context) : IRequestHandler<ReconcileBalancesQuery, ReconciliationResultDto>
{
    public async Task<ReconciliationResultDto> Handle(
        ReconcileBalancesQuery request,
        CancellationToken cancellationToken)
    {
        // Get materialized balances
        var materializedBalances = await context.AccountBalances
            .Include(x => x.Account)
            .Include(x => x.Currency)
            .Where(x => x.FiscalYearId == request.FiscalYearId
                     && x.FiscalPeriodId == request.FiscalPeriodId)
            .ToListAsync(cancellationToken);

        // Calculate from source JournalEntryLines (source of truth)
        var sourceLines = await context.JournalEntryLines
            .Include(x => x.JournalEntry)
            .Include(x => x.Account)
            .Where(x => x.JournalEntry.EntryStatus == EntryStatus.Posted
                     && x.JournalEntry.FiscalYearId == request.FiscalYearId
                     && x.JournalEntry.PeriodId == request.FiscalPeriodId)
            .ToListAsync(cancellationToken);

        // Load currencies for the lines
        var currencyIds = sourceLines.Select(x => x.CurrencyId).Distinct().ToList();
        var currencies = await context.Currencies
            .Where(x => currencyIds.Contains(x.Id))
            .ToListAsync(cancellationToken);
        var currencyLookup = currencies.ToDictionary(x => x.Id);

        var calculatedGroups = sourceLines
            .GroupBy(x => new { x.AccountId, x.CurrencyId })
            .Select(g => new
            {
                g.Key.AccountId,
                g.Key.CurrencyId,
                AccountCode = g.First().Account.Code,
                AccountName = g.First().Account.Name,
                CurrencyCode = currencyLookup.TryGetValue(g.Key.CurrencyId, out var cur) ? cur.Code : "UNKNOWN",
                CalculatedDebit = g.Sum(x => x.Debit),
                CalculatedCredit = g.Sum(x => x.Credit)
            })
            .ToList();

        var discrepancies = new List<ReconciliationDiscrepancyDto>();

        // Check for missing records (in source but not materialized)
        foreach (var calc in calculatedGroups)
        {
            var existing = materializedBalances
                .FirstOrDefault(x => x.AccountId == calc.AccountId
                                  && x.CurrencyId == calc.CurrencyId);

            if (existing is null)
            {
                discrepancies.Add(new ReconciliationDiscrepancyDto
                {
                    AccountId = calc.AccountId,
                    AccountCode = calc.AccountCode,
                    AccountName = calc.AccountName,
                    CurrencyId = calc.CurrencyId,
                    CurrencyCode = calc.CurrencyCode,
                    MaterializedDebit = 0,
                    CalculatedDebit = calc.CalculatedDebit,
                    MaterializedCredit = 0,
                    CalculatedCredit = calc.CalculatedCredit,
                    DebitDifference = calc.CalculatedDebit,
                    CreditDifference = calc.CalculatedCredit,
                    DiscrepancyType = "MissingRecord"
                });
            }
            else if (existing.Debit != calc.CalculatedDebit || existing.Credit != calc.CalculatedCredit)
            {
                discrepancies.Add(new ReconciliationDiscrepancyDto
                {
                    AccountId = calc.AccountId,
                    AccountCode = calc.AccountCode,
                    AccountName = calc.AccountName,
                    CurrencyId = calc.CurrencyId,
                    CurrencyCode = calc.CurrencyCode,
                    MaterializedDebit = existing.Debit,
                    CalculatedDebit = calc.CalculatedDebit,
                    MaterializedCredit = existing.Credit,
                    CalculatedCredit = calc.CalculatedCredit,
                    DebitDifference = calc.CalculatedDebit - existing.Debit,
                    CreditDifference = calc.CalculatedCredit - existing.Credit,
                    DiscrepancyType = "AmountMismatch"
                });
            }
        }

        // Check for orphan records (materialized but not in source)
        foreach (var mat in materializedBalances)
        {
            if (!calculatedGroups.Any(c => c.AccountId == mat.AccountId && c.CurrencyId == mat.CurrencyId))
            {
                discrepancies.Add(new ReconciliationDiscrepancyDto
                {
                    AccountId = mat.AccountId,
                    AccountCode = mat.Account.Code,
                    AccountName = mat.Account.Name,
                    CurrencyId = mat.CurrencyId,
                    CurrencyCode = mat.Currency.Code,
                    MaterializedDebit = mat.Debit,
                    CalculatedDebit = 0,
                    MaterializedCredit = mat.Credit,
                    CalculatedCredit = 0,
                    DebitDifference = -mat.Debit,
                    CreditDifference = -mat.Credit,
                    DiscrepancyType = "OrphanRecord"
                });
            }
        }

        return new ReconciliationResultDto
        {
            FiscalYearId = request.FiscalYearId,
            FiscalPeriodId = request.FiscalPeriodId,
            Discrepancies = discrepancies,
            IsBalanced = discrepancies.Count == 0,
            TotalAccountsChecked = calculatedGroups.Count,
            DiscrepancyCount = discrepancies.Count
        };
    }
}

public class ReconcileBalancesQueryValidator : AbstractValidator<ReconcileBalancesQuery>
{
    public ReconcileBalancesQueryValidator()
    {
        RuleFor(x => x.FiscalYearId)
            .GreaterThan(0).WithMessage("Invalid fiscal year ID.");

        RuleFor(x => x.FiscalPeriodId)
            .GreaterThan(0).WithMessage("Invalid fiscal period ID.");
    }
}
