using ERP_Government.Domain.Procurement.Enums;

namespace ERP_Government.Application.Procurement.Queries.SupplierInvoices.GetSupplierInvoiceById;

public record GetSupplierInvoiceByIdQuery(int Id) : IRequest<Result<SupplierInvoiceDetailResponse>>;

public record SupplierInvoiceDetailResponse(
    int Id,
    string InvoiceNumber,
    string SupplierInvoiceNumber,
    int PurchaseOrderId,
    string? PurchaseOrderNumber,
    int SupplierPartyId,
    string? SupplierName,
    DateOnly InvoiceDate,
    string? CurrencyCode,
    decimal? ExchangeRate,
    decimal? SubTotal,
    decimal? DiscountAmount,
    decimal? TaxAmount,
    decimal? ShippingCost,
    decimal? OtherCharges,
    decimal? GrandTotal,
    DateOnly? DueDate,
    SupplierInvoiceStatus Status,
    string? Notes,
    IReadOnlyList<SupplierInvoiceDetailLineResponse> Lines);

public record SupplierInvoiceDetailLineResponse(
    int Id,
    int PurchaseOrderDetailId,
    int? GoodsReceiptNoteDetailId,
    int ItemId,
    string? ItemNameAr,
    decimal Quantity,
    decimal UnitPrice,
    decimal? DiscountAmount,
    decimal? TaxAmount,
    decimal? LineTotal,
    string? Notes);
