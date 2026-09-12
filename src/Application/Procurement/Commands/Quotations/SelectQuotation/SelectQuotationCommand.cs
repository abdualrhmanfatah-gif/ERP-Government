using ERP_Government.Application.Common.Security;

namespace ERP_Government.Application.Procurement.Commands.Quotations.SelectQuotation;

[Authorize(Policy = PermissionCodes.QuotationsSelect)]
public record SelectQuotationCommand(int Id, string SelectionReason) : IRequest<Result>;
