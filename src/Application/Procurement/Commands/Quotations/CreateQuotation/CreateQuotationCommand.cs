using ERP_Government.Application.Common.Security;
using ERP_Government.Application.FinancialSettings.Common.Services;

namespace ERP_Government.Application.Procurement.Commands.Quotations.CreateQuotation;

[Authorize(Policy = PermissionCodes.QuotationsCreate)]
public record CreateQuotationCommand(
    int SupplierPartyId,
    DateTime QuotationDate,
    DateTime? ValidUntil,
    string? CurrencyCode,
    decimal? ExchangeRate,
    decimal? ShippingCost,
    decimal? OtherCharges,
    string? PaymentTerms,
    string? DeliveryTerms,
    int? LeadTimeDays,
    int? WarrantyPeriodMonths,
    string? Notes,
    List<QuotationLineDto> Lines) : IRequest<Result<int>>;

public record QuotationLineDto(
    int PurchaseRequestDetailId,
    int ItemId,
    int UnitId,
    decimal Quantity,
    decimal UnitPrice,
    decimal? DiscountPercent,
    decimal? TaxPercent,
    string? Notes);
