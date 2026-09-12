using ERP_Government.Application.Common.Interfaces;
using ERP_Government.Application.FinancialSettings.Common.Services;
using ERP_Government.Domain.Procurement.Entities;
using ERP_Government.Domain.Procurement.Enums;

namespace ERP_Government.Application.Procurement.Commands.PurchaseOrders.CreatePurchaseOrder;

public class CreatePurchaseOrderCommandHandler(
    IApplicationDbContext context,
    IDocumentSequenceService sequenceService) : IRequestHandler<CreatePurchaseOrderCommand, Result<int>>
{
    public async Task<Result<int>> Handle(
        CreatePurchaseOrderCommand request,
        CancellationToken cancellationToken)
    {
        string number;
        try
        {
            number = await sequenceService.GenerateNextNumberAsync("PurchaseOrder", cancellationToken);
        }
        catch (DocumentSequenceException ex)
        {
            return Result<int>.Failure([ex.Message]);
        }

        var entity = new PurchaseOrder
        {
            PONumber = number,
            PODate = DateTime.UtcNow,
            PurchaseRequestId = request.PurchaseRequestId,
            QuotationId = request.QuotationId,
            SupplierPartyId = request.SupplierPartyId,
            WarehouseId = request.WarehouseId,
            DeliveryLocationId = request.DeliveryLocationId,
            CurrencyCode = request.CurrencyCode,
            ExchangeRate = request.ExchangeRate,
            PaymentTerms = request.PaymentTerms,
            DeliveryTerms = request.DeliveryTerms,
            ExpectedDeliveryDate = request.ExpectedDeliveryDate,
            Status = PurchaseOrderStatus.Draft,
            Notes = request.Notes,
            Created = DateTimeOffset.UtcNow,
            LastModified = DateTimeOffset.UtcNow
        };

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

            entity.Details.Add(new PurchaseOrderDetail
            {
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

        entity.SubTotal = entity.Details.Sum(d => d.LineTotal ?? 0m);
        entity.DiscountAmount = entity.Details.Sum(d => d.DiscountAmount ?? 0m);
        entity.TaxAmount = entity.Details.Sum(d => d.TaxAmount ?? 0m);
        entity.GrandTotal = entity.SubTotal - entity.DiscountAmount + entity.TaxAmount
            + entity.ShippingCost + entity.OtherCharges;

        context.PurchaseOrders.Add(entity);
        await context.SaveChangesAsync(cancellationToken);

        return Result<int>.Success(entity.Id);
    }
}
