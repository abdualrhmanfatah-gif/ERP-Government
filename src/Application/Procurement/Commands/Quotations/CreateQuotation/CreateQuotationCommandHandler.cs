using ERP_Government.Application.Common.Interfaces;
using ERP_Government.Application.FinancialSettings.Common.Services;
using ERP_Government.Domain.Procurement.Entities;
using ERP_Government.Domain.Procurement.Enums;

namespace ERP_Government.Application.Procurement.Commands.Quotations.CreateQuotation;

public class CreateQuotationCommandHandler(
    IApplicationDbContext context,
    IDocumentSequenceService sequenceService) : IRequestHandler<CreateQuotationCommand, Result<int>>
{
    public async Task<Result<int>> Handle(
        CreateQuotationCommand request,
        CancellationToken cancellationToken)
    {
        string number;
        try
        {
            number = await sequenceService.GenerateNextNumberAsync("Quotation", cancellationToken);
        }
        catch (DocumentSequenceException ex)
        {
            return Result<int>.Failure([ex.Message]);
        }

        var entity = new Quotation
        {
            QuotationNumber = number,
            SupplierPartyId = request.SupplierPartyId,
            QuotationDate = request.QuotationDate,
            ValidUntil = request.ValidUntil,
            CurrencyCode = request.CurrencyCode,
            ExchangeRate = request.ExchangeRate,
            ShippingCost = request.ShippingCost,
            OtherCharges = request.OtherCharges,
            PaymentTerms = request.PaymentTerms,
            DeliveryTerms = request.DeliveryTerms,
            LeadTimeDays = request.LeadTimeDays,
            WarrantyPeriodMonths = request.WarrantyPeriodMonths,
            Status = QuotationStatus.Draft,
            Notes = request.Notes,
            Created = DateTimeOffset.UtcNow,
            LastModified = DateTimeOffset.UtcNow
        };

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

            entity.Details.Add(new QuotationDetail
            {
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

        context.Quotations.Add(entity);
        await context.SaveChangesAsync(cancellationToken);

        return Result<int>.Success(entity.Id);
    }
}
