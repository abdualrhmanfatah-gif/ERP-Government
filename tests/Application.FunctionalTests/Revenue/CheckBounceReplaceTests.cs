using ERP_Government.Application.Revenue.Commands.ReceiptVouchers.CreateReceiptVoucher;
using ERP_Government.Application.Revenue.Commands.ReceiptVouchers.SubmitReceiptVoucher;
using ERP_Government.Application.Revenue.Commands.ReceiptVouchers.ApproveReceiptVoucher;
using ERP_Government.Application.Revenue.Commands.Checks.BounceCheck;
using ERP_Government.Application.Revenue.Commands.Checks.ReplaceCheck;
using ERP_Government.Application.Revenue.Queries.Checks.GetChecks;
using ERP_Government.Application.Revenue.Queries.ReceiptVouchers.GetReceiptVoucherById;
using ERP_Government.Application.Revenue.Common.DTOs;
using ERP_Government.Domain.Revenue.Entities;
using ERP_Government.Domain.Revenue.Enums;
using NUnit.Framework;
using Shouldly;

namespace ERP_Government.Application.FunctionalTests.Revenue;

[TestFixture]
public class CheckBounceReplaceTests : TestBase
{
    private async Task<(int VoucherId, int CheckId)> CreateApprovedCheckVoucher(
        string bankName = "Al Rajhi Bank",
        string checkNumber = "BNC001",
        decimal amount = 50000m,
        DateOnly? checkDate = null)
    {
        var effectiveCheckDate = checkDate ?? DateOnly.FromDateTime(DateTime.Today);

        var createResult = await TestApp.SendAsync(new CreateReceiptVoucherCommand
        {
            VoucherDate = DateOnly.FromDateTime(DateTime.Today),
            PartyId = 1,
            PaymentMethod = PaymentMethod.Check,
            ReceivedFrom = "Test Party",
            Lines =
            [
                new CreateReceiptVoucherLineDto
                {
                    RevenueAccountId = 1,
                    Amount = amount
                }
            ],
            Checks =
            [
                new CreateCheckDto
                {
                    BankName = bankName,
                    CheckNumber = checkNumber,
                    CheckDate = effectiveCheckDate,
                    Amount = amount
                }
            ]
        });
        createResult.Succeeded.ShouldBeTrue();
        var voucherId = createResult.Value!.Id;

        var voucher = await TestApp.SendAsync(new GetReceiptVoucherByIdQuery { Id = voucherId });
        await TestApp.SendAsync(new SubmitReceiptVoucherCommand
        {
            Id = voucherId,
            RowVersion = voucher.Value!.RowVersion
        });

        voucher = await TestApp.SendAsync(new GetReceiptVoucherByIdQuery { Id = voucherId });
        await TestApp.SendAsync(new ApproveReceiptVoucherCommand
        {
            Id = voucherId,
            Reason = "Approved for test",
            RowVersion = voucher.Value!.RowVersion
        });

        var listResult = await TestApp.SendAsync(new GetChecksQuery
        {
            From = DateOnly.FromDateTime(DateTime.Today.AddDays(-1)),
            To = DateOnly.FromDateTime(DateTime.Today.AddDays(1))
        });
        var checkId = listResult.Value!.First(c => c.CheckNumber == checkNumber).Id;

        return (voucherId, checkId);
    }

    [Test]
    public async Task BounceCheck_UnderCollection_ShouldTransitionToBounced()
    {
        await TestApp.RunAsAdministratorAsync();

        var (_, checkId) = await CreateApprovedCheckVoucher("Bank", "BNC002", 40000m);

        var check = (await TestApp.FindAsync<Check>(checkId))!;
        var result = await TestApp.SendAsync(new BounceCheckCommand
        {
            Id = checkId,
            BouncedAt = DateTimeOffset.UtcNow,
            Reason = "Insufficient funds",
            RowVersion = check.RowVersion
        });

        result.Succeeded.ShouldBeTrue();

        check = (await TestApp.FindAsync<Check>(checkId))!;
        check.Status.ShouldBe(CheckStatus.Bounced);
        check.BouncedAt.ShouldNotBeNull();
        check.ReplacementVoucherId.ShouldBeNull();
    }

    [Test]
    public async Task BounceCheck_ClearedCheck_ShouldReject()
    {
        await TestApp.RunAsAdministratorAsync();

        var (_, checkId) = await CreateApprovedCheckVoucher("Bank", "BNC003", 30000m);

        var check = (await TestApp.FindAsync<Check>(checkId))!;
        await TestApp.SendAsync(new ERP_Government.Application.Revenue.Commands.Checks.ClearCheck.ClearCheckCommand
        {
            Id = checkId,
            ClearedAt = DateTimeOffset.UtcNow,
            RowVersion = check.RowVersion
        });

        check = (await TestApp.FindAsync<Check>(checkId))!;
        var result = await TestApp.SendAsync(new BounceCheckCommand
        {
            Id = checkId,
            BouncedAt = DateTimeOffset.UtcNow,
            Reason = "Late bounce",
            RowVersion = check.RowVersion
        });

        result.Succeeded.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.Contains("Under-Collection"));
    }

    [Test]
    public async Task BounceCheck_BouncedAtBeforeCheckDate_ShouldReject()
    {
        await TestApp.RunAsAdministratorAsync();

        var (_, checkId) = await CreateApprovedCheckVoucher("Bank", "BNC004", 25000m,
            checkDate: DateOnly.FromDateTime(DateTime.Today.AddDays(-5)));

        var check = (await TestApp.FindAsync<Check>(checkId))!;
        var result = await TestApp.SendAsync(new BounceCheckCommand
        {
            Id = checkId,
            BouncedAt = DateTimeOffset.UtcNow.AddDays(-10),
            Reason = "Backdated bounce",
            RowVersion = check.RowVersion
        });

        result.Succeeded.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.Contains("before the check date"));
    }

    [Test]
    public async Task BounceCheck_FutureBouncedAt_ShouldReject()
    {
        await TestApp.RunAsAdministratorAsync();

        var (_, checkId) = await CreateApprovedCheckVoucher("Bank", "BNC005", 35000m);

        var check = (await TestApp.FindAsync<Check>(checkId))!;
        var result = await TestApp.SendAsync(new BounceCheckCommand
        {
            Id = checkId,
            BouncedAt = DateTimeOffset.UtcNow.AddDays(1),
            Reason = "Future bounce",
            RowVersion = check.RowVersion
        });

        result.Succeeded.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.Contains("future"));
    }

    [Test]
    public async Task BounceCheck_CancelledVoucher_ShouldReject()
    {
        await TestApp.RunAsAdministratorAsync();

        var (voucherId, checkId) = await CreateApprovedCheckVoucher("Bank", "BNC006", 60000m);

        var voucher = await TestApp.SendAsync(new GetReceiptVoucherByIdQuery { Id = voucherId });
        await TestApp.SendAsync(new ERP_Government.Application.Revenue.Commands.ReceiptVouchers.CancelReceiptVoucher.CancelReceiptVoucherCommand
        {
            Id = voucherId,
            Reason = "Test cancellation",
            RowVersion = voucher.Value!.RowVersion
        });

        var check = (await TestApp.FindAsync<Check>(checkId))!;
        var result = await TestApp.SendAsync(new BounceCheckCommand
        {
            Id = checkId,
            BouncedAt = DateTimeOffset.UtcNow,
            RowVersion = check.RowVersion
        });

        result.Succeeded.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.Contains("Cancelled"));
    }

    [Test]
    public async Task ReplaceCheck_BouncedWithCash_ShouldCreateReplacementVoucher()
    {
        await TestApp.RunAsAdministratorAsync();

        var (_, checkId) = await CreateApprovedCheckVoucher("Bank", "RPL001", 50000m);

        var check = (await TestApp.FindAsync<Check>(checkId))!;
        await TestApp.SendAsync(new BounceCheckCommand
        {
            Id = checkId,
            BouncedAt = DateTimeOffset.UtcNow,
            Reason = "Insufficient funds",
            RowVersion = check.RowVersion
        });

        check = (await TestApp.FindAsync<Check>(checkId))!;
        var result = await TestApp.SendAsync(new ReplaceCheckCommand
        {
            CheckId = checkId,
            PaymentMethod = PaymentMethod.Cash,
            VoucherDate = DateOnly.FromDateTime(DateTime.Today),
            RowVersion = check.RowVersion
        });

        result.Succeeded.ShouldBeTrue();

        check = (await TestApp.FindAsync<Check>(checkId))!;
        check.ReplacementVoucherId.ShouldNotBeNull();

        var replacementVoucher = (await TestApp.FindAsync<ReceiptVoucher>(check.ReplacementVoucherId!.Value))!;
        replacementVoucher.Status.ShouldBe(ReceiptVoucherStatus.Draft);
        replacementVoucher.PaymentMethod.ShouldBe(PaymentMethod.Cash);
    }

    [Test]
    public async Task ReplaceCheck_BouncedWithCheck_ShouldCreateNewCheckRow()
    {
        await TestApp.RunAsAdministratorAsync();

        var (_, checkId) = await CreateApprovedCheckVoucher("Bank", "RPL002", 75000m);

        var check = (await TestApp.FindAsync<Check>(checkId))!;
        await TestApp.SendAsync(new BounceCheckCommand
        {
            Id = checkId,
            BouncedAt = DateTimeOffset.UtcNow,
            RowVersion = check.RowVersion
        });

        check = (await TestApp.FindAsync<Check>(checkId))!;
        var result = await TestApp.SendAsync(new ReplaceCheckCommand
        {
            CheckId = checkId,
            PaymentMethod = PaymentMethod.Check,
            VoucherDate = DateOnly.FromDateTime(DateTime.Today),
            CheckDetails = new CreateCheckDto
            {
                BankName = "New Bank",
                CheckNumber = "RPL002-NEW",
                CheckDate = DateOnly.FromDateTime(DateTime.Today.AddDays(3)),
                Amount = 75000m
            },
            RowVersion = check.RowVersion
        });

        result.Succeeded.ShouldBeTrue();

        check = (await TestApp.FindAsync<Check>(checkId))!;
        check.ReplacementVoucherId.ShouldNotBeNull();

        var newCheck = (await TestApp.FindAsync<Check>(
            (await TestApp.SendAsync(new GetChecksQuery
            {
                From = DateOnly.FromDateTime(DateTime.Today.AddDays(-1)),
                To = DateOnly.FromDateTime(DateTime.Today.AddDays(1))
            })).Value!.First(c => c.CheckNumber == "RPL002-NEW").Id))!;
        newCheck.Status.ShouldBe(CheckStatus.UnderCollection);
        newCheck.BankName.ShouldBe("New Bank");
    }

    [Test]
    public async Task ReplaceCheck_DoubleReplace_ShouldReject()
    {
        await TestApp.RunAsAdministratorAsync();

        var (_, checkId) = await CreateApprovedCheckVoucher("Bank", "RPL003", 40000m);

        var check = (await TestApp.FindAsync<Check>(checkId))!;
        await TestApp.SendAsync(new BounceCheckCommand
        {
            Id = checkId,
            BouncedAt = DateTimeOffset.UtcNow,
            RowVersion = check.RowVersion
        });

        check = (await TestApp.FindAsync<Check>(checkId))!;
        await TestApp.SendAsync(new ReplaceCheckCommand
        {
            CheckId = checkId,
            PaymentMethod = PaymentMethod.Cash,
            VoucherDate = DateOnly.FromDateTime(DateTime.Today),
            RowVersion = check.RowVersion
        });

        check = (await TestApp.FindAsync<Check>(checkId))!;
        var result = await TestApp.SendAsync(new ReplaceCheckCommand
        {
            CheckId = checkId,
            PaymentMethod = PaymentMethod.Cash,
            VoucherDate = DateOnly.FromDateTime(DateTime.Today),
            RowVersion = check.RowVersion
        });

        result.Succeeded.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.Contains("already been replaced") || e.Contains("double replace") || e.Contains("ReplacementVoucherId"));
    }

    [Test]
    public async Task ReplaceCheck_CheckMethodWithoutDetails_ShouldReject()
    {
        await TestApp.RunAsAdministratorAsync();

        var (_, checkId) = await CreateApprovedCheckVoucher("Bank", "RPL004", 30000m);

        var check = (await TestApp.FindAsync<Check>(checkId))!;
        await TestApp.SendAsync(new BounceCheckCommand
        {
            Id = checkId,
            BouncedAt = DateTimeOffset.UtcNow,
            RowVersion = check.RowVersion
        });

        check = (await TestApp.FindAsync<Check>(checkId))!;
        var result = await TestApp.SendAsync(new ReplaceCheckCommand
        {
            CheckId = checkId,
            PaymentMethod = PaymentMethod.Check,
            VoucherDate = DateOnly.FromDateTime(DateTime.Today),
            CheckDetails = null,
            RowVersion = check.RowVersion
        });

        result.Succeeded.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.Contains("Check details are required"));
    }

    [Test]
    public async Task ReplaceCheck_NonBouncedCheck_ShouldReject()
    {
        await TestApp.RunAsAdministratorAsync();

        var (_, checkId) = await CreateApprovedCheckVoucher("Bank", "RPL005", 25000m);

        var check = (await TestApp.FindAsync<Check>(checkId))!;
        var result = await TestApp.SendAsync(new ReplaceCheckCommand
        {
            CheckId = checkId,
            PaymentMethod = PaymentMethod.Cash,
            VoucherDate = DateOnly.FromDateTime(DateTime.Today),
            RowVersion = check.RowVersion
        });

        result.Succeeded.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.Contains("Bounced"));
    }
}
