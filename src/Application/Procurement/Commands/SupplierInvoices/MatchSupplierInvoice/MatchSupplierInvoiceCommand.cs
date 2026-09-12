using ERP_Government.Application.Common.Security;

namespace ERP_Government.Application.Procurement.Commands.SupplierInvoices.MatchSupplierInvoice;

[Authorize(Policy = PermissionCodes.SupplierInvoicesMatch)]
public record MatchSupplierInvoiceCommand(int Id) : IRequest<Result>;
