using ERP_Government.Domain.Procurement.Enums;
using ERP_Government.Application.Common.Security;

namespace ERP_Government.Application.Procurement.Queries.Quotations.GetQuotationById;

[Authorize(Policy = PermissionCodes.QuotationsView)]
public record GetQuotationByIdQuery(int Id) : IRequest<Result<QuotationDetailResponse>>;

public record QuotationDetailResponse(
    int Id,
    string QuotationNumber,
    int SupplierPartyId,
    DateTime QuotationDate,
    DateTime? ValidUntil,
    string? CurrencyCode,
    decimal? ExchangeRate,
    decimal? SubTotal,
    decimal? DiscountAmount,
    decimal? TaxAmount,
    decimal? ShippingCost,
    decimal? OtherCharges,
    decimal? GrandTotal,
    string? PaymentTerms,
    string? DeliveryTerms,
    int? LeadTimeDays,
    int? WarrantyPeriodMonths,
    QuotationStatus Status,
    decimal? TechnicalScore,
    decimal? FinancialScore,
    string? SelectionReason,
    string? RejectionReason,
    string? Notes,
    IReadOnlyList<QuotationDetailLineResponse> Lines);

public record QuotationDetailLineResponse(
    int Id,
    int PurchaseRequestDetailId,
    int ItemId,
    int UnitId,
    decimal Quantity,
    decimal? UnitPrice,
    decimal? DiscountPercent,
    decimal? DiscountAmount,
    decimal? NetUnitPrice,
    decimal? TaxPercent,
    decimal? TaxAmount,
    decimal? LineTotal,
    decimal? LineTotalWithTax,
    string? Notes);
