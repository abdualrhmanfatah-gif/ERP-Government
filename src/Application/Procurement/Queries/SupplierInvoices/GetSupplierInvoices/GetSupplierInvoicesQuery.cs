using ERP_Government.Domain.Procurement.Enums;

namespace ERP_Government.Application.Procurement.Queries.SupplierInvoices.GetSupplierInvoices;

public record GetSupplierInvoicesQuery(
    int? PurchaseOrderId = null,
    SupplierInvoiceStatus? Status = null,
    string? Search = null,
    int Page = 1,
    int PageSize = 20) : IRequest<Result<PaginatedList<SupplierInvoiceListItem>>>;

public record SupplierInvoiceListItem(
    int Id,
    string InvoiceNumber,
    string SupplierInvoiceNumber,
    int PurchaseOrderId,
    string? PurchaseOrderNumber,
    int SupplierPartyId,
    DateOnly InvoiceDate,
    SupplierInvoiceStatus Status,
    decimal? GrandTotal,
    DateTimeOffset Created);
