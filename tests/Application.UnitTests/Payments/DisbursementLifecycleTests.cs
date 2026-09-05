using ERP_Government.Application.Common.Interfaces;
using ERP_Government.Application.Payments.Commands.DisbursementRequests.ApproveDisbursementRequest;
using ERP_Government.Application.Payments.Commands.DisbursementRequests.CancelDisbursementRequest;
using ERP_Government.Application.Payments.Commands.DisbursementRequests.RejectDisbursementRequest;
using ERP_Government.Application.Payments.Commands.DisbursementRequests.SubmitDisbursementRequest;
using ERP_Government.Application.Security.Common;
using ERP_Government.Domain.Payments.Entities;
using ERP_Government.Domain.Payments.Enums;
using ERP_Government.Domain.Security.Entities;
using ERP_Government.Domain.Security.Enums;
using ERP_Government.Application.UnitTests.Accounting;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using Shouldly;

namespace ERP_Government.Application.UnitTests.Payments;

[TestFixture]
public class DisbursementLifecycleTests
{
    private Mock<IApplicationDbContext> _contextMock = null!;
    private Mock<IIdentityService> _identityServiceMock = null!;
    private Mock<IUser> _validUserMock = null!;
    private Mock<IUser> _secondUserMock = null!;
    private const int UserId1 = 5;
    private const int UserId2 = 10;

    [SetUp]
    public void Setup()
    {
        _contextMock = new Mock<IApplicationDbContext>();
        _identityServiceMock = new Mock<IIdentityService>();
        _validUserMock = new Mock<IUser>();
        _validUserMock.Setup(x => x.Id).Returns(UserId1);
        _secondUserMock = new Mock<IUser>();
        _secondUserMock.Setup(x => x.Id).Returns(UserId2);
    }

    private void SetupDisbursement(
        DisbursementRequestStatus status,
        List<ApprovalHistory>? existingApprovals = null)
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

        var approvals = (existingApprovals ?? new List<ApprovalHistory>())
            .AsQueryable().BuildMockForAsync();
        _contextMock.Setup(x => x.ApprovalHistory).Returns(approvals.Object);

        _contextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);
    }

    private void SetupPayments(List<Domain.Payments.Entities.Payment>? payments = null)
    {
        var paymentsList = payments ?? new List<Domain.Payments.Entities.Payment>();
        _contextMock.Setup(x => x.Payments)
            .Returns(paymentsList.AsQueryable().BuildMockForAsync().Object);
    }

    [Test]
    public async Task T034_SubmitDisbursementRequest_Draft_ShouldTransitionToPendingApproval()
    {
        SetupDisbursement(DisbursementRequestStatus.Draft);

        var handler = new SubmitDisbursementRequestCommandHandler(
            _contextMock.Object, _validUserMock.Object);

        var result = await handler.Handle(
            new SubmitDisbursementRequestCommand { Id = 1, RowVersion = [1, 2, 3] },
            CancellationToken.None);

        result.Succeeded.ShouldBeTrue();
        _contextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task T028_ApproveDisbursementRequest_FirstApprover_AccountsManager_ShouldRecordStep1()
    {
        SetupDisbursement(DisbursementRequestStatus.PendingApproval);

        _identityServiceMock
            .Setup(x => x.IsInRoleAsync(UserId1, "AccountsManager"))
            .ReturnsAsync(true);

        var handler = new ApproveDisbursementRequestCommandHandler(
            _contextMock.Object, _identityServiceMock.Object, _validUserMock.Object);

        var result = await handler.Handle(
            new ApproveDisbursementRequestCommand { Id = 1, Reason = "OK", RowVersion = [1, 2, 3] },
            CancellationToken.None);

        result.Succeeded.ShouldBeTrue();
        _contextMock.Verify(x => x.ApprovalHistory.Add(It.Is<ApprovalHistory>(
            h => h.ApprovalStep == 1
                && h.ApproverUserId == UserId1
                && h.Action == ApprovalAction.Approve)), Times.Once);
        _contextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task T029_ApproveDisbursementRequest_SecondDistinctApprover_ShouldTransitionToApproved()
    {
        var existingApproval = new ApprovalHistory
        {
            DocumentType = "DisbursementRequest",
            DocumentId = 1,
            ApprovalStep = 1,
            Action = ApprovalAction.Approve,
            ApproverUserId = UserId1
        };

        SetupDisbursement(
            DisbursementRequestStatus.PendingApproval,
            new List<ApprovalHistory> { existingApproval });

        var handler = new ApproveDisbursementRequestCommandHandler(
            _contextMock.Object, _identityServiceMock.Object, _secondUserMock.Object);

        var result = await handler.Handle(
            new ApproveDisbursementRequestCommand { Id = 1, Reason = "Approved", RowVersion = [1, 2, 3] },
            CancellationToken.None);

        result.Succeeded.ShouldBeTrue();
        _contextMock.Verify(x => x.ApprovalHistory.Add(It.Is<ApprovalHistory>(
            h => h.ApprovalStep == 2
                && h.ApproverUserId == UserId2
                && h.Action == ApprovalAction.Approve)), Times.Once);
    }

    [Test]
    public async Task T030_ApproveDisbursementRequest_SameUserDoubleApproval_ShouldReject()
    {
        var existingApproval = new ApprovalHistory
        {
            DocumentType = "DisbursementRequest",
            DocumentId = 1,
            ApprovalStep = 1,
            Action = ApprovalAction.Approve,
            ApproverUserId = UserId1
        };

        SetupDisbursement(
            DisbursementRequestStatus.PendingApproval,
            new List<ApprovalHistory> { existingApproval });

        var handler = new ApproveDisbursementRequestCommandHandler(
            _contextMock.Object, _identityServiceMock.Object, _validUserMock.Object);

        var result = await handler.Handle(
            new ApproveDisbursementRequestCommand { Id = 1, Reason = "Again", RowVersion = [1, 2, 3] },
            CancellationToken.None);

        result.Succeeded.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.Contains("A different approver is required"));
        _contextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task T031_ApproveDisbursementRequest_FirstApprover_WrongRole_ShouldReject()
    {
        SetupDisbursement(DisbursementRequestStatus.PendingApproval);

        _identityServiceMock
            .Setup(x => x.IsInRoleAsync(UserId1, It.IsAny<string>()))
            .ReturnsAsync(false);

        var handler = new ApproveDisbursementRequestCommandHandler(
            _contextMock.Object, _identityServiceMock.Object, _validUserMock.Object);

        var result = await handler.Handle(
            new ApproveDisbursementRequestCommand { Id = 1, Reason = "OK", RowVersion = [1, 2, 3] },
            CancellationToken.None);

        result.Succeeded.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.Contains("First approver must hold"));
        _contextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task T033_CancelDisbursementRequest_ApprovedNoPayment_ShouldTransitionToCancelled()
    {
        SetupDisbursement(DisbursementRequestStatus.Approved);
        SetupPayments(new List<Domain.Payments.Entities.Payment>());

        var handler = new CancelDisbursementRequestCommandHandler(
            _contextMock.Object, _validUserMock.Object);

        var result = await handler.Handle(
            new CancelDisbursementRequestCommand
            {
                Id = 1,
                Reason = "No longer needed",
                RowVersion = [1, 2, 3]
            },
            CancellationToken.None);

        result.Succeeded.ShouldBeTrue();
        _contextMock.Verify(x => x.ApprovalHistory.Add(It.Is<ApprovalHistory>(
            h => h.Action == ApprovalAction.Cancel
                && h.ApproverUserId == UserId1)), Times.Once);
        _contextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task T032_RejectDisbursementRequest_PendingApproval_ShouldTransitionToRejected()
    {
        SetupDisbursement(DisbursementRequestStatus.PendingApproval);

        var handler = new RejectDisbursementRequestCommandHandler(
            _contextMock.Object, _validUserMock.Object);

        var result = await handler.Handle(
            new RejectDisbursementRequestCommand
            {
                Id = 1,
                Reason = "Insufficient documentation",
                RowVersion = [1, 2, 3]
            },
            CancellationToken.None);

        result.Succeeded.ShouldBeTrue();
        _contextMock.Verify(x => x.ApprovalHistory.Add(It.Is<ApprovalHistory>(
            h => h.Action == ApprovalAction.Reject
                && h.ApproverUserId == UserId1)), Times.Once);
        _contextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
