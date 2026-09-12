using ERP_Government.Application.Common.Interfaces;
using ERP_Government.Application.FinancialSettings.Common.Services;
using ERP_Government.Domain.Procurement.Entities;
using ERP_Government.Domain.Procurement.Enums;

namespace ERP_Government.Application.Procurement.Commands.PurchaseRequests.CreatePurchaseRequest;

public class CreatePurchaseRequestCommandHandler(
    IApplicationDbContext context,
    IDocumentSequenceService sequenceService) : IRequestHandler<CreatePurchaseRequestCommand, Result<int>>
{
    public async Task<Result<int>> Handle(
        CreatePurchaseRequestCommand request,
        CancellationToken cancellationToken)
    {
        string number;
        try
        {
            number = await sequenceService.GenerateNextNumberAsync("PurchaseRequest", cancellationToken);
        }
        catch (DocumentSequenceException ex)
        {
            return Result<int>.Failure([ex.Message]);
        }

        var entity = new PurchaseRequest
        {
            RequestNumber = number,
            RequestDate = request.RequestDate,
            RequiredDate = request.RequiredDate,
            DepartmentId = request.DepartmentId,
            CostCenterId = request.CostCenterId,
            RequesterName = request.RequesterName,
            Priority = request.Priority,
            Status = PurchaseRequestStatus.Draft,
            TotalEstimatedCost = request.Lines.Sum(l => (l.RequestedQuantity * l.UnitCostEstimate) ?? 0m),
            Notes = request.Notes,
            Created = DateTimeOffset.UtcNow,
            LastModified = DateTimeOffset.UtcNow
        };

        foreach (var line in request.Lines)
        {
            entity.Details.Add(new PurchaseRequestDetail
            {
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

        context.PurchaseRequests.Add(entity);
        await context.SaveChangesAsync(cancellationToken);

        return Result<int>.Success(entity.Id);
    }
}
