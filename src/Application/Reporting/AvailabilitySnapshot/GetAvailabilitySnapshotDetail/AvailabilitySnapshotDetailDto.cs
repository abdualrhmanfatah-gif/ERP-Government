namespace ERP_Government.Application.Reporting.AvailabilitySnapshot.GetAvailabilitySnapshotDetail;

public record AvailabilitySnapshotDetailDto
{
    public int BudgetItemId { get; init; }
    public string ItemCode { get; init; } = string.Empty;
    public string ItemName { get; init; } = string.Empty;
    public List<AppropriationDetailDto> Appropriations { get; init; } = [];
    public List<EncumbranceDetailDto> Encumbrances { get; init; } = [];
    public List<PaymentDetailDto> Payments { get; init; } = [];
}

public record AppropriationDetailDto
{
    public int AppropriationId { get; init; }
    public string AppropriationNumber { get; init; } = string.Empty;
    public string Type { get; init; } = string.Empty;
    public decimal Amount { get; init; }
    public string Status { get; init; } = string.Empty;
}

public record EncumbranceDetailDto
{
    public int EncumbranceId { get; init; }
    public string EncumbranceNumber { get; init; } = string.Empty;
    public decimal Amount { get; init; }
    public string Status { get; init; } = string.Empty;
}

public record PaymentDetailDto
{
    public int PaymentOrderId { get; init; }
    public string OrderNumber { get; init; } = string.Empty;
    public decimal Amount { get; init; }
    public string Status { get; init; } = string.Empty;
}
