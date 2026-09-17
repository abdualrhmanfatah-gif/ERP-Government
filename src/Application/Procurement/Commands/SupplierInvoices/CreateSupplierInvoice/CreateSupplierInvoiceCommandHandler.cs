using ERP_Government.Application.Common.Interfaces;
using ERP_Government.Application.FinancialSettings.Common.Services;
using ERP_Government.Domain.Procurement.Entities;
using ERP_Government.Domain.Procurement.Enums;

namespace ERP_Government.Application.Procurement.Commands.SupplierInvoices.CreateSupplierInvoice;

public class CreateSupplierInvoiceCommandHandler(
    IApplicationDbContext context,
    IDocumentSequenceService documentSequenceService) : IRequestHandler<CreateSupplierInvoiceCommand, Result<int>>
{
    public async Task<Result<int>> Handle(CreateSupplierInvoiceCommand request, CancellationToken cancellationToken)
    {
        var po = await context.PurchaseOrders.FindAsync(request.PurchaseOrderId, cancellationToken);
        if (po is null)
            return Result<int>.Failure(["Purchase order not found."]);

        var invoiceNumber = await documentSequenceService.GenerateNextNumberAsync("SupplierInvoice", cancellationToken);

        var entity = new SupplierInvoice
        {
            InvoiceNumber = invoiceNumber,
            SupplierInvoiceNumber = request.SupplierInvoiceNumber,
            InvoiceDate = request.InvoiceDate,
            PurchaseOrderId = request.PurchaseOrderId,
            SupplierPartyId = po.SupplierPartyId,
            CurrencyCode = request.CurrencyCode,
            ExchangeRate = request.ExchangeRate,
            SubTotal = request.SubTotal,
            DiscountAmount = request.DiscountAmount,
            TaxAmount = request.TaxAmount,
            ShippingCost = request.ShippingCost,
            OtherCharges = request.OtherCharges,
            GrandTotal = request.GrandTotal,
            DueDate = request.DueDate,
            Status = SupplierInvoiceStatus.Draft,
            Notes = request.Notes,
            CreatedBy = null,
            Created = DateTimeOffset.UtcNow
        };

        foreach (var d in request.Details)
        {
            entity.Details.Add(new SupplierInvoiceDetail
            {
                PurchaseOrderDetailId = d.PurchaseOrderDetailId,
                GoodsReceiptNoteDetailId = d.GoodsReceiptNoteDetailId,
                ItemId = d.ItemId,
                Quantity = d.Quantity,
                UnitPrice = d.UnitPrice,
                DiscountAmount = d.DiscountAmount,
                TaxAmount = d.TaxAmount,
                LineTotal = d.LineTotal,
                Notes = d.Notes,
                CreatedBy = null,
                Created = DateTimeOffset.UtcNow
            });
        }

        context.SupplierInvoices.Add(entity);
        await context.SaveChangesAsync(cancellationToken);

        return Result<int>.Success(entity.Id);
    }
}
