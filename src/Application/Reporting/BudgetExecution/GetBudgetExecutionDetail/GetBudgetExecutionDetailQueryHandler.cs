using ERP_Government.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ERP_Government.Application.Reporting.BudgetExecution.GetBudgetExecutionDetail;

internal class GetBudgetExecutionDetailQueryHandler(IApplicationDbContext dbContext)
    : IRequestHandler<GetBudgetExecutionDetailQuery, BudgetExecutionDetailDto>
{
    public async Task<BudgetExecutionDetailDto> Handle(
        GetBudgetExecutionDetailQuery request,
        CancellationToken cancellationToken)
    {
        var budgetItem = await dbContext.BudgetItems
            .AsNoTracking()
            .Include(bi => bi.Budget)
            .FirstAsync(bi => bi.Id == request.BudgetItemId, cancellationToken);

        var appropriationIds = await dbContext.Appropriations
            .AsNoTracking()
            .Where(a => a.BudgetItemId == request.BudgetItemId
                     && a.Status != Domain.Budgeting.Enums.AppropriationStatus.Cancelled)
            .Select(a => a.Id)
            .ToListAsync(cancellationToken);

        var encumbrances = await dbContext.Encumbrances
            .AsNoTracking()
            .Include(e => e.Appropriation)
            .Where(e => appropriationIds.Contains(e.AppropriationId)
                     && e.Status != Domain.Budgeting.Enums.EncumbranceStatus.Cancelled)
            .Select(e => new EncumbranceDetailDto
            {
                EncumbranceId = e.Id,
                EncumbranceNumber = e.EncumbranceNumber,
                EncumbranceDate = e.EncumbranceDate,
                Amount = e.Amount,
                Status = e.Status.ToString()
            })
            .ToListAsync(cancellationToken);

        var encumbranceIds = encumbrances.Select(e => e.EncumbranceId).ToList();

        var payments = await dbContext.PaymentOrders
            .AsNoTracking()
            .Where(po => encumbranceIds.Contains(po.EncumbranceId ?? 0)
                      && po.Status != Domain.Payments.Enums.PaymentOrderStatus.Cancelled)
            .Select(po => new PaymentDetailDto
            {
                PaymentOrderId = po.Id,
                OrderNumber = po.PaymentOrderNumber,
                OrderDate = po.PaymentOrderDate,
                Amount = po.AmountGross,
                Status = po.Status.ToString(),
                PaidAt = po.PaidAt
            })
            .ToListAsync(cancellationToken);

        var fund = await dbContext.Funds
            .AsNoTracking()
            .FirstAsync(f => f.Id == budgetItem.Budget!.FundId, cancellationToken);

        return new BudgetExecutionDetailDto
        {
            BudgetItemId = request.BudgetItemId,
            ItemCode = budgetItem.ItemCode,
            ItemName = budgetItem.ItemName,
            FundId = fund.Id,
            FundNumber = fund.FundNumber,
            Encumbrances = encumbrances,
            Payments = payments
        };
    }
}
