using ERP_Government.Application.Common.Security;

namespace ERP_Government.Application.Procurement.Commands.PurchaseRequests.ApprovePurchaseRequest;

[Authorize(Policy = PermissionCodes.PurchaseRequestsApprove)]
public record ApprovePurchaseRequestCommand(int Id) : IRequest<Result>;
