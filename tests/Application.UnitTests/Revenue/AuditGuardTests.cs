using ERP_Government.Application.Common.Interfaces;
using ERP_Government.Application.Revenue.Commands.ReceiptVouchers.SubmitReceiptVoucher;
using ERP_Government.Application.Revenue.Commands.ReceiptVouchers.ApproveReceiptVoucher;
using ERP_Government.Application.Revenue.Commands.DepositSlips.ApproveDepositSlip;
using ERP_Government.Application.Revenue.Commands.Checks.ClearCheck;
using ERP_Government.Domain.Revenue.Entities;
using ERP_Government.Domain.Revenue.Enums;
using ERP_Government.Domain.Security.Entities;
using ERP_Government.Application.UnitTests.Accounting;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using Shouldly;

namespace ERP_Government.Application.UnitTests.Revenue;

[TestFixture]
public class AuditGuardTests
{
    private Mock<IApplicationDbContext> _contextMock = null!;
    private Mock<IUser> _nullUserMock = null!;
    private Mock<IUser> _validUserMock = null!;

    [SetUp]
    public void Setup()
    {
        _contextMock = new Mock<IApplicationDbContext>();
        _nullUserMock = new Mock<IUser>();
        _nullUserMock.Setup(x => x.Id).Returns((int?)null);
        _validUserMock = new Mock<IUser>();
        _validUserMock.Setup(x => x.Id).Returns(5);
    }

    // ─── SubmitReceiptVoucher ─────────────────────────────────────

    [Test]
    public async Task SubmitReceiptVoucher_NullUserId_ShouldRejectAndNotLogAudit()
    {
        var vouchers = new List<ReceiptVoucher>
        {
            new() { Id = 1, Status = ReceiptVoucherStatus.Draft, SubmittedById = null }
        }.AsQueryable().BuildMockForAsync();

        var statusLogs = new List<DocumentStatusLog>().AsQueryable().BuildMockForAsync();
        var approvalHistory = new List<ApprovalHistory>().AsQueryable().BuildMockForAsync();

        _contextMock.Setup(x => x.ReceiptVouchers).Returns(vouchers.Object);
        _contextMock.Setup(x => x.DocumentStatusLogs).Returns(statusLogs.Object);
        _contextMock.Setup(x => x.ApprovalHistory).Returns(approvalHistory.Object);
        _contextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var handler = new SubmitReceiptVoucherCommandHandler(_contextMock.Object, _nullUserMock.Object);
        var result = await handler.Handle(
            new SubmitReceiptVoucherCommand { Id = 1, RowVersion = [] },
            CancellationToken.None);

        result.Succeeded.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.Contains("User identity is required"));
        (await statusLogs.Object.CountAsync()).ShouldBe(0);
    }

    [Test]
    public async Task SubmitReceiptVoucher_ValidUserId_ShouldSucceed()
    {
        var voucher = new ReceiptVoucher { Id = 1, Status = ReceiptVoucherStatus.Draft, RowVersion = [] };
        var vouchers = new List<ReceiptVoucher> { voucher }
            .AsQueryable().BuildMockForAsync();

        var statusLogs = new List<DocumentStatusLog>().AsQueryable().BuildMockForAsync();
        _contextMock.Setup(x => x.ReceiptVouchers).Returns(vouchers.Object);
        _contextMock.Setup(x => x.DocumentStatusLogs).Returns(statusLogs.Object);
        _contextMock.Setup(x => x.ReceiptVouchers.FindAsync(
            It.IsAny<object[]>()))
            .Returns<object[]>(keyValues =>
            {
                var id = (int)keyValues[0];
                return ValueTask.FromResult(voucher.Id == id ? voucher : null);
            });
        _contextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var handler = new SubmitReceiptVoucherCommandHandler(_contextMock.Object, _validUserMock.Object);
        var result = await handler.Handle(
            new SubmitReceiptVoucherCommand { Id = 1, RowVersion = [] },
            CancellationToken.None);

        result.Succeeded.ShouldBeTrue();
    }

    // ─── ApproveReceiptVoucher ────────────────────────────────────

    [Test]
    public async Task ApproveReceiptVoucher_NullUserId_ShouldRejectAndNotLogAudit()
    {
        var vouchers = new List<ReceiptVoucher>
        {
            new() { Id = 1, Status = ReceiptVoucherStatus.PendingReview, SubmittedById = 2 }
        }.AsQueryable().BuildMockForAsync();

        var statusLogs = new List<DocumentStatusLog>().AsQueryable().BuildMockForAsync();
        var approvalHistory = new List<ApprovalHistory>().AsQueryable().BuildMockForAsync();

        _contextMock.Setup(x => x.ReceiptVouchers).Returns(vouchers.Object);
        _contextMock.Setup(x => x.DocumentStatusLogs).Returns(statusLogs.Object);
        _contextMock.Setup(x => x.ApprovalHistory).Returns(approvalHistory.Object);
        _contextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var handler = new ApproveReceiptVoucherCommandHandler(_contextMock.Object, _nullUserMock.Object);
        var result = await handler.Handle(
            new ApproveReceiptVoucherCommand { Id = 1, Reason = "Verified", RowVersion = [] },
            CancellationToken.None);

        result.Succeeded.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.Contains("User identity is required"));
        (await statusLogs.Object.CountAsync()).ShouldBe(0);
        (await approvalHistory.Object.CountAsync()).ShouldBe(0);
    }

    [Test]
    public async Task ApproveReceiptVoucher_ValidUserId_ShouldSucceedAndLogAudit()
    {
        var voucher = new ReceiptVoucher
        {
            Id = 1, Status = ReceiptVoucherStatus.PendingReview,
            SubmittedById = 2, RowVersion = []
        };
        var vouchers = new List<ReceiptVoucher> { voucher }
            .AsQueryable().BuildMockForAsync();

        var statusLogs = new List<DocumentStatusLog>().AsQueryable().BuildMockForAsync();
        var approvalHistory = new List<ApprovalHistory>().AsQueryable().BuildMockForAsync();

        _contextMock.Setup(x => x.ReceiptVouchers).Returns(vouchers.Object);
        _contextMock.Setup(x => x.DocumentStatusLogs).Returns(statusLogs.Object);
        _contextMock.Setup(x => x.ApprovalHistory).Returns(approvalHistory.Object);
        _contextMock.Setup(x => x.ReceiptVouchers.FindAsync(
            It.IsAny<object[]>()))
            .Returns<object[]>(keyValues =>
            {
                var id = (int)keyValues[0];
                return ValueTask.FromResult(voucher.Id == id ? voucher : null);
            });
        _contextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var handler = new ApproveReceiptVoucherCommandHandler(_contextMock.Object, _validUserMock.Object);
        var result = await handler.Handle(
            new ApproveReceiptVoucherCommand { Id = 1, Reason = "Verified", RowVersion = [] },
            CancellationToken.None);

        result.Succeeded.ShouldBeTrue();
    }

    // ─── ApproveDepositSlip ───────────────────────────────────────

    [Test]
    public async Task ApproveDepositSlip_NullUserId_ShouldRejectAndNotLogAudit()
    {
        var slips = new List<DepositSlip>
        {
            new() { Id = 1, Status = DepositSlipStatus.Draft, FormType = FormType.Form47, ReceiptVouchers = new List<ReceiptVoucher>() }
        }.AsQueryable().BuildMockForAsync();

        var statusLogs = new List<DocumentStatusLog>().AsQueryable().BuildMockForAsync();
        var approvalHistory = new List<ApprovalHistory>().AsQueryable().BuildMockForAsync();

        _contextMock.Setup(x => x.DepositSlips).Returns(slips.Object);
        _contextMock.Setup(x => x.DocumentStatusLogs).Returns(statusLogs.Object);
        _contextMock.Setup(x => x.ApprovalHistory).Returns(approvalHistory.Object);
        _contextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var handler = new ApproveDepositSlipCommandHandler(_contextMock.Object, _nullUserMock.Object);
        var result = await handler.Handle(
            new ApproveDepositSlipCommand { Id = 1, Reason = "Verified", RowVersion = [] },
            CancellationToken.None);

        result.Succeeded.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.Contains("User identity is required"));
        (await statusLogs.Object.CountAsync()).ShouldBe(0);
        (await approvalHistory.Object.CountAsync()).ShouldBe(0);
    }

    [Test]
    public async Task ApproveDepositSlip_ValidUserId_ShouldSucceedAndLogAudit()
    {
        var slips = new List<DepositSlip>
        {
            new() { Id = 1, Status = DepositSlipStatus.Draft, FormType = FormType.Form47, ReceiptVouchers = new List<ReceiptVoucher>() }
        }.AsQueryable().BuildMockForAsync();

        var statusLogs = new List<DocumentStatusLog>().AsQueryable().BuildMockForAsync();
        var approvalHistory = new List<ApprovalHistory>().AsQueryable().BuildMockForAsync();

        _contextMock.Setup(x => x.DepositSlips).Returns(slips.Object);
        _contextMock.Setup(x => x.DocumentStatusLogs).Returns(statusLogs.Object);
        _contextMock.Setup(x => x.ApprovalHistory).Returns(approvalHistory.Object);
        _contextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var handler = new ApproveDepositSlipCommandHandler(_contextMock.Object, _validUserMock.Object);
        var result = await handler.Handle(
            new ApproveDepositSlipCommand { Id = 1, Reason = "Verified", RowVersion = [] },
            CancellationToken.None);

        result.Succeeded.ShouldBeTrue();
    }

    // ─── ClearCheck ───────────────────────────────────────────────

    [Test]
    public async Task ClearCheck_NullUserId_ShouldRejectAndNotLogAudit()
    {
        var checks = new List<Check>
        {
            new() { Id = 1, Status = CheckStatus.UnderCollection, RowVersion = [], ReceiptVoucher = new ReceiptVoucher { Party = null } }
        }.AsQueryable().BuildMockForAsync();

        var statusLogs = new List<DocumentStatusLog>().AsQueryable().BuildMockForAsync();

        _contextMock.Setup(x => x.Checks).Returns(checks.Object);
        _contextMock.Setup(x => x.DocumentStatusLogs).Returns(statusLogs.Object);
        _contextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var handler = new ClearCheckCommandHandler(_contextMock.Object, _nullUserMock.Object);
        var result = await handler.Handle(
            new ClearCheckCommand { Id = 1, ClearedAt = DateTimeOffset.UtcNow, RowVersion = [] },
            CancellationToken.None);

        result.Succeeded.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.Contains("User identity is required"));
        (await statusLogs.Object.CountAsync()).ShouldBe(0);
    }
}
