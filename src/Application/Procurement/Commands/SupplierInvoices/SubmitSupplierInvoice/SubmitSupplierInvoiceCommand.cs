using ERP_Government.Application.Common.Security;

namespace ERP_Government.Application.Procurement.Commands.SupplierInvoices.SubmitSupplierInvoice;

[Authorize(Policy = PermissionCodes.SupplierInvoicesSubmit)]
public record SubmitSupplierInvoiceCommand(int Id) : IRequest<Result>;
