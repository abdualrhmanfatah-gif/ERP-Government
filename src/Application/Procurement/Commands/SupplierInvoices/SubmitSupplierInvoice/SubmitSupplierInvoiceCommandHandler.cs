using ERP_Government.Application.Common.Interfaces;
using ERP_Government.Domain.Procurement.Enums;

namespace ERP_Government.Application.Procurement.Commands.SupplierInvoices.SubmitSupplierInvoice;

public class SubmitSupplierInvoiceCommandHandler(
    IApplicationDbContext context) : IRequestHandler<SubmitSupplierInvoiceCommand, Result>
{
    public async Task<Result> Handle(SubmitSupplierInvoiceCommand request, CancellationToken cancellationToken)
    {
        var entity = await context.SupplierInvoices.FindAsync(request.Id, cancellationToken);
        if (entity is null)
            return Result.Failure(["Supplier invoice not found."]);

        if (entity.Status != SupplierInvoiceStatus.Draft)
            return Result.Failure(["Only draft invoices can be submitted."]);

        entity.Status = SupplierInvoiceStatus.Submitted;
        entity.LastModified = DateTimeOffset.UtcNow;

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
