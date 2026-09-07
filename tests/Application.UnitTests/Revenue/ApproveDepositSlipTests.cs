using ERP_Government.Application.Common.Interfaces;
using ERP_Government.Application.Revenue.Commands.DepositSlips.ApproveDepositSlip;
using ERP_Government.Domain.Revenue.Entities;
using ERP_Government.Domain.Revenue.Enums;
using ERP_Government.Domain.Security.Entities;
using ERP_Government.Domain.Security.Enums;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using Shouldly;

namespace ERP_Government.Application.UnitTests.Revenue;

public class ApproveDepositSlipTests
{
    private Mock<IApplicationDbContext> _contextMock = null!;
    private Mock<IUser> _userMock = null!;

    [SetUp]
    public void Setup()
    {
        _contextMock = new Mock<IApplicationDbContext>();
        _contextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        _userMock = new Mock<IUser>();
        _userMock.Setup(u => u.Id).Returns(1);
    }

    private void SetupSlipWithMembers(DepositSlipStatus status, FormType formType, List<ReceiptVoucher>? members = null)
    {
        members ??= new List<ReceiptVoucher>
        {
            new() { Id = 1, Status = ReceiptVoucherStatus.Approved, PaymentMethod = PaymentMethod.Cash,
                     Lines = new List<ReceiptVoucherLine> { new() { Amount = 1000m } },
                     Checks = new List<Check>() }
        };

        var slip = new DepositSlip
        {
            Id = 1,
            SlipNumber = "DSL-000001",
            SlipDate = DateOnly.FromDateTime(DateTime.Today),
            FormType = formType,
            Status = status,
            TotalAmount = members.Sum(m => m.Lines.Sum(l => l.Amount)),
            RowVersion = [],
            ReceiptVouchers = members
        };

        // Use AsQueryable for the DbSet mock so Include/FirstOrDefaultAsync work
        var slips = new List<DepositSlip> { slip };
        var slipMock = new Mock<DbSet<DepositSlip>>();
        slipMock.As<IQueryable<DepositSlip>>().Setup(m => m.Provider).Returns(slips.AsQueryable().Provider);
        slipMock.As<IQueryable<DepositSlip>>().Setup(m => m.Expression).Returns(slips.AsQueryable().Expression);
        slipMock.As<IQueryable<DepositSlip>>().Setup(m => m.ElementType).Returns(slips.AsQueryable().ElementType);
        slipMock.As<IQueryable<DepositSlip>>().Setup(m => m.GetEnumerator()).Returns(() => slips.AsQueryable().GetEnumerator());
        _contextMock.Setup(x => x.DepositSlips).Returns(slipMock.Object);
    }

    private void SetupApprovalHistory()
    {
        _contextMock.Setup(x => x.ApprovalHistory)
            .Returns(new Mock<DbSet<ApprovalHistory>>().Object);
        _contextMock.Setup(x => x.DocumentStatusLogs)
            .Returns(new Mock<DbSet<DocumentStatusLog>>().Object);
    }

    [Test]
    public async Task Handle_EmptyMembers_ShouldReject()
    {
        // FR-005: cannot approve with no members
        SetupSlipWithMembers(DepositSlipStatus.Draft, FormType.Form47,
            new List<ReceiptVoucher>());

        var handler = new ApproveDepositSlipCommandHandler(_contextMock.Object, _userMock.Object);
        var command = new ApproveDepositSlipCommand { Id = 1, RowVersion = [] };

        var result = await handler.Handle(command, CancellationToken.None);

        result.Succeeded.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.Contains("member"));
    }

    [Test]
    public async Task Handle_NonDraft_ShouldReject()
    {
        // Only Draft can be approved
        SetupSlipWithMembers(DepositSlipStatus.Approved, FormType.Form47);

        var handler = new ApproveDepositSlipCommandHandler(_contextMock.Object, _userMock.Object);
        var command = new ApproveDepositSlipCommand { Id = 1, RowVersion = [] };

        var result = await handler.Handle(command, CancellationToken.None);

        result.Succeeded.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.Contains("Draft"));
    }

    [Test]
    public async Task Handle_CancelledMember_ShouldReject()
    {
        // Edge case: member cancelled after joining — must reject with voucher name
        var members = new List<ReceiptVoucher>
        {
            new() { Id = 1, VoucherNumber = "DSL-000042", Status = ReceiptVoucherStatus.Approved,
                     PaymentMethod = PaymentMethod.Cash,
                     Lines = new List<ReceiptVoucherLine> { new() { Amount = 1000m } },
                     Checks = new List<Check>() },
            new() { Id = 2, VoucherNumber = "DSL-000043", Status = ReceiptVoucherStatus.Cancelled,
                     PaymentMethod = PaymentMethod.Cash,
                     Lines = new List<ReceiptVoucherLine> { new() { Amount = 500m } },
                     Checks = new List<Check>() }
        };

        SetupSlipWithMembers(DepositSlipStatus.Draft, FormType.Form47, members);

        var handler = new ApproveDepositSlipCommandHandler(_contextMock.Object, _userMock.Object);
        var command = new ApproveDepositSlipCommand { Id = 1, RowVersion = [] };

        var result = await handler.Handle(command, CancellationToken.None);

        result.Succeeded.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.Contains("DSL-000043"));
    }

    [Test]
    public async Task Handle_Form47_AllApproved_ShouldSucceed()
    {
        // Happy path: Form47 with all approved members → Approved
        SetupSlipWithMembers(DepositSlipStatus.Draft, FormType.Form47);
        SetupApprovalHistory();

        var handler = new ApproveDepositSlipCommandHandler(_contextMock.Object, _userMock.Object);
        var command = new ApproveDepositSlipCommand { Id = 1, RowVersion = [] };

        var result = await handler.Handle(command, CancellationToken.None);

        result.Succeeded.ShouldBeTrue();
    }

    [Test]
    public async Task Handle_Form48_AllApproved_ShouldSucceed()
    {
        // Happy path: Form48 with all approved check members → Approved
        var members = new List<ReceiptVoucher>
        {
            new() { Id = 1, Status = ReceiptVoucherStatus.Approved,
                     PaymentMethod = PaymentMethod.Check,
                     Lines = new List<ReceiptVoucherLine> { new() { Amount = 2000m } },
                     Checks = new List<Check> { new() { Id = 1, Status = CheckStatus.UnderCollection, Amount = 2000m } } }
        };

        SetupSlipWithMembers(DepositSlipStatus.Draft, FormType.Form48, members);
        SetupApprovalHistory();

        var handler = new ApproveDepositSlipCommandHandler(_contextMock.Object, _userMock.Object);
        var command = new ApproveDepositSlipCommand { Id = 1, RowVersion = [] };

        var result = await handler.Handle(command, CancellationToken.None);

        result.Succeeded.ShouldBeTrue();
    }

    [Test]
    public void Validator_RowVersionEmpty_ShouldFail()
    {
        var command = new ApproveDepositSlipCommand { Id = 1, RowVersion = [] };
        var validator = new ApproveDepositSlipCommandValidator();
        var result = validator.Validate(command);
        result.IsValid.ShouldBeFalse();
    }

    [Test]
    public void Validator_ValidCommand_ShouldPass()
    {
        var command = new ApproveDepositSlipCommand { Id = 1, RowVersion = [1, 2, 3] };
        var validator = new ApproveDepositSlipCommandValidator();
        var result = validator.Validate(command);
        result.IsValid.ShouldBeTrue();
    }
}
