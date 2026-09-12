using ERP_Government.Application.Budgeting.Commands.Budgets;
using ERP_Government.Application.Common.Interfaces;
using ERP_Government.Application.Parties.Common;
using ERP_Government.Application.Security.Common;
using ERP_Government.Domain.Budgeting.Entities;
using ERP_Government.Domain.Budgeting.Enums;
using ERP_Government.Domain.Security.Entities;
using ERP_Government.Application.UnitTests.Accounting;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using Shouldly;

namespace ERP_Government.Application.UnitTests.Budgeting;

[TestFixture]
public class BudgetAuditGuardTests
{
    private Mock<IApplicationDbContext> _contextMock = null!;
    private Mock<IDocumentStatusLogger> _statusLoggerMock = null!;
    private Mock<IAttachmentGateService> _attachmentGateMock = null!;
    private Mock<IUser> _nullUserMock = null!;
    private Mock<IUser> _validUserMock = null!;
    private const int ValidUserId = 5;

    [SetUp]
    public void Setup()
    {
        _contextMock = new Mock<IApplicationDbContext>();
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

    private void SetupBudget(BudgetStatus status)
    {
        var entity = new Budget
        {
            Id = 1,
            Status = status,
            RowVersion = [1, 2, 3]
        };

        var budgetsMock = new Mock<DbSet<Budget>>();
        budgetsMock.Setup(x => x.FindAsync(It.IsAny<object[]>()))
            .Returns<object[]>(kvs => ValueTask.FromResult(
                ((int)kvs[0]) == entity.Id ? entity : null));

        _contextMock.Setup(x => x.Budgets).Returns(budgetsMock.Object);

        var allocations = new List<BudgetItemAllocation>().AsQueryable().BuildMockForAsync();
        _contextMock.Setup(x => x.BudgetItemAllocations).Returns(allocations.Object);

        var approvalHistory = new List<ApprovalHistory>().AsQueryable().BuildMockForAsync();
        _contextMock.Setup(x => x.ApprovalHistory).Returns(approvalHistory.Object);

        var statusLogs = new List<DocumentStatusLog>().AsQueryable().BuildMockForAsync();
        _contextMock.Setup(x => x.DocumentStatusLogs).Returns(statusLogs.Object);

        _contextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
    }

    [Test]
    public async Task SubmitBudget_NullUserId_ShouldRejectAndNotLogAudit()
    {
        SetupBudget(BudgetStatus.Draft);

        var handler = new SubmitBudgetCommandHandler(
            _contextMock.Object, _nullUserMock.Object, _statusLoggerMock.Object);

        var result = await handler.Handle(
            new SubmitBudgetCommand(1, [1, 2, 3]),
            CancellationToken.None);

        result.Succeeded.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.Contains("User identity is required"));
        _contextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        _contextMock.Verify(x => x.ApprovalHistory.Add(It.IsAny<ApprovalHistory>()), Times.Never);
        _statusLoggerMock.Verify(x => x.LogAsync(
            It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(),
            It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task SubmitBudget_ValidUserId_ShouldSucceedWithCorrectAuditId()
    {
        SetupBudget(BudgetStatus.Draft);

        var handler = new SubmitBudgetCommandHandler(
            _contextMock.Object, _validUserMock.Object, _statusLoggerMock.Object);

        var result = await handler.Handle(
            new SubmitBudgetCommand(1, [1, 2, 3]),
            CancellationToken.None);

        result.Succeeded.ShouldBeTrue();
        _contextMock.Verify(x => x.ApprovalHistory.Add(It.Is<ApprovalHistory>(
            h => h.ApproverUserId == ValidUserId)), Times.Once);
        _statusLoggerMock.Verify(x => x.LogAsync(
            "budgets", 1,
            BudgetStatus.Draft.ToString(),
            BudgetStatus.Submitted.ToString(),
            ValidUserId,
            null,
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task ApproveBudget_NullUserId_ShouldRejectAndNotLogAudit()
    {
        SetupBudget(BudgetStatus.Submitted);

        var handler = new ApproveBudgetCommandHandler(
            _contextMock.Object, _nullUserMock.Object,
            _statusLoggerMock.Object, _attachmentGateMock.Object);

        var result = await handler.Handle(
            new ApproveBudgetCommand(1, [1, 2, 3]),
            CancellationToken.None);

        result.Succeeded.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.Contains("User identity is required"));
        _contextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        _contextMock.Verify(x => x.ApprovalHistory.Add(It.IsAny<ApprovalHistory>()), Times.Never);
    }

    [Test]
    public async Task ApproveBudget_ValidUserId_ShouldSucceedWithCorrectAuditId()
    {
        SetupBudget(BudgetStatus.Submitted);

        var handler = new ApproveBudgetCommandHandler(
            _contextMock.Object, _validUserMock.Object,
            _statusLoggerMock.Object, _attachmentGateMock.Object);

        var result = await handler.Handle(
            new ApproveBudgetCommand(1, [1, 2, 3]),
            CancellationToken.None);

        result.Succeeded.ShouldBeTrue();
        _contextMock.Verify(x => x.ApprovalHistory.Add(It.Is<ApprovalHistory>(
            h => h.ApproverUserId == ValidUserId)), Times.Once);
        _statusLoggerMock.Verify(x => x.LogAsync(
            "budgets", 1,
            BudgetStatus.Submitted.ToString(),
            BudgetStatus.Approved.ToString(),
            ValidUserId,
            null,
            It.IsAny<CancellationToken>()), Times.Once);
    }
}
