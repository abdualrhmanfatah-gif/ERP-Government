namespace ERP_Government.Application.Reporting.RevenueCollections.GetRevenueCollectionsDetail;

public record RevenueCollectionsDetailDto
{
    public int ReceiptVoucherId { get; init; }
    public string VoucherNumber { get; init; } = string.Empty;
    public DateOnly VoucherDate { get; init; }
    public string PartyName { get; init; } = string.Empty;
    public decimal TotalAmount { get; init; }
    public string PaymentMethod { get; init; } = string.Empty;
    public string? DepositSlipNumber { get; init; }
    public string? DepositSlipStatus { get; init; }
    public List<RevenueCollectionsLineDetailDto> Lines { get; init; } = [];
    public List<CheckDetailDto> Checks { get; init; } = [];
}

public record RevenueCollectionsLineDetailDto
{
    public int RevenueAccountId { get; init; }
    public string AccountCode { get; init; } = string.Empty;
    public string AccountName { get; init; } = string.Empty;
    public decimal Amount { get; init; }
}

public record CheckDetailDto
{
    public int CheckId { get; init; }
    public string BankName { get; init; } = string.Empty;
    public string CheckNumber { get; init; } = string.Empty;
    public DateOnly CheckDate { get; init; }
    public decimal Amount { get; init; }
    public string Status { get; init; } = string.Empty;
    public DateTimeOffset? ClearedAt { get; init; }
}
