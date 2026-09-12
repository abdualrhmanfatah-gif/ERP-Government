namespace ERP_Government.Application.Reporting.DisbursementRegister.GetDisbursementRegisterDetail;

public record DisbursementRegisterDetailDto
{
    public int PaymentOrderId { get; init; }
    public string OrderNumber { get; init; } = string.Empty;
    public DateOnly OrderDate { get; init; }
    public string PayeeName { get; init; } = string.Empty;
    public decimal Amount { get; init; }
    public string Status { get; init; } = string.Empty;
    public string FundCode { get; init; } = string.Empty;
    public string? ApproverName { get; init; }
    public DateOnly? PaidAt { get; init; }
    public int? AccrualJournalEntryId { get; init; }
    public string? AccrualEntryNumber { get; init; }
    public string? AccrualEntryStatus { get; init; }
    public List<PaymentDetailDto> Payments { get; init; } = [];
}

public record PaymentDetailDto
{
    public int PaymentId { get; init; }
    public string PaymentNumber { get; init; } = string.Empty;
    public decimal Amount { get; init; }
    public string PaymentMethod { get; init; } = string.Empty;
    public DateTimeOffset PaidAt { get; init; }
    public string Status { get; init; } = string.Empty;
}
