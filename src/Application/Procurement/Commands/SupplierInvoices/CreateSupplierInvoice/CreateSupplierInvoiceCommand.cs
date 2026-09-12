using ERP_Government.Application.Common.Security;
using ERP_Government.Domain.Procurement.Enums;

namespace ERP_Government.Application.Procurement.Commands.SupplierInvoices.CreateSupplierInvoice;

[Authorize(Policy = PermissionCodes.SupplierInvoicesCreate)]
public record CreateSupplierInvoiceCommand(
    int PurchaseOrderId,
    string SupplierInvoiceNumber,
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
    string? Notes,
    List<CreateSupplierInvoiceDetailDto> Details) : IRequest<Result<int>>;

public record CreateSupplierInvoiceDetailDto(
    int PurchaseOrderDetailId,
    int? GoodsReceiptNoteDetailId,
    int ItemId,
    decimal Quantity,
    decimal UnitPrice,
    decimal? DiscountAmount,
    decimal? TaxAmount,
    decimal? LineTotal,
    string? Notes);
