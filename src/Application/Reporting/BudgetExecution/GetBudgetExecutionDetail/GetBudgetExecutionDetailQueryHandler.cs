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

        var openEncumbranceStatuses = new[]
        {
            Domain.Budgeting.Enums.EncumbranceStatus.Active,
            Domain.Budgeting.Enums.EncumbranceStatus.PartiallyReleased,
            Domain.Budgeting.Enums.EncumbranceStatus.PartiallyLiquidated,
        };

        var encumbrances = await dbContext.EncumbranceLines
            .AsNoTracking()
            .Where(l => l.BudgetItemId == request.BudgetItemId
                && openEncumbranceStatuses.Contains(l.Encumbrance.Status)
                && l.Encumbrance.ReversalOfId == null)
            .Select(l => new EncumbranceDetailDto
            {
                EncumbranceId = l.Encumbrance.Id,
                EncumbranceNumber = l.Encumbrance.EncumbranceNumber,
                EncumbranceDate = l.Encumbrance.EncumbranceDate,
                Amount = l.Amount - l.LiquidatedAmount - l.CancelledAmount,
                Status = l.Encumbrance.Status.ToString()
            })
            .ToListAsync(cancellationToken);

        var executedPaymentStatuses = new[]
        {
            Domain.Payments.Enums.PaymentOrderStatus.Approved,
            Domain.Payments.Enums.PaymentOrderStatus.SentToTreasury,
            Domain.Payments.Enums.PaymentOrderStatus.Paid,
        };

        var payments = await dbContext.PaymentOrders
            .AsNoTracking()
            .Join(dbContext.BudgetItemAllocations,
                po => po.BudgetItemAllocationId,
                alloc => alloc.Id,
                (po, alloc) => new { po, alloc.BudgetItemId })
            .Where(x => x.BudgetItemId == request.BudgetItemId
                && executedPaymentStatuses.Contains(x.po.Status))
            .Select(x => new PaymentDetailDto
            {
                PaymentOrderId = x.po.Id,
                OrderNumber = x.po.PaymentOrderNumber,
                OrderDate = x.po.PaymentOrderDate,
                Amount = x.po.AmountGross,
                Status = x.po.Status.ToString(),
                PaidAt = x.po.PaidAt
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
