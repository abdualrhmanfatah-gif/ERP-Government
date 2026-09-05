using ERP_Government.Domain.Revenue.Enums;

namespace ERP_Government.Application.Revenue.Common.DTOs;

public class RevenueReceiptDto
{
    public int Id { get; init; }
    public string ReceiptNumber { get; init; } = string.Empty;
    public RevenueReceiptType ReceiptType { get; init; }
    public DateTime ReceiptDate { get; init; }
    public string PayerName { get; init; } = string.Empty;
    public string? PayerNationalId { get; init; }
    public int FundId { get; init; }
    public int? BudgetClassificationId { get; init; }
    public int CurrencyId { get; init; }
    public decimal AmountTotal { get; init; }
    public ERP_Government.Domain.Payments.Enums.PaymentMethod PaymentMethod { get; init; }
    public string PaymentMethodName { get; init; } = string.Empty;
    public string? ExternalTransactionRef { get; init; }
    public int? JournalEntryId { get; init; }
    public RevenueReceiptStatus Status { get; init; }
    public DateTimeOffset Created { get; init; }
    public string? CreatedBy { get; init; }
    public DateTimeOffset LastModified { get; init; }
    public string? LastModifiedBy { get; init; }
    public List<RevenueReceiptLineDto> Lines { get; set; } = [];
}

public class RevenueReceiptLineDto
{
    public int Id { get; init; }
    public int ReceiptId { get; init; }
    public int AccountId { get; init; }
    public string? Description { get; init; }
    public decimal Amount { get; init; }
}

public class CreateRevenueReceiptLineDto
{
    public int AccountId { get; init; }
    public string? Description { get; init; }
    public decimal Amount { get; init; }
}
