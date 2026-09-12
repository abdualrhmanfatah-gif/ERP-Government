using ERP_Government.Application.Common.Interfaces;

namespace ERP_Government.Application.Procurement.Queries.GoodsReceiptNotes.GetGRNById;

public class GetGRNByIdQueryHandler(
    IApplicationDbContext context) : IRequestHandler<GetGRNByIdQuery, Result<GRNDetailResponse>>
{
    public async Task<Result<GRNDetailResponse>> Handle(GetGRNByIdQuery request, CancellationToken cancellationToken)
    {
        var entity = await context.GoodsReceiptNotes
            .Include(g => g.PurchaseOrder)
            .Include(g => g.Details)
            .FirstOrDefaultAsync(g => g.Id == request.Id, cancellationToken);

        if (entity is null)
            return Result<GRNDetailResponse>.Failure(["GRN not found."]);

        var response = new GRNDetailResponse(
            entity.Id,
            entity.GRNNumber,
            entity.PurchaseOrderId,
            entity.PurchaseOrder?.PONumber,
            entity.SupplierPartyId,
            entity.WarehouseId,
            entity.LocationId,
            entity.ReceivedBy,
            entity.Status,
            entity.GRNDate,
            entity.Notes,
            entity.Details.Select(d => new GRNDetailLineResponse(
                d.Id,
                d.PurchaseOrderDetailId,
                d.ItemId,
                null,
                d.UnitId,
                d.OrderedQuantity,
                d.ReceivedQuantity,
                d.AcceptedQuantity,
                d.RejectedQuantity,
                d.RemainingQuantity,
                d.UnitCost,
                d.TotalCost,
                d.BatchNumber,
                d.ExpiryDate,
                d.Notes)).ToList());

        return Result<GRNDetailResponse>.Success(response);
    }
}
