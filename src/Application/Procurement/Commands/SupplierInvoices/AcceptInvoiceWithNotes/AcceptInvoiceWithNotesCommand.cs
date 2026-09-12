using ERP_Government.Application.Common.Security;

namespace ERP_Government.Application.Procurement.Commands.SupplierInvoices.AcceptInvoiceWithNotes;

[Authorize(Policy = PermissionCodes.SupplierInvoicesAcceptWithNotes)]
public record AcceptInvoiceWithNotesCommand(int Id, string Notes) : IRequest<Result>;
