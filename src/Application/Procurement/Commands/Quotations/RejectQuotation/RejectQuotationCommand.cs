using ERP_Government.Application.Common.Security;

namespace ERP_Government.Application.Procurement.Commands.Quotations.RejectQuotation;

[Authorize(Policy = PermissionCodes.QuotationsEvaluate)]
public record RejectQuotationCommand(int Id, string RejectionReason) : IRequest<Result>;
