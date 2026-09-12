using ERP_Government.Application.Common.Interfaces;
using ERP_Government.Domain.Procurement.Enums;

namespace ERP_Government.Application.Procurement.Commands.PurchaseRequests.CancelPurchaseRequest;

public class CancelPurchaseRequestCommandHandler(
    IApplicationDbContext context) : IRequestHandler<CancelPurchaseRequestCommand, Result>
{
    public async Task<Result> Handle(
        CancelPurchaseRequestCommand request,
        CancellationToken cancellationToken)
    {
        var entity = await context.PurchaseRequests.FindAsync(request.Id, cancellationToken);
        if (entity is null)
            return Result.Failure(["Purchase request not found."]);

        if (entity.Status != PurchaseRequestStatus.Approved)
            return Result.Failure(["Only approved purchase requests can be cancelled."]);

        var hasDownstream = await context.PurchaseOrders.AnyAsync(p => p.PurchaseRequestId == request.Id, cancellationToken);

        if (hasDownstream)
            return Result.Failure(["Cannot cancel purchase request with existing purchase orders."]);

        entity.Status = PurchaseRequestStatus.Cancelled;
        entity.LastModified = DateTimeOffset.UtcNow;

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
