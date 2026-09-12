using ERP_Government.Application.Common.Interfaces;
using ERP_Government.Domain.Budgeting.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ERP_Government.Application.Reporting.BudgetExecution.GetBudgetExecutionReport;

internal class GetBudgetExecutionReportQueryHandler(IApplicationDbContext dbContext)
    : IRequestHandler<GetBudgetExecutionReportQuery, BudgetExecutionReportDto>
{
    private static readonly Domain.Budgeting.Enums.EncumbranceStatus[] OpenEncumbranceStatuses =
    [
        Domain.Budgeting.Enums.EncumbranceStatus.Active,
        Domain.Budgeting.Enums.EncumbranceStatus.PartiallyReleased,
        Domain.Budgeting.Enums.EncumbranceStatus.PartiallyLiquidated,
    ];

    private static readonly Domain.Payments.Enums.PaymentOrderStatus[] ExecutedPaymentStatuses =
    [
        Domain.Payments.Enums.PaymentOrderStatus.Approved,
        Domain.Payments.Enums.PaymentOrderStatus.SentToTreasury,
        Domain.Payments.Enums.PaymentOrderStatus.Paid,
    ];

    public async Task<BudgetExecutionReportDto> Handle(
        GetBudgetExecutionReportQuery request,
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

        // Classification ancestry
        var classifications = await dbContext.BudgetClassifications
            .AsNoTracking()
            .ToListAsync(cancellationToken);
        var classificationById = classifications.ToDictionary(c => c.Id);

        List<int> GetChain(int? classificationId)
        {
            var chain = new List<int>();
            var current = classificationId;
            while (current.HasValue && classificationById.TryGetValue(current.Value, out var node))
            {
                chain.Add(node.Id);
                current = node.ParentId;
            }
            return chain;
        }

        if (request.ProgramId.HasValue)
        {
            var programId = request.ProgramId.Value;
            budgetItems = budgetItems
                .Where(bi => GetChain(bi.BudgetClassificationId).Contains(programId))
                .ToList();
        }

        if (request.ProjectId.HasValue)
        {
            var projectId = request.ProjectId.Value;
            budgetItems = budgetItems
                .Where(bi => GetChain(bi.BudgetClassificationId).Contains(projectId))
                .ToList();
        }

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
                && OpenEncumbranceStatuses.Contains(l.Encumbrance.Status)
                && l.Encumbrance.ReversalOfId == null)
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
                && ExecutedPaymentStatuses.Contains(x.po.Status))
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

        var lines = budgetItems
            .Select(bi =>
            {
                var fundId = bi.Budget!.FundId;
                var revisedBudget = revisedBudgets.GetValueOrDefault(bi.Id);
                var encumbered = encumberedAmounts.GetValueOrDefault(bi.Id);
                var paid = paidAmounts.GetValueOrDefault(bi.Id);
                var fund = funds.GetValueOrDefault(fundId);

                int? programId = null;
                string? programCode = null;
                int? projectId = null;
                string? projectCode = null;
                if (bi.BudgetClassificationId.HasValue && classificationById.TryGetValue(bi.BudgetClassificationId.Value, out var leaf))
                {
                    projectId = leaf.Id;
                    projectCode = leaf.Code;
                    if (leaf.ParentId.HasValue && classificationById.TryGetValue(leaf.ParentId.Value, out var parent))
                    {
                        programId = parent.Id;
                        programCode = parent.Code;
                    }
                }

                return new BudgetExecutionLineDto
                {
                    BudgetItemId = bi.Id,
                    ItemCode = bi.ItemCode,
                    ItemName = bi.ItemName,
                    FundId = fundId,
                    FundNumber = fund.FundNumber,
                    FundName = fund.FundName,
                    ProgramId = programId,
                    ProgramCode = programCode,
                    ProjectId = projectId,
                    ProjectCode = projectCode,
                    AppropriatedAmount = revisedBudget,
                    EncumberedAmount = encumbered,
                    PaidAmount = paid,
                    AvailableAmount = revisedBudget - encumbered - paid
                };
            })
            .OrderBy(l => l.ItemCode)
            .ToList();

        return new BudgetExecutionReportDto
        {
            FiscalYearId = request.FiscalYearId,
            FiscalYearName = fiscalYear.YearNumber.ToString(),
            Lines = lines,
            Totals = new BudgetExecutionTotalDto
            {
                AppropriatedAmount = lines.Sum(l => l.AppropriatedAmount),
                EncumberedAmount = lines.Sum(l => l.EncumberedAmount),
                PaidAmount = lines.Sum(l => l.PaidAmount),
                AvailableAmount = lines.Sum(l => l.AvailableAmount)
            }
        };
    }
}
