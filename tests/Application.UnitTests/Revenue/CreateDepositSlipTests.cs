using ERP_Government.Application.Common.Interfaces;
using ERP_Government.Application.FinancialSettings.Common.Services;
using ERP_Government.Application.Revenue.Commands.DepositSlips.CreateDepositSlip;
using ERP_Government.Domain.Revenue.Entities;
using ERP_Government.Domain.Revenue.Enums;
using ERP_Government.Domain.Security.Entities;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using Shouldly;

namespace ERP_Government.Application.UnitTests.Revenue;

public class CreateDepositSlipTests
{
    private Mock<IApplicationDbContext> _contextMock = null!;
    private Mock<IDocumentSequenceService> _seqMock = null!;
    private CreateDepositSlipCommandHandler _handler = null!;

    [SetUp]
    public void Setup()
    {
        _contextMock = new Mock<IApplicationDbContext>();
        _seqMock = new Mock<IDocumentSequenceService>();
        _seqMock.Setup(s => s.GenerateNextNumberAsync("DepositSlip", It.IsAny<CancellationToken>()))
            .ReturnsAsync("DSL-000001");

        _contextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        _handler = new CreateDepositSlipCommandHandler(
            _contextMock.Object, _seqMock.Object, Mock.Of<IUser>(u => u.Id == 1));
    }

    private void SetupVouchers(List<ReceiptVoucher> vouchers)
    {
        var vouchersMock = new Mock<DbSet<ReceiptVoucher>>();
        var queryable = vouchers.AsQueryable();
        vouchersMock.As<IQueryable<ReceiptVoucher>>().Setup(m => m.Provider).Returns(queryable.Provider);
        vouchersMock.As<IQueryable<ReceiptVoucher>>().Setup(m => m.Expression).Returns(queryable.Expression);
        vouchersMock.As<IQueryable<ReceiptVoucher>>().Setup(m => m.ElementType).Returns(queryable.ElementType);
        vouchersMock.As<IQueryable<ReceiptVoucher>>().Setup(m => m.GetEnumerator()).Returns(() => queryable.GetEnumerator());
        _contextMock.Setup(x => x.ReceiptVouchers).Returns(vouchersMock.Object);
    }

    private void SetupSlips()
    {
        var slipsMock = new Mock<DbSet<DepositSlip>>();
        _contextMock.Setup(x => x.DepositSlips).Returns(slipsMock.Object);
        _contextMock.Setup(x => x.DocumentStatusLogs).Returns(new Mock<DbSet<DocumentStatusLog>>().Object);
    }

    [Test]
    public async Task Handle_EmptyVoucherIds_ShouldCreateDraftSlipWithNoMembers()
    {
        // FR-015: empty creation allowed
        var today = DateOnly.FromDateTime(DateTime.Today);
        SetupVouchers(new List<ReceiptVoucher>());
        SetupSlips();

        var command = new CreateDepositSlipCommand
        {
            SlipDate = today,
            FormType = FormType.Form47,
            VoucherIds = new List<int>()
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Succeeded.ShouldBeTrue();
    }

    [Test]
    public async Task Handle_FutureSlipDate_ShouldReject()
    {
        var tomorrow = DateOnly.FromDateTime(DateTime.Today.AddDays(1));
        SetupVouchers(new List<ReceiptVoucher>());

        var command = new CreateDepositSlipCommand
        {
            SlipDate = tomorrow,
            FormType = FormType.Form47,
            VoucherIds = new List<int>()
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Succeeded.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.Contains("future"));
    }

    [Test]
    public async Task Handle_SlipDateBeforeLatestVoucherDate_ShouldReject()
    {
        var slipDate = new DateOnly(2026, 9, 1);
        var voucherDate = new DateOnly(2026, 9, 5);

        var voucher = new ReceiptVoucher
        {
            Id = 1,
            VoucherDate = voucherDate,
            Status = ReceiptVoucherStatus.Approved,
            PaymentMethod = PaymentMethod.Cash,
            DepositSlipId = null,
            Lines = new List<ReceiptVoucherLine>(),
            Checks = new List<Check>()
        };

        SetupVouchers(new List<ReceiptVoucher> { voucher });

        var command = new CreateDepositSlipCommand
        {
            SlipDate = slipDate,
            FormType = FormType.Form47,
            VoucherIds = new List<int> { 1 }
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Succeeded.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.Contains("latest voucher date"));
    }

    [Test]
    public async Task Handle_NonApprovedVoucher_ShouldReject()
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        var voucher = new ReceiptVoucher
        {
            Id = 1,
            VoucherDate = today,
            Status = ReceiptVoucherStatus.Draft,
            PaymentMethod = PaymentMethod.Cash,
            DepositSlipId = null,
            Lines = new List<ReceiptVoucherLine>(),
            Checks = new List<Check>()
        };

        SetupVouchers(new List<ReceiptVoucher> { voucher });

        var command = new CreateDepositSlipCommand
        {
            SlipDate = today,
            FormType = FormType.Form47,
            VoucherIds = new List<int> { 1 }
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Succeeded.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.Contains("Approved"));
    }

    [Test]
    public async Task Handle_WrongPaymentMethod_ShouldRejectWithHomogeneityMessage()
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        var voucher = new ReceiptVoucher
        {
            Id = 1,
            VoucherDate = today,
            Status = ReceiptVoucherStatus.Approved,
            PaymentMethod = PaymentMethod.Check,
            DepositSlipId = null,
            Lines = new List<ReceiptVoucherLine>(),
            Checks = new List<Check>()
        };

        SetupVouchers(new List<ReceiptVoucher> { voucher });

        var command = new CreateDepositSlipCommand
        {
            SlipDate = today,
            FormType = FormType.Form47,
            VoucherIds = new List<int> { 1 }
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Succeeded.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.Contains("Form 47"));
    }

    [Test]
    public async Task Handle_CashVoucherInForm48_ShouldReject()
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        var voucher = new ReceiptVoucher
        {
            Id = 1,
            VoucherDate = today,
            Status = ReceiptVoucherStatus.Approved,
            PaymentMethod = PaymentMethod.Cash,
            DepositSlipId = null,
            Lines = new List<ReceiptVoucherLine>(),
            Checks = new List<Check>()
        };

        SetupVouchers(new List<ReceiptVoucher> { voucher });

        var command = new CreateDepositSlipCommand
        {
            SlipDate = today,
            FormType = FormType.Form48,
            VoucherIds = new List<int> { 1 }
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Succeeded.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.Contains("Form 48"));
    }

    [Test]
    public void Validator_EmptyVoucherIds_ShouldPass()
    {
        // FR-015: validator must NOT require voucherIds
        var command = new CreateDepositSlipCommand
        {
            SlipDate = DateOnly.FromDateTime(DateTime.Today),
            FormType = FormType.Form47,
            VoucherIds = new List<int>()
        };

        var validator = new CreateDepositSlipCommandValidator();
        var result = validator.Validate(command);

        result.IsValid.ShouldBeTrue();
    }

    [Test]
    public void Validator_FutureDate_ShouldFail()
    {
        var command = new CreateDepositSlipCommand
        {
            SlipDate = DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
            FormType = FormType.Form47,
            VoucherIds = new List<int>()
        };

        var validator = new CreateDepositSlipCommandValidator();
        var result = validator.Validate(command);

        result.IsValid.ShouldBeFalse();
    }
}
