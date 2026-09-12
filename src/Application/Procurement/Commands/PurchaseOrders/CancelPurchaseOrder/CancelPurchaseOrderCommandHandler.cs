using ERP_Government.Application.Common.Interfaces;
using ERP_Government.Application.Parties.Common;
using ERP_Government.Domain.Procurement.Enums;
using ERP_Government.Domain.Security.Entities;

namespace ERP_Government.Application.Procurement.Commands.PurchaseOrders.CancelPurchaseOrder;

public class CancelPurchaseOrderCommandHandler(
    IApplicationDbContext context,
    IDocumentStatusLogger statusLogger,
    IUser user) : IRequestHandler<CancelPurchaseOrderCommand, Result>
{
    public async Task<Result> Handle(
        CancelPurchaseOrderCommand request,
        CancellationToken cancellationToken)
    {
        if (user.Id is not int userId)
            return Result.Failure(["User identity is required for this operation."]);

        var entity = await context.PurchaseOrders.FindAsync(request.Id, cancellationToken);
        if (entity is null)
            return Result.Failure(["Purchase order not found."]);

        if (entity.Status == PurchaseOrderStatus.Cancelled)
            return Result.Failure(["Purchase order is already cancelled."]);

        var previousStatus = entity.Status;
        entity.Status = PurchaseOrderStatus.Cancelled;
        entity.LastModified = DateTimeOffset.UtcNow;

        await statusLogger.LogAsync(
            "purchaseorders",
            entity.Id,
            previousStatus.ToString(),
            PurchaseOrderStatus.Cancelled.ToString(),
            userId,
            request.Reason,
            cancellationToken);

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
