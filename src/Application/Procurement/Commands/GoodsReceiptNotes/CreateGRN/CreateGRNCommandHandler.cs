using ERP_Government.Application.Common.Interfaces;
using ERP_Government.Application.FinancialSettings.Common.Services;
using ERP_Government.Domain.Procurement.Entities;
using ERP_Government.Domain.Procurement.Enums;

namespace ERP_Government.Application.Procurement.Commands.GoodsReceiptNotes.CreateGRN;

public class CreateGRNCommandHandler(
    IApplicationDbContext context,
    IDocumentSequenceService documentSequenceService) : IRequestHandler<CreateGRNCommand, Result<int>>
{
    public async Task<Result<int>> Handle(CreateGRNCommand request, CancellationToken cancellationToken)
    {
        var po = await context.PurchaseOrders.FindAsync(request.PurchaseOrderId, cancellationToken);
        if (po is null)
            return Result<int>.Failure(["Purchase order not found."]);

        if (po.Status != PurchaseOrderStatus.Issued && po.Status != PurchaseOrderStatus.PartiallyReceived)
            return Result<int>.Failure(["Purchase order must be Issued or Partially Received."]);

        var grnNumber = await documentSequenceService.GenerateNextNumberAsync("GRN", cancellationToken);

        var entity = new GoodsReceiptNote
        {
            GRNNumber = grnNumber,
            GRNDate = request.GRNDate,
            SupplierPartyId = po.SupplierPartyId,
            PurchaseOrderId = request.PurchaseOrderId,
            WarehouseId = request.WarehouseId,
            LocationId = request.LocationId,
            ReceivedBy = request.ReceivedBy,
            Status = GRNStatus.Draft,
            Notes = request.Notes,
            CreatedBy = null,
            Created = DateTimeOffset.UtcNow
        };

        foreach (var d in request.Details)
        {
            entity.Details.Add(new GoodsReceiptNoteDetail
            {
                PurchaseOrderDetailId = d.PurchaseOrderDetailId,
                ItemId = d.ItemId,
                UnitId = d.UnitId,
                OrderedQuantity = d.OrderedQuantity,
                ReceivedQuantity = d.ReceivedQuantity,
                AcceptedQuantity = d.AcceptedQuantity,
                RejectedQuantity = d.RejectedQuantity,
                RemainingQuantity = d.RemainingQuantity,
                UnitCost = d.UnitCost,
                TotalCost = d.TotalCost,
                BatchNumber = d.BatchNumber,
                ExpiryDate = d.ExpiryDate,
                Notes = d.Notes,
                CreatedBy = null,
                Created = DateTimeOffset.UtcNow
            });
        }

        context.GoodsReceiptNotes.Add(entity);
        await context.SaveChangesAsync(cancellationToken);

        return Result<int>.Success(entity.Id);
    }
}
