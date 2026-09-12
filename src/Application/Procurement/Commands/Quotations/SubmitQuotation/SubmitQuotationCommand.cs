using ERP_Government.Application.Common.Security;

namespace ERP_Government.Application.Procurement.Commands.Quotations.SubmitQuotation;

[Authorize(Policy = PermissionCodes.QuotationsCreate)]
public record SubmitQuotationCommand(int Id) : IRequest<Result>;
