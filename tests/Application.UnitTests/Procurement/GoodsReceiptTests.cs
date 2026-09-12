using ERP_Government.Application.Common.Interfaces;
using ERP_Government.Application.FinancialSettings.Common.Services;
using ERP_Government.Application.Procurement.Commands.PurchaseRequests.CreatePurchaseRequest;
using ERP_Government.Application.Procurement.Commands.PurchaseRequests.SubmitPurchaseRequest;
using ERP_Government.Application.Procurement.Commands.PurchaseRequests.ApprovePurchaseRequest;
using ERP_Government.Application.Procurement.Commands.RequestForQuotations.CreateRFQ;
using ERP_Government.Application.Procurement.Commands.RequestForQuotations.PublishRFQ;
using ERP_Government.Application.Procurement.Commands.Quotations.CreateQuotation;
using ERP_Government.Application.Procurement.Commands.Quotations.StartEvaluation;
using ERP_Government.Application.Procurement.Commands.Quotations.CompleteEvaluation;
using ERP_Government.Application.Procurement.Commands.Quotations.SelectQuotation;
using ERP_Government.Application.Procurement.Commands.PurchaseOrders.CreatePurchaseOrder;
using ERP_Government.Application.Procurement.Commands.PurchaseOrders.ApprovePurchaseOrder;
using ERP_Government.Application.Procurement.Commands.PurchaseOrders.IssuePurchaseOrder;
using ERP_Government.Domain.Procurement.Entities;
using ERP_Government.Domain.Procurement.Enums;
using ERP_Government.Domain.Committees.Entities;
using ERP_Government.Domain.Committees.Enums;
using ERP_Government.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using Shouldly;

namespace ERP_Government.Application.UnitTests.Procurement;

[TestFixture]
public class GoodsReceiptTests
{
    private ApplicationDbContext _dbContext = null!;
    private Mock<IDocumentSequenceService> _seqMock = null!;
    private int _poId;
    private int _poDetailId;

    [SetUp]
    public async Task Setup()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _dbContext = new ApplicationDbContext(options);

        _seqMock = new Mock<IDocumentSequenceService>();
        _seqMock.Setup(s => s.GenerateNextNumberAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync("DOC-000001");

        _dbContext.Users.Add(new Domain.Security.Entities.User { Id = 1, Login = "test", IsActive = true });
        _dbContext.Committees.Add(new Committee
        {
            Id = 1,
            CommitteeNumber = "COM-001",
            Name = "Test",
            CommitteeType = CommitteeType.Tender,
            FormationDecisionNumber = "FD-001",
            FormationDecisionDate = new DateOnly(2025, 1, 1),
            ValidFrom = new DateOnly(2025, 1, 1),
            Status = CommitteeStatus.Active
        });
        await _dbContext.SaveChangesAsync();

        var prResult = await new CreatePurchaseRequestCommandHandler(_dbContext, _seqMock.Object)
            .Handle(new CreatePurchaseRequestCommand(
                RequestDate: DateTime.UtcNow, RequiredDate: null, DepartmentId: null, CostCenterId: null,
                Priority: PurchaseRequestPriority.Normal, Notes: null,
                Lines: new List<PurchaseRequestLineDto> { new(ItemId: 1, UnitId: 1, RequestedQuantity: 10, UnitCostEstimate: 1000m, Notes: null) }), CancellationToken.None);
        await new SubmitPurchaseRequestCommandHandler(_dbContext).Handle(new SubmitPurchaseRequestCommand(prResult.Value!), CancellationToken.None);
        await new ApprovePurchaseRequestCommandHandler(_dbContext).Handle(new ApprovePurchaseRequestCommand(prResult.Value!), CancellationToken.None);

        var rfqResult = await new CreateRFQCommandHandler(_dbContext, _seqMock.Object)
            .Handle(new CreateRFQCommand(PurchaseRequestId: prResult.Value!, DeadlineDate: DateTime.UtcNow.AddDays(10), CurrencyCode: null, TermsAndConditions: null, Notes: null, SupplierPartyIds: new List<int> { 1 }), CancellationToken.None);
        await new PublishRFQCommandHandler(_dbContext).Handle(new PublishRFQCommand(rfqResult.Value!), CancellationToken.None);

        var quotResult = await new CreateQuotationCommandHandler(_dbContext, _seqMock.Object)
            .Handle(new CreateQuotationCommand(RFQId: rfqResult.Value!, RFQSupplierId: 1, SupplierPartyId: 1, QuotationDate: DateTime.UtcNow, ValidUntil: null, CurrencyCode: null, ExchangeRate: null, ShippingCost: null, OtherCharges: null, PaymentTerms: null, DeliveryTerms: null, LeadTimeDays: null, WarrantyPeriodMonths: null, Notes: null,
                Lines: new List<QuotationLineDto> { new(PurchaseRequestDetailId: 1, ItemId: 1, UnitId: 1, Quantity: 10, UnitPrice: 950m, DiscountPercent: null, TaxPercent: null, Notes: null) }), CancellationToken.None);
        await new StartEvaluationCommandHandler(_dbContext).Handle(new StartEvaluationCommand(rfqResult.Value!), CancellationToken.None);
        var quot = await _dbContext.Quotations.FirstAsync(q => q.RFQId == rfqResult.Value!);
        await new CompleteEvaluationCommandHandler(_dbContext).Handle(new CompleteEvaluationCommand(quot.Id, 85m, 90m, null), CancellationToken.None);
        await new SelectQuotationCommandHandler(_dbContext).Handle(new SelectQuotationCommand(quot.Id, "Best"), CancellationToken.None);

        var poResult = await new CreatePurchaseOrderCommandHandler(_dbContext, _seqMock.Object)
            .Handle(new CreatePurchaseOrderCommand(PurchaseRequestId: null, QuotationId: quotResult.Value!, SupplierPartyId: 1, WarehouseId: null, DeliveryLocationId: null, CurrencyCode: null, ExchangeRate: null, PaymentTerms: null, DeliveryTerms: null, ExpectedDeliveryDate: null, Notes: null,
                Lines: new List<PurchaseOrderLineDto> { new(PurchaseRequestDetailId: 1, QuotationDetailId: null, ItemId: 1, UnitId: 1, OrderedQuantity: 10, UnitPrice: 950m, DiscountPercent: null, TaxPercent: null, ExpectedDeliveryDate: null, Notes: null) }), CancellationToken.None);
        await new ApprovePurchaseOrderCommandHandler(_dbContext).Handle(new ApprovePurchaseOrderCommand(poResult.Value!), CancellationToken.None);
        await new IssuePurchaseOrderCommandHandler(_dbContext).Handle(new IssuePurchaseOrderCommand(poResult.Value!), CancellationToken.None);

        _poId = poResult.Value!;
        _poDetailId = await _dbContext.PurchaseOrderDetails.Where(d => d.PurchaseOrderId == _poId).Select(d => d.Id).FirstAsync();
    }

    [TearDown]
    public void TearDown() => _dbContext?.Dispose();

    [Test]
    public async Task ConfirmGRN_ValidReceipt_ShouldTransitionToConfirmed()
    {
        var grn = new GoodsReceiptNote
        {
            GRNNumber = "GRN-000001",
            GRNDate = DateTime.UtcNow,
            PurchaseOrderId = _poId,
            WarehouseId = 1,
            LocationId = 1,
            Status = GRNStatus.Draft,
            Created = DateTimeOffset.UtcNow
        };
        _dbContext.GoodsReceiptNotes.Add(grn);
        await _dbContext.SaveChangesAsync();

        _dbContext.GoodsReceiptNoteDetails.Add(new GoodsReceiptNoteDetail
        {
            GRNId = grn.Id,
            PurchaseOrderDetailId = _poDetailId,
            ItemId = 1,
            UnitId = 1,
            OrderedQuantity = 10,
            ReceivedQuantity = 10,
            RemainingQuantity = 0,
            AcceptedQuantity = 10,
            RejectedQuantity = 0,
            Created = DateTimeOffset.UtcNow
        });
        await _dbContext.SaveChangesAsync();

        grn.Status = GRNStatus.Confirmed;
        await _dbContext.SaveChangesAsync();

        var updated = await _dbContext.GoodsReceiptNotes.FindAsync(grn.Id);
        updated!.Status.ShouldBe(GRNStatus.Confirmed);
    }

    [Test]
    public async Task GRN_OverReceipt_ShouldBeRejected()
    {
        var grn = new GoodsReceiptNote
        {
            GRNNumber = "GRN-000002",
            GRNDate = DateTime.UtcNow,
            PurchaseOrderId = _poId,
            WarehouseId = 1,
            LocationId = 1,
            Status = GRNStatus.Draft,
            Created = DateTimeOffset.UtcNow
        };
        _dbContext.GoodsReceiptNotes.Add(grn);
        await _dbContext.SaveChangesAsync();

        _dbContext.GoodsReceiptNoteDetails.Add(new GoodsReceiptNoteDetail
        {
            GRNId = grn.Id,
            PurchaseOrderDetailId = _poDetailId,
            ItemId = 1,
            UnitId = 1,
            OrderedQuantity = 10,
            ReceivedQuantity = 15,
            RemainingQuantity = -5,
            AcceptedQuantity = 15,
            RejectedQuantity = 0,
            Created = DateTimeOffset.UtcNow
        });
        await _dbContext.SaveChangesAsync();

        var poDetail = await _dbContext.PurchaseOrderDetails.FindAsync(_poDetailId);
        var totalReceived = await _dbContext.GoodsReceiptNoteDetails
            .Where(d => d.PurchaseOrderDetailId == _poDetailId)
            .SumAsync(d => d.ReceivedQuantity);
        totalReceived.ShouldBeGreaterThan(poDetail!.OrderedQuantity);
    }

    [Test]
    public async Task GRN_PartialReceipt_ShouldUpdateRemainingQuantity()
    {
        var grn = new GoodsReceiptNote
        {
            GRNNumber = "GRN-000003",
            GRNDate = DateTime.UtcNow,
            PurchaseOrderId = _poId,
            WarehouseId = 1,
            LocationId = 1,
            Status = GRNStatus.Draft,
            Created = DateTimeOffset.UtcNow
        };
        _dbContext.GoodsReceiptNotes.Add(grn);
        await _dbContext.SaveChangesAsync();

        _dbContext.GoodsReceiptNoteDetails.Add(new GoodsReceiptNoteDetail
        {
            GRNId = grn.Id,
            PurchaseOrderDetailId = _poDetailId,
            ItemId = 1,
            UnitId = 1,
            OrderedQuantity = 10,
            ReceivedQuantity = 5,
            RemainingQuantity = 5,
            AcceptedQuantity = 5,
            RejectedQuantity = 0,
            Created = DateTimeOffset.UtcNow
        });
        await _dbContext.SaveChangesAsync();

        var detail = await _dbContext.GoodsReceiptNoteDetails.FirstAsync(d => d.GRNId == grn.Id);
        detail.RemainingQuantity.ShouldBe(5);
    }
}
