using ERP_Government.Application.Revenue.Commands.ReceiptVouchers.CreateReceiptVoucher;
using ERP_Government.Application.Revenue.Commands.ReceiptVouchers.ApproveReceiptVoucher;
using ERP_Government.Application.Revenue.Commands.Checks.ClearCheck;
using ERP_Government.Application.Revenue.Commands.Checks.BounceCheck;
using ERP_Government.Application.Revenue.Queries.ReceiptVouchers.GetReceiptVoucherById;
using ERP_Government.Application.Revenue.Common.DTOs;
using ERP_Government.Domain.Revenue.Enums;
using NUnit.Framework;
using Shouldly;

namespace ERP_Government.Application.FunctionalTests.Revenue;

[TestFixture]
public class CheckClearingTests : TestBase
{
    private async Task<(int VoucherId, int CheckId)> CreateApprovedCheckVoucher(
        string bankName = "Al Rajhi Bank",
        string checkNumber = "CLR001",
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
        await TestApp.SendAsync(new ApproveReceiptVoucherCommand
        {
            Id = voucherId,
            Reason = "Approved for test",
            RowVersion = voucher.Value!.RowVersion
        });

        voucher = await TestApp.SendAsync(new GetReceiptVoucherByIdQuery { Id = voucherId });
        var checkId = voucher.Value!.Checks.First(c => c.CheckNumber == checkNumber).Id;

        return (voucherId, checkId);
    }

    [Test]
    public async Task ClearCheck_UnderCollection_ShouldTransitionToCleared()
    {
        await TestApp.RunAsAdministratorAsync();

        var (_, checkId) = await CreateApprovedCheckVoucher();

        var result = await TestApp.SendAsync(new ClearCheckCommand
        {
            Id = checkId,
            ClearedAt = DateTimeOffset.UtcNow,
            RowVersion = (await TestApp.FindAsync<ERP_Government.Domain.Revenue.Entities.Check>(checkId))!.RowVersion
        });

        result.Succeeded.ShouldBeTrue();

        var check = (await TestApp.FindAsync<ERP_Government.Domain.Revenue.Entities.Check>(checkId))!;
        check.Status.ShouldBe(CheckStatus.Cleared);
        check.ClearedAt.ShouldNotBeNull();
    }

    [Test]
    public async Task ClearCheck_BouncedCheck_ShouldReject()
    {
        await TestApp.RunAsAdministratorAsync();

        var (_, checkId) = await CreateApprovedCheckVoucher("Bank", "CLR002", 30000m);

        var check = (await TestApp.FindAsync<ERP_Government.Domain.Revenue.Entities.Check>(checkId))!;
        await TestApp.SendAsync(new BounceCheckCommand
        {
            Id = checkId,
            BouncedAt = DateTimeOffset.UtcNow,
            Reason = "Insufficient funds",
            RowVersion = check.RowVersion
        });

        check = (await TestApp.FindAsync<ERP_Government.Domain.Revenue.Entities.Check>(checkId))!;
        var result = await TestApp.SendAsync(new ClearCheckCommand
        {
            Id = checkId,
            ClearedAt = DateTimeOffset.UtcNow,
            RowVersion = check.RowVersion
        });

        result.Succeeded.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.Contains("Under-Collection"));
    }

    [Test]
    public async Task ClearCheck_ClearedAtBeforeCheckDate_ShouldReject()
    {
        await TestApp.RunAsAdministratorAsync();

        var (_, checkId) = await CreateApprovedCheckVoucher("Bank", "CLR003", 40000m,
            checkDate: DateOnly.FromDateTime(DateTime.Today.AddDays(-5)));

        var check = (await TestApp.FindAsync<ERP_Government.Domain.Revenue.Entities.Check>(checkId))!;
        var result = await TestApp.SendAsync(new ClearCheckCommand
        {
            Id = checkId,
            ClearedAt = DateTimeOffset.UtcNow.AddDays(-10),
            RowVersion = check.RowVersion
        });

        result.Succeeded.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.Contains("before the check date"));
    }

    [Test]
    public async Task ClearCheck_FutureClearedAt_ShouldReject()
    {
        await TestApp.RunAsAdministratorAsync();

        var (_, checkId) = await CreateApprovedCheckVoucher("Bank", "CLR004", 25000m);

        var check = (await TestApp.FindAsync<ERP_Government.Domain.Revenue.Entities.Check>(checkId))!;
        var result = await TestApp.SendAsync(new ClearCheckCommand
        {
            Id = checkId,
            ClearedAt = DateTimeOffset.UtcNow.AddDays(1),
            RowVersion = check.RowVersion
        });

        result.Succeeded.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.Contains("future"));
    }

    [Test]
    public async Task ClearCheck_CancelledVoucher_ShouldReject()
    {
        await TestApp.RunAsAdministratorAsync();

        var (voucherId, checkId) = await CreateApprovedCheckVoucher("Bank", "CLR005", 60000m);

        var voucher = await TestApp.SendAsync(new GetReceiptVoucherByIdQuery { Id = voucherId });
        await TestApp.SendAsync(new ERP_Government.Application.Revenue.Commands.ReceiptVouchers.CancelReceiptVoucher.CancelReceiptVoucherCommand
        {
            Id = voucherId,
            Reason = "Test cancellation",
            RowVersion = voucher.Value!.RowVersion
        });

        var check = (await TestApp.FindAsync<ERP_Government.Domain.Revenue.Entities.Check>(checkId))!;
        var result = await TestApp.SendAsync(new ClearCheckCommand
        {
            Id = checkId,
            ClearedAt = DateTimeOffset.UtcNow,
            RowVersion = check.RowVersion
        });

        result.Succeeded.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.Contains("Cancelled"));
    }

    [Test]
    public async Task ClearCheck_StaleRowVersion_ShouldFailWithConcurrencyError()
    {
        await TestApp.RunAsAdministratorAsync();

        var (_, checkId) = await CreateApprovedCheckVoucher("Bank", "CLR006", 35000m);

        var staleRowVersion = new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01 };
        var result = await TestApp.SendAsync(new ClearCheckCommand
        {
            Id = checkId,
            ClearedAt = DateTimeOffset.UtcNow,
            RowVersion = staleRowVersion
        });

        result.Succeeded.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.Contains("modified by another user"));
    }
}
