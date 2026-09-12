using ERP_Government.Application.Common.Security;
using ERP_Government.Domain.Procurement.Enums;

namespace ERP_Government.Application.Procurement.Commands.GoodsReceiptNotes.CreateGRN;

[Authorize(Policy = PermissionCodes.GoodsReceiptsCreate)]
public record CreateGRNCommand(
    DateTime GRNDate,
    int PurchaseOrderId,
    int WarehouseId,
    int LocationId,
    int? ReceivedBy,
    string? Notes,
    List<CreateGRNDetailDto> Details) : IRequest<Result<int>>;

public record CreateGRNDetailDto(
    int PurchaseOrderDetailId,
    int ItemId,
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
