using ERP_Government.Application.Common.Security;

namespace ERP_Government.Application.Procurement.Commands.PurchaseRequests.RejectPurchaseRequest;

[Authorize(Policy = PermissionCodes.PurchaseRequestsReject)]
public record RejectPurchaseRequestCommand(int Id, string Reason) : IRequest<Result>;
