using ERP_Government.Application.Common.Interfaces;
using ERP_Government.Domain.Procurement.Enums;

namespace ERP_Government.Application.Procurement.Commands.Quotations.RejectQuotation;

public class RejectQuotationCommandHandler(
    IApplicationDbContext context) : IRequestHandler<RejectQuotationCommand, Result>
{
    public async Task<Result> Handle(
        RejectQuotationCommand request,
        CancellationToken cancellationToken)
    {
        var entity = await context.Quotations.FindAsync(request.Id, cancellationToken);
        if (entity is null)
            return Result.Failure(["Quotation not found."]);

        if (entity.Status != QuotationStatus.UnderEvaluation)
            return Result.Failure(["Only quotations under evaluation can be rejected."]);

        entity.Status = QuotationStatus.Rejected;
        entity.RejectionReason = request.RejectionReason;
        entity.LastModified = DateTimeOffset.UtcNow;

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
