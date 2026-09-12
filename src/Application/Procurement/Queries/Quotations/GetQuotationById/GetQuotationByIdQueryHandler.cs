using ERP_Government.Application.Common.Interfaces;
using ERP_Government.Domain.Procurement.Enums;

namespace ERP_Government.Application.Procurement.Queries.Quotations.GetQuotationById;

public class GetQuotationByIdQueryHandler(
    IApplicationDbContext context) : IRequestHandler<GetQuotationByIdQuery, Result<QuotationDetailResponse>>
{
    public async Task<Result<QuotationDetailResponse>> Handle(
        GetQuotationByIdQuery request,
        CancellationToken cancellationToken)
    {
        var entity = await context.Quotations
            .Include(q => q.Details)
            .FirstOrDefaultAsync(q => q.Id == request.Id, cancellationToken);

        if (entity is null)
            return Result<QuotationDetailResponse>.Failure(["Quotation not found."]);

        var response = new QuotationDetailResponse(
            entity.Id,
            entity.QuotationNumber,
            entity.SupplierPartyId,
            entity.QuotationDate,
            entity.ValidUntil,
            entity.CurrencyCode,
            entity.ExchangeRate ?? 0,
            entity.SubTotal,
            entity.DiscountAmount,
            entity.TaxAmount,
            entity.ShippingCost,
            entity.OtherCharges,
            entity.GrandTotal,
            entity.PaymentTerms,
            entity.DeliveryTerms,
            entity.LeadTimeDays,
            entity.WarrantyPeriodMonths,
            entity.Status,
            entity.TechnicalScore,
            entity.FinancialScore,
            entity.SelectionReason,
            entity.RejectionReason,
            entity.Notes,
            entity.Details.Select(d => new QuotationDetailLineResponse(
                d.Id,
                d.PurchaseRequestDetailId,
                d.ItemId,
                d.UnitId,
                d.Quantity,
                d.UnitPrice,
                d.DiscountPercent,
                d.DiscountAmount,
                d.NetUnitPrice,
                d.TaxPercent,
                d.TaxAmount,
                d.LineTotal,
                d.LineTotalWithTax,
                d.Notes)).ToList());

        return Result<QuotationDetailResponse>.Success(response);
    }
}
