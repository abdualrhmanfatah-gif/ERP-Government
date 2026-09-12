using ERP_Government.Application.Common.Interfaces;
using ERP_Government.Domain.Procurement.Enums;

namespace ERP_Government.Application.Procurement.Commands.PurchaseRequests.RejectPurchaseRequest;

public class RejectPurchaseRequestCommandHandler(
    IApplicationDbContext context) : IRequestHandler<RejectPurchaseRequestCommand, Result>
{
    public async Task<Result> Handle(
        RejectPurchaseRequestCommand request,
        CancellationToken cancellationToken)
    {
        var entity = await context.PurchaseRequests.FindAsync(request.Id, cancellationToken);
        if (entity is null)
            return Result.Failure(["Purchase request not found."]);

        if (entity.Status != PurchaseRequestStatus.Submitted)
            return Result.Failure(["Only submitted purchase requests can be rejected."]);

        entity.Status = PurchaseRequestStatus.Rejected;
        entity.LastModified = DateTimeOffset.UtcNow;

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
