using ERP_Government.Application.Common.Interfaces;
using ERP_Government.Application.Common.Models;
using ERP_Government.Application.Common.Security;
using ERP_Government.Domain.Budgeting.Entities;
using ERP_Government.Domain.Budgeting.Enums;
using ERP_Government.Domain.Events.Budgeting;
using MediatR;

namespace ERP_Government.Application.Budgeting.Commands.FinancialControl.GenerateFinalAccount;

[Authorize(Policy = PermissionCodes.FinancialControlApproveFinalAccount)]
public record GenerateFinalAccountCommand(int FiscalYearId) : IRequest<Result<GenerateFinalAccountResult>>;

public record GenerateFinalAccountResult(
    int FinalAccountId,
    int FiscalYearId,
    string FiscalYearName,
    int LineCount);

public class GenerateFinalAccountCommandHandler(
    IApplicationDbContext context,
    IUser user) : IRequestHandler<GenerateFinalAccountCommand, Result<GenerateFinalAccountResult>>
{
    public async Task<Result<GenerateFinalAccountResult>> Handle(
        GenerateFinalAccountCommand request,
        CancellationToken cancellationToken)
    {
        var fiscalYear = await context.FiscalYears.FindAsync(request.FiscalYearId, cancellationToken);
        if (fiscalYear is null)
            return Result<GenerateFinalAccountResult>.Failure(new[] { "Fiscal year not found." });

        if (!fiscalYear.IsClosed)
            return Result<GenerateFinalAccountResult>.Failure(new[]
                { $"Fiscal year {fiscalYear.Name} must be closed before generating the final account." });

        var existingAccount = await context.FinalAccounts
            .Where(f => f.FiscalYearId == request.FiscalYearId)
            .FirstOrDefaultAsync(cancellationToken);

        if (existingAccount is not null)
            return Result<GenerateFinalAccountResult>.Failure(new[]
                { $"Final account already exists for fiscal year {fiscalYear.Name}." });

        var closingRun = await context.YearClosingRuns
            .Where(r => r.FiscalYearId == request.FiscalYearId && r.Status == YearClosingRunStatus.Completed)
            .FirstOrDefaultAsync(cancellationToken);

        if (closingRun is null)
        {
            var failedRun = await context.YearClosingRuns
                .Where(r => r.FiscalYearId == request.FiscalYearId && r.Status == YearClosingRunStatus.Failed)
                .FirstOrDefaultAsync(cancellationToken);

            if (failedRun is not null)
                return Result<GenerateFinalAccountResult>.Failure(new[]
                    { $"Year closing run failed for fiscal year {fiscalYear.Name} (Run ID: {failedRun.Id}). Retry or investigate before generating final account." });

            return Result<GenerateFinalAccountResult>.Failure(new[]
                { $"No completed year closing run found for fiscal year {fiscalYear.Name}." });
        }

        var budgetIds = await context.Budgets
            .Where(b => b.FiscalYearId == request.FiscalYearId)
            .Select(b => b.Id)
            .ToListAsync(cancellationToken);

        var budgetItems = await context.BudgetItems
            .Where(bi => budgetIds.Contains(bi.BudgetId))
            .ToListAsync(cancellationToken);

        var finalAccount = new FinalAccount
        {
            FiscalYearId = request.FiscalYearId,
            FundId = budgetItems.FirstOrDefault()?.Budget?.FundId ?? 0,
            YearClosingRunId = closingRun.Id,
            GeneratedAt = DateTimeOffset.UtcNow,
            GeneratedById = user.Id ?? 0,
            Status = FinalAccountStatus.Draft
        };

        var lines = new List<FinalAccountLine>();

        foreach (var budgetItem in budgetItems)
        {
            var postedTransactions = await context.BudgetTransactions
                .Where(t => t.BudgetItemAllocation.BudgetItemId == budgetItem.Id
                    && t.Status == BudgetTransactionStatus.Posted)
                .ToListAsync(cancellationToken);

            var originalBudgetAmount = postedTransactions
                .Where(t => t.TransactionType == BudgetTransactionType.InitialAppropriation)
                .Sum(t => t.Direction == TransactionDirection.Increase ? t.Amount : -t.Amount);

            var revisedBudgetAmount = postedTransactions
                .Sum(t => t.Direction == TransactionDirection.Increase ? t.Amount : -t.Amount);

            var outstandingEncumbrance = await context.EncumbranceLines
                .Where(l => l.BudgetItemId == budgetItem.Id
                    && (l.Encumbrance.Status == EncumbranceStatus.Active
                        || l.Encumbrance.Status == EncumbranceStatus.PartiallyReleased
                        || l.Encumbrance.Status == EncumbranceStatus.PartiallyLiquidated)
                    && l.Encumbrance.ReversalOfId == null)
                .SumAsync(l => l.Amount - l.LiquidatedAmount - l.CancelledAmount, cancellationToken);

            var actualAmount = await context.PaymentOrders
                .Join(context.BudgetItemAllocations,
                    po => po.BudgetItemAllocationId,
                    alloc => alloc.Id,
                    (po, alloc) => new { po, alloc.BudgetItemId })
                .Where(x => x.BudgetItemId == budgetItem.Id
                    && x.po.Status == Domain.Payments.Enums.PaymentOrderStatus.Paid)
                .SumAsync(x => x.po.AmountGross - x.po.DeductionAmount, cancellationToken);

            var varianceAmount = revisedBudgetAmount - actualAmount - outstandingEncumbrance;

            lines.Add(new FinalAccountLine
            {
                Dimension = FinalAccountLineDimension.Item,
                DimensionId = budgetItem.Id,
                DimensionCode = budgetItem.ItemCode,
                DimensionName = budgetItem.ItemName,
                OriginalBudgetAmount = originalBudgetAmount,
                RevisedBudgetAmount = revisedBudgetAmount,
                EncumberedAmount = outstandingEncumbrance,
                ActualAmount = actualAmount,
                VarianceAmount = varianceAmount
            });
        }

        foreach (var line in lines)
        {
            finalAccount.AddLine(line);
        }

        finalAccount.AddDomainEvent(new ClosingEntryGenerated
        {
            SourceEntityId = finalAccount.Id,
            FinalAccountId = finalAccount.Id,
            FiscalYearId = request.FiscalYearId,
            OccurredAt = DateTimeOffset.UtcNow
        });

        context.FinalAccounts.Add(finalAccount);
        await context.SaveChangesAsync(cancellationToken);

        finalAccount.ClearDomainEvents();

        return Result<GenerateFinalAccountResult>.Success(new GenerateFinalAccountResult(
            finalAccount.Id,
            request.FiscalYearId,
            fiscalYear.Name,
            lines.Count));
    }
}
