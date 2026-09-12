using ERP_Government.Application.Common.Interfaces;
using ERP_Government.Application.FinancialSettings.Common.Services;
using ERP_Government.Application.Parties.Common;
using ERP_Government.Application.Payments.Commands.DisbursementRequests.ApproveDisbursementRequest;
using ERP_Government.Application.Payments.Commands.DisbursementRequests.RejectDisbursementRequest;
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
public class DisbursementAuditGuardTests
{
    private Mock<IApplicationDbContext> _contextMock = null!;
    private Mock<IIdentityService> _identityServiceMock = null!;
    private Mock<IDocumentSequenceService> _sequenceServiceMock = null!;
    private Mock<IDocumentStatusLogger> _statusLoggerMock = null!;
    private Mock<IUser> _nullUserMock = null!;
    private Mock<IUser> _validUserMock = null!;
    private const int ValidUserId = 5;

    [SetUp]
    public void Setup()
    {
        _contextMock = new Mock<IApplicationDbContext>();
        _identityServiceMock = new Mock<IIdentityService>();
        _sequenceServiceMock = new Mock<IDocumentSequenceService>();
        _statusLoggerMock = new Mock<IDocumentStatusLogger>();
        _nullUserMock = new Mock<IUser>();
        _nullUserMock.Setup(x => x.Id).Returns((int?)null);
        _validUserMock = new Mock<IUser>();
        _validUserMock.Setup(x => x.Id).Returns(ValidUserId);
    }

    private void SetupDisbursement(DisbursementRequestStatus status, List<ApprovalHistory>? existingApprovals = null)
    {
        var entity = new DisbursementRequest
        {
            Id = 1,
            Status = status,
            RowVersion = [1, 2, 3]
        };

        var requestsMock = new Mock<DbSet<DisbursementRequest>>();
        requestsMock.Setup(x => x.FindAsync(It.IsAny<object[]>()))
            .Returns<object[]>(kvs => ValueTask.FromResult(
                ((int)kvs[0]) == entity.Id ? entity : null));

        _contextMock.Setup(x => x.DisbursementRequests).Returns(requestsMock.Object);

        var approvals = (existingApprovals ?? new List<ApprovalHistory>()).AsQueryable().BuildMockForAsync();
        _contextMock.Setup(x => x.ApprovalHistory).Returns(approvals.Object);

        _contextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
    }

    [Test]
    public async Task RejectDisbursement_NullUserId_ShouldRejectAndNotLogAudit()
    {
        SetupDisbursement(DisbursementRequestStatus.PendingApproval);

        var handler = new RejectDisbursementRequestCommandHandler(
            _contextMock.Object, _nullUserMock.Object);

        var result = await handler.Handle(
            new RejectDisbursementRequestCommand { Id = 1, Reason = "Invalid", RowVersion = [1, 2, 3] },
            CancellationToken.None);

        result.Succeeded.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.Contains("User identity is required"));
        _contextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        _contextMock.Verify(x => x.ApprovalHistory.Add(It.IsAny<ApprovalHistory>()), Times.Never);
    }

    [Test]
    public async Task RejectDisbursement_ValidUserId_ShouldSucceedWithCorrectAuditId()
    {
        SetupDisbursement(DisbursementRequestStatus.PendingApproval);

        var handler = new RejectDisbursementRequestCommandHandler(
            _contextMock.Object, _validUserMock.Object);

        var result = await handler.Handle(
            new RejectDisbursementRequestCommand { Id = 1, Reason = "Invalid", RowVersion = [1, 2, 3] },
            CancellationToken.None);

        result.Succeeded.ShouldBeTrue();
        _contextMock.Verify(x => x.ApprovalHistory.Add(It.Is<ApprovalHistory>(
            h => h.ApproverUserId == ValidUserId)), Times.Once);
    }

    [Test]
    public async Task ApproveDisbursement_NullUserId_ShouldRejectAndNotLogAudit()
    {
        SetupDisbursement(DisbursementRequestStatus.PendingApproval);

        var handler = new ApproveDisbursementRequestCommandHandler(
            _contextMock.Object, _identityServiceMock.Object, _sequenceServiceMock.Object, _statusLoggerMock.Object, _nullUserMock.Object);

        var result = await handler.Handle(
            new ApproveDisbursementRequestCommand { Id = 1, Reason = "OK", RowVersion = [1, 2, 3] },
            CancellationToken.None);

        result.Succeeded.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.Contains("User identity is required"));
        _contextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        _contextMock.Verify(x => x.ApprovalHistory.Add(It.IsAny<ApprovalHistory>()), Times.Never);
    }

    [Test]
    public async Task ApproveDisbursement_ValidUserId_ShouldSucceedWithCorrectAuditId()
    {
        SetupDisbursement(DisbursementRequestStatus.PendingApproval);

        _identityServiceMock.Setup(x => x.IsInRoleAsync(ValidUserId, It.IsAny<string>()))
            .ReturnsAsync(true);

        var handler = new ApproveDisbursementRequestCommandHandler(
            _contextMock.Object, _identityServiceMock.Object, _sequenceServiceMock.Object, _statusLoggerMock.Object, _validUserMock.Object);

        var result = await handler.Handle(
            new ApproveDisbursementRequestCommand { Id = 1, Reason = "OK", RowVersion = [1, 2, 3] },
            CancellationToken.None);

        result.Succeeded.ShouldBeTrue();
        _contextMock.Verify(x => x.ApprovalHistory.Add(It.Is<ApprovalHistory>(
            h => h.ApproverUserId == ValidUserId)), Times.Once);
    }
}
