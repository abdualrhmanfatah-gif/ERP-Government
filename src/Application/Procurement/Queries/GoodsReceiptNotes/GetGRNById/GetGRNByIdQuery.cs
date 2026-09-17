using ERP_Government.Application.Common.Interfaces;
using ERP_Government.Domain.Procurement.Enums;
using ERP_Government.Application.Common.Security;

namespace ERP_Government.Application.Procurement.Queries.GoodsReceiptNotes.GetGRNById;

[Authorize(Policy = PermissionCodes.GoodsReceiptsView)]
public record GetGRNByIdQuery(int Id) : IRequest<Result<GRNDetailResponse>>;

public record GRNDetailResponse(
    int Id,
    string GRNNumber,
    int PurchaseOrderId,
    string? PurchaseOrderNumber,
    int? SupplierPartyId,
    int WarehouseId,
    int LocationId,
    int? ReceivedBy,
    GRNStatus Status,
    DateTime GRNDate,
    string? Notes,
    IReadOnlyList<GRNDetailLineResponse> Lines);

public record GRNDetailLineResponse(
    int Id,
    int PurchaseOrderDetailId,
    int ItemId,
    string? ItemNameAr,
    int UnitId,
    decimal OrderedQuantity,
    decimal ReceivedQuantity,
    decimal? AcceptedQuantity,
    decimal? RejectedQuantity,
    decimal RemainingQuantity,
    decimal? UnitCost,
    decimal? TotalCost,
    string? BatchNumber,
    DateOnly? ExpiryDate,
    string? Notes);
