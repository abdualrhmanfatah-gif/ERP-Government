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

        var appropriationQuery = dbContext.Appropriations
            .AsNoTracking()
            .Where(a => a.BudgetItemId == request.BudgetItemId
                     && a.Status != Domain.Budgeting.Enums.AppropriationStatus.Cancelled);

        var appropriationDtos = await appropriationQuery
            .Select(a => new AppropriationDetailDto
            {
                AppropriationId = a.Id,
                AppropriationNumber = a.AppropriationNumber,
                Type = a.AppropriationType.ToString(),
                Amount = a.Amount,
                Status = a.Status.ToString()
            })
            .ToListAsync(cancellationToken);

        var appropriationIds = await appropriationQuery
            .Select(a => a.Id)
            .ToListAsync(cancellationToken);

        var encumbranceDtos = await dbContext.Encumbrances
            .AsNoTracking()
            .Where(e => appropriationIds.Contains(e.AppropriationId)
                     && e.Status != Domain.Budgeting.Enums.EncumbranceStatus.Cancelled)
            .Select(e => new EncumbranceDetailDto
            {
                EncumbranceId = e.Id,
                EncumbranceNumber = e.EncumbranceNumber,
                Amount = e.Amount,
                Status = e.Status.ToString()
            })
            .ToListAsync(cancellationToken);

        var paymentDtos = await dbContext.PaymentOrders
            .AsNoTracking()
            .Where(po => appropriationIds.Contains(po.AppropriationId)
                      && po.Status != Domain.Payments.Enums.PaymentOrderStatus.Cancelled)
            .Select(po => new PaymentDetailDto
            {
                PaymentOrderId = po.Id,
                OrderNumber = po.PaymentOrderNumber,
                Amount = po.AmountGross,
                Status = po.Status.ToString()
            })
            .ToListAsync(cancellationToken);

        return new AvailabilitySnapshotDetailDto
        {
            BudgetItemId = request.BudgetItemId,
            ItemCode = budgetItem.ItemCode,
            ItemName = budgetItem.ItemName,
            Appropriations = appropriationDtos,
            Encumbrances = encumbranceDtos,
            Payments = paymentDtos
        };
    }
}
