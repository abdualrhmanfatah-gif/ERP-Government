using ERP_Government.Application.Common.Interfaces;
using ERP_Government.Application.Common.Models;
using ERP_Government.Application.Common.Security;
using ERP_Government.Domain.Budgeting.Entities;
using ERP_Government.Domain.Budgeting.Enums;
using MediatR;

namespace ERP_Government.Application.Budgeting.Commands.FinancialControl.ReopenFiscalYear;

[Authorize(Policy = PermissionCodes.FinancialControlLapseYear)]
public record ReopenFiscalYearCommand(int FiscalYearId) : IRequest<Result<ReopenFiscalYearResult>>;

public record ReopenFiscalYearResult(
    int YearClosingRunId,
    int FiscalYearId,
    decimal RestoredBudgetTotal,
    decimal RestoredEncumbranceTotal);

public class ReopenFiscalYearCommandHandler(
    IApplicationDbContext context,
    IUser user) : IRequestHandler<ReopenFiscalYearCommand, Result<ReopenFiscalYearResult>>
{
    public async Task<Result<ReopenFiscalYearResult>> Handle(
        ReopenFiscalYearCommand request,
        CancellationToken cancellationToken)
    {
        var fiscalYear = await context.FiscalYears.FindAsync(request.FiscalYearId, cancellationToken);
        if (fiscalYear is null)
            return Result<ReopenFiscalYearResult>.Failure(new[] { "Fiscal year not found." });

        var finalAccount = await context.FinalAccounts
            .Where(f => f.FiscalYearId == request.FiscalYearId && f.Status == FinalAccountStatus.Issued)
            .FirstOrDefaultAsync(cancellationToken);

        if (finalAccount is not null)
            return Result<ReopenFiscalYearResult>.Failure(new[]
                { $"Cannot reopen fiscal year {fiscalYear.Name} — final account has been issued." });

        var lapsedRun = await context.YearClosingRuns
            .Where(r => r.FiscalYearId == request.FiscalYearId && r.Status == YearClosingRunStatus.Completed)
            .FirstOrDefaultAsync(cancellationToken);

        if (lapsedRun is null)
            return Result<ReopenFiscalYearResult>.Failure(new[]
                { $"Fiscal year {fiscalYear.Name} has not been lapsed." });

        // Reverse lapsed encumbrances
        var encumbrances = await context.Encumbrances
            .Where(e => e.Status == EncumbranceStatus.Cancelled
                && e.DocumentType == "BudgetTransaction"
                && e.DocumentId.HasValue
                && context.BudgetTransactions.Any(bt => bt.Id == e.DocumentId.Value
                    && bt.Budget.FiscalYearId == request.FiscalYearId))
            .ToListAsync(cancellationToken);

        var restoredEncumbranceTotal = 0m;

        foreach (var encumbrance in encumbrances)
        {
            restoredEncumbranceTotal += encumbrance.TotalAmount;
            encumbrance.Status = EncumbranceStatus.Active;
        }

        // Reverse lapse transactions (posted Lapse transactions)
        var lapseTransactions = await context.BudgetTransactions
            .Where(bt => bt.TransactionType == BudgetTransactionType.Lapse
                && bt.Status == BudgetTransactionStatus.Posted
                && bt.Budget.FiscalYearId == request.FiscalYearId)
            .ToListAsync(cancellationToken);

        decimal restoredBudgetTotal = 0m;
        foreach (var lapseTransaction in lapseTransactions)
        {
            lapseTransaction.Status = BudgetTransactionStatus.Reversed;
            restoredBudgetTotal += lapseTransaction.Direction == TransactionDirection.Decrease ? lapseTransaction.Amount : -lapseTransaction.Amount;

            var allocation = await context.BudgetItemAllocations
                .FirstOrDefaultAsync(a => a.Id == lapseTransaction.BudgetItemAllocationId, cancellationToken);

            if (allocation is not null && allocation.ApprovedAmount.HasValue)
            {
                if (lapseTransaction.Direction == TransactionDirection.Decrease)
                    allocation.ApprovedAmount += lapseTransaction.Amount;
                else
                    allocation.ApprovedAmount -= lapseTransaction.Amount;
            }
        }

        fiscalYear.IsClosed = false;

        lapsedRun.Status = YearClosingRunStatus.Reversed;

        var reopenRun = new YearClosingRun
        {
            FiscalYearId = request.FiscalYearId,
            StartedAt = DateTimeOffset.UtcNow,
            RunById = user.Id ?? 0,
            RunType = YearClosingRunType.Reopen,
            LapsedAppropriationTotal = restoredBudgetTotal,
            LapsedEncumbranceTotal = restoredEncumbranceTotal,
            Status = YearClosingRunStatus.Completed
        };

        context.YearClosingRuns.Add(reopenRun);
        await context.SaveChangesAsync(cancellationToken);

        return Result<ReopenFiscalYearResult>.Success(new ReopenFiscalYearResult(
            reopenRun.Id,
            request.FiscalYearId,
            restoredBudgetTotal,
            restoredEncumbranceTotal));
    }
}
