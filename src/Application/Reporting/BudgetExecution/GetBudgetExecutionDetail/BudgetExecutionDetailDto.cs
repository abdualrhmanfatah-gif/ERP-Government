namespace ERP_Government.Application.Reporting.BudgetExecution.GetBudgetExecutionDetail;

public record BudgetExecutionDetailDto
{
    public int BudgetItemId { get; init; }
    public string ItemCode { get; init; } = string.Empty;
    public string ItemName { get; init; } = string.Empty;
    public int FundId { get; init; }
    public string FundNumber { get; init; } = string.Empty;
    public List<EncumbranceDetailDto> Encumbrances { get; init; } = [];
    public List<PaymentDetailDto> Payments { get; init; } = [];
}

public record EncumbranceDetailDto
{
    public int EncumbranceId { get; init; }
    public string EncumbranceNumber { get; init; } = string.Empty;
    public DateOnly EncumbranceDate { get; init; }
    public string? VendorName { get; init; }
    public decimal Amount { get; init; }
    public string Status { get; init; } = string.Empty;
}

public record PaymentDetailDto
{
    public int PaymentOrderId { get; init; }
    public string OrderNumber { get; init; } = string.Empty;
    public DateOnly OrderDate { get; init; }
    public string PayeeName { get; init; } = string.Empty;
    public decimal Amount { get; init; }
    public string Status { get; init; } = string.Empty;
    public DateTimeOffset? PaidAt { get; init; }
}
