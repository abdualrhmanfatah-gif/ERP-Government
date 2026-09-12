using ERP_Government.Application.Common.Interfaces;

namespace ERP_Government.Application.Procurement.Queries.PurchaseOrders.GetPurchaseOrderById;

public class GetPurchaseOrderByIdQueryHandler(
    IApplicationDbContext context) : IRequestHandler<GetPurchaseOrderByIdQuery, Result<PurchaseOrderDetailResponse>>
{
    public async Task<Result<PurchaseOrderDetailResponse>> Handle(
        GetPurchaseOrderByIdQuery request,
        CancellationToken cancellationToken)
    {
        var entity = await context.PurchaseOrders
            .Include(po => po.Details)
            .FirstOrDefaultAsync(po => po.Id == request.Id, cancellationToken);

        if (entity is null)
            return Result<PurchaseOrderDetailResponse>.Failure(["Purchase order not found."]);

        var supplierName = await context.Parties
            .Where(p => p.Id == entity.SupplierPartyId)
            .Select(p => p.NameAr)
            .FirstOrDefaultAsync(cancellationToken);

        var response = new PurchaseOrderDetailResponse(
            entity.Id,
            entity.PONumber,
            entity.PurchaseRequestId,
            entity.SupplierPartyId,
            supplierName,
            entity.QuotationId,
            entity.Status,
            entity.PODate,
            entity.ExpectedDeliveryDate,
            entity.PaymentTerms,
            entity.DeliveryTerms,
            entity.SubTotal,
            entity.DiscountAmount,
            entity.TaxAmount,
            entity.ShippingCost,
            entity.OtherCharges,
            entity.GrandTotal,
            entity.Notes,
            entity.Details.Select(d => new PurchaseOrderDetailLineResponse(
                d.Id,
                d.ItemId,
                d.UnitId,
                d.PurchaseRequestDetailId,
                d.QuotationDetailId,
                d.OrderedQuantity,
                d.ReceivedQuantity,
                d.RemainingQuantity,
                d.UnitPrice,
                d.DiscountPercent,
                d.DiscountAmount,
                d.NetUnitPrice,
                d.TaxPercent,
                d.TaxAmount,
                d.LineTotal,
                d.LineTotalWithTax,
                d.Notes)).ToList());

        return Result<PurchaseOrderDetailResponse>.Success(response);
    }
}
