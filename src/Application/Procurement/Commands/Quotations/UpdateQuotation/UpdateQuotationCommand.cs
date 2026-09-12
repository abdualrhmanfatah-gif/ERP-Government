using ERP_Government.Application.Common.Security;

namespace ERP_Government.Application.Procurement.Commands.Quotations.UpdateQuotation;

[Authorize(Policy = PermissionCodes.QuotationsCreate)]
public record UpdateQuotationCommand(
    int Id,
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
    List<ERP_Government.Application.Procurement.Commands.Quotations.CreateQuotation.QuotationLineDto> Lines) : IRequest<Result>;
