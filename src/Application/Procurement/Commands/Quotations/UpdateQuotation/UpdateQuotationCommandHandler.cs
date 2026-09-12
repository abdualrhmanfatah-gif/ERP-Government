using ERP_Government.Application.Common.Interfaces;
using ERP_Government.Domain.Procurement.Enums;

namespace ERP_Government.Application.Procurement.Commands.Quotations.UpdateQuotation;

public class UpdateQuotationCommandHandler(
    IApplicationDbContext context) : IRequestHandler<UpdateQuotationCommand, Result>
{
    public async Task<Result> Handle(
        UpdateQuotationCommand request,
        CancellationToken cancellationToken)
    {
        var entity = await context.Quotations.FindAsync(request.Id, cancellationToken);
        if (entity is null)
            return Result.Failure(["Quotation not found."]);

        if (entity.Status != QuotationStatus.Draft)
            return Result.Failure(["Only draft quotations can be updated."]);

        entity.SupplierPartyId = request.SupplierPartyId;
        entity.QuotationDate = request.QuotationDate;
        entity.ValidUntil = request.ValidUntil;
        entity.CurrencyCode = request.CurrencyCode;
        entity.ExchangeRate = request.ExchangeRate;
        entity.ShippingCost = request.ShippingCost;
        entity.OtherCharges = request.OtherCharges;
        entity.PaymentTerms = request.PaymentTerms;
        entity.DeliveryTerms = request.DeliveryTerms;
        entity.LeadTimeDays = request.LeadTimeDays;
        entity.WarrantyPeriodMonths = request.WarrantyPeriodMonths;
        entity.Notes = request.Notes;
        entity.LastModified = DateTimeOffset.UtcNow;

        var existingDetails = await context.QuotationDetails
            .Where(d => d.QuotationId == request.Id)
            .ToListAsync(cancellationToken);

        context.QuotationDetails.RemoveRange(existingDetails);

        foreach (var line in request.Lines)
        {
            var discountAmount = line.DiscountPercent.HasValue
                ? line.Quantity * line.UnitPrice * line.DiscountPercent.Value / 100m
                : (decimal?)null;

            var netUnitPrice = discountAmount.HasValue
                ? line.UnitPrice - discountAmount.Value / line.Quantity
                : line.UnitPrice;

            var taxAmount = line.TaxPercent.HasValue
                ? line.Quantity * netUnitPrice * line.TaxPercent.Value / 100m
                : (decimal?)null;

            var lineTotal = line.Quantity * netUnitPrice;
            var lineTotalWithTax = lineTotal + taxAmount;

            context.QuotationDetails.Add(new Domain.Procurement.Entities.QuotationDetail
            {
                QuotationId = request.Id,
                PurchaseRequestDetailId = line.PurchaseRequestDetailId,
                ItemId = line.ItemId,
                UnitId = line.UnitId,
                Quantity = line.Quantity,
                UnitPrice = line.UnitPrice,
                DiscountPercent = line.DiscountPercent,
                DiscountAmount = discountAmount,
                NetUnitPrice = netUnitPrice,
                TaxPercent = line.TaxPercent,
                TaxAmount = taxAmount,
                LineTotal = lineTotal,
                LineTotalWithTax = lineTotalWithTax,
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

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
