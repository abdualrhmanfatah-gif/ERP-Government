using ERP_Government.Application.Common.Interfaces;
using ERP_Government.Application.Revenue.Commands.DepositSlips.AddVoucherToSlip;
using ERP_Government.Application.Revenue.Commands.DepositSlips.RemoveVoucherFromSlip;
using ERP_Government.Domain.Revenue.Entities;
using ERP_Government.Domain.Revenue.Enums;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using Shouldly;

namespace ERP_Government.Application.UnitTests.Revenue;

public class SlipMembershipTests
{
    private Mock<IApplicationDbContext> _contextMock = null!;
    private IUser _user = null!;

    [SetUp]
    public void Setup()
    {
        _contextMock = new Mock<IApplicationDbContext>();
        _contextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        _user = Mock.Of<IUser>(u => u.Id == 1);
    }

    private void SetupSlip(DepositSlipStatus status, FormType formType, List<ReceiptVoucher>? members = null)
    {
        var slip = new DepositSlip
        {
            Id = 1,
            SlipNumber = "DSL-000001",
            SlipDate = DateOnly.FromDateTime(DateTime.Today),
            FormType = formType,
            Status = status,
            TotalAmount = 0,
            RowVersion = [],
            ReceiptVouchers = members ?? new List<ReceiptVoucher>()
        };

        var slipMock = new Mock<DbSet<DepositSlip>>();
        slipMock.Setup(x => x.FindAsync(It.IsAny<object[]>())).ReturnsAsync(slip);
        _contextMock.Setup(x => x.DepositSlips).Returns(slipMock.Object);
    }

    private void SetupVoucher(int id, ReceiptVoucherStatus status, PaymentMethod method, int slipId = 0)
    {
        var voucher = new ReceiptVoucher
        {
            Id = id,
            VoucherDate = DateOnly.FromDateTime(DateTime.Today),
            Status = status,
            PaymentMethod = method,
            DepositSlipId = slipId == 0 ? null : slipId,
            Lines = new List<ReceiptVoucherLine> { new() { Amount = 1000m } },
            Checks = new List<Check>()
        };

        var voucherMock = new Mock<DbSet<ReceiptVoucher>>();
        voucherMock.Setup(x => x.FindAsync(It.IsAny<object[]>())).ReturnsAsync(voucher);
        _contextMock.Setup(x => x.ReceiptVouchers).Returns(voucherMock.Object);
    }

    [Test]
    public async Task RemoveVoucher_EmptyReason_ShouldReject()
    {
        // FR-003: removal MUST require a reason
        SetupSlip(DepositSlipStatus.Draft, FormType.Form47);
        SetupVoucher(1, ReceiptVoucherStatus.Approved, PaymentMethod.Cash, slipId: 1);

        var handler = new RemoveVoucherFromSlipCommandHandler(_contextMock.Object);
        var command = new RemoveVoucherFromSlipCommand
        {
            SlipId = 1,
            VoucherId = 1,
            Reason = "",
            RowVersion = []
        };

        var result = await handler.Handle(command, CancellationToken.None);

        result.Succeeded.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.Contains("reason"));
    }

    [Test]
    public async Task RemoveVoucher_WhitespaceReason_ShouldReject()
    {
        // FR-003: whitespace-only reason is still empty
        SetupSlip(DepositSlipStatus.Draft, FormType.Form47);
        SetupVoucher(1, ReceiptVoucherStatus.Approved, PaymentMethod.Cash, slipId: 1);

        var handler = new RemoveVoucherFromSlipCommandHandler(_contextMock.Object);
        var command = new RemoveVoucherFromSlipCommand
        {
            SlipId = 1,
            VoucherId = 1,
            Reason = "   ",
            RowVersion = []
        };

        var result = await handler.Handle(command, CancellationToken.None);

        result.Succeeded.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.Contains("reason"));
    }

    [Test]
    public async Task RemoveVoucher_ApprovedSlip_ShouldReject()
    {
        // FR-003: only Draft slips can be modified
        SetupSlip(DepositSlipStatus.Approved, FormType.Form47);
        SetupVoucher(1, ReceiptVoucherStatus.Approved, PaymentMethod.Cash, slipId: 1);

        var handler = new RemoveVoucherFromSlipCommandHandler(_contextMock.Object);
        var command = new RemoveVoucherFromSlipCommand
        {
            SlipId = 1,
            VoucherId = 1,
            Reason = "error",
            RowVersion = []
        };

        var result = await handler.Handle(command, CancellationToken.None);

        result.Succeeded.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.Contains("Draft"));
    }

    [Test]
    public async Task AddVoucher_ApprovedSlip_ShouldReject()
    {
        // FR-003: only Draft slips can be modified
        SetupSlip(DepositSlipStatus.Approved, FormType.Form47);
        SetupVoucher(1, ReceiptVoucherStatus.Approved, PaymentMethod.Cash);

        var handler = new AddVoucherToSlipCommandHandler(_contextMock.Object);
        var command = new AddVoucherToSlipCommand
        {
            SlipId = 1,
            VoucherId = 1,
            RowVersion = []
        };

        var result = await handler.Handle(command, CancellationToken.None);

        result.Succeeded.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.Contains("Draft"));
    }

    [Test]
    public async Task AddVoucher_WrongPaymentMethod_ShouldReject()
    {
        // FR-001: homogeneity by PaymentMethod
        SetupSlip(DepositSlipStatus.Draft, FormType.Form47);
        SetupVoucher(1, ReceiptVoucherStatus.Approved, PaymentMethod.Check);

        var handler = new AddVoucherToSlipCommandHandler(_contextMock.Object);
        var command = new AddVoucherToSlipCommand
        {
            SlipId = 1,
            VoucherId = 1,
            RowVersion = []
        };

        var result = await handler.Handle(command, CancellationToken.None);

        result.Succeeded.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.Contains("Form 47"));
    }

    [Test]
    public async Task AddVoucher_AlreadyMember_ShouldReject()
    {
        // Single active membership rule
        SetupSlip(DepositSlipStatus.Draft, FormType.Form47);
        SetupVoucher(1, ReceiptVoucherStatus.Approved, PaymentMethod.Cash, slipId: 1);

        var handler = new AddVoucherToSlipCommandHandler(_contextMock.Object);
        var command = new AddVoucherToSlipCommand
        {
            SlipId = 1,
            VoucherId = 1,
            RowVersion = []
        };

        var result = await handler.Handle(command, CancellationToken.None);

        result.Succeeded.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.Contains("already"));
    }

    [Test]
    public void RemoveValidator_EmptySlipId_ShouldFail()
    {
        var command = new RemoveVoucherFromSlipCommand
        {
            SlipId = 0,
            VoucherId = 1,
            Reason = "test",
            RowVersion = []
        };

        var validator = new RemoveVoucherFromSlipCommandValidator();
        var result = validator.Validate(command);

        result.IsValid.ShouldBeFalse();
    }

    [Test]
    public void AddValidator_EmptyVoucherId_ShouldFail()
    {
        var command = new AddVoucherToSlipCommand
        {
            SlipId = 1,
            VoucherId = 0,
            RowVersion = []
        };

        var validator = new AddVoucherToSlipCommandValidator();
        var result = validator.Validate(command);

        result.IsValid.ShouldBeFalse();
    }
}
