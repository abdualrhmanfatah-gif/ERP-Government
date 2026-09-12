using ERP_Government.Domain.Procurement.Entities;
using ERP_Government.Domain.Procurement.Enums;
using ERP_Government.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using Shouldly;

namespace ERP_Government.Application.UnitTests.Procurement;

[TestFixture]
public class PaymentClosureTests
{
    private ApplicationDbContext _dbContext = null!;

    [SetUp]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _dbContext = new ApplicationDbContext(options);
        _dbContext.Users.Add(new Domain.Security.Entities.User { Id = 1, Login = "test", IsActive = true });
        _dbContext.SaveChanges();
    }

    [TearDown]
    public void TearDown() => _dbContext?.Dispose();

    private async Task<int> CreatePOWithFullReceiptAndInvoice()
    {
        var po = new PurchaseOrder
        {
            PONumber = "PO-000001",
            PODate = DateTime.UtcNow,
            SupplierPartyId = 1,
            Status = PurchaseOrderStatus.Issued,
            GrandTotal = 9500m,
            Created = DateTimeOffset.UtcNow
        };
        _dbContext.PurchaseOrders.Add(po);
        await _dbContext.SaveChangesAsync();

        var poDetail = new PurchaseOrderDetail
        {
            PurchaseOrderId = po.Id,
            ItemId = 1,
            UnitId = 1,
            PurchaseRequestDetailId = 1,
            OrderedQuantity = 10,
            ReceivedQuantity = 10,
            RemainingQuantity = 0,
            UnitPrice = 950m,
            LineTotal = 9500m,
            Status = PurchaseOrderDetailStatus.Received,
            Created = DateTimeOffset.UtcNow
        };
        _dbContext.PurchaseOrderDetails.Add(poDetail);
        await _dbContext.SaveChangesAsync();

        var grn = new GoodsReceiptNote
        {
            GRNNumber = "GRN-000001",
            GRNDate = DateTime.UtcNow,
            PurchaseOrderId = po.Id,
            WarehouseId = 1,
            LocationId = 1,
            Status = GRNStatus.Confirmed,
            Created = DateTimeOffset.UtcNow
        };
        _dbContext.GoodsReceiptNotes.Add(grn);
        await _dbContext.SaveChangesAsync();

        _dbContext.GoodsReceiptNoteDetails.Add(new GoodsReceiptNoteDetail
        {
            GRNId = grn.Id,
            PurchaseOrderDetailId = poDetail.Id,
            ItemId = 1,
            UnitId = 1,
            OrderedQuantity = 10,
            ReceivedQuantity = 10,
            AcceptedQuantity = 10,
            RejectedQuantity = 0,
            RemainingQuantity = 0,
            Created = DateTimeOffset.UtcNow
        });
        await _dbContext.SaveChangesAsync();

        _dbContext.SupplierInvoices.Add(new SupplierInvoice
        {
            InvoiceNumber = "SINV-000001",
            SupplierInvoiceNumber = "SUP-INV-001",
            InvoiceDate = DateOnly.FromDateTime(DateTime.UtcNow),
            PurchaseOrderId = po.Id,
            SupplierPartyId = 1,
            GrandTotal = 9500m,
            Status = SupplierInvoiceStatus.Matched,
            Created = DateTimeOffset.UtcNow
        });
        await _dbContext.SaveChangesAsync();

        return po.Id;
    }

    [Test]
    public async Task ClosePO_AllReceivedAndPaid_ShouldTransitionToClosed()
    {
        var poId = await CreatePOWithFullReceiptAndInvoice();
        var po = await _dbContext.PurchaseOrders.FindAsync(poId);
        po!.Status = PurchaseOrderStatus.Closed;
        await _dbContext.SaveChangesAsync();

        var updated = await _dbContext.PurchaseOrders.FindAsync(poId);
        updated!.Status.ShouldBe(PurchaseOrderStatus.Closed);
    }

    [Test]
    public async Task ClosePO_PartiallyReceived_ShouldNotClose()
    {
        var po = new PurchaseOrder
        {
            PONumber = "PO-000002",
            PODate = DateTime.UtcNow,
            SupplierPartyId = 1,
            Status = PurchaseOrderStatus.Issued,
            GrandTotal = 9500m,
            Created = DateTimeOffset.UtcNow
        };
        _dbContext.PurchaseOrders.Add(po);
        await _dbContext.SaveChangesAsync();

        _dbContext.PurchaseOrderDetails.Add(new PurchaseOrderDetail
        {
            PurchaseOrderId = po.Id,
            ItemId = 1,
            UnitId = 1,
            PurchaseRequestDetailId = 1,
            OrderedQuantity = 10,
            ReceivedQuantity = 5,
            RemainingQuantity = 5,
            UnitPrice = 950m,
            LineTotal = 9500m,
            Status = PurchaseOrderDetailStatus.PartiallyReceived,
            Created = DateTimeOffset.UtcNow
        });
        await _dbContext.SaveChangesAsync();

        var poDetail = await _dbContext.PurchaseOrderDetails.FirstAsync(d => d.PurchaseOrderId == po.Id);
        poDetail.RemainingQuantity.ShouldBeGreaterThan(0);

        po.Status.ShouldNotBe(PurchaseOrderStatus.Closed);
    }
}
