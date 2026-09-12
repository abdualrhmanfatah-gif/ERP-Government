using ERP_Government.Application.Common.Security;

namespace ERP_Government.Application.Procurement.Commands.SupplierInvoices.CancelSupplierInvoice;

[Authorize(Policy = PermissionCodes.SupplierInvoicesCancel)]
public record CancelSupplierInvoiceCommand(int Id, string? Notes) : IRequest<Result>;
