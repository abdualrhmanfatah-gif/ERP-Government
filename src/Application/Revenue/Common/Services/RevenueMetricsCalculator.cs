using ERP_Government.Domain.Revenue.Entities;
using ERP_Government.Domain.Revenue.Enums;

namespace ERP_Government.Application.Revenue.Common.Services;

public static class RevenueMetricsCalculator
{
    public static (decimal Collected, decimal UnderCollection, decimal Outstanding, decimal Available) CalculateClaimMetrics(
        decimal totalAmount,
        IEnumerable<CollectionOrder> collectionOrders)
    {
        var vouchers = collectionOrders
            .SelectMany(o => o.ReceiptVouchers)
            .Where(v => v.Status == ReceiptVoucherStatus.Approved)
            .ToList();

        decimal cashCollected = vouchers
            .Where(v => v.PaymentMethod == PaymentMethod.Cash)
            .SelectMany(v => v.Lines)
            .Sum(l => l.Amount);

        decimal clearedChecks = vouchers
            .Where(v => v.PaymentMethod == PaymentMethod.Check)
            .SelectMany(v => v.Checks)
            .Where(c => c.Status == CheckStatus.Cleared)
            .Sum(c => c.Amount);

        decimal collectedAmount = cashCollected + clearedChecks;

        decimal underCollectionAmount = vouchers
            .Where(v => v.PaymentMethod == PaymentMethod.Check)
            .SelectMany(v => v.Checks)
            .Where(c => c.Status == CheckStatus.Received || c.Status == CheckStatus.UnderCollection)
            .Sum(c => c.Amount);

        decimal outstandingAmount = Math.Max(0, totalAmount - collectedAmount);
        decimal availableAmount = Math.Max(0, outstandingAmount - underCollectionAmount);

        return (collectedAmount, underCollectionAmount, outstandingAmount, availableAmount);
    }

    public static (decimal Collected, decimal UnderCollection, decimal Outstanding, decimal Available) CalculateOrderMetrics(
        decimal authorizedAmount,
        IEnumerable<ReceiptVoucher> receiptVouchers)
    {
        var approvedVouchers = receiptVouchers
            .Where(v => v.Status == ReceiptVoucherStatus.Approved)
            .ToList();

        decimal cashCollected = approvedVouchers
            .Where(v => v.PaymentMethod == PaymentMethod.Cash)
            .SelectMany(v => v.Lines)
            .Sum(l => l.Amount);

        decimal clearedChecks = approvedVouchers
            .Where(v => v.PaymentMethod == PaymentMethod.Check)
            .SelectMany(v => v.Checks)
            .Where(c => c.Status == CheckStatus.Cleared)
            .Sum(c => c.Amount);

        decimal collectedAmount = cashCollected + clearedChecks;

        decimal underCollectionAmount = approvedVouchers
            .Where(v => v.PaymentMethod == PaymentMethod.Check)
            .SelectMany(v => v.Checks)
            .Where(c => c.Status == CheckStatus.Received || c.Status == CheckStatus.UnderCollection)
            .Sum(c => c.Amount);

        decimal outstandingAmount = Math.Max(0, authorizedAmount - collectedAmount);
        decimal availableAmount = Math.Max(0, outstandingAmount - underCollectionAmount);

        return (collectedAmount, underCollectionAmount, outstandingAmount, availableAmount);
    }
}
