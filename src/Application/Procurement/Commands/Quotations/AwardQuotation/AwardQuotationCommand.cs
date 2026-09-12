using ERP_Government.Application.Common.Security;

namespace ERP_Government.Application.Procurement.Commands.Quotations.AwardQuotation;

[Authorize(Policy = PermissionCodes.QuotationsAward)]
public record AwardQuotationCommand(int Id) : IRequest<Result>;
