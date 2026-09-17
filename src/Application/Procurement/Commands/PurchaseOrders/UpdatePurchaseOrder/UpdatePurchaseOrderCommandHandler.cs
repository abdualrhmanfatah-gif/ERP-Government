using ERP_Government.Application.Common.Interfaces;
using ERP_Government.Domain.Procurement.Entities;
using ERP_Government.Domain.Procurement.Enums;

namespace ERP_Government.Application.Procurement.Commands.PurchaseOrders.UpdatePurchaseOrder;

public class UpdatePurchaseOrderCommandHandler(
    IApplicationDbContext context) : IRequestHandler<UpdatePurchaseOrderCommand, Result>
{
    public async Task<Result> Handle(
        UpdatePurchaseOrderCommand request,
        CancellationToken cancellationToken)
    {
        var entity = await context.PurchaseOrders
            .Include(po => po.Details)
            .FirstOrDefaultAsync(po => po.Id == request.Id, cancellationToken);

        if (entity is null)
            return Result.Failure(["Purchase order not found."]);

        if (entity.Status != PurchaseOrderStatus.Draft)
            return Result.Failure(["Only draft purchase orders can be updated."]);

        if (request.Lines is null)
            return Result.Failure(["Lines are required."]);

        entity.PaymentTerms = request.PaymentTerms;
        entity.DeliveryTerms = request.DeliveryTerms;
        entity.ExpectedDeliveryDate = request.ExpectedDeliveryDate;
        entity.Notes = request.Notes;
        entity.LastModified = DateTimeOffset.UtcNow;

        var existingLines = entity.Details.ToDictionary(d => d.Id);
        var requestLineIds = new HashSet<int>();
        foreach (var line in request.Lines)
        {
            if (line.Id.HasValue)
                requestLineIds.Add(line.Id.Value);
        }

        var linesToRemove = existingLines.Keys.Where(id => !requestLineIds.Contains(id)).ToList();
        foreach (var lineId in linesToRemove)
        {
            var line = existingLines[lineId];
            entity.Details.Remove(line);
            context.PurchaseOrderDetails.Remove(line);
        }

        foreach (var line in request.Lines)
        {
            var discountAmount = line.DiscountPercent.HasValue
                ? line.OrderedQuantity * line.UnitPrice * line.DiscountPercent.Value / 100m
                : (decimal?)null;

            var netUnitPrice = discountAmount.HasValue
                ? line.UnitPrice - discountAmount.Value / line.OrderedQuantity
                : line.UnitPrice;

            var taxAmount = line.TaxPercent.HasValue
                ? line.OrderedQuantity * netUnitPrice * line.TaxPercent.Value / 100m
                : (decimal?)null;

            var lineTotal = line.OrderedQuantity * netUnitPrice;
            var lineTotalWithTax = lineTotal + taxAmount;

            if (line.Id.HasValue && existingLines.TryGetValue(line.Id.Value, out var existingLine))
            {
                existingLine.PurchaseRequestDetailId = line.PurchaseRequestDetailId;
                existingLine.QuotationDetailId = line.QuotationDetailId;
                existingLine.ItemId = line.ItemId;
                existingLine.UnitId = line.UnitId;
                existingLine.OrderedQuantity = line.OrderedQuantity;
                existingLine.UnitPrice = line.UnitPrice;
                existingLine.DiscountPercent = line.DiscountPercent;
                existingLine.DiscountAmount = discountAmount;
                existingLine.NetUnitPrice = netUnitPrice;
                existingLine.TaxPercent = line.TaxPercent;
                existingLine.TaxAmount = taxAmount;
                existingLine.LineTotal = lineTotal;
                existingLine.LineTotalWithTax = lineTotalWithTax;
                existingLine.ExpectedDeliveryDate = line.ExpectedDeliveryDate;
                existingLine.Notes = line.Notes;
                existingLine.LastModified = DateTimeOffset.UtcNow;
            }
            else
            {
                entity.Details.Add(new PurchaseOrderDetail
                {
                    PurchaseOrderId = entity.Id,
                    PurchaseRequestDetailId = line.PurchaseRequestDetailId,
                    QuotationDetailId = line.QuotationDetailId,
                    ItemId = line.ItemId,
                    UnitId = line.UnitId,
                    OrderedQuantity = line.OrderedQuantity,
                    ReceivedQuantity = 0m,
                    RemainingQuantity = line.OrderedQuantity,
                    UnitPrice = line.UnitPrice,
                    DiscountPercent = line.DiscountPercent,
                    DiscountAmount = discountAmount,
                    NetUnitPrice = netUnitPrice,
                    TaxPercent = line.TaxPercent,
                    TaxAmount = taxAmount,
                    LineTotal = lineTotal,
                    LineTotalWithTax = lineTotalWithTax,
                    ExpectedDeliveryDate = line.ExpectedDeliveryDate,
                    Status = PurchaseOrderDetailStatus.Pending,
                    Notes = line.Notes,
                    Created = DateTimeOffset.UtcNow,
                    LastModified = DateTimeOffset.UtcNow
                });
            }
        }

        entity.SubTotal = entity.Details.Sum(d => d.LineTotal ?? 0m);
        entity.DiscountAmount = entity.Details.Sum(d => d.DiscountAmount ?? 0m);
        entity.TaxAmount = entity.Details.Sum(d => d.TaxAmount ?? 0m);
        entity.GrandTotal = entity.SubTotal - entity.DiscountAmount + entity.TaxAmount
            + entity.ShippingCost + entity.OtherCharges;

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
