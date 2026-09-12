using ERP_Government.Application.Common.Security;

namespace ERP_Government.Application.Procurement.Commands.PurchaseRequests.CancelPurchaseRequest;

[Authorize(Policy = PermissionCodes.PurchaseRequestsCancel)]
public record CancelPurchaseRequestCommand(int Id, string? Reason) : IRequest<Result>;
