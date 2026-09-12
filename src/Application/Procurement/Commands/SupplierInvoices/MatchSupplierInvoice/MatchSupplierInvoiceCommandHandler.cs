using ERP_Government.Application.Common.Interfaces;
using ERP_Government.Domain.Procurement.Enums;

namespace ERP_Government.Application.Procurement.Commands.SupplierInvoices.MatchSupplierInvoice;

public class MatchSupplierInvoiceCommandHandler(
    IApplicationDbContext context) : IRequestHandler<MatchSupplierInvoiceCommand, Result>
{
    public async Task<Result> Handle(MatchSupplierInvoiceCommand request, CancellationToken cancellationToken)
    {
        var entity = await context.SupplierInvoices.FindAsync(request.Id, cancellationToken);
        if (entity is null)
            return Result.Failure(["Supplier invoice not found."]);

        if (entity.Status != SupplierInvoiceStatus.Submitted)
            return Result.Failure(["Only submitted invoices can be matched."]);

        entity.Status = SupplierInvoiceStatus.Matched;
        entity.LastModified = DateTimeOffset.UtcNow;

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
