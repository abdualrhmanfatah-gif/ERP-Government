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

        // Same status semantics as the main report (research D3) so detail sums
        // reconcile with the summary line figures.
        var openEncumbranceStatuses = new[]
        {
            Domain.Budgeting.Enums.EncumbranceStatus.Active,
            Domain.Budgeting.Enums.EncumbranceStatus.PartiallyReleased,
            Domain.Budgeting.Enums.EncumbranceStatus.PartiallyLiquidated,
        };

        var encumbrances = await dbContext.Encumbrances
            .AsNoTracking()
            .Include(e => e.Appropriation)
            .Where(e => appropriationIds.Contains(e.AppropriationId)
                     && openEncumbranceStatuses.Contains(e.Status))
            .Select(e => new EncumbranceDetailDto
            {
                EncumbranceId = e.Id,
                EncumbranceNumber = e.EncumbranceNumber,
                EncumbranceDate = e.EncumbranceDate,
                Amount = e.Amount,
                Status = e.Status.ToString()
            })
            .ToListAsync(cancellationToken);

        var executedPaymentStatuses = new[]
        {
            Domain.Payments.Enums.PaymentOrderStatus.Approved,
            Domain.Payments.Enums.PaymentOrderStatus.SentToTreasury,
            Domain.Payments.Enums.PaymentOrderStatus.Paid,
            Domain.Payments.Enums.PaymentOrderStatus.PartiallyPaid,
        };

        var payments = await dbContext.PaymentOrders
            .AsNoTracking()
            .Where(po => appropriationIds.Contains(po.AppropriationId)
                      && executedPaymentStatuses.Contains(po.Status))
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
