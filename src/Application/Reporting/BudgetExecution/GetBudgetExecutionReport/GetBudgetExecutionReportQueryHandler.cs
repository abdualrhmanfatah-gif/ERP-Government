using ERP_Government.Application.Common.Interfaces;
using ERP_Government.Domain.Budgeting.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ERP_Government.Application.Reporting.BudgetExecution.GetBudgetExecutionReport;

internal class GetBudgetExecutionReportQueryHandler(IApplicationDbContext dbContext)
    : IRequestHandler<GetBudgetExecutionReportQuery, BudgetExecutionReportDto>
{
    public async Task<BudgetExecutionReportDto> Handle(
        GetBudgetExecutionReportQuery request,
        CancellationToken cancellationToken)
    {
        var fiscalYear = await dbContext.FiscalYears
            .AsNoTracking()
            .FirstAsync(fy => fy.Id == request.FiscalYearId, cancellationToken);

        var query = dbContext.Appropriations
            .AsNoTracking()
            .Include(a => a.BudgetItem)
            .ThenInclude(bi => bi!.Budget)
            .Where(a => a.BudgetItem != null
                     && a.BudgetItem.Budget != null
                     && a.BudgetItem.Budget.FiscalYearId == request.FiscalYearId
                     && a.Status != Domain.Budgeting.Enums.AppropriationStatus.Cancelled);

        if (request.FundId.HasValue)
            query = query.Where(a => a.BudgetItem!.Budget!.FundId == request.FundId.Value);

        if (request.BudgetItemId.HasValue)
            query = query.Where(a => a.BudgetItemId == request.BudgetItemId.Value);

        var appropriations = await query.ToListAsync(cancellationToken);

        var budgetItemIds = appropriations.Select(a => a.BudgetItemId).Distinct().ToList();

        var encumbrances = await dbContext.Encumbrances
            .AsNoTracking()
            .Include(e => e.Appropriation)
            .Where(e => budgetItemIds.Contains(e.Appropriation.BudgetItemId)
                     && e.Status != Domain.Budgeting.Enums.EncumbranceStatus.Cancelled)
            .ToListAsync(cancellationToken);

        var paymentOrders = await dbContext.PaymentOrders
            .AsNoTracking()
            .Where(po => budgetItemIds.Contains(po.AppropriationId)
                      && po.Status != Domain.Payments.Enums.PaymentOrderStatus.Cancelled)
            .ToListAsync(cancellationToken);

        var funds = await dbContext.Funds
            .AsNoTracking()
            .ToDictionaryAsync(f => f.Id, f => (f.FundNumber, f.FundName), cancellationToken);

        var lines = appropriations
            .GroupBy(a => new { a.BudgetItemId, FundId = a.BudgetItem!.Budget!.FundId })
            .Select(g =>
            {
                var budgetItemId = g.Key.BudgetItemId;
                var fundId = g.Key.FundId;
                var appropriated = g.Sum(a => a.Amount);
                var encumbered = encumbrances
                    .Where(e => e.Appropriation.BudgetItemId == budgetItemId)
                    .Sum(e => e.Amount);
                var paid = paymentOrders
                    .Where(po => po.AppropriationId == budgetItemId)
                    .Sum(po => po.AmountGross);

                var fund = funds.GetValueOrDefault(fundId);

                return new BudgetExecutionLineDto
                {
                    BudgetItemId = budgetItemId,
                    ItemCode = g.First().BudgetItem!.ItemCode,
                    ItemName = g.First().BudgetItem!.ItemName,
                    FundId = fundId,
                    FundNumber = fund.FundNumber,
                    FundName = fund.FundName,
                    AppropriatedAmount = appropriated,
                    EncumberedAmount = encumbered,
                    PaidAmount = paid,
                    AvailableAmount = appropriated - encumbered - paid
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
