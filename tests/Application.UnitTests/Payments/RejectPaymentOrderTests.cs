using ERP_Government.Application.Common.Interfaces;
using ERP_Government.Application.Parties.Common;
using ERP_Government.Application.Payments.Commands.PaymentOrders.RejectPaymentOrder;
using ERP_Government.Domain.Payments.Entities;
using ERP_Government.Domain.Payments.Enums;
using ERP_Government.Application.UnitTests.Accounting;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using Shouldly;

namespace ERP_Government.Application.UnitTests.Payments;

[TestFixture]
public class RejectPaymentOrderTests
{
    private Mock<IApplicationDbContext> _contextMock = null!;
    private Mock<IUser> _userMock = null!;
    private Mock<IDocumentStatusLogger> _statusLoggerMock = null!;
    private PaymentOrder _entity = null!;

    private const int OrderId = 1;

    [SetUp]
    public void Setup()
    {
        _contextMock = new Mock<IApplicationDbContext>();
        _userMock = new Mock<IUser>();
        _userMock.Setup(x => x.Id).Returns(1);
        _statusLoggerMock = new Mock<IDocumentStatusLogger>();

        _entity = new PaymentOrder
        {
            Id = OrderId,
            PaymentOrderNumber = "PO-000001",
            Status = PaymentOrderStatus.Submitted,
            AmountGross = 5000m,
            DeductionAmount = 750m,
            FundId = 1,
            FiscalYearId = 1,
            CurrencyId = 1,
            BeneficiaryName = "Vendor",
            RowVersion = [1, 2, 3]
        };

        var ordersMock = new Mock<DbSet<PaymentOrder>>();
        ordersMock.Setup(x => x.FindAsync(It.IsAny<object[]>()))
            .Returns<object[]>(kvs => ValueTask.FromResult(
                ((int)kvs[0]) == OrderId ? _entity : null));
        _contextMock.Setup(x => x.PaymentOrders).Returns(ordersMock.Object);
        _contextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
    }

    [Test]
    public async Task Reject_SubmittedOrder_ShouldSucceed()
    {
        var handler = new RejectPaymentOrderCommandHandler(
            _contextMock.Object, _statusLoggerMock.Object, _userMock.Object);

        var result = await handler.Handle(
            new RejectPaymentOrderCommand { Id = OrderId, RejectionReason = "Incomplete docs", RowVersion = [1, 2, 3] },
            CancellationToken.None);

        result.Succeeded.ShouldBeTrue();
        _entity.Status.ShouldBe(PaymentOrderStatus.Rejected);
    }

    [Test]
    public async Task Reject_RowVersionConflict_ShouldReject()
    {
        var handler = new RejectPaymentOrderCommandHandler(
            _contextMock.Object, _statusLoggerMock.Object, _userMock.Object);

        var result = await handler.Handle(
            new RejectPaymentOrderCommand { Id = OrderId, RejectionReason = "Incomplete docs", RowVersion = [9, 9, 9] },
            CancellationToken.None);

        result.Succeeded.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.Contains("RowVersion conflict"));
        _contextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task Reject_NonSubmitted_ShouldReject()
    {
        _entity.Status = PaymentOrderStatus.Draft;

        var handler = new RejectPaymentOrderCommandHandler(
            _contextMock.Object, _statusLoggerMock.Object, _userMock.Object);

        var result = await handler.Handle(
            new RejectPaymentOrderCommand { Id = OrderId, RejectionReason = "Incomplete docs", RowVersion = [1, 2, 3] },
            CancellationToken.None);

        result.Succeeded.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.Contains("Only submitted payment orders can be rejected"));
    }
}
