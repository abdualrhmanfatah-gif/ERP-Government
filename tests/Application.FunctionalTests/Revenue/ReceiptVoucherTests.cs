using ERP_Government.Application.Revenue.Commands.ReceiptVouchers.CreateReceiptVoucher;
using ERP_Government.Application.Revenue.Commands.ReceiptVouchers.ApproveReceiptVoucher;
using ERP_Government.Application.Revenue.Commands.ReceiptVouchers.CancelReceiptVoucher;
using ERP_Government.Application.Revenue.Queries.ReceiptVouchers.GetReceiptVouchers;
using ERP_Government.Application.Revenue.Queries.ReceiptVouchers.GetReceiptVoucherById;
using ERP_Government.Domain.Revenue.Enums;
using NUnit.Framework;
using Shouldly;

namespace ERP_Government.Application.FunctionalTests.Revenue;

[TestFixture]
public class ReceiptVoucherTests : TestBase
{
    [Test]
    public async Task CreateVoucher_WithCashPayment_ShouldAssignVoucherNumber()
    {
        await TestApp.RunAsAdministratorAsync();

        var result = await TestApp.SendAsync(new CreateReceiptVoucherCommand
        {
            VoucherDate = DateOnly.FromDateTime(DateTime.Today),
            PartyId = 1,
            PaymentMethod = PaymentMethod.Cash,
            ReceivedFrom = "Mohammed Al-Rashid",
            Notes = "Monthly tax payment",
            Lines =
            [
                new Application.Revenue.Common.DTOs.CreateReceiptVoucherLineDto
                {
                    RevenueAccountId = 1,
                    Amount = 5000.00m,
                    Description = "September 2026 tax"
                }
            ],
            Checks = []
        });

        result.Succeeded.ShouldBeTrue();
        var voucherId = result.Value!.Id;

        var voucherResult = await TestApp.SendAsync(new GetReceiptVoucherByIdQuery { Id = voucherId });
        voucherResult.Succeeded.ShouldBeTrue();
        var voucher = voucherResult.Value!;
        voucher.VoucherNumber.ShouldStartWith("DSL-");
        System.Text.RegularExpressions.Regex.IsMatch(voucher.VoucherNumber, @"^DSL-\d{6}$").ShouldBeTrue();
        voucher.Status.ShouldBe(ReceiptVoucherStatus.Draft);
        voucher.TotalAmount.ShouldBe(5000.00m);
        voucher.PaymentMethod.ShouldBe(PaymentMethod.Cash);
        voucher.Lines.Count.ShouldBe(1);
    }

    [Test]
    public async Task CreateVoucher_ShouldReturnFullDtoWithNumberImmediately()
    {
        await TestApp.RunAsAdministratorAsync();

        var result = await TestApp.SendAsync(new CreateReceiptVoucherCommand
        {
            VoucherDate = DateOnly.FromDateTime(DateTime.Today),
            PartyId = 1,
            PaymentMethod = PaymentMethod.Cash,
            ReceivedFrom = "",
            Lines =
            [
                new Application.Revenue.Common.DTOs.CreateReceiptVoucherLineDto
                {
                    RevenueAccountId = 1,
                    Amount = 2500.00m
                }
            ],
            Checks = []
        });

        result.Succeeded.ShouldBeTrue();
        var dto = result.Value!;
        dto.VoucherNumber.ShouldStartWith("DSL-");
        dto.Status.ShouldBe(ReceiptVoucherStatus.Draft);
        dto.TotalAmount.ShouldBe(2500.00m);
        dto.PartyId.ShouldBe(1);
        dto.RowVersion.ShouldNotBeNull();
    }

    [Test]
    public async Task CreateVoucher_WithEmptyReceivedFrom_ShouldSucceed()
    {
        await TestApp.RunAsAdministratorAsync();

        var result = await TestApp.SendAsync(new CreateReceiptVoucherCommand
        {
            VoucherDate = DateOnly.FromDateTime(DateTime.Today),
            PartyId = 1,
            PaymentMethod = PaymentMethod.Cash,
            ReceivedFrom = "",
            Lines =
            [
                new Application.Revenue.Common.DTOs.CreateReceiptVoucherLineDto
                {
                    RevenueAccountId = 1,
                    Amount = 1000.00m
                }
            ],
            Checks = []
        });

        result.Succeeded.ShouldBeTrue();
    }

    [Test]
    public async Task CreateVoucher_ChecksExceedingTotal_ShouldReject()
    {
        await TestApp.RunAsAdministratorAsync();

        var result = await TestApp.SendAsync(new CreateReceiptVoucherCommand
        {
            VoucherDate = DateOnly.FromDateTime(DateTime.Today),
            PartyId = 1,
            PaymentMethod = PaymentMethod.Check,
            ReceivedFrom = "Test Party",
            Lines =
            [
                new Application.Revenue.Common.DTOs.CreateReceiptVoucherLineDto
                {
                    RevenueAccountId = 1,
                    Amount = 3000.00m
                }
            ],
            Checks =
            [
                new Application.Revenue.Common.DTOs.CreateCheckDto
                {
                    BankName = "Bank",
                    CheckNumber = "001",
                    CheckDate = DateOnly.FromDateTime(DateTime.Today),
                    Amount = 3500.00m
                }
            ]
        });

        result.Succeeded.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.Contains("exceed"));
    }

    [Test]
    public async Task CreateVoucher_ChecksBelowTotal_ShouldSucceed()
    {
        await TestApp.RunAsAdministratorAsync();

        var result = await TestApp.SendAsync(new CreateReceiptVoucherCommand
        {
            VoucherDate = DateOnly.FromDateTime(DateTime.Today),
            PartyId = 1,
            PaymentMethod = PaymentMethod.Check,
            ReceivedFrom = "Test Party",
            Lines =
            [
                new Application.Revenue.Common.DTOs.CreateReceiptVoucherLineDto
                {
                    RevenueAccountId = 1,
                    Amount = 3000.00m
                }
            ],
            Checks =
            [
                new Application.Revenue.Common.DTOs.CreateCheckDto
                {
                    BankName = "Bank",
                    CheckNumber = "002",
                    CheckDate = DateOnly.FromDateTime(DateTime.Today),
                    Amount = 2500.00m
                }
            ]
        });

        result.Succeeded.ShouldBeTrue();
    }

    [Test]
    public async Task CreateVoucher_WithUnknownParty_ShouldReject()
    {
        await TestApp.RunAsAdministratorAsync();

        var result = await TestApp.SendAsync(new CreateReceiptVoucherCommand
        {
            VoucherDate = DateOnly.FromDateTime(DateTime.Today),
            PartyId = 99999,
            PaymentMethod = PaymentMethod.Cash,
            ReceivedFrom = "",
            Lines =
            [
                new Application.Revenue.Common.DTOs.CreateReceiptVoucherLineDto
                {
                    RevenueAccountId = 1,
                    Amount = 1000.00m
                }
            ],
            Checks = []
        });

        result.Succeeded.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.Contains("Party not found or inactive"));
    }

    [Test]
    public async Task CreateVoucher_WithCheckPayment_ShouldCaptureCheckDetails()
    {
        await TestApp.RunAsAdministratorAsync();

        var result = await TestApp.SendAsync(new CreateReceiptVoucherCommand
        {
            VoucherDate = DateOnly.FromDateTime(DateTime.Today),
            PartyId = 1,
            PaymentMethod = PaymentMethod.Check,
            ReceivedFrom = "Sara Company",
            Lines =
            [
                new Application.Revenue.Common.DTOs.CreateReceiptVoucherLineDto
                {
                    RevenueAccountId = 1,
                    Amount = 7500.00m,
                    Description = "License fee"
                }
            ],
            Checks =
            [
                new Application.Revenue.Common.DTOs.CreateCheckDto
                {
                    BankName = "Al Rajhi Bank",
                    CheckNumber = "123456",
                    CheckDate = DateOnly.FromDateTime(DateTime.Today),
                    Amount = 7500.00m
                }
            ]
        });

        result.Succeeded.ShouldBeTrue();
        var voucherId = result.Value!.Id;

        var voucherResult = await TestApp.SendAsync(new GetReceiptVoucherByIdQuery { Id = voucherId });
        voucherResult.Succeeded.ShouldBeTrue();
        var voucher = voucherResult.Value!;
        voucher.Checks.Count.ShouldBe(1);
        voucher.Checks.First().BankName.ShouldBe("Al Rajhi Bank");
        voucher.Checks.First().CheckNumber.ShouldBe("123456");
    }

    [Test]
    public async Task CreateVoucher_WithoutLines_ShouldReject()
    {
        await TestApp.RunAsAdministratorAsync();

        var result = await TestApp.SendAsync(new CreateReceiptVoucherCommand
        {
            VoucherDate = DateOnly.FromDateTime(DateTime.Today),
            PartyId = 1,
            PaymentMethod = PaymentMethod.Cash,
            ReceivedFrom = "Test Party",
            Lines = [],
            Checks = []
        });

        result.Succeeded.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.Contains("At least one line is required"));
    }

    [Test]
    public async Task CreateVoucher_WithZeroAmountLine_ShouldReject()
    {
        await TestApp.RunAsAdministratorAsync();

        var result = await TestApp.SendAsync(new CreateReceiptVoucherCommand
        {
            VoucherDate = DateOnly.FromDateTime(DateTime.Today),
            PartyId = 1,
            PaymentMethod = PaymentMethod.Cash,
            ReceivedFrom = "Test Party",
            Lines =
            [
                new Application.Revenue.Common.DTOs.CreateReceiptVoucherLineDto
                {
                    RevenueAccountId = 1,
                    Amount = 0
                }
            ],
            Checks = []
        });

        result.Succeeded.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.Contains("amount must be greater than zero"));
    }

    [Test]
    public async Task CreateVoucher_CheckPaymentWithoutCheckDetails_ShouldReject()
    {
        await TestApp.RunAsAdministratorAsync();

        var result = await TestApp.SendAsync(new CreateReceiptVoucherCommand
        {
            VoucherDate = DateOnly.FromDateTime(DateTime.Today),
            PartyId = 1,
            PaymentMethod = PaymentMethod.Check,
            ReceivedFrom = "Test Party",
            Lines =
            [
                new Application.Revenue.Common.DTOs.CreateReceiptVoucherLineDto
                {
                    RevenueAccountId = 1,
                    Amount = 5000.00m
                }
            ],
            Checks = []
        });

        result.Succeeded.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.Contains("At least one check is required"));
    }

    [Test]
    public async Task ApproveVoucher_FromDraft_ShouldTransitionToApproved()
    {
        await TestApp.RunAsAdministratorAsync();

        var createResult = await TestApp.SendAsync(new CreateReceiptVoucherCommand
        {
            VoucherDate = DateOnly.FromDateTime(DateTime.Today),
            PartyId = 1,
            PaymentMethod = PaymentMethod.Cash,
            ReceivedFrom = "Test Party",
            Lines =
            [
                new Application.Revenue.Common.DTOs.CreateReceiptVoucherLineDto
                {
                    RevenueAccountId = 1,
                    Amount = 1000.00m
                }
            ],
            Checks = []
        });
        createResult.Succeeded.ShouldBeTrue();
        var voucherId = createResult.Value!.Id;

        var voucherResult = await TestApp.SendAsync(new GetReceiptVoucherByIdQuery { Id = voucherId });
        var approveResult = await TestApp.SendAsync(new ApproveReceiptVoucherCommand
        {
            Id = voucherId,
            Reason = "Verified against party records",
            RowVersion = voucherResult.Value!.RowVersion
        });
        approveResult.Succeeded.ShouldBeTrue();

        voucherResult = await TestApp.SendAsync(new GetReceiptVoucherByIdQuery { Id = voucherId });
        voucherResult.Value!.Status.ShouldBe(ReceiptVoucherStatus.Approved);
        voucherResult.Value!.ApprovedAt.ShouldNotBeNull();
    }

    [Test]
    public async Task ApproveVoucher_AlreadyApproved_ShouldReject()
    {
        await TestApp.RunAsAdministratorAsync();

        var createResult = await TestApp.SendAsync(new CreateReceiptVoucherCommand
        {
            VoucherDate = DateOnly.FromDateTime(DateTime.Today),
            PartyId = 1,
            PaymentMethod = PaymentMethod.Cash,
            ReceivedFrom = "Test Party",
            Lines =
            [
                new Application.Revenue.Common.DTOs.CreateReceiptVoucherLineDto
                {
                    RevenueAccountId = 1,
                    Amount = 1000.00m
                }
            ],
            Checks = []
        });
        createResult.Succeeded.ShouldBeTrue();
        var voucherId = createResult.Value!.Id;

        var voucherResult = await TestApp.SendAsync(new GetReceiptVoucherByIdQuery { Id = voucherId });
        await TestApp.SendAsync(new ApproveReceiptVoucherCommand
        {
            Id = voucherId,
            Reason = "First approval",
            RowVersion = voucherResult.Value!.RowVersion
        });

        voucherResult = await TestApp.SendAsync(new GetReceiptVoucherByIdQuery { Id = voucherId });
        var approveResult = await TestApp.SendAsync(new ApproveReceiptVoucherCommand
        {
            Id = voucherId,
            Reason = "Second approval attempt",
            RowVersion = voucherResult.Value!.RowVersion
        });
        approveResult.Succeeded.ShouldBeFalse();
        approveResult.Errors.ShouldContain(e => e.Contains("Only Draft receipt vouchers can be approved"));
    }

    [Test]
    public async Task CancelVoucher_ShouldTransitionToCancelled()
    {
        await TestApp.RunAsAdministratorAsync();

        var createResult = await TestApp.SendAsync(new CreateReceiptVoucherCommand
        {
            VoucherDate = DateOnly.FromDateTime(DateTime.Today),
            PartyId = 1,
            PaymentMethod = PaymentMethod.Cash,
            ReceivedFrom = "Test Party",
            Lines =
            [
                new Application.Revenue.Common.DTOs.CreateReceiptVoucherLineDto
                {
                    RevenueAccountId = 1,
                    Amount = 1000.00m
                }
            ],
            Checks = []
        });
        createResult.Succeeded.ShouldBeTrue();
        var voucherId = createResult.Value!.Id;

        var voucherResult = await TestApp.SendAsync(new GetReceiptVoucherByIdQuery { Id = voucherId });
        var cancelResult = await TestApp.SendAsync(new CancelReceiptVoucherCommand
        {
            Id = voucherId,
            Reason = "Party requested cancellation",
            RowVersion = voucherResult.Value!.RowVersion
        });
        cancelResult.Succeeded.ShouldBeTrue();

        voucherResult = await TestApp.SendAsync(new GetReceiptVoucherByIdQuery { Id = voucherId });
        voucherResult.Value!.Status.ShouldBe(ReceiptVoucherStatus.Cancelled);
        voucherResult.Value!.CancellationReason.ShouldBe("Party requested cancellation");
    }

    [Test]
    public async Task GetVouchers_ShouldReturnList()
    {
        await TestApp.RunAsAdministratorAsync();

        for (int i = 0; i < 3; i++)
        {
            await TestApp.SendAsync(new CreateReceiptVoucherCommand
            {
                VoucherDate = DateOnly.FromDateTime(DateTime.Today),
                PartyId = 1,
                PaymentMethod = PaymentMethod.Cash,
                ReceivedFrom = $"Test Party {i}",
                Lines =
                [
                    new Application.Revenue.Common.DTOs.CreateReceiptVoucherLineDto
                    {
                        RevenueAccountId = 1,
                        Amount = 1000.00m * (i + 1)
                    }
                ],
                Checks = []
            });
        }

        var result = await TestApp.SendAsync(new GetReceiptVouchersQuery
        {
            PartyId = 1
        });

        result.Count.ShouldBeGreaterThanOrEqualTo(3);
    }
}
