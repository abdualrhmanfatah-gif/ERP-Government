using ERP_Government.Domain.Procurement.Enums;
using ERP_Government.Application.Common.Security;

namespace ERP_Government.Application.Procurement.Queries.PurchaseOrders.GetPurchaseOrders;

[Authorize(Policy = PermissionCodes.PurchaseOrdersView)]
public record GetPurchaseOrdersQuery(
    int? PurchaseRequestId = null,
    int? QuotationId = null,
    int? SupplierPartyId = null,
    PurchaseOrderStatus? Status = null,
    string? Search = null,
    DateTime? ExpectedDeliveryDateFrom = null,
    DateTime? ExpectedDeliveryDateTo = null,
    int Page = 1,
    int PageSize = 20) : IRequest<Result<PaginatedList<PurchaseOrderListItem>>>;

public record PurchaseOrderListItem(
    int Id,
    string PurchaseOrderNumber,
    int PurchaseRequestId,
    int SupplierPartyId,
    string? SupplierName,
    PurchaseOrderStatus Status,
    decimal? GrandTotal,
    DateTime? ExpectedDeliveryDate,
    DateTimeOffset Created);
