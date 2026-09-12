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
public class QuotationTests
{
    private ApplicationDbContext _dbContext = null!;
    private Mock<IDocumentSequenceService> _prSeqMock = null!;
    private Mock<IDocumentSequenceService> _rfqSeqMock = null!;
    private Mock<IDocumentSequenceService> _quotSeqMock = null!;
    private int _rfqId;

    [SetUp]
    public async Task Setup()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _dbContext = new ApplicationDbContext(options);

        _prSeqMock = new Mock<IDocumentSequenceService>();
        _rfqSeqMock = new Mock<IDocumentSequenceService>();
        _quotSeqMock = new Mock<IDocumentSequenceService>();
        _prSeqMock.Setup(s => s.GenerateNextNumberAsync("PurchaseRequest", It.IsAny<CancellationToken>())).ReturnsAsync("PR-000001");
        _rfqSeqMock.Setup(s => s.GenerateNextNumberAsync("RequestForQuotation", It.IsAny<CancellationToken>())).ReturnsAsync("RFQ-000001");
        _quotSeqMock.Setup(s => s.GenerateNextNumberAsync("Quotation", It.IsAny<CancellationToken>())).ReturnsAsync("QT-000001");

        _dbContext.Users.Add(new Domain.Security.Entities.User { Id = 1, Login = "test", IsActive = true });
        _dbContext.Committees.Add(new Committee
        {
            Id = 1,
            CommitteeNumber = "COM-001",
            Name = "Test Committee",
            CommitteeType = CommitteeType.Tender,
            FormationDecisionNumber = "FD-001",
            FormationDecisionDate = new DateOnly(2025, 1, 1),
            ValidFrom = new DateOnly(2025, 1, 1),
            Status = CommitteeStatus.Active
        });
        await _dbContext.SaveChangesAsync();

        var prResult = await new CreatePurchaseRequestCommandHandler(_dbContext, _prSeqMock.Object)
            .Handle(new CreatePurchaseRequestCommand(
                RequestDate: DateTime.UtcNow,
                RequiredDate: null,
                DepartmentId: null,
                CostCenterId: null,
                Priority: PurchaseRequestPriority.Normal,
                Notes: null,
                Lines: new List<PurchaseRequestLineDto>
                {
                    new(ItemId: 1, UnitId: 1, RequestedQuantity: 10, UnitCostEstimate: 1000m, Notes: null)
                }), CancellationToken.None);

        await new SubmitPurchaseRequestCommandHandler(_dbContext)
            .Handle(new SubmitPurchaseRequestCommand(prResult.Value!), CancellationToken.None);
        await new ApprovePurchaseRequestCommandHandler(_dbContext)
            .Handle(new ApprovePurchaseRequestCommand(prResult.Value!), CancellationToken.None);

        var rfqResult = await new CreateRFQCommandHandler(_dbContext, _rfqSeqMock.Object)
            .Handle(new CreateRFQCommand(
                PurchaseRequestId: prResult.Value!,
                DeadlineDate: DateTime.UtcNow.AddDays(10),
                CurrencyCode: null,
                TermsAndConditions: null,
                Notes: null,
                SupplierPartyIds: new List<int> { 1, 2 }), CancellationToken.None);
        _rfqId = rfqResult.Value!;

        await new PublishRFQCommandHandler(_dbContext)
            .Handle(new PublishRFQCommand(_rfqId), CancellationToken.None);
    }

    [TearDown]
    public void TearDown() => _dbContext?.Dispose();

    [Test]
    public async Task CreateQuotation_ValidCommand_ShouldReturnDraftStatus()
    {
        var result = await new CreateQuotationCommandHandler(_dbContext, _quotSeqMock.Object)
            .Handle(new CreateQuotationCommand(
                RFQId: _rfqId,
                RFQSupplierId: 1,
                SupplierPartyId: 1,
                QuotationDate: DateTime.UtcNow,
                ValidUntil: null,
                CurrencyCode: null,
                ExchangeRate: null,
                ShippingCost: null,
                OtherCharges: null,
                PaymentTerms: null,
                DeliveryTerms: null,
                LeadTimeDays: null,
                WarrantyPeriodMonths: null,
                Notes: null,
                Lines: new List<QuotationLineDto>
                {
                    new(PurchaseRequestDetailId: 1, ItemId: 1, UnitId: 1, Quantity: 10, UnitPrice: 950m, DiscountPercent: null, TaxPercent: null, Notes: null)
                }), CancellationToken.None);

        result.Succeeded.ShouldBeTrue();
        var quotation = await _dbContext.Quotations.FindAsync(result.Value);
        quotation.ShouldNotBeNull();
        quotation.Status.ShouldBe(QuotationStatus.Draft);
    }

    [Test]
    public async Task StartEvaluation_WithQuotations_ShouldTransitionToCollectingResponses()
    {
        await new CreateQuotationCommandHandler(_dbContext, _quotSeqMock.Object)
            .Handle(new CreateQuotationCommand(
                RFQId: _rfqId,
                RFQSupplierId: 1,
                SupplierPartyId: 1,
                QuotationDate: DateTime.UtcNow,
                ValidUntil: null,
                CurrencyCode: null,
                ExchangeRate: null,
                ShippingCost: null,
                OtherCharges: null,
                PaymentTerms: null,
                DeliveryTerms: null,
                LeadTimeDays: null,
                WarrantyPeriodMonths: null,
                Notes: null,
                Lines: new List<QuotationLineDto>
                {
                    new(PurchaseRequestDetailId: 1, ItemId: 1, UnitId: 1, Quantity: 10, UnitPrice: 950m, DiscountPercent: null, TaxPercent: null, Notes: null)
                }), CancellationToken.None);

        var evalResult = await new StartEvaluationCommandHandler(_dbContext)
            .Handle(new StartEvaluationCommand(_rfqId), CancellationToken.None);

        evalResult.Succeeded.ShouldBeTrue();
        var rfq = await _dbContext.RequestForQuotations.FindAsync(_rfqId);
        rfq!.Status.ShouldBe(RFQStatus.CollectingResponses);
    }

    [Test]
    public async Task CompleteEvaluation_InProgress_ShouldTransitionToEvaluated()
    {
        await new CreateQuotationCommandHandler(_dbContext, _quotSeqMock.Object)
            .Handle(new CreateQuotationCommand(
                RFQId: _rfqId,
                RFQSupplierId: 1,
                SupplierPartyId: 1,
                QuotationDate: DateTime.UtcNow,
                ValidUntil: null,
                CurrencyCode: null,
                ExchangeRate: null,
                ShippingCost: null,
                OtherCharges: null,
                PaymentTerms: null,
                DeliveryTerms: null,
                LeadTimeDays: null,
                WarrantyPeriodMonths: null,
                Notes: null,
                Lines: new List<QuotationLineDto>
                {
                    new(PurchaseRequestDetailId: 1, ItemId: 1, UnitId: 1, Quantity: 10, UnitPrice: 950m, DiscountPercent: null, TaxPercent: null, Notes: null)
                }), CancellationToken.None);

        await new StartEvaluationCommandHandler(_dbContext)
            .Handle(new StartEvaluationCommand(_rfqId), CancellationToken.None);

        var quot = await _dbContext.Quotations.FirstAsync(q => q.RFQId == _rfqId);
        var result = await new CompleteEvaluationCommandHandler(_dbContext)
            .Handle(new CompleteEvaluationCommand(quot.Id, 85m, 90m, null), CancellationToken.None);

        result.Succeeded.ShouldBeTrue();
    }

    [Test]
    public async Task SelectQuotation_Evaluated_ShouldMarkSelected()
    {
        await new CreateQuotationCommandHandler(_dbContext, _quotSeqMock.Object)
            .Handle(new CreateQuotationCommand(
                RFQId: _rfqId,
                RFQSupplierId: 1,
                SupplierPartyId: 1,
                QuotationDate: DateTime.UtcNow,
                ValidUntil: null,
                CurrencyCode: null,
                ExchangeRate: null,
                ShippingCost: null,
                OtherCharges: null,
                PaymentTerms: null,
                DeliveryTerms: null,
                LeadTimeDays: null,
                WarrantyPeriodMonths: null,
                Notes: null,
                Lines: new List<QuotationLineDto>
                {
                    new(PurchaseRequestDetailId: 1, ItemId: 1, UnitId: 1, Quantity: 10, UnitPrice: 950m, DiscountPercent: null, TaxPercent: null, Notes: null)
                }), CancellationToken.None);

        await new StartEvaluationCommandHandler(_dbContext)
            .Handle(new StartEvaluationCommand(_rfqId), CancellationToken.None);
        var quot = await _dbContext.Quotations.FirstAsync(q => q.RFQId == _rfqId);
        await new CompleteEvaluationCommandHandler(_dbContext)
            .Handle(new CompleteEvaluationCommand(quot.Id, 85m, 90m, null), CancellationToken.None);

        var result = await new SelectQuotationCommandHandler(_dbContext)
            .Handle(new SelectQuotationCommand(quot.Id, "Best value"), CancellationToken.None);

        result.Succeeded.ShouldBeTrue();
        var updated = await _dbContext.Quotations.FindAsync(quot.Id);
        updated!.Status.ShouldBe(QuotationStatus.Selected);
    }
}
