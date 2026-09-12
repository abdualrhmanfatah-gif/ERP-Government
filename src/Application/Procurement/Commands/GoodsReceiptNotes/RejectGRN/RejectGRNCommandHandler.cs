using ERP_Government.Application.Common.Interfaces;
using ERP_Government.Domain.Procurement.Enums;

namespace ERP_Government.Application.Procurement.Commands.GoodsReceiptNotes.RejectGRN;

public class RejectGRNCommandHandler(
    IApplicationDbContext context) : IRequestHandler<RejectGRNCommand, Result>
{
    public async Task<Result> Handle(RejectGRNCommand request, CancellationToken cancellationToken)
    {
        var entity = await context.GoodsReceiptNotes.FindAsync(request.Id, cancellationToken);
        if (entity is null)
            return Result.Failure(["Goods receipt note not found."]);

        if (entity.Status != GRNStatus.Draft)
            return Result.Failure(["Only draft GRNs can be rejected."]);

        entity.Status = GRNStatus.Rejected;
        entity.Notes = request.Notes;
        entity.LastModified = DateTimeOffset.UtcNow;

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
