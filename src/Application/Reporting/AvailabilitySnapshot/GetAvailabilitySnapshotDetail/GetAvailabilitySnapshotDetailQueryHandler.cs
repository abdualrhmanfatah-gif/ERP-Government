using ERP_Government.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ERP_Government.Application.Reporting.AvailabilitySnapshot.GetAvailabilitySnapshotDetail;

internal class GetAvailabilitySnapshotDetailQueryHandler(IApplicationDbContext dbContext)
    : IRequestHandler<GetAvailabilitySnapshotDetailQuery, AvailabilitySnapshotDetailDto>
{
    public async Task<AvailabilitySnapshotDetailDto> Handle(
        GetAvailabilitySnapshotDetailQuery request,
        CancellationToken cancellationToken)
    {
        var budgetItem = await dbContext.BudgetItems
            .AsNoTracking()
            .Include(bi => bi.Budget)
            .FirstAsync(bi => bi.Id == request.BudgetItemId, cancellationToken);

        // Budget transactions for this item
        var transactionDtos = await dbContext.BudgetTransactions
            .AsNoTracking()
            .Where(t => t.BudgetItemAllocation.BudgetItemId == request.BudgetItemId)
            .Select(t => new BudgetTransactionDetailDto
            {
                TransactionId = t.Id,
                TransactionNumber = t.TransactionNumber,
                Type = t.TransactionType.ToString(),
                Amount = t.Direction == Domain.Budgeting.Enums.TransactionDirection.Increase ? t.Amount : -t.Amount,
                Status = t.Status.ToString()
            })
            .ToListAsync(cancellationToken);

        // Encumbrances for this item
        var encumbranceDtos = await dbContext.EncumbranceLines
            .AsNoTracking()
            .Where(l => l.BudgetItemId == request.BudgetItemId
                && l.Encumbrance.Status != Domain.Budgeting.Enums.EncumbranceStatus.Cancelled)
            .Select(l => new EncumbranceDetailDto
            {
                EncumbranceId = l.Encumbrance.Id,
                EncumbranceNumber = l.Encumbrance.EncumbranceNumber,
                Amount = l.Amount,
                Status = l.Encumbrance.Status.ToString()
            })
            .ToListAsync(cancellationToken);

        // Payments for this item (via BudgetItemAllocation)
        var paymentDtos = await dbContext.PaymentOrders
            .AsNoTracking()
            .Join(dbContext.BudgetItemAllocations,
                po => po.BudgetItemAllocationId,
                alloc => alloc.Id,
                (po, alloc) => new { po, alloc.BudgetItemId })
            .Where(x => x.BudgetItemId == request.BudgetItemId
                && x.po.Status != Domain.Payments.Enums.PaymentOrderStatus.Cancelled)
            .Select(x => new PaymentDetailDto
            {
                PaymentOrderId = x.po.Id,
                OrderNumber = x.po.PaymentOrderNumber,
                Amount = x.po.AmountGross,
                Status = x.po.Status.ToString()
            })
            .ToListAsync(cancellationToken);

        return new AvailabilitySnapshotDetailDto
        {
            BudgetItemId = request.BudgetItemId,
            ItemCode = budgetItem.ItemCode,
            ItemName = budgetItem.ItemName,
            Transactions = transactionDtos,
            Encumbrances = encumbranceDtos,
            Payments = paymentDtos
        };
    }
}
