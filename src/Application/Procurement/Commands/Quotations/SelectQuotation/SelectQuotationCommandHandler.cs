using ERP_Government.Application.Common.Interfaces;
using ERP_Government.Domain.Procurement.Enums;

namespace ERP_Government.Application.Procurement.Commands.Quotations.SelectQuotation;

public class SelectQuotationCommandHandler(
    IApplicationDbContext context) : IRequestHandler<SelectQuotationCommand, Result>
{
    public async Task<Result> Handle(
        SelectQuotationCommand request,
        CancellationToken cancellationToken)
    {
        var entity = await context.Quotations.FindAsync(request.Id, cancellationToken);
        if (entity is null)
            return Result.Failure(["Quotation not found."]);

        if (entity.Status != QuotationStatus.Evaluated)
            return Result.Failure(["Only evaluated quotations can be selected."]);

        entity.SelectionReason = request.SelectionReason;
        entity.Status = QuotationStatus.Selected;
        entity.LastModified = DateTimeOffset.UtcNow;

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
