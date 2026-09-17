using ERP_Government.Domain.Revenue.Entities;
using ERP_Government.Domain.Revenue.Enums;
using NUnit.Framework;
using Shouldly;

namespace ERP_Government.Domain.UnitTests.Revenue;

[TestFixture]
public class ReceiptVoucherTests
{
    [Test]
    public void NewVoucher_ShouldHaveDraftStatus()
    {
        var voucher = new ReceiptVoucher
        {
            VoucherNumber = "RCV-000001",
            VoucherDate = DateOnly.FromDateTime(DateTime.Today),
            PartyId = 1,
            PaymentMethod = PaymentMethod.Cash,
            ReceivedFrom = "Test Party",
            Status = ReceiptVoucherStatus.Draft
        };

        voucher.Status.ShouldBe(ReceiptVoucherStatus.Draft);
        voucher.VoucherNumber.ShouldBe("RCV-000001");
    }

    [Test]
    public void Voucher_WithCheckPayment_ShouldAllowCheckDetails()
    {
        var voucher = new ReceiptVoucher
        {
            VoucherNumber = "RCV-000002",
            VoucherDate = DateOnly.FromDateTime(DateTime.Today),
            PartyId = 1,
            PaymentMethod = PaymentMethod.Check,
            ReceivedFrom = "Test Party",
            Status = ReceiptVoucherStatus.Draft
        };

        var check = new Check
        {
            ReceiptVoucherId = voucher.Id,
            BankName = "Al Rajhi Bank",
            CheckNumber = "123456",
            CheckDate = DateOnly.FromDateTime(DateTime.Today),
            Amount = 5000.00m,
            Status = CheckStatus.UnderCollection
        };

        voucher.Checks.Add(check);

        voucher.Checks.Count.ShouldBe(1);
        voucher.Checks.First().BankName.ShouldBe("Al Rajhi Bank");
    }

    [Test]
    public void Voucher_LineAmount_ShouldBePositive()
    {
        var line = new ReceiptVoucherLine
        {
            ReceiptVoucherId = 1,
            RevenueAccountId = 1,
            Amount = 1000.00m
        };

        line.Amount.ShouldBeGreaterThan(0);
    }

    [Test]
    public void Voucher_CanTransitionToApproved()
    {
        var voucher = new ReceiptVoucher
        {
            VoucherNumber = "RCV-000004",
            VoucherDate = DateOnly.FromDateTime(DateTime.Today),
            PartyId = 1,
            PaymentMethod = PaymentMethod.Cash,
            ReceivedFrom = "Test Party",
            Status = ReceiptVoucherStatus.Draft
        };

        voucher.Status = ReceiptVoucherStatus.Approved;
        voucher.Status.ShouldBe(ReceiptVoucherStatus.Approved);
    }

    [Test]
    public void Voucher_CanTransitionToCancelled()
    {
        var voucher = new ReceiptVoucher
        {
            VoucherNumber = "RCV-000005",
            VoucherDate = DateOnly.FromDateTime(DateTime.Today),
            PartyId = 1,
            PaymentMethod = PaymentMethod.Cash,
            ReceivedFrom = "Test Party",
            Status = ReceiptVoucherStatus.Draft
        };

        voucher.Status = ReceiptVoucherStatus.Cancelled;
        voucher.CancellationReason = "Party requested cancellation";
        voucher.Status.ShouldBe(ReceiptVoucherStatus.Cancelled);
        voucher.CancellationReason.ShouldBe("Party requested cancellation");
    }

    [Test]
    public void Check_CanTransitionToCleared()
    {
        var check = new Check
        {
            ReceiptVoucherId = 1,
            BankName = "Al Rajhi Bank",
            CheckNumber = "123456",
            CheckDate = DateOnly.FromDateTime(DateTime.Today),
            Amount = 5000.00m,
            Status = CheckStatus.UnderCollection
        };

        check.Status = CheckStatus.Cleared;
        check.ClearedAt = DateTimeOffset.UtcNow;

        check.Status.ShouldBe(CheckStatus.Cleared);
        check.ClearedAt.ShouldNotBeNull();
    }

    [Test]
    public void Check_CanTransitionToBounced()
    {
        var check = new Check
        {
            ReceiptVoucherId = 1,
            BankName = "Al Rajhi Bank",
            CheckNumber = "123456",
            CheckDate = DateOnly.FromDateTime(DateTime.Today),
            Amount = 5000.00m,
            Status = CheckStatus.UnderCollection
        };

        check.Status = CheckStatus.Bounced;
        check.BouncedAt = DateTimeOffset.UtcNow;

        check.Status.ShouldBe(CheckStatus.Bounced);
        check.BouncedAt.ShouldNotBeNull();
    }
}
