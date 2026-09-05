using ERP_Government.Application.Common.Interfaces;
using ERP_Government.Application.Parties.Common;
using ERP_Government.Application.Payments.Commands.PaymentOrders.ApprovePaymentOrder;
using ERP_Government.Application.Security.Common;
using ERP_Government.Domain.Payments.Entities;
using ERP_Government.Domain.Payments.Enums;
using ERP_Government.Domain.Security.Entities;
using ERP_Government.Application.UnitTests.Accounting;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using Shouldly;

namespace ERP_Government.Application.UnitTests.Payments;

[TestFixture]
public class PaymentOrderAuditGuardTests
{
    private Mock<IApplicationDbContext> _contextMock = null!;
    private Mock<IApprovalRuleEvaluationService> _evalServiceMock = null!;
    private Mock<IIdentityService> _identityServiceMock = null!;
    private Mock<IDocumentStatusLogger> _statusLoggerMock = null!;
    private Mock<IAttachmentGateService> _attachmentGateMock = null!;
    private Mock<IUser> _nullUserMock = null!;
    private Mock<IUser> _validUserMock = null!;
    private const int ValidUserId = 5;

    [SetUp]
    public void Setup()
    {
        _contextMock = new Mock<IApplicationDbContext>();
        _evalServiceMock = new Mock<IApprovalRuleEvaluationService>();
        _identityServiceMock = new Mock<IIdentityService>();
        _statusLoggerMock = new Mock<IDocumentStatusLogger>();
        _attachmentGateMock = new Mock<IAttachmentGateService>();
        _nullUserMock = new Mock<IUser>();
        _nullUserMock.Setup(x => x.Id).Returns((int?)null);
        _validUserMock = new Mock<IUser>();
        _validUserMock.Setup(x => x.Id).Returns(ValidUserId);

        _attachmentGateMock
            .Setup(x => x.CheckMandatoryAttachmentsAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<string>());
    }

    private void SetupPaymentOrder(PaymentOrderStatus status, BudgetCheckStatus budgetCheck)
    {
        var entity = new PaymentOrder
        {
            Id = 1,
            Status = status,
            BudgetCheckStatus = budgetCheck,
            AmountGross = 1000m,
            FundId = 1,
            CurrencyId = 1,
            RowVersion = [1, 2, 3]
        };

        var ordersMock = new Mock<DbSet<PaymentOrder>>();
        ordersMock.Setup(x => x.FindAsync(It.IsAny<object[]>()))
            .Returns<object[]>(kvs => ValueTask.FromResult(
                ((int)kvs[0]) == entity.Id ? entity : null));

        _contextMock.Setup(x => x.PaymentOrders).Returns(ordersMock.Object);

        var approvalHistory = new List<ApprovalHistory>().AsQueryable().BuildMockForAsync();
        _contextMock.Setup(x => x.ApprovalHistory).Returns(approvalHistory.Object);

        var statusLogs = new List<DocumentStatusLog>().AsQueryable().BuildMockForAsync();
        _contextMock.Setup(x => x.DocumentStatusLogs).Returns(statusLogs.Object);

        _contextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
    }

    [Test]
    public async Task ApprovePaymentOrder_NullUserId_ShouldRejectAndNotLogAudit()
    {
        SetupPaymentOrder(PaymentOrderStatus.Submitted, BudgetCheckStatus.Passed);

        var handler = new ApprovePaymentOrderCommandHandler(
            _contextMock.Object, _evalServiceMock.Object,
            _identityServiceMock.Object, _statusLoggerMock.Object,
            _attachmentGateMock.Object, _nullUserMock.Object);

        var result = await handler.Handle(
            new ApprovePaymentOrderCommand { Id = 1, RowVersion = [1, 2, 3] },
            CancellationToken.None);

        result.Succeeded.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.Contains("User identity is required"));
        _contextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task ApprovePaymentOrder_ValidUserId_ShouldSucceedWithCorrectAuditId()
    {
        SetupPaymentOrder(PaymentOrderStatus.Submitted, BudgetCheckStatus.Passed);

        _evalServiceMock.Setup(x => x.EvaluateAsync(
                It.IsAny<string>(), It.IsAny<decimal>(),
                It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ApprovalRuleResult>
            {
                new() { Sequence = 1, RequiredRole = "Treasurer", ApproverRoleId = 1 }
            });

        _identityServiceMock.Setup(x => x.IsInRoleAsync(ValidUserId, "Treasurer"))
            .ReturnsAsync(true);

        var handler = new ApprovePaymentOrderCommandHandler(
            _contextMock.Object, _evalServiceMock.Object,
            _identityServiceMock.Object, _statusLoggerMock.Object,
            _attachmentGateMock.Object, _validUserMock.Object);

        var result = await handler.Handle(
            new ApprovePaymentOrderCommand { Id = 1, RowVersion = [1, 2, 3] },
            CancellationToken.None);

        result.Succeeded.ShouldBeTrue();
        _contextMock.Verify(x => x.ApprovalHistory.Add(It.Is<ApprovalHistory>(
            h => h.ApproverUserId == ValidUserId)), Times.Once);
        _statusLoggerMock.Verify(x => x.LogAsync(
            "paymentorders", 1,
            PaymentOrderStatus.Submitted.ToString(),
            PaymentOrderStatus.Approved.ToString(),
            ValidUserId,
            null,
            It.IsAny<CancellationToken>()), Times.Once);
    }
}
