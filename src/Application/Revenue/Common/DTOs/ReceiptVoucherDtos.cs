using ERP_Government.Domain.Revenue.Enums;

namespace ERP_Government.Application.Revenue.Common.DTOs;

public class ReceiptVoucherDto
{
    public int Id { get; init; }
    public string VoucherNumber { get; init; } = string.Empty;
    public DateOnly VoucherDate { get; init; }
    public int PartyId { get; init; }
    public string PartyName { get; init; } = string.Empty;
    public PaymentMethod PaymentMethod { get; init; }
    public string PaymentMethodName { get; init; } = string.Empty;
    public string ReceivedFrom { get; init; } = string.Empty;
    public string? Notes { get; init; }
    public int? DepositSlipId { get; init; }
    public string? DepositSlipNumber { get; init; }
    public ReceiptVoucherStatus Status { get; init; }
    public string StatusName { get; init; } = string.Empty;
    public decimal TotalAmount { get; init; }
    public int? SubmittedById { get; init; }
    public DateTimeOffset? SubmittedAt { get; init; }
    public int? ReviewedById { get; init; }
    public DateTimeOffset? ReviewedAt { get; init; }
    public string? CancellationReason { get; init; }
    public List<ReceiptVoucherLineDto> Lines { get; init; } = new();
    public List<CheckDto> Checks { get; init; } = new();
    public byte[] RowVersion { get; init; } = [];
    public DateTimeOffset Created { get; init; }
    public string? CreatedBy { get; init; }
    public DateTimeOffset LastModified { get; init; }
    public string? LastModifiedBy { get; init; }
}

public class ReceiptVoucherLineDto
{
    public int Id { get; init; }
    public int ReceiptVoucherId { get; init; }
    public int RevenueAccountId { get; init; }
    public decimal Amount { get; init; }
    public string? Description { get; init; }
}

public class CreateReceiptVoucherLineDto
{
    public int RevenueAccountId { get; init; }
    public decimal Amount { get; init; }
    public string? Description { get; init; }
}

public class CheckDto
{
    public int Id { get; init; }
    public int ReceiptVoucherId { get; init; }
    public string BankName { get; init; } = string.Empty;
    public string CheckNumber { get; init; } = string.Empty;
    public DateOnly CheckDate { get; init; }
    public decimal Amount { get; init; }
    public CheckStatus Status { get; init; }
    public string StatusName { get; init; } = string.Empty;
    public DateTimeOffset? ClearedAt { get; init; }
    public DateTimeOffset? BouncedAt { get; init; }
    public int? ReplacementVoucherId { get; init; }
    public DateTimeOffset Created { get; init; }
}

public class CreateCheckDto
{
    public string BankName { get; init; } = string.Empty;
    public string CheckNumber { get; init; } = string.Empty;
    public DateOnly CheckDate { get; init; }
    public decimal Amount { get; init; }
}

public class DepositSlipDto
{
    public int Id { get; init; }
    public string SlipNumber { get; init; } = string.Empty;
    public DateOnly SlipDate { get; init; }
    public FormType FormType { get; init; }
    public string FormTypeName { get; init; } = string.Empty;
    public DepositSlipStatus Status { get; init; }
    public string StatusName { get; init; } = string.Empty;
    public decimal TotalAmount { get; init; }
    public int? ApprovedById { get; init; }
    public DateTimeOffset? ApprovedAt { get; init; }
    public List<ReceiptVoucherDto> ReceiptVouchers { get; init; } = new();
    public byte[] RowVersion { get; init; } = [];
    public DateTimeOffset Created { get; init; }
    public string? CreatedBy { get; init; }
    public DateTimeOffset LastModified { get; init; }
    public string? LastModifiedBy { get; init; }
}

public class MonthlyStatementDto
{
    public int Year { get; init; }
    public int Month { get; init; }
    public int FundId { get; init; }
    public string FundName { get; init; } = string.Empty;
    public DateTimeOffset GeneratedAt { get; init; }
    public MonthlyStatementSummaryDto Summary { get; init; } = new();
    public List<ReceiptVoucherDto> Vouchers { get; init; } = new();
    public List<CheckClearingDto> Clearings { get; init; } = new();
}

public class MonthlyStatementSummaryDto
{
    public decimal TotalCashCollections { get; init; }
    public decimal TotalCheckCollections { get; init; }
    public decimal TotalDeposited { get; init; }
    public decimal TotalUnderCollection { get; init; }
    public decimal TotalCleared { get; init; }
    public decimal TotalBounced { get; init; }
}

public class CheckClearingDto
{
    public string CheckNumber { get; init; } = string.Empty;
    public string BankName { get; init; } = string.Empty;
    public DateTimeOffset ClearedAt { get; init; }
    public decimal Amount { get; init; }
}
