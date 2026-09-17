using ERP_Government.Application.Common.Interfaces;
using ERP_Government.Application.FinancialSettings.Common.Services;
using ERP_Government.Application.Procurement.Commands.PurchaseRequests.CreatePurchaseRequest;
using ERP_Government.Application.Procurement.Commands.PurchaseRequests.SubmitPurchaseRequest;
using ERP_Government.Application.Procurement.Commands.PurchaseRequests.ApprovePurchaseRequest;
using ERP_Government.Application.Procurement.Commands.Quotations.CreateQuotation;
using ERP_Government.Application.Procurement.Commands.Quotations.SubmitQuotation;
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
    private Mock<IDocumentSequenceService> _quotSeqMock = null!;

    [SetUp]
    public async Task Setup()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _dbContext = new ApplicationDbContext(options);

        _prSeqMock = new Mock<IDocumentSequenceService>();
        _quotSeqMock = new Mock<IDocumentSequenceService>();
        _prSeqMock.Setup(s => s.GenerateNextNumberAsync("PurchaseRequest", It.IsAny<CancellationToken>())).ReturnsAsync("PR-000001");
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
                RequesterName: "Test User",
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
    }

    [TearDown]
    public void TearDown() => _dbContext?.Dispose();

    private async Task<int> CreateQuotation()
    {
        var result = await new CreateQuotationCommandHandler(_dbContext, _quotSeqMock.Object)
            .Handle(new CreateQuotationCommand(
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
        return result.Value!;
    }

    [Test]
    public async Task CreateQuotation_ValidCommand_ShouldReturnDraftStatus()
    {
        var quotId = await CreateQuotation();
        var quotation = await _dbContext.Quotations.FindAsync(quotId);
        quotation.ShouldNotBeNull();
        quotation.Status.ShouldBe(QuotationStatus.Draft);
    }

    [Test]
    public async Task StartEvaluation_WithQuotation_ShouldTransitionToUnderEvaluation()
    {
        var quotId = await CreateQuotation();
        await new SubmitQuotationCommandHandler(_dbContext)
            .Handle(new SubmitQuotationCommand(quotId), CancellationToken.None);

        var evalResult = await new StartEvaluationCommandHandler(_dbContext)
            .Handle(new StartEvaluationCommand(quotId), CancellationToken.None);

        evalResult.Succeeded.ShouldBeTrue();
        var quotation = await _dbContext.Quotations.FindAsync(quotId);
        quotation!.Status.ShouldBe(QuotationStatus.UnderEvaluation);
    }

    [Test]
    public async Task CompleteEvaluation_InProgress_ShouldTransitionToEvaluated()
    {
        var quotId = await CreateQuotation();
        await new SubmitQuotationCommandHandler(_dbContext)
            .Handle(new SubmitQuotationCommand(quotId), CancellationToken.None);

        await new StartEvaluationCommandHandler(_dbContext)
            .Handle(new StartEvaluationCommand(quotId), CancellationToken.None);

        var result = await new CompleteEvaluationCommandHandler(_dbContext)
            .Handle(new CompleteEvaluationCommand(quotId, 85m, 90m, null), CancellationToken.None);

        result.Succeeded.ShouldBeTrue();
    }

    [Test]
    public async Task SelectQuotation_Evaluated_ShouldMarkSelected()
    {
        var quotId = await CreateQuotation();
        await new SubmitQuotationCommandHandler(_dbContext)
            .Handle(new SubmitQuotationCommand(quotId), CancellationToken.None);

        await new StartEvaluationCommandHandler(_dbContext)
            .Handle(new StartEvaluationCommand(quotId), CancellationToken.None);
        await new CompleteEvaluationCommandHandler(_dbContext)
            .Handle(new CompleteEvaluationCommand(quotId, 85m, 90m, null), CancellationToken.None);

        var result = await new SelectQuotationCommandHandler(_dbContext)
            .Handle(new SelectQuotationCommand(quotId, "Best value"), CancellationToken.None);

        result.Succeeded.ShouldBeTrue();
        var updated = await _dbContext.Quotations.FindAsync(quotId);
        updated!.Status.ShouldBe(QuotationStatus.Selected);
    }
}
