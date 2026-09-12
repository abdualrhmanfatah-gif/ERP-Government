using ERP_Government.Application.Common.Security;

namespace ERP_Government.Application.Procurement.Commands.PurchaseRequests.SubmitPurchaseRequest;

[Authorize(Policy = PermissionCodes.PurchaseRequestsSubmit)]
public record SubmitPurchaseRequestCommand(int Id) : IRequest<Result>;
