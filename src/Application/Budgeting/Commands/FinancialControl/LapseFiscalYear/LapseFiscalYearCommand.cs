using ERP_Government.Application.Common.Interfaces;
using ERP_Government.Application.Common.Models;
using ERP_Government.Application.Common.Security;
using ERP_Government.Application.FinancialSettings.Common.Services;
using ERP_Government.Domain.Budgeting.Entities;
using ERP_Government.Domain.Budgeting.Enums;
using ERP_Government.Domain.Common;
using MediatR;

namespace ERP_Government.Application.Budgeting.Commands.FinancialControl.LapseFiscalYear;

[Authorize(Policy = PermissionCodes.FinancialControlLapseYear)]
public record LapseFiscalYearCommand(int FiscalYearId) : IRequest<Result<LapseFiscalYearResult>>;

public record LapseFiscalYearResult(
    int YearClosingRunId,
    int FiscalYearId,
    string FiscalYearName,
    decimal LapsedAppropriationTotal,
    decimal LapsedEncumbranceTotal);

public class LapseFiscalYearCommandHandler(
    IApplicationDbContext context,
    IDocumentSequenceService sequenceService,
    IUser user) : IRequestHandler<LapseFiscalYearCommand, Result<LapseFiscalYearResult>>
{
    public async Task<Result<LapseFiscalYearResult>> Handle(
        LapseFiscalYearCommand request,
        CancellationToken cancellationToken)
    {
        var fiscalYear = await context.FiscalYears.FindAsync(request.FiscalYearId, cancellationToken);
        if (fiscalYear is null)
            return Result<LapseFiscalYearResult>.Failure(new[] { "Fiscal year not found." });

        var existingRun = await context.YearClosingRuns
            .Where(r => r.FiscalYearId == request.FiscalYearId && r.Status == YearClosingRunStatus.Completed)
            .FirstOrDefaultAsync(cancellationToken);

        if (existingRun is not null)
            return Result<LapseFiscalYearResult>.Failure(new[]
                { $"Fiscal year {fiscalYear.Name} has already been lapsed. Run ID: {existingRun.Id}." });

        var pendingOrRunningRun = await context.YearClosingRuns
            .Where(r => r.FiscalYearId == request.FiscalYearId
                && (r.Status == YearClosingRunStatus.Pending || r.Status == YearClosingRunStatus.Running))
            .FirstOrDefaultAsync(cancellationToken);

        if (pendingOrRunningRun is not null)
            return Result<LapseFiscalYearResult>.Failure(new[]
                { $"Fiscal year {fiscalYear.Name} has a year closing run in progress (Run ID: {pendingOrRunningRun.Id}). Wait for it to complete or fail before lapping." });

        var budgetIds = await context.Budgets
            .Where(b => b.FiscalYearId == request.FiscalYearId && b.Status == BudgetStatus.Active)
            .Select(b => b.Id)
            .ToListAsync(cancellationToken);

        var budgetItemAllocations = await context.BudgetItemAllocations
            .Where(a => budgetIds.Contains(a.BudgetId))
            .ToListAsync(cancellationToken);

        var lapsedAppropriationTotal = 0m;
        var lapsedEncumbranceTotal = 0m;

        foreach (var allocation in budgetItemAllocations)
        {
            var revisedBudget = await context.BudgetTransactions
                .Where(t => t.BudgetItemAllocationId == allocation.Id
                    && t.Status == BudgetTransactionStatus.Posted)
                .SumAsync(t => t.Direction == TransactionDirection.Increase ? t.Amount : -t.Amount, cancellationToken);

            var outstandingEncumbrance = await context.EncumbranceLines
                .Where(l => l.BudgetItemId == allocation.BudgetItemId
                    && (l.Encumbrance.Status == EncumbranceStatus.Active
                        || l.Encumbrance.Status == EncumbranceStatus.PartiallyReleased
                        || l.Encumbrance.Status == EncumbranceStatus.PartiallyLiquidated)
                    && l.Encumbrance.ReversalOfId == null)
                .SumAsync(l => l.Amount - l.LiquidatedAmount - l.CancelledAmount, cancellationToken);

            var actualExpenditure = await context.PaymentOrders
                .Where(po => po.BudgetItemAllocationId == allocation.Id
                    && po.Status == Domain.Payments.Enums.PaymentOrderStatus.Paid)
                .SumAsync(po => po.AmountGross - po.DeductionAmount, cancellationToken);

            var availableBudget = revisedBudget - actualExpenditure - outstandingEncumbrance;

            if (availableBudget <= 0)
                continue;

            var transactionNumber = await sequenceService.GenerateNextNumberAsync("BudgetTransaction", cancellationToken);

            var lapseTransaction = new BudgetTransaction
            {
                TransactionNumber = transactionNumber,
                BudgetId = allocation.BudgetId,
                BudgetItemAllocationId = allocation.Id,
                TransactionType = BudgetTransactionType.Lapse,
                TransactionDate = DateOnly.FromDateTime(DateTime.UtcNow),
                Amount = availableBudget,
                Direction = TransactionDirection.Decrease,
                Description = $"Year-end lapse for fiscal year {fiscalYear.Name}",
                Status = BudgetTransactionStatus.Posted,
                PostedAt = DateTimeOffset.UtcNow,
                PostedBy = user.Id?.ToString() ?? ""
            };

            context.BudgetTransactions.Add(lapseTransaction);

            lapsedAppropriationTotal += availableBudget;
        }

        var encumbrances = await context.Encumbrances
            .Where(e => e.DocumentType == "BudgetTransaction"
                && e.DocumentId.HasValue
                && context.BudgetTransactions.Any(bt => bt.Id == e.DocumentId.Value
                    && bt.Budget.FiscalYearId == request.FiscalYearId)
                && (e.Status == EncumbranceStatus.Active
                    || e.Status == EncumbranceStatus.PartiallyReleased
                    || e.Status == EncumbranceStatus.PartiallyLiquidated)
                && e.ReversalOfId == null)
            .ToListAsync(cancellationToken);

        foreach (var encumbrance in encumbrances)
        {
            lapsedEncumbranceTotal += encumbrance.TotalAmount;
            encumbrance.Status = EncumbranceStatus.Cancelled;
        }

        fiscalYear.IsClosed = true;

        var closingRun = new YearClosingRun
        {
            FiscalYearId = request.FiscalYearId,
            StartedAt = DateTimeOffset.UtcNow,
            RunById = user.Id ?? 0,
            RunType = YearClosingRunType.Lapse,
            LapsedAppropriationTotal = lapsedAppropriationTotal,
            LapsedEncumbranceTotal = lapsedEncumbranceTotal,
            Status = YearClosingRunStatus.Completed
        };

        context.YearClosingRuns.Add(closingRun);
        await context.SaveChangesAsync(cancellationToken);

        return Result<LapseFiscalYearResult>.Success(new LapseFiscalYearResult(
            closingRun.Id,
            request.FiscalYearId,
            fiscalYear.Name,
            lapsedAppropriationTotal,
            lapsedEncumbranceTotal));
    }
}
