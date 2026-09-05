using ERP_Government.Application.Common.Security;
using ERP_Government.Domain.Accounting.Entities;
using ERP_Government.Domain.Accounting.Enums;

namespace ERP_Government.Application.Accounting.Commands.Balances.RebuildAccountBalances;

[Authorize(Policy = PermissionCodes.BalancesRebuild)]
public class RebuildAccountBalancesCommand : IRequest<Result>
{
    public int FiscalYearId { get; init; }
    public int? FiscalPeriodId { get; init; }
}

public class RebuildAccountBalancesCommandHandler(
    IApplicationDbContext context) : IRequestHandler<RebuildAccountBalancesCommand, Result>
{
    public async Task<Result> Handle(
        RebuildAccountBalancesCommand request,
        CancellationToken cancellationToken)
    {
        // Step 1: Delete existing balances for the target period(s)
        IQueryable<AccountBalance> deleteQuery = context.AccountBalances
            .Where(x => x.FiscalYearId == request.FiscalYearId);

        if (request.FiscalPeriodId.HasValue)
            deleteQuery = deleteQuery.Where(x => x.FiscalPeriodId == request.FiscalPeriodId.Value);

        var existingBalances = await deleteQuery.ToListAsync(cancellationToken);
        context.AccountBalances.RemoveRange(existingBalances);

        // Step 2: Get all posted JournalEntryLines for the target period(s)
        IQueryable<Domain.Accounting.Entities.JournalEntryLine> journalEntryLineQuery = context.JournalEntryLines
            .Where(x => x.JournalEntry.EntryStatus == EntryStatus.Posted
                     && x.JournalEntry.FiscalYearId == request.FiscalYearId);

        if (request.FiscalPeriodId.HasValue)
            journalEntryLineQuery = journalEntryLineQuery.Where(x => x.JournalEntry.PeriodId == request.FiscalPeriodId.Value);

        var journalEntryLines = await journalEntryLineQuery
            .Include(x => x.JournalEntry)
            .ToListAsync(cancellationToken);

        // Step 3: Aggregate by Account + Currency + Period
        var aggregated = journalEntryLines
            .GroupBy(x => new
            {
                x.AccountId,
                x.CurrencyId,
                PeriodId = x.JournalEntry.PeriodId,
                FiscalYearId = x.JournalEntry.FiscalYearId
            })
            .Select(g => new
            {
                g.Key.AccountId,
                g.Key.CurrencyId,
                g.Key.PeriodId,
                g.Key.FiscalYearId,
                TotalDebit = g.Sum(x => x.Debit),
                TotalCredit = g.Sum(x => x.Credit)
            })
            .ToList();

        // Step 4: Insert fresh balances
        foreach (var agg in aggregated)
        {
            context.AccountBalances.Add(new AccountBalance
            {
                AccountId = agg.AccountId,
                FiscalYearId = agg.FiscalYearId,
                FiscalPeriodId = agg.PeriodId,
                CurrencyId = agg.CurrencyId,
                OpeningDebit = 0,
                OpeningCredit = 0,
                Debit = agg.TotalDebit,
                Credit = agg.TotalCredit,
                ClosingDebit = agg.TotalDebit,
                ClosingCredit = agg.TotalCredit,
                IsFinalized = false
            });
        }

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

public class RebuildAccountBalancesCommandValidator : AbstractValidator<RebuildAccountBalancesCommand>
{
    public RebuildAccountBalancesCommandValidator()
    {
        RuleFor(x => x.FiscalYearId)
            .GreaterThan(0).WithMessage("Invalid fiscal year ID.");
    }
}
