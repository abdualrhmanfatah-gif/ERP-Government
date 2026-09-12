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
using ERP_Government.Application.Procurement.Commands.PurchaseOrders.SubmitPurchaseOrder;
using ERP_Government.Application.Procurement.Commands.PurchaseOrders.UpdatePurchaseOrder;
using ERP_Government.Application.Procurement.Commands.PurchaseOrders.ClosePurchaseOrder;
using ERP_Government.Application.Procurement.Commands.PurchaseOrders.CancelPurchaseOrder;
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
public class PurchaseOrderTests
{
    private ApplicationDbContext _dbContext = null!;
    private Mock<IDocumentSequenceService> _seqMock = null!;
    private int _quotationId;

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
                RequesterName: "Test User", Priority: PurchaseRequestPriority.Normal, Notes: null,
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

        _quotationId = quotResult.Value!;
    }

    [TearDown]
    public void TearDown() => _dbContext?.Dispose();

    private async Task<int> CreateValidPO()
    {
        var result = await new CreatePurchaseOrderCommandHandler(_dbContext, _seqMock.Object)
            .Handle(new CreatePurchaseOrderCommand(
                PurchaseRequestId: null, QuotationId: _quotationId, SupplierPartyId: 1,
                WarehouseId: null, DeliveryLocationId: null, CurrencyCode: null, ExchangeRate: null,
                PaymentTerms: null, DeliveryTerms: null, ExpectedDeliveryDate: null, Notes: null,
                Lines: new List<PurchaseOrderLineDto> { new(Id: null, PurchaseRequestDetailId: 1, QuotationDetailId: null, ItemId: 1, UnitId: 1, OrderedQuantity: 10, UnitPrice: 950m, DiscountPercent: null, TaxPercent: null, ExpectedDeliveryDate: null, Notes: null) }), CancellationToken.None);
        result.Succeeded.ShouldBeTrue();
        return result.Value!;
    }

    [Test]
    public async Task CreatePO_ValidCommand_ShouldReturnDraftStatus()
    {
        var id = await CreateValidPO();
        var po = await _dbContext.PurchaseOrders.FindAsync(id);
        po.ShouldNotBeNull();
        po.Status.ShouldBe(PurchaseOrderStatus.Draft);
        po.PONumber.ShouldBe("DOC-000001");
    }

    [Test]
    public async Task ApprovePO_Draft_ShouldTransitionToApproved()
    {
        var id = await CreateValidPO();
        var result = await new ApprovePurchaseOrderCommandHandler(_dbContext)
            .Handle(new ApprovePurchaseOrderCommand(id), CancellationToken.None);

        result.Succeeded.ShouldBeTrue();
        var po = await _dbContext.PurchaseOrders.FindAsync(id);
        po!.Status.ShouldBe(PurchaseOrderStatus.Approved);
    }

    [Test]
    public async Task IssuePO_Approved_ShouldTransitionToIssued()
    {
        var id = await CreateValidPO();
        await new ApprovePurchaseOrderCommandHandler(_dbContext)
            .Handle(new ApprovePurchaseOrderCommand(id), CancellationToken.None);

        var result = await new IssuePurchaseOrderCommandHandler(_dbContext)
            .Handle(new IssuePurchaseOrderCommand(id), CancellationToken.None);

        result.Succeeded.ShouldBeTrue();
        var po = await _dbContext.PurchaseOrders.FindAsync(id);
        po!.Status.ShouldBe(PurchaseOrderStatus.Issued);
    }

    [Test]
    public async Task CancelPO_Draft_ShouldTransitionToCancelled()
    {
        var id = await CreateValidPO();
        var result = await new CancelPurchaseOrderCommandHandler(_dbContext)
            .Handle(new CancelPurchaseOrderCommand(id), CancellationToken.None);

        result.Succeeded.ShouldBeTrue();
        var po = await _dbContext.PurchaseOrders.FindAsync(id);
        po!.Status.ShouldBe(PurchaseOrderStatus.Cancelled);
    }

    [Test]
    public async Task SubmitPO_Draft_ShouldTransitionToSubmitted()
    {
        var id = await CreateValidPO();
        var result = await new SubmitPurchaseOrderCommandHandler(_dbContext)
            .Handle(new SubmitPurchaseOrderCommand(id), CancellationToken.None);

        result.Succeeded.ShouldBeTrue();
        var po = await _dbContext.PurchaseOrders.FindAsync(id);
        po!.Status.ShouldBe(PurchaseOrderStatus.Submitted);
    }

    [Test]
    public async Task UpdatePO_Draft_ShouldSucceed()
    {
        var id = await CreateValidPO();
        var result = await new UpdatePurchaseOrderCommandHandler(_dbContext)
            .Handle(new UpdatePurchaseOrderCommand(
                Id: id,
                PaymentTerms: "Net 30",
                DeliveryTerms: "FOB",
                ExpectedDeliveryDate: DateTime.UtcNow.AddDays(30),
                Notes: "Updated",
                Lines: new List<PurchaseOrderLineDto> { new(Id: null, PurchaseRequestDetailId: 1, QuotationDetailId: null, ItemId: 1, UnitId: 1, OrderedQuantity: 5, UnitPrice: 1000m, DiscountPercent: null, TaxPercent: null, ExpectedDeliveryDate: null, Notes: null) }), CancellationToken.None);

        result.Succeeded.ShouldBeTrue();
        var po = await _dbContext.PurchaseOrders.FindAsync(id);
        po!.PaymentTerms.ShouldBe("Net 30");
        po!.Notes.ShouldBe("Updated");
    }

    [Test]
    public async Task ClosePO_Issued_ShouldTransitionToClosed()
    {
        var id = await CreateValidPO();
        await new SubmitPurchaseOrderCommandHandler(_dbContext)
            .Handle(new SubmitPurchaseOrderCommand(id), CancellationToken.None);
        await new ApprovePurchaseOrderCommandHandler(_dbContext)
            .Handle(new ApprovePurchaseOrderCommand(id), CancellationToken.None);
        await new IssuePurchaseOrderCommandHandler(_dbContext)
            .Handle(new IssuePurchaseOrderCommand(id), CancellationToken.None);

        var result = await new ClosePurchaseOrderCommandHandler(_dbContext)
            .Handle(new ClosePurchaseOrderCommand(id, "Completed"), CancellationToken.None);

        result.Succeeded.ShouldBeTrue();
        var po = await _dbContext.PurchaseOrders.FindAsync(id);
        po!.Status.ShouldBe(PurchaseOrderStatus.Closed);
    }

    [Test]
    public async Task SubmitPO_NonDraft_ShouldFail()
    {
        var id = await CreateValidPO();
        await new SubmitPurchaseOrderCommandHandler(_dbContext)
            .Handle(new SubmitPurchaseOrderCommand(id), CancellationToken.None);

        var result = await new SubmitPurchaseOrderCommandHandler(_dbContext)
            .Handle(new SubmitPurchaseOrderCommand(id), CancellationToken.None);

        result.Succeeded.ShouldBeFalse();
    }

    [Test]
    public async Task ApprovePO_NonSubmitted_ShouldFail()
    {
        var id = await CreateValidPO();

        var result = await new ApprovePurchaseOrderCommandHandler(_dbContext)
            .Handle(new ApprovePurchaseOrderCommand(id), CancellationToken.None);

        result.Succeeded.ShouldBeFalse();
    }
}
