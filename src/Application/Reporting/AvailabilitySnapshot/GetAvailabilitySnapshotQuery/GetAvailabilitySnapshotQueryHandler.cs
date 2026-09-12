using ERP_Government.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ERP_Government.Application.Reporting.AvailabilitySnapshot.GetAvailabilitySnapshotQuery;

internal class GetAvailabilitySnapshotQueryHandler(IApplicationDbContext dbContext)
    : IRequestHandler<GetAvailabilitySnapshotQuery, AvailabilitySnapshotDto>
{
    public async Task<AvailabilitySnapshotDto> Handle(
        GetAvailabilitySnapshotQuery request,
        CancellationToken cancellationToken)
    {
        var fiscalYear = await dbContext.FiscalYears
            .AsNoTracking()
            .FirstAsync(fy => fy.Id == request.FiscalYearId, cancellationToken);

        var budgetIds = await dbContext.Budgets
            .Where(b => b.FiscalYearId == request.FiscalYearId && b.Status != Domain.Budgeting.Enums.BudgetStatus.Cancelled)
            .Select(b => b.Id)
            .ToListAsync(cancellationToken);

        var budgetItemsQuery = dbContext.BudgetItems
            .AsNoTracking()
            .Include(bi => bi.Budget)
            .Where(bi => budgetIds.Contains(bi.BudgetId));

        if (request.FundId.HasValue)
            budgetItemsQuery = budgetItemsQuery.Where(bi => bi.Budget!.FundId == request.FundId.Value);

        if (request.BudgetItemId.HasValue)
            budgetItemsQuery = budgetItemsQuery.Where(bi => bi.Id == request.BudgetItemId.Value);

        var budgetItems = await budgetItemsQuery.ToListAsync(cancellationToken);
        var budgetItemIds = budgetItems.Select(bi => bi.Id).Distinct().ToList();

        // Revised budget from BudgetTransactions
        var revisedBudgets = await dbContext.BudgetTransactions
            .AsNoTracking()
            .Where(t => budgetItemIds.Contains(t.BudgetItemAllocation.BudgetItemId)
                && t.Status == Domain.Budgeting.Enums.BudgetTransactionStatus.Posted)
            .GroupBy(t => t.BudgetItemAllocation.BudgetItemId)
            .Select(g => new
            {
                BudgetItemId = g.Key,
                RevisedBudget = g.Sum(t => t.Direction == Domain.Budgeting.Enums.TransactionDirection.Increase ? t.Amount : -t.Amount)
            })
            .ToDictionaryAsync(x => x.BudgetItemId, x => x.RevisedBudget, cancellationToken);

        // Outstanding encumbrances
        var encumberedAmounts = await dbContext.EncumbranceLines
            .AsNoTracking()
            .Where(l => budgetItemIds.Contains(l.BudgetItemId)
                && l.Encumbrance.Status != Domain.Budgeting.Enums.EncumbranceStatus.Cancelled)
            .GroupBy(l => l.BudgetItemId)
            .Select(g => new
            {
                BudgetItemId = g.Key,
                Encumbered = g.Sum(l => l.Amount - l.LiquidatedAmount - l.CancelledAmount)
            })
            .ToDictionaryAsync(x => x.BudgetItemId, x => x.Encumbered, cancellationToken);

        // Actual expenditure from PaymentOrders (via BudgetItemAllocation)
        var paidAmounts = await dbContext.PaymentOrders
            .AsNoTracking()
            .Join(dbContext.BudgetItemAllocations,
                po => po.BudgetItemAllocationId,
                alloc => alloc.Id,
                (po, alloc) => new { po, alloc.BudgetItemId })
            .Where(x => budgetItemIds.Contains(x.BudgetItemId)
                && x.po.Status != Domain.Payments.Enums.PaymentOrderStatus.Cancelled)
            .GroupBy(x => x.BudgetItemId)
            .Select(g => new
            {
                BudgetItemId = g.Key,
                Paid = g.Sum(x => x.po.AmountGross)
            })
            .ToDictionaryAsync(x => x.BudgetItemId, x => x.Paid, cancellationToken);

        var funds = await dbContext.Funds
            .AsNoTracking()
            .ToDictionaryAsync(f => f.Id, f => (f.FundNumber, f.FundName), cancellationToken);

        var breakdown = budgetItems
            .Select(bi =>
            {
                var fundId = bi.Budget!.FundId;
                var revisedBudget = revisedBudgets.GetValueOrDefault(bi.Id);
                var encumbered = encumberedAmounts.GetValueOrDefault(bi.Id);
                var paid = paidAmounts.GetValueOrDefault(bi.Id);
                var fund = funds.GetValueOrDefault(fundId);

                return new AvailabilitySnapshotLineDto
                {
                    FundId = fundId,
                    FundCode = fund.FundNumber,
                    FundName = fund.FundName,
                    AppropriationAmount = revisedBudget,
                    EncumberedAmount = encumbered,
                    PaidAmount = paid,
                    AvailableAmount = revisedBudget - encumbered - paid
                };
            })
            .OrderBy(l => l.FundCode)
            .ToList();

        var budgetItem = budgetItems.FirstOrDefault();

        return new AvailabilitySnapshotDto
        {
            BudgetItemId = budgetItem?.Id ?? 0,
            ItemCode = budgetItem?.ItemCode ?? string.Empty,
            ItemName = budgetItem?.ItemName ?? string.Empty,
            FiscalYearId = request.FiscalYearId,
            FiscalYearName = fiscalYear.Name,
            ControlState = budgetItem?.Budget?.Status.ToString() ?? string.Empty,
            Breakdown = breakdown,
            Totals = new AvailabilitySnapshotTotalDto
            {
                AppropriationAmount = breakdown.Sum(l => l.AppropriationAmount),
                EncumberedAmount = breakdown.Sum(l => l.EncumberedAmount),
                PaidAmount = breakdown.Sum(l => l.PaidAmount),
                AvailableAmount = breakdown.Sum(l => l.AvailableAmount)
            }
        };
    }
}
