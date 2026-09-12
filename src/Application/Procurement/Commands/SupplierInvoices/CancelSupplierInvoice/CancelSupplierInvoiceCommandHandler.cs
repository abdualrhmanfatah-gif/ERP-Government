using ERP_Government.Application.Common.Interfaces;
using ERP_Government.Domain.Procurement.Enums;

namespace ERP_Government.Application.Procurement.Commands.SupplierInvoices.CancelSupplierInvoice;

public class CancelSupplierInvoiceCommandHandler(
    IApplicationDbContext context) : IRequestHandler<CancelSupplierInvoiceCommand, Result>
{
    public async Task<Result> Handle(CancelSupplierInvoiceCommand request, CancellationToken cancellationToken)
    {
        var entity = await context.SupplierInvoices.FindAsync(request.Id, cancellationToken);
        if (entity is null)
            return Result.Failure(["Supplier invoice not found."]);

        if (entity.Status == SupplierInvoiceStatus.Cancelled)
            return Result.Failure(["Invoice is already cancelled."]);

        if (entity.Status == SupplierInvoiceStatus.Paid || entity.Status == SupplierInvoiceStatus.PartiallyPaid)
            return Result.Failure(["Cannot cancel a paid invoice."]);

        entity.Status = SupplierInvoiceStatus.Cancelled;
        entity.Notes = request.Notes;
        entity.LastModified = DateTimeOffset.UtcNow;

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
