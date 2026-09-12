using ERP_Government.Application.Common.Interfaces;
using ERP_Government.Domain.Procurement.Enums;

namespace ERP_Government.Application.Procurement.Commands.PurchaseRequests.ApprovePurchaseRequest;

public class ApprovePurchaseRequestCommandHandler(
    IApplicationDbContext context) : IRequestHandler<ApprovePurchaseRequestCommand, Result>
{
    public async Task<Result> Handle(
        ApprovePurchaseRequestCommand request,
        CancellationToken cancellationToken)
    {
        var entity = await context.PurchaseRequests.FindAsync(request.Id, cancellationToken);
        if (entity is null)
            return Result.Failure(["Purchase request not found."]);

        if (entity.Status != PurchaseRequestStatus.Submitted)
            return Result.Failure(["Only submitted purchase requests can be approved."]);

        entity.Status = PurchaseRequestStatus.Approved;
        entity.LastModified = DateTimeOffset.UtcNow;

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
