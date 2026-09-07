using ERP_Government.Application.Revenue.Commands.ReceiptVouchers.CreateReceiptVoucher;
using ERP_Government.Application.Revenue.Commands.ReceiptVouchers.SubmitReceiptVoucher;
using ERP_Government.Application.Revenue.Commands.ReceiptVouchers.ApproveReceiptVoucher;
using ERP_Government.Application.Revenue.Queries.Checks.GetChecks;
using ERP_Government.Application.Revenue.Queries.Checks.GetCheckById;
using ERP_Government.Application.Revenue.Queries.ReceiptVouchers.GetReceiptVoucherById;
using ERP_Government.Application.Revenue.Common.DTOs;
using ERP_Government.Domain.Revenue.Enums;
using NUnit.Framework;
using Shouldly;

namespace ERP_Government.Application.FunctionalTests.Revenue;

[TestFixture]
public class CheckTests : TestBase
{
    private async Task<int> CreateApprovedCheckVoucher(string bankName, string checkNumber, decimal amount)
    {
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
                    CheckDate = DateOnly.FromDateTime(DateTime.Today),
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

        return voucherId;
    }

    [Test]
    public async Task GetChecks_ShouldReturnApprovedCheckMethodVouchers()
    {
        await TestApp.RunAsAdministratorAsync();

        await CreateApprovedCheckVoucher("Al Rajhi Bank", "CHK001", 50000m);

        var result = await TestApp.SendAsync(new GetChecksQuery
        {
            From = DateOnly.FromDateTime(DateTime.Today),
            To = DateOnly.FromDateTime(DateTime.Today)
        });

        result.Succeeded.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value!.Count.ShouldBeGreaterThanOrEqualTo(1);
        result.Value.ShouldContain(c => c.CheckNumber == "CHK001");
    }

    [Test]
    public async Task GetChecks_WithStatusFilter_ShouldReturnOnlyMatchingChecks()
    {
        await TestApp.RunAsAdministratorAsync();

        await CreateApprovedCheckVoucher("Al Rajhi Bank", "CHK002", 30000m);

        var result = await TestApp.SendAsync(new GetChecksQuery
        {
            From = DateOnly.FromDateTime(DateTime.Today),
            To = DateOnly.FromDateTime(DateTime.Today),
            Status = CheckStatus.UnderCollection
        });

        result.Succeeded.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value!.All(c => c.Status == CheckStatus.UnderCollection).ShouldBeTrue();
    }

    [Test]
    public async Task GetChecks_OutsideDateRange_ShouldReturnEmpty()
    {
        await TestApp.RunAsAdministratorAsync();

        await CreateApprovedCheckVoucher("Al Rajhi Bank", "CHK003", 25000m);

        var result = await TestApp.SendAsync(new GetChecksQuery
        {
            From = DateOnly.FromDateTime(DateTime.Today.AddDays(-30)),
            To = DateOnly.FromDateTime(DateTime.Today.AddDays(-31))
        });

        result.Succeeded.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value!.Count.ShouldBe(0);
    }

    [Test]
    public async Task GetCheckById_ShouldReturnCheckDetailDto()
    {
        await TestApp.RunAsAdministratorAsync();

        await CreateApprovedCheckVoucher("Saudi National Bank", "CHK004", 75000m);

        var listResult = await TestApp.SendAsync(new GetChecksQuery
        {
            From = DateOnly.FromDateTime(DateTime.Today),
            To = DateOnly.FromDateTime(DateTime.Today)
        });
        listResult.Succeeded.ShouldBeTrue();
        var checkId = listResult.Value!.First(c => c.CheckNumber == "CHK004").Id;

        var result = await TestApp.SendAsync(new GetCheckByIdQuery { Id = checkId });

        result.Succeeded.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value!.CheckNumber.ShouldBe("CHK004");
        result.Value.BankName.ShouldBe("Saudi National Bank");
        result.Value.Amount.ShouldBe(75000m);
        result.Value.Status.ShouldBe(CheckStatus.UnderCollection);
    }

    [Test]
    public async Task GetCheckById_NonExistent_ShouldReturnNotFound()
    {
        await TestApp.RunAsAdministratorAsync();

        var result = await TestApp.SendAsync(new GetCheckByIdQuery { Id = 999999 });

        result.Succeeded.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.Contains("not found"));
    }
}
