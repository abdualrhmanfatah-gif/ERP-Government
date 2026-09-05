using ERP_Government.Application.Budgeting.Commands.Encumbrances;
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
public class EncumbranceAuditGuardTests
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

    private void SetupEncumbrance(EncumbranceStatus status)
    {
        var entity = new Encumbrance
        {
            Id = 1,
            Status = status,
            RowVersion = [1, 2, 3]
        };

        var encumbrancesMock = new Mock<DbSet<Encumbrance>>();
        encumbrancesMock.Setup(x => x.FindAsync(It.IsAny<object[]>()))
            .Returns<object[]>(kvs => ValueTask.FromResult(
                ((int)kvs[0]) == entity.Id ? entity : null));

        _contextMock.Setup(x => x.Encumbrances).Returns(encumbrancesMock.Object);

        var approvalHistory = new List<ApprovalHistory>().AsQueryable().BuildMockForAsync();
        _contextMock.Setup(x => x.ApprovalHistory).Returns(approvalHistory.Object);

        var statusLogs = new List<DocumentStatusLog>().AsQueryable().BuildMockForAsync();
        _contextMock.Setup(x => x.DocumentStatusLogs).Returns(statusLogs.Object);

        _contextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
    }

    // ─── ApproveEncumbranceCommand ─────────────────────────────────

    [Test]
    public async Task ApproveEncumbrance_NullUserId_ShouldRejectAndNotLogAudit()
    {
        SetupEncumbrance(EncumbranceStatus.PendingApproval);

        var handler = new ApproveEncumbranceCommandHandler(
            _contextMock.Object, _nullUserMock.Object,
            _statusLoggerMock.Object, _attachmentGateMock.Object);

        var result = await handler.Handle(
            new ApproveEncumbranceCommand(1, [1, 2, 3], null),
            CancellationToken.None);

        result.Succeeded.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.Contains("User identity is required"));
        _contextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task ApproveEncumbrance_ValidUserId_ShouldSucceedWithCorrectAuditId()
    {
        SetupEncumbrance(EncumbranceStatus.PendingApproval);

        var handler = new ApproveEncumbranceCommandHandler(
            _contextMock.Object, _validUserMock.Object,
            _statusLoggerMock.Object, _attachmentGateMock.Object);

        var result = await handler.Handle(
            new ApproveEncumbranceCommand(1, [1, 2, 3], "Approved"),
            CancellationToken.None);

        result.Succeeded.ShouldBeTrue();
        _contextMock.Verify(x => x.ApprovalHistory.Add(It.Is<ApprovalHistory>(
            h => h.ApproverUserId == ValidUserId)), Times.Once);
        _statusLoggerMock.Verify(x => x.LogAsync(
            "encumbrances", 1,
            EncumbranceStatus.PendingApproval.ToString(),
            EncumbranceStatus.Approved.ToString(),
            ValidUserId,
            "Approved",
            It.IsAny<CancellationToken>()), Times.Once);
    }

    // ─── SubmitEncumbranceCommand ──────────────────────────────────

    [Test]
    public async Task SubmitEncumbrance_NullUserId_ShouldRejectAndNotLogAudit()
    {
        SetupEncumbrance(EncumbranceStatus.Draft);

        var handler = new SubmitEncumbranceCommandHandler(
            _contextMock.Object, _nullUserMock.Object, _statusLoggerMock.Object);

        var result = await handler.Handle(
            new SubmitEncumbranceCommand(1, [1, 2, 3]),
            CancellationToken.None);

        result.Succeeded.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.Contains("User identity is required"));
        _contextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task SubmitEncumbrance_ValidUserId_ShouldSucceedWithCorrectAuditId()
    {
        SetupEncumbrance(EncumbranceStatus.Draft);

        var handler = new SubmitEncumbranceCommandHandler(
            _contextMock.Object, _validUserMock.Object, _statusLoggerMock.Object);

        var result = await handler.Handle(
            new SubmitEncumbranceCommand(1, [1, 2, 3]),
            CancellationToken.None);

        result.Succeeded.ShouldBeTrue();
        _contextMock.Verify(x => x.ApprovalHistory.Add(It.Is<ApprovalHistory>(
            h => h.ApproverUserId == ValidUserId)), Times.Once);
    }

    // ─── CancelEncumbranceCommand ──────────────────────────────────

    [Test]
    public async Task CancelEncumbrance_NullUserId_ShouldRejectAndNotLogAudit()
    {
        SetupEncumbrance(EncumbranceStatus.Active);

        var handler = new CancelEncumbranceCommandHandler(
            _contextMock.Object, _nullUserMock.Object, _statusLoggerMock.Object);

        var result = await handler.Handle(
            new CancelEncumbranceCommand(1, [1, 2, 3], null),
            CancellationToken.None);

        result.Succeeded.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.Contains("User identity is required"));
        _contextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task CancelEncumbrance_ValidUserId_ShouldSucceedWithCorrectAuditId()
    {
        SetupEncumbrance(EncumbranceStatus.Active);

        var handler = new CancelEncumbranceCommandHandler(
            _contextMock.Object, _validUserMock.Object, _statusLoggerMock.Object);

        var result = await handler.Handle(
            new CancelEncumbranceCommand(1, [1, 2, 3], "Not needed"),
            CancellationToken.None);

        result.Succeeded.ShouldBeTrue();
        _contextMock.Verify(x => x.ApprovalHistory.Add(It.Is<ApprovalHistory>(
            h => h.ApproverUserId == ValidUserId)), Times.Once);
    }
}
