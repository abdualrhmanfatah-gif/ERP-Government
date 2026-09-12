using ERP_Government.Application.Common.Interfaces;

namespace ERP_Government.Application.Procurement.Queries.SupplierInvoices.GetSupplierInvoiceById;

public class GetSupplierInvoiceByIdQueryHandler(
    IApplicationDbContext context) : IRequestHandler<GetSupplierInvoiceByIdQuery, Result<SupplierInvoiceDetailResponse>>
{
    public async Task<Result<SupplierInvoiceDetailResponse>> Handle(
        GetSupplierInvoiceByIdQuery request,
        CancellationToken cancellationToken)
    {
        var entity = await context.SupplierInvoices
            .Include(i => i.PurchaseOrder)
            .Include(i => i.Details)
            .FirstOrDefaultAsync(i => i.Id == request.Id, cancellationToken);

        if (entity is null)
            return Result<SupplierInvoiceDetailResponse>.Failure(["Supplier invoice not found."]);

        var response = new SupplierInvoiceDetailResponse(
            entity.Id,
            entity.InvoiceNumber,
            entity.SupplierInvoiceNumber,
            entity.PurchaseOrderId,
            entity.PurchaseOrder?.PONumber,
            entity.SupplierPartyId,
            null,
            entity.InvoiceDate,
            entity.CurrencyCode,
            entity.ExchangeRate,
            entity.SubTotal,
            entity.DiscountAmount,
            entity.TaxAmount,
            entity.ShippingCost,
            entity.OtherCharges,
            entity.GrandTotal,
            entity.DueDate,
            entity.Status,
            entity.Notes,
            entity.Details.Select(d => new SupplierInvoiceDetailLineResponse(
                d.Id,
                d.PurchaseOrderDetailId,
                d.GoodsReceiptNoteDetailId,
                d.ItemId,
                null,
                d.Quantity,
                d.UnitPrice,
                d.DiscountAmount,
                d.TaxAmount,
                d.LineTotal,
                d.Notes)).ToList());

        return Result<SupplierInvoiceDetailResponse>.Success(response);
    }
}
