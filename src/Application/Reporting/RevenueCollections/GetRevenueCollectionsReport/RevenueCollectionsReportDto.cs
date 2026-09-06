namespace ERP_Government.Application.Reporting.RevenueCollections.GetRevenueCollectionsReport;

public record RevenueCollectionsReportDto
{
    public int FiscalYearId { get; init; }
    public string FiscalYearName { get; init; } = string.Empty;
    public List<RevenueCollectionsLineDto> Lines { get; init; } = [];
    public RevenueCollectionsTotalDto Totals { get; init; } = new();
}

public record RevenueCollectionsLineDto
{
    public int ReceiptVoucherId { get; init; }
    public string VoucherNumber { get; init; } = string.Empty;
    public DateOnly VoucherDate { get; init; }
    public int RevenueAccountId { get; init; }
    public string AccountCode { get; init; } = string.Empty;
    public string AccountName { get; init; } = string.Empty;
    public int PartyId { get; init; }
    public string PartyName { get; init; } = string.Empty;
    public decimal Amount { get; init; }
    public string PaymentMethod { get; init; } = string.Empty;
    public int? DepositSlipId { get; init; }
    public string? DepositSlipNumber { get; init; }
    public string? DepositSlipStatus { get; init; }
    public string CheckClearingStatus { get; init; } = string.Empty;
}

public record RevenueCollectionsTotalDto
{
    public decimal TotalAmount { get; init; }
    public decimal TotalCash { get; init; }
    public decimal TotalChecks { get; init; }
    public int PendingDeposits { get; init; }
    public int ClearedChecks { get; init; }
    public int BouncedChecks { get; init; }
}
