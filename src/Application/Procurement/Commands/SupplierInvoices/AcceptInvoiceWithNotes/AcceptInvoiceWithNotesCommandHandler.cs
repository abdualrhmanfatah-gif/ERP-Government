using ERP_Government.Application.Common.Interfaces;
using ERP_Government.Domain.Procurement.Enums;

namespace ERP_Government.Application.Procurement.Commands.SupplierInvoices.AcceptInvoiceWithNotes;

public class AcceptInvoiceWithNotesCommandHandler(
    IApplicationDbContext context) : IRequestHandler<AcceptInvoiceWithNotesCommand, Result>
{
    public async Task<Result> Handle(AcceptInvoiceWithNotesCommand request, CancellationToken cancellationToken)
    {
        var entity = await context.SupplierInvoices.FindAsync(request.Id, cancellationToken);
        if (entity is null)
            return Result.Failure(["Supplier invoice not found."]);

        if (entity.Status == SupplierInvoiceStatus.Cancelled)
            return Result.Failure(["Cannot accept a cancelled invoice."]);

        entity.Notes = request.Notes;
        entity.LastModified = DateTimeOffset.UtcNow;

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
