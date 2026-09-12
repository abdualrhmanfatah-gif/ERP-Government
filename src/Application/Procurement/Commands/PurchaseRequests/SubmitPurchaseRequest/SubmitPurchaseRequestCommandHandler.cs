using ERP_Government.Application.Common.Interfaces;
using ERP_Government.Domain.Procurement.Enums;

namespace ERP_Government.Application.Procurement.Commands.PurchaseRequests.SubmitPurchaseRequest;

public class SubmitPurchaseRequestCommandHandler(
    IApplicationDbContext context) : IRequestHandler<SubmitPurchaseRequestCommand, Result>
{
    public async Task<Result> Handle(
        SubmitPurchaseRequestCommand request,
        CancellationToken cancellationToken)
    {
        var entity = await context.PurchaseRequests.FindAsync(request.Id, cancellationToken);
        if (entity is null)
            return Result.Failure(["Purchase request not found."]);

        if (entity.Status != PurchaseRequestStatus.Draft)
            return Result.Failure(["Only draft purchase requests can be submitted."]);

        entity.Status = PurchaseRequestStatus.Submitted;
        entity.LastModified = DateTimeOffset.UtcNow;

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
