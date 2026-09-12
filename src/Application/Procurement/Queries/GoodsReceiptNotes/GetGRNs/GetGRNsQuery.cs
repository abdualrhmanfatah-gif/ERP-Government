using ERP_Government.Domain.Procurement.Enums;

namespace ERP_Government.Application.Procurement.Queries.GoodsReceiptNotes.GetGRNs;

public record GetGRNsQuery(
    int? PurchaseOrderId = null,
    GRNStatus? Status = null,
    string? Search = null,
    int Page = 1,
    int PageSize = 20) : IRequest<Result<PaginatedList<GRNListItem>>>;

public record GRNListItem(
    int Id,
    string GRNNumber,
    int PurchaseOrderId,
    string? PurchaseOrderNumber,
    int? SupplierPartyId,
    GRNStatus Status,
    DateTime GRNDate,
    DateTimeOffset Created);
