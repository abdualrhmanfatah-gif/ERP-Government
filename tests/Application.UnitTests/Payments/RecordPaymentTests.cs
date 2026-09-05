using ERP_Government.Application.Common.Interfaces;
using ERP_Government.Application.Payments.Commands.Payments.RecordPayment;
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
public class RecordPaymentTests
{
    private ApplicationDbContext _dbContext = null!;
    private Mock<IUser> _userMock = null!;

    [SetUp]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _dbContext = new ApplicationDbContext(options);
        _userMock = new Mock<IUser>();
        _userMock.Setup(x => x.Id).Returns(1);
    }

    [TearDown]
    public void TearDown()
    {
        _dbContext?.Dispose();
    }

    private async Task<(DisbursementRequest request, PaymentOrder order)> SeedApprovedRequestAsync(
        decimal amountGross = 5000m, decimal deduction = 750m)
    {
        var order = new PaymentOrder
        {
            Id = 10,
            AmountGross = amountGross,
            DeductionAmount = deduction,
            PaymentOrderNumber = "PO-000001",
            BeneficiaryName = "Acme Corp",
            Status = PaymentOrderStatus.Approved
        };
        _dbContext.PaymentOrders.Add(order);
        await _dbContext.SaveChangesAsync();

        var request = new DisbursementRequest
        {
            Id = 1,
            PaymentOrderId = 10,
            Status = DisbursementRequestStatus.Approved,
            RequestNumber = "DR-000001",
            RequestedById = 1,
            RequestDate = DateOnly.FromDateTime(DateTime.UtcNow),
            RowVersion = [1, 2, 3]
        };
        _dbContext.DisbursementRequests.Add(request);
        await _dbContext.SaveChangesAsync();

        _dbContext.DocumentSequences.Add(new DocumentSequence
        {
            Id = 1,
            DocumentType = "Payment",
            CurrentNumber = 42
        });
        await _dbContext.SaveChangesAsync();

        return (request, order);
    }

    // ─── T040: Approved request → payment with correct amount ──────

    [Test]
    public async Task RecordPayment_ApprovedRequest_ShouldCreatePaymentWithCorrectAmount()
    {
        var (request, order) = await SeedApprovedRequestAsync(5000m, 750m);

        var handler = new RecordPaymentCommandHandler(_dbContext, _userMock.Object);

        var result = await handler.Handle(
            new RecordPaymentCommand
            {
                DisbursementRequestId = 1,
                PaymentMethod = PaymentMethod.BankTransfer,
                ReferenceNumber = "REF-001"
            },
            CancellationToken.None);

        result.Succeeded.ShouldBeTrue();
        result.Value!.Amount.ShouldBe(4250m); // 5000 - 750
        result.Value!.Status.ShouldBe(PaymentStatus.Completed);
        result.Value!.PaymentNumber.ShouldBe("PAY-000042");
        result.Value!.DisbursementRequestId.ShouldBe(1);
        result.Value!.PaymentOrderId.ShouldBe(10);
    }

    // ─── T041: Not approved → reject ───────────────────────────────

    [Test]
    public async Task RecordPayment_NotApproved_ShouldReject()
    {
        var order = new PaymentOrder
        {
            Id = 10,
            AmountGross = 5000m,
            DeductionAmount = 750m,
            PaymentOrderNumber = "PO-000001",
            BeneficiaryName = "Acme Corp",
            Status = PaymentOrderStatus.Approved
        };
        _dbContext.PaymentOrders.Add(order);
        await _dbContext.SaveChangesAsync();

        var request = new DisbursementRequest
        {
            Id = 1,
            PaymentOrderId = 10,
            Status = DisbursementRequestStatus.Draft,
            RequestNumber = "DR-000001",
            RequestedById = 1,
            RequestDate = DateOnly.FromDateTime(DateTime.UtcNow),
            RowVersion = [1, 2, 3]
        };
        _dbContext.DisbursementRequests.Add(request);
        await _dbContext.SaveChangesAsync();

        var handler = new RecordPaymentCommandHandler(_dbContext, _userMock.Object);

        var result = await handler.Handle(
            new RecordPaymentCommand
            {
                DisbursementRequestId = 1,
                PaymentMethod = PaymentMethod.BankTransfer
            },
            CancellationToken.None);

        result.Succeeded.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.Contains("Disbursement request must be approved before payment execution"));
    }

    // ─── T042: Status transitions ──────────────────────────────────

    [Test]
    public async Task RecordPayment_ShouldTransitionRequestToDisbursedAndPOToPaid()
    {
        var (request, order) = await SeedApprovedRequestAsync(3000m, 500m);

        var handler = new RecordPaymentCommandHandler(_dbContext, _userMock.Object);

        var result = await handler.Handle(
            new RecordPaymentCommand
            {
                DisbursementRequestId = 1,
                PaymentMethod = PaymentMethod.Cash
            },
            CancellationToken.None);

        result.Succeeded.ShouldBeTrue();

        var updatedRequest = await _dbContext.DisbursementRequests.FindAsync(1);
        updatedRequest!.Status.ShouldBe(DisbursementRequestStatus.Disbursed);

        var updatedOrder = await _dbContext.PaymentOrders.FindAsync(10);
        updatedOrder!.Status.ShouldBe(PaymentOrderStatus.Paid);
        updatedOrder.PaidAt.ShouldNotBeNull();
    }

    // ─── T043: Domain event raised ─────────────────────────────────

    [Test]
    public async Task RecordPayment_ShouldRaisePaymentRecordedEvent()
    {
        var (request, order) = await SeedApprovedRequestAsync(1000m, 100m);

        var handler = new RecordPaymentCommandHandler(_dbContext, _userMock.Object);

        var result = await handler.Handle(
            new RecordPaymentCommand
            {
                DisbursementRequestId = 1,
                PaymentMethod = PaymentMethod.Check
            },
            CancellationToken.None);

        result.Succeeded.ShouldBeTrue();

        var payment = await _dbContext.Payments.FirstOrDefaultAsync(p => p.DisbursementRequestId == 1);
        payment.ShouldNotBeNull();
        payment.DomainEvents.ShouldContain(e => e is PaymentRecordedEvent);
    }
}
