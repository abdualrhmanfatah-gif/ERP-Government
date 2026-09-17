using ERP_Government.Domain.Procurement.Enums;
using ERP_Government.Application.Common.Security;

namespace ERP_Government.Application.Procurement.Queries.PurchaseOrders.GetPurchaseOrderById;

[Authorize(Policy = PermissionCodes.PurchaseOrdersView)]
public record GetPurchaseOrderByIdQuery(int Id) : IRequest<Result<PurchaseOrderDetailResponse>>;

public record PurchaseOrderDetailResponse(
    int Id,
    string PONumber,
    int? PurchaseRequestId,
    int SupplierPartyId,
    string? SupplierName,
    int? QuotationId,
    PurchaseOrderStatus Status,
    DateTime? PODate,
    DateTime? ExpectedDeliveryDate,
    string? PaymentTerms,
    string? DeliveryTerms,
    decimal? SubTotal,
    decimal? DiscountAmount,
    decimal? TaxAmount,
    decimal? ShippingCost,
    decimal? OtherCharges,
    decimal? GrandTotal,
    string? Notes,
    IReadOnlyList<PurchaseOrderDetailLineResponse> Lines);

public record PurchaseOrderDetailLineResponse(
    int Id,
    int ItemId,
    int UnitId,
    int PurchaseRequestDetailId,
    int? QuotationDetailId,
    decimal OrderedQuantity,
    decimal ReceivedQuantity,
    decimal RemainingQuantity,
    decimal UnitPrice,
    decimal? DiscountPercent,
    decimal? DiscountAmount,
    decimal? NetUnitPrice,
    decimal? TaxPercent,
    decimal? TaxAmount,
    decimal? LineTotal,
    decimal? LineTotalWithTax,
    string? Notes);
