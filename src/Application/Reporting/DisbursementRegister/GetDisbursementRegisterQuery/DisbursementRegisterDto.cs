namespace ERP_Government.Application.Reporting.DisbursementRegister.GetDisbursementRegisterQuery;

public record DisbursementRegisterDto
{
    public int FiscalYearId { get; init; }
    public string FiscalYearName { get; init; } = string.Empty;
    public List<DisbursementRegisterLineDto> Lines { get; init; } = [];
    public DisbursementRegisterTotalDto Totals { get; init; } = new();
}

public record DisbursementRegisterLineDto
{
    public int PaymentOrderId { get; init; }
    public string OrderNumber { get; init; } = string.Empty;
    public DateOnly OrderDate { get; init; }
    public string PayeeName { get; init; } = string.Empty;
    public decimal Amount { get; init; }
    public string Status { get; init; } = string.Empty;
    public int FundId { get; init; }
    public string FundCode { get; init; } = string.Empty;
    public string FundName { get; init; } = string.Empty;
    public int? ApproverId { get; init; }
    public string? ApproverName { get; init; }
    public DateOnly? PaidAt { get; init; }
    public int? AccrualJournalEntryId { get; init; }
    public string? AccrualEntryNumber { get; init; }
    public string? AccrualEntryStatus { get; init; }
}

public record DisbursementRegisterTotalDto
{
    public int TotalRequests { get; init; }
    public int DraftCount { get; init; }
    public int SubmittedCount { get; init; }
    public int ApprovedCount { get; init; }
    public int PaidCount { get; init; }
    public int RejectedCount { get; init; }
    public decimal TotalAmount { get; init; }
    public decimal PaidAmount { get; init; }
}
