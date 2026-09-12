using ERP_Government.Application.Common.Security;
using ERP_Government.Application.FinancialSettings.Common.Services;
using ERP_Government.Domain.Procurement.Enums;

namespace ERP_Government.Application.Procurement.Commands.PurchaseRequests.CreatePurchaseRequest;

[Authorize(Policy = PermissionCodes.PurchaseRequestsCreate)]
public record CreatePurchaseRequestCommand(
    DateTime RequestDate,
    DateOnly? RequiredDate,
    int? DepartmentId,
    int? CostCenterId,
    string RequesterName,
    PurchaseRequestPriority Priority,
    string? Notes,
    List<PurchaseRequestLineDto> Lines) : IRequest<Result<int>>;

public record PurchaseRequestLineDto(
    int ItemId,
    int UnitId,
    decimal RequestedQuantity,
    decimal? UnitCostEstimate,
    string? Notes);
