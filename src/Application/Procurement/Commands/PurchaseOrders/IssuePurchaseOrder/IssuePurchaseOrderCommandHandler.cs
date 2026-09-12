using ERP_Government.Application.Common.Interfaces;
using ERP_Government.Application.Parties.Common;
using ERP_Government.Domain.Procurement.Enums;
using ERP_Government.Domain.Security.Entities;

namespace ERP_Government.Application.Procurement.Commands.PurchaseOrders.IssuePurchaseOrder;

public class IssuePurchaseOrderCommandHandler(
    IApplicationDbContext context,
    IDocumentStatusLogger statusLogger,
    IUser user) : IRequestHandler<IssuePurchaseOrderCommand, Result>
{
    public async Task<Result> Handle(
        IssuePurchaseOrderCommand request,
        CancellationToken cancellationToken)
    {
        if (user.Id is not int userId)
            return Result.Failure(["User identity is required for this operation."]);

        var entity = await context.PurchaseOrders.FindAsync(request.Id, cancellationToken);
        if (entity is null)
            return Result.Failure(["Purchase order not found."]);

        if (entity.Status != PurchaseOrderStatus.Approved)
            return Result.Failure(["Only approved purchase orders can be issued."]);

        var previousStatus = entity.Status;
        entity.Status = PurchaseOrderStatus.Issued;
        entity.LastModified = DateTimeOffset.UtcNow;

        await statusLogger.LogAsync(
            "purchaseorders",
            entity.Id,
            previousStatus.ToString(),
            PurchaseOrderStatus.Issued.ToString(),
            userId,
            null,
            cancellationToken);

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
