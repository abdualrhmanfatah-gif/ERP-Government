using ERP_Government.Application.Common.Interfaces;
using ERP_Government.Application.Parties.Common;
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
    private Mock<IDocumentStatusLogger> _statusLoggerMock = null!;

    [SetUp]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _dbContext = new ApplicationDbContext(options);
        _userMock = new Mock<IUser>();
        _userMock.Setup(x => x.Id).Returns(1);
        _statusLoggerMock = new Mock<IDocumentStatusLogger>();
    }

    [TearDown]
    public void TearDown()
    {
        _dbContext?.Dispose();
    }

    private async Task<PaymentOrder> SeedApprovedOrderAsync(
        decimal amountGross = 5000m, decimal deduction = 750m)
    {
        var order = new PaymentOrder
        {
            Id = 10,
            AmountGross = amountGross,
            DeductionAmount = deduction,
            PaymentOrderNumber = "PO-000001",
            BeneficiaryName = "Acme Corp",
            Status = PaymentOrderStatus.Approved,
            FundId = 1,
            FiscalYearId = 1,
            CurrencyId = 1
        };
        _dbContext.PaymentOrders.Add(order);
        await _dbContext.SaveChangesAsync();

        _dbContext.DocumentSequences.Add(new DocumentSequence
        {
            Id = 1,
            DocumentType = "Payment",
            CurrentNumber = 42
        });
        await _dbContext.SaveChangesAsync();

        return order;
    }

    [Test]
    public async Task RecordPayment_ApprovedOrder_ShouldCreatePaymentWithCorrectAmount()
    {
        var order = await SeedApprovedOrderAsync(5000m, 750m);

        var handler = new RecordPaymentCommandHandler(_dbContext, _statusLoggerMock.Object, _userMock.Object);

        var result = await handler.Handle(
            new RecordPaymentCommand
            {
                PaymentOrderId = 10,
                PaymentMethod = PaymentMethod.Cash,
                ReferenceNumber = "REF-001"
            },
            CancellationToken.None);

        result.Succeeded.ShouldBeTrue();
        result.Value!.Amount.ShouldBe(4250m); // 5000 - 750
        result.Value!.Status.ShouldBe(PaymentStatus.Completed);
        result.Value!.PaymentNumber.ShouldBe("PAY-000042");
        result.Value!.PaymentOrderId.ShouldBe(10);
    }

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
            Status = PaymentOrderStatus.Draft,
            FundId = 1,
            FiscalYearId = 1,
            CurrencyId = 1
        };
        _dbContext.PaymentOrders.Add(order);
        await _dbContext.SaveChangesAsync();

        var handler = new RecordPaymentCommandHandler(_dbContext, _statusLoggerMock.Object, _userMock.Object);

        var result = await handler.Handle(
            new RecordPaymentCommand
            {
                PaymentOrderId = 10,
                PaymentMethod = PaymentMethod.Cash
            },
            CancellationToken.None);

        result.Succeeded.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.Contains("Payment order must be approved before payment execution"));
    }

    [Test]
    public async Task RecordPayment_ShouldTransitionOrderToPaid()
    {
        var order = await SeedApprovedOrderAsync(3000m, 500m);

        var handler = new RecordPaymentCommandHandler(_dbContext, _statusLoggerMock.Object, _userMock.Object);

        var result = await handler.Handle(
            new RecordPaymentCommand
            {
                PaymentOrderId = 10,
                PaymentMethod = PaymentMethod.Cash
            },
            CancellationToken.None);

        result.Succeeded.ShouldBeTrue();

        var updatedOrder = await _dbContext.PaymentOrders.FindAsync(10);
        updatedOrder!.Status.ShouldBe(PaymentOrderStatus.Paid);
        updatedOrder.PaidAt.ShouldNotBeNull();
    }

    [Test]
    public async Task RecordPayment_ShouldRaisePaymentRecordedEvent()
    {
        var order = await SeedApprovedOrderAsync(1000m, 100m);

        var handler = new RecordPaymentCommandHandler(_dbContext, _statusLoggerMock.Object, _userMock.Object);

        var result = await handler.Handle(
            new RecordPaymentCommand
            {
                PaymentOrderId = 10,
                PaymentMethod = PaymentMethod.Check
            },
            CancellationToken.None);

        result.Succeeded.ShouldBeTrue();

        var payment = await _dbContext.Payments.FirstOrDefaultAsync(p => p.PaymentOrderId == 10);
        payment.ShouldNotBeNull();
        payment.DomainEvents.ShouldContain(e => e is PaymentRecordedEvent);
    }
}
