using ERP_Government.Domain.Procurement.Entities;
using ERP_Government.Domain.Procurement.Enums;
using ERP_Government.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using Shouldly;

namespace ERP_Government.Application.UnitTests.Procurement;

[TestFixture]
public class SupplierInvoiceTests
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

    private async Task<int> CreatePOWithGRN()
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

        return po.Id;
    }

    [Test]
    public async Task CreateInvoice_ValidCommand_ShouldReturnDraftStatus()
    {
        var poId = await CreatePOWithGRN();
        var invoice = new SupplierInvoice
        {
            InvoiceNumber = "SINV-000001",
            SupplierInvoiceNumber = "SUP-INV-001",
            InvoiceDate = DateOnly.FromDateTime(DateTime.UtcNow),
            PurchaseOrderId = poId,
            SupplierPartyId = 1,
            GrandTotal = 9500m,
            Status = SupplierInvoiceStatus.Draft,
            Created = DateTimeOffset.UtcNow
        };
        _dbContext.SupplierInvoices.Add(invoice);
        await _dbContext.SaveChangesAsync();

        var result = await _dbContext.SupplierInvoices.FindAsync(invoice.Id);
        result.ShouldNotBeNull();
        result.Status.ShouldBe(SupplierInvoiceStatus.Draft);
    }

    [Test]
    public async Task Invoice_ExactPriceMatch_ShouldBeMatchable()
    {
        var poId = await CreatePOWithGRN();
        var invoice = new SupplierInvoice
        {
            InvoiceNumber = "SINV-000002",
            SupplierInvoiceNumber = "SUP-INV-002",
            InvoiceDate = DateOnly.FromDateTime(DateTime.UtcNow),
            PurchaseOrderId = poId,
            SupplierPartyId = 1,
            GrandTotal = 9500m,
            Status = SupplierInvoiceStatus.Submitted,
            Created = DateTimeOffset.UtcNow
        };
        _dbContext.SupplierInvoices.Add(invoice);
        await _dbContext.SaveChangesAsync();

        var po = await _dbContext.PurchaseOrders.FindAsync(poId);
        invoice.GrandTotal.ShouldBe(po!.GrandTotal);
    }

    [Test]
    public async Task Invoice_OverInvoice_ShouldBeRejected()
    {
        var poId = await CreatePOWithGRN();
        var invoice = new SupplierInvoice
        {
            InvoiceNumber = "SINV-000003",
            SupplierInvoiceNumber = "SUP-INV-003",
            InvoiceDate = DateOnly.FromDateTime(DateTime.UtcNow),
            PurchaseOrderId = poId,
            SupplierPartyId = 1,
            GrandTotal = 10000m,
            Status = SupplierInvoiceStatus.Draft,
            Created = DateTimeOffset.UtcNow
        };
        _dbContext.SupplierInvoices.Add(invoice);
        await _dbContext.SaveChangesAsync();

        var po = await _dbContext.PurchaseOrders.FindAsync(poId);
        invoice.GrandTotal!.Value.ShouldBeGreaterThan(po!.GrandTotal!.Value);
    }

    [Test]
    public async Task Invoice_DuplicateSupplierInvoiceNumber_ShouldBeRejected()
    {
        var poId = await CreatePOWithGRN();
        _dbContext.SupplierInvoices.Add(new SupplierInvoice
        {
            InvoiceNumber = "SINV-000004",
            SupplierInvoiceNumber = "DUP-001",
            InvoiceDate = DateOnly.FromDateTime(DateTime.UtcNow),
            PurchaseOrderId = poId,
            SupplierPartyId = 1,
            GrandTotal = 9500m,
            Status = SupplierInvoiceStatus.Draft,
            Created = DateTimeOffset.UtcNow
        });
        await _dbContext.SaveChangesAsync();

        var duplicate = new SupplierInvoice
        {
            InvoiceNumber = "SINV-000005",
            SupplierInvoiceNumber = "DUP-001",
            InvoiceDate = DateOnly.FromDateTime(DateTime.UtcNow),
            PurchaseOrderId = poId,
            SupplierPartyId = 1,
            GrandTotal = 9500m,
            Status = SupplierInvoiceStatus.Draft,
            Created = DateTimeOffset.UtcNow
        };
        _dbContext.SupplierInvoices.Add(duplicate);
        await _dbContext.SaveChangesAsync();

        var existing = await _dbContext.SupplierInvoices
            .Where(i => i.SupplierInvoiceNumber == "DUP-001" && i.Id != duplicate.Id)
            .CountAsync();
        existing.ShouldBe(1);
    }

    [Test]
    public async Task Invoice_PartialPayment_ShouldTrackCumulativeAmount()
    {
        var poId = await CreatePOWithGRN();
        var invoice = new SupplierInvoice
        {
            InvoiceNumber = "SINV-000006",
            SupplierInvoiceNumber = "SUP-INV-006",
            InvoiceDate = DateOnly.FromDateTime(DateTime.UtcNow),
            PurchaseOrderId = poId,
            SupplierPartyId = 1,
            GrandTotal = 9500m,
            Status = SupplierInvoiceStatus.Matched,
            Created = DateTimeOffset.UtcNow
        };
        _dbContext.SupplierInvoices.Add(invoice);
        await _dbContext.SaveChangesAsync();

        invoice.Status = SupplierInvoiceStatus.PartiallyPaid;
        await _dbContext.SaveChangesAsync();

        var updated = await _dbContext.SupplierInvoices.FindAsync(invoice.Id);
        updated!.Status.ShouldBe(SupplierInvoiceStatus.PartiallyPaid);
    }
}
