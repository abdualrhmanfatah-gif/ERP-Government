using ERP_Government.Domain.Procurement.Enums;
using ERP_Government.Application.Common.Security;

namespace ERP_Government.Application.Procurement.Queries.GoodsReceiptNotes.GetGRNs;

[Authorize(Policy = PermissionCodes.GoodsReceiptsView)]
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
