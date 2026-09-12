using ERP_Government.Application.Common.Interfaces;
using ERP_Government.Domain.Procurement.Enums;

namespace ERP_Government.Application.Procurement.Commands.Quotations.AwardQuotation;

public class AwardQuotationCommandHandler(
    IApplicationDbContext context) : IRequestHandler<AwardQuotationCommand, Result>
{
    public async Task<Result> Handle(
        AwardQuotationCommand request,
        CancellationToken cancellationToken)
    {
        var entity = await context.Quotations.FindAsync(request.Id, cancellationToken);
        if (entity is null)
            return Result.Failure(["Quotation not found."]);

        if (entity.Status != QuotationStatus.Selected)
            return Result.Failure(["Only selected quotations can be awarded."]);

        entity.Status = QuotationStatus.Awarded;
        entity.LastModified = DateTimeOffset.UtcNow;

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
