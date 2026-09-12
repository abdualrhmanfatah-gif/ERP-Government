using ERP_Government.Application.Common.Interfaces;
using ERP_Government.Domain.Procurement.Enums;

namespace ERP_Government.Application.Procurement.Commands.Quotations.SubmitQuotation;

public class SubmitQuotationCommandHandler(
    IApplicationDbContext context) : IRequestHandler<SubmitQuotationCommand, Result>
{
    public async Task<Result> Handle(
        SubmitQuotationCommand request,
        CancellationToken cancellationToken)
    {
        var entity = await context.Quotations.FindAsync(request.Id, cancellationToken);
        if (entity is null)
            return Result.Failure(["Quotation not found."]);

        if (entity.Status != QuotationStatus.Draft)
            return Result.Failure(["Only draft quotations can be submitted."]);

        entity.Status = QuotationStatus.Submitted;
        entity.LastModified = DateTimeOffset.UtcNow;

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
