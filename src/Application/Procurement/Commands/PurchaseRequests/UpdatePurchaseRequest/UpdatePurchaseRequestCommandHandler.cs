using ERP_Government.Application.Common.Interfaces;
using ERP_Government.Domain.Procurement.Enums;

namespace ERP_Government.Application.Procurement.Commands.PurchaseRequests.UpdatePurchaseRequest;

public class UpdatePurchaseRequestCommandHandler(
    IApplicationDbContext context) : IRequestHandler<UpdatePurchaseRequestCommand, Result>
{
    public async Task<Result> Handle(
        UpdatePurchaseRequestCommand request,
        CancellationToken cancellationToken)
    {
        var entity = await context.PurchaseRequests.FindAsync(request.Id, cancellationToken);
        if (entity is null)
            return Result.Failure(["Purchase request not found."]);

        if (entity.Status != PurchaseRequestStatus.Draft)
            return Result.Failure(["Only draft purchase requests can be updated."]);

        entity.RequestDate = request.RequestDate;
        entity.RequiredDate = request.RequiredDate;
        entity.DepartmentId = request.DepartmentId;
        entity.CostCenterId = request.CostCenterId;
        entity.RequesterName = request.RequesterName;
        entity.Priority = request.Priority;
        entity.Notes = request.Notes;
        entity.LastModified = DateTimeOffset.UtcNow;

        var existingDetails = await context.PurchaseRequestDetails
            .Where(d => d.PurchaseRequestId == request.Id)
            .ToListAsync(cancellationToken);

        context.PurchaseRequestDetails.RemoveRange(existingDetails);

        foreach (var line in request.Lines)
        {
            context.PurchaseRequestDetails.Add(new Domain.Procurement.Entities.PurchaseRequestDetail
            {
                PurchaseRequestId = request.Id,
                ItemId = line.ItemId,
                UnitId = line.UnitId,
                RequestedQuantity = line.RequestedQuantity,
                UnitCostEstimate = line.UnitCostEstimate,
                TotalCostEstimate = line.RequestedQuantity * line.UnitCostEstimate,
                Notes = line.Notes,
                Created = DateTimeOffset.UtcNow,
                LastModified = DateTimeOffset.UtcNow
            });
        }

        entity.TotalEstimatedCost = request.Lines.Sum(l => (l.RequestedQuantity * l.UnitCostEstimate) ?? 0m);

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
