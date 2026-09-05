using ERP_Government.Application.Budgeting.Common;
using ERP_Government.Application.Common.Interfaces;
using ERP_Government.Application.FinancialSettings.Common.Services;
using ERP_Government.Application.Payments.Commands.DisbursementRequests.CreateDisbursementRequest;
using ERP_Government.Domain.Budgeting.Entities;
using ERP_Government.Domain.Budgeting.Enums;
using ERP_Government.Domain.FinancialSettings.Entities;
using ERP_Government.Domain.Payments.Entities;
using ERP_Government.Domain.Payments.Enums;
using ERP_Government.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using Shouldly;

namespace ERP_Government.Application.UnitTests.Payments;

[TestFixture]
public class CreateDisbursementRequestTests
{
    private ApplicationDbContext _dbContext = null!;
    private Mock<IBudgetAvailabilityService> _availabilityServiceMock = null!;
    private Mock<IDocumentSequenceService> _sequenceServiceMock = null!;
    private Mock<IUser> _userMock = null!;

    [SetUp]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _dbContext = new ApplicationDbContext(options);

        _availabilityServiceMock = new Mock<IBudgetAvailabilityService>();
        _sequenceServiceMock = new Mock<IDocumentSequenceService>();
        _userMock = new Mock<IUser>();
        _sequenceServiceMock.Setup(s => s.GenerateNextNumberAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("DR-000001");
        _userMock.Setup(x => x.Id).Returns(1);
    }

    [TearDown]
    public void TearDown()
    {
        _dbContext?.Dispose();
    }

    private async Task SeedPaymentOrderAsync(PaymentOrder po)
    {
        _dbContext.PaymentOrders.Add(po);
        await _dbContext.SaveChangesAsync();
    }

    private async Task SeedAppropriationAsync(Appropriation app)
    {
        _dbContext.Appropriations.Add(app);
        await _dbContext.SaveChangesAsync();
    }

    private async Task SeedExistingRequestAsync(DisbursementRequest req)
    {
        _dbContext.DisbursementRequests.Add(req);
        await _dbContext.SaveChangesAsync();
    }

    // ─── T019: Sufficient Budget ──────────────────────────────────

    [Test]
    public async Task CreateDisbursementRequest_SufficientBudget_ShouldSucceed()
    {
        var po = new PaymentOrder
        {
            Id = 1,
            Status = PaymentOrderStatus.Approved,
            AmountGross = 10000m,
            DeductionAmount = 1000m,
            AppropriationId = 1,
            PaymentOrderNumber = "PO-001",
            BeneficiaryName = "Vendor A"
        };
        await SeedPaymentOrderAsync(po);
        await SeedAppropriationAsync(new Appropriation { Id = 1, BudgetItemId = 10 });

        _availabilityServiceMock.Setup(s => s.GetAvailabilitySummaryAsync(10))
            .ReturnsAsync(new BudgetAvailabilitySummary(10, 50000m, 20000m, 30000m, false, BudgetControlMethod.Blocking));
        _availabilityServiceMock.Setup(s => s.EvaluateControlMethod(BudgetControlMethod.Blocking, 9000m, 30000m))
            .Returns((true, (string?)null));

        var result = await new CreateDisbursementRequestCommandHandler(
                _dbContext, _availabilityServiceMock.Object, _sequenceServiceMock.Object, _userMock.Object)
            .Handle(new CreateDisbursementRequestCommand { PaymentOrderId = 1 }, CancellationToken.None);

        result.Succeeded.ShouldBeTrue();
        result.Value!.Status.ShouldBe(DisbursementRequestStatus.Draft);
        result.Value.HasWarning.ShouldBeFalse();
    }

    // ─── T020: Blocking Insufficient ──────────────────────────────

    [Test]
    public async Task CreateDisbursementRequest_BlockingInsufficient_ShouldRejectWithBreakdown()
    {
        var po = new PaymentOrder
        {
            Id = 1,
            Status = PaymentOrderStatus.Approved,
            AmountGross = 50000m,
            DeductionAmount = 0m,
            AppropriationId = 1,
            PaymentOrderNumber = "PO-002",
            BeneficiaryName = "Vendor B"
        };
        await SeedPaymentOrderAsync(po);
        await SeedAppropriationAsync(new Appropriation { Id = 1, BudgetItemId = 10 });

        _availabilityServiceMock.Setup(s => s.GetAvailabilitySummaryAsync(10))
            .ReturnsAsync(new BudgetAvailabilitySummary(10, 10000m, 8000m, 2000m, false, BudgetControlMethod.Blocking));
        _availabilityServiceMock.Setup(s => s.EvaluateControlMethod(BudgetControlMethod.Blocking, 50000m, 2000m))
            .Returns((false, "Blocked"));

        var result = await new CreateDisbursementRequestCommandHandler(
                _dbContext, _availabilityServiceMock.Object, _sequenceServiceMock.Object, _userMock.Object)
            .Handle(new CreateDisbursementRequestCommand { PaymentOrderId = 1 }, CancellationToken.None);

        result.Succeeded.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.Contains("Budget availability insufficient"));
        result.Errors.ShouldContain(e => e.Contains("Net appropriated: 10"));
        result.Errors.ShouldContain(e => e.Contains("Available: 2"));
        result.Errors.ShouldContain(e => e.Contains("Requested: 50"));
        result.Errors.ShouldContain(e => e.Contains("Shortfall: 48"));
    }

    // ─── T021: Warning Insufficient ───────────────────────────────

    [Test]
    public async Task CreateDisbursementRequest_WarningInsufficient_ShouldAllowWithWarning()
    {
        var po = new PaymentOrder
        {
            Id = 1,
            Status = PaymentOrderStatus.Approved,
            AmountGross = 20000m,
            DeductionAmount = 0m,
            AppropriationId = 1,
            PaymentOrderNumber = "PO-003",
            BeneficiaryName = "Vendor C"
        };
        await SeedPaymentOrderAsync(po);
        await SeedAppropriationAsync(new Appropriation { Id = 1, BudgetItemId = 10 });

        _availabilityServiceMock.Setup(s => s.GetAvailabilitySummaryAsync(10))
            .ReturnsAsync(new BudgetAvailabilitySummary(10, 30000m, 25000m, 5000m, false, BudgetControlMethod.Warning));
        _availabilityServiceMock.Setup(s => s.EvaluateControlMethod(BudgetControlMethod.Warning, 20000m, 5000m))
            .Returns((true, "Requested amount exceeds available. Warning only."));

        var result = await new CreateDisbursementRequestCommandHandler(
                _dbContext, _availabilityServiceMock.Object, _sequenceServiceMock.Object, _userMock.Object)
            .Handle(new CreateDisbursementRequestCommand { PaymentOrderId = 1 }, CancellationToken.None);

        result.Succeeded.ShouldBeTrue();
        result.Value!.HasWarning.ShouldBeTrue();
        result.Value.Status.ShouldBe(DisbursementRequestStatus.Draft);
    }

    // ─── T022: None Control ───────────────────────────────────────

    [Test]
    public async Task CreateDisbursementRequest_NoneControl_ShouldSkipCheck()
    {
        var po = new PaymentOrder
        {
            Id = 1,
            Status = PaymentOrderStatus.Approved,
            AmountGross = 15000m,
            DeductionAmount = 0m,
            AppropriationId = 1,
            PaymentOrderNumber = "PO-004",
            BeneficiaryName = "Vendor D"
        };
        await SeedPaymentOrderAsync(po);
        await SeedAppropriationAsync(new Appropriation { Id = 1, BudgetItemId = 10 });

        _availabilityServiceMock.Setup(s => s.GetAvailabilitySummaryAsync(10))
            .ReturnsAsync(new BudgetAvailabilitySummary(10, 5000m, 3000m, 2000m, false, BudgetControlMethod.None));
        _availabilityServiceMock.Setup(s => s.EvaluateControlMethod(BudgetControlMethod.None, 15000m, 2000m))
            .Returns((true, (string?)null));

        var result = await new CreateDisbursementRequestCommandHandler(
                _dbContext, _availabilityServiceMock.Object, _sequenceServiceMock.Object, _userMock.Object)
            .Handle(new CreateDisbursementRequestCommand { PaymentOrderId = 1 }, CancellationToken.None);

        result.Succeeded.ShouldBeTrue();
        result.Value!.HasWarning.ShouldBeFalse();
        _availabilityServiceMock.Verify(s => s.GetAvailabilitySummaryAsync(It.IsAny<int>()), Times.Once);
        _availabilityServiceMock.Verify(s => s.EvaluateControlMethod(It.IsAny<BudgetControlMethod>(), It.IsAny<decimal>(), It.IsAny<decimal>()), Times.Once);
    }

    // ─── T023: PO Not Approved ────────────────────────────────────

    [Test]
    public async Task CreateDisbursementRequest_PO_NOT_Approved_ShouldReject()
    {
        var po = new PaymentOrder
        {
            Id = 1,
            Status = PaymentOrderStatus.Draft,
            AmountGross = 10000m,
            DeductionAmount = 0m,
            AppropriationId = 1,
            PaymentOrderNumber = "PO-005",
            BeneficiaryName = "Vendor E"
        };
        await SeedPaymentOrderAsync(po);

        var result = await new CreateDisbursementRequestCommandHandler(
                _dbContext, _availabilityServiceMock.Object, _sequenceServiceMock.Object, _userMock.Object)
            .Handle(new CreateDisbursementRequestCommand { PaymentOrderId = 1 }, CancellationToken.None);

        result.Succeeded.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.Contains("Payment order must be approved first"));
    }

    // ─── T024: PO Zero Net Total ──────────────────────────────────

    [Test]
    public async Task CreateDisbursementRequest_PO_ZeroNetTotal_ShouldReject()
    {
        var po = new PaymentOrder
        {
            Id = 1,
            Status = PaymentOrderStatus.Approved,
            AmountGross = 5000m,
            DeductionAmount = 5000m,
            AppropriationId = 1,
            PaymentOrderNumber = "PO-006",
            BeneficiaryName = "Vendor F"
        };
        await SeedPaymentOrderAsync(po);

        var result = await new CreateDisbursementRequestCommandHandler(
                _dbContext, _availabilityServiceMock.Object, _sequenceServiceMock.Object, _userMock.Object)
            .Handle(new CreateDisbursementRequestCommand { PaymentOrderId = 1 }, CancellationToken.None);

        result.Succeeded.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.Contains("Payment order net total must be greater than zero"));
    }

    // ─── T025: Duplicate Request ──────────────────────────────────

    [Test]
    public async Task CreateDisbursementRequest_DuplicateRequest_ShouldReject()
    {
        var po = new PaymentOrder
        {
            Id = 1,
            Status = PaymentOrderStatus.Approved,
            AmountGross = 10000m,
            DeductionAmount = 0m,
            AppropriationId = 1,
            PaymentOrderNumber = "PO-007",
            BeneficiaryName = "Vendor G"
        };
        await SeedPaymentOrderAsync(po);
        await SeedExistingRequestAsync(new DisbursementRequest
        {
            Id = 99,
            PaymentOrderId = 1,
            RequestNumber = "DR-EXISTING",
            Status = DisbursementRequestStatus.Draft,
            RequestedById = 1,
            RequestDate = DateOnly.FromDateTime(DateTime.UtcNow),
            RowVersion = [1, 2, 3]
        });

        var result = await new CreateDisbursementRequestCommandHandler(
                _dbContext, _availabilityServiceMock.Object, _sequenceServiceMock.Object, _userMock.Object)
            .Handle(new CreateDisbursementRequestCommand { PaymentOrderId = 1 }, CancellationToken.None);

        result.Succeeded.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.Contains("A disbursement request already exists for this payment order"));
    }
}
