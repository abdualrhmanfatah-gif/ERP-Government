using ERP_Government.Application.Common.Security;
using ERP_Government.Domain.Procurement.Enums;

namespace ERP_Government.Application.Procurement.Commands.PurchaseRequests.UpdatePurchaseRequest;

[Authorize(Policy = PermissionCodes.PurchaseRequestsCreate)]
public record UpdatePurchaseRequestCommand(
    int Id,
    DateTime RequestDate,
    DateOnly? RequiredDate,
    int? DepartmentId,
    int? CostCenterId,
    string RequesterName,
    PurchaseRequestPriority Priority,
    string? Notes,
    List<UpdatePurchaseRequestLineDto> Lines) : IRequest<Result>;

public record UpdatePurchaseRequestLineDto(
    int? Id,
    int ItemId,
    int UnitId,
    decimal RequestedQuantity,
    decimal? UnitCostEstimate,
    string? Notes);
