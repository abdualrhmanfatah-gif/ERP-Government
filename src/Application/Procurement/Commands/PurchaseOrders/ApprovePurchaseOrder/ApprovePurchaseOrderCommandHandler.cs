using ERP_Government.Application.Common.Interfaces;
using ERP_Government.Application.Parties.Common;
using ERP_Government.Domain.Budgeting.Entities;
using ERP_Government.Domain.Budgeting.Enums;
using ERP_Government.Domain.Events.Procurement;
using ERP_Government.Domain.Procurement.Entities;
using ERP_Government.Domain.Procurement.Enums;
using ERP_Government.Domain.Security.Entities;
using MediatR;

namespace ERP_Government.Application.Procurement.Commands.PurchaseOrders.ApprovePurchaseOrder;

public class ApprovePurchaseOrderCommandHandler(
    IApplicationDbContext context,
    IDocumentStatusLogger statusLogger,
    IUser user,
    IPublisher publisher) : IRequestHandler<ApprovePurchaseOrderCommand, Result>
{
    public async Task<Result> Handle(
        ApprovePurchaseOrderCommand request,
        CancellationToken cancellationToken)
    {
        if (user.Id is not int userId)
            return Result.Failure(["User identity is required for this operation."]);

        var entity = await context.PurchaseOrders
            .Include(po => po.Details)
            .FirstOrDefaultAsync(po => po.Id == request.Id, cancellationToken);
        if (entity is null)
            return Result.Failure(["Purchase order not found."]);

        if (entity.Status != PurchaseOrderStatus.Submitted)
            return Result.Failure(["Only submitted purchase orders can be approved."]);

        var details = await context.PurchaseOrderDetails
            .Where(d => d.PurchaseOrderId == request.Id)
            .ToListAsync(cancellationToken);

        var encumbranceNumber = await context.Encumbrances
            .CountAsync() == 0
            ? "ENC-000001"
            : "ENC-" + (await context.Encumbrances.CountAsync(cancellationToken) + 1).ToString("D6");

        var encumbrance = new Encumbrance
        {
            EncumbranceNumber = encumbranceNumber,
            EncumbranceType = EncumbranceType.Commitment,
            VendorPartyId = entity.SupplierPartyId,
            PurchaseOrderId = entity.Id,
            DocumentType = "PurchaseOrder",
            DocumentId = entity.Id,
            Description = "PO " + entity.PONumber,
            EncumbranceDate = DateOnly.FromDateTime(DateTime.UtcNow),
            TotalAmount = details.Sum(d => d.LineTotal ?? d.OrderedQuantity * d.UnitPrice),
            Status = EncumbranceStatus.Draft
        };

        context.Encumbrances.Add(encumbrance);
        await context.SaveChangesAsync(cancellationToken);

        foreach (var detail in details)
        {
            var prDetail = await context.PurchaseRequestDetails
                .FirstOrDefaultAsync(prd => prd.Id == detail.PurchaseRequestDetailId, cancellationToken);
            var item = prDetail is null ? null : await context.Items.FirstOrDefaultAsync(i => i.Id == prDetail.ItemId, cancellationToken);
            if (item is null)
                continue;

            var allocations = await context.BudgetItemAllocations
                .Include(a => a.BudgetItem)
                .Where(a => a.BudgetItem.ItemCode == item.Code)
                .ToListAsync(cancellationToken);

            foreach (var allocation in allocations)
            {
                context.EncumbranceLines.Add(new EncumbranceLine
                {
                    EncumbranceId = encumbrance.Id,
                    BudgetItemId = allocation.BudgetItemId,
                    Amount = detail.NetUnitPrice ?? detail.UnitPrice * detail.OrderedQuantity,
                    Description = detail.Notes
                });
            }
        }

        await context.SaveChangesAsync(cancellationToken);

        var previousStatus = entity.Status;
        entity.Status = PurchaseOrderStatus.Approved;
        entity.LastModified = DateTimeOffset.UtcNow;

        await statusLogger.LogAsync(
            "purchaseorders",
            entity.Id,
            previousStatus.ToString(),
            PurchaseOrderStatus.Approved.ToString(),
            userId,
            null,
            cancellationToken);

        await context.SaveChangesAsync(cancellationToken);

        await publisher.Publish(new PurchaseOrderApproved
        {
            SourceEntityId = entity.Id,
            OccurredAt = DateTimeOffset.UtcNow,
            SupplierId = entity.SupplierPartyId,
            GrandTotal = entity.GrandTotal,
            CurrencyCode = entity.CurrencyCode
        }, cancellationToken);

        return Result.Success();
    }
}
