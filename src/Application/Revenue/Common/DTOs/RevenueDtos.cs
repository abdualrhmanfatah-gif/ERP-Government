using ERP_Government.Domain.Revenue.Enums;

namespace ERP_Government.Application.Revenue.Common.DTOs;

public class RevenueClaimDto
{
    public int Id { get; init; }
    public string ClaimNumber { get; init; } = string.Empty;
    public DateOnly ClaimDate { get; init; }
    public int PartyId { get; init; }
    public string PartyName { get; init; } = string.Empty;
    public decimal TotalAmount { get; init; }
    public decimal CollectedAmount { get; init; }
    public decimal UnderCollectionAmount { get; init; }
    public decimal OutstandingAmount { get; init; }
    public decimal AvailableAmount { get; init; }
    public string? Notes { get; init; }
    public ClaimStatus Status { get; init; }
    public string StatusName { get; init; } = string.Empty;
    public List<CollectionOrderDto> CollectionOrders { get; init; } = [];
    public byte[] RowVersion { get; init; } = [];
    public DateTimeOffset Created { get; init; }
    public string? CreatedBy { get; init; }
}

public class CollectionOrderDto
{
    public int Id { get; init; }
    public int RevenueClaimId { get; init; }
    public string RevenueClaimNumber { get; init; } = string.Empty;
    public string OrderNumber { get; init; } = string.Empty;
    public DateOnly OrderDate { get; init; }
    public decimal AuthorizedAmount { get; init; }
    public decimal CollectedAmount { get; init; }
    public decimal UnderCollectionAmount { get; init; }
    public decimal OutstandingAmount { get; init; }
    public decimal AvailableAmount { get; init; }
    public string? Notes { get; init; }
    public CollectionOrderStatus Status { get; init; }
    public string StatusName { get; init; } = string.Empty;
    public List<ReceiptVoucherDto> ReceiptVouchers { get; init; } = [];
    public byte[] RowVersion { get; init; } = [];
    public DateTimeOffset Created { get; init; }
    public string? CreatedBy { get; init; }
}

public class ReceiptVoucherDto
{
    public int Id { get; init; }
    public int CollectionOrderId { get; init; }
    public string CollectionOrderNumber { get; init; } = string.Empty;
    public string VoucherNumber { get; init; } = string.Empty;
    public DateOnly VoucherDate { get; init; }
    public int PartyId { get; init; }
    public string PartyName { get; init; } = string.Empty;
    public PaymentMethod PaymentMethod { get; init; }
    public string PaymentMethodName { get; init; } = string.Empty;
    public string? ReceivedFrom { get; init; }
    public string? Notes { get; init; }
    public int? DepositSlip47Id { get; init; }
    public string? DepositSlip47Number { get; init; }
    public ReceiptVoucherStatus Status { get; init; }
    public string StatusName { get; init; } = string.Empty;
    public decimal TotalAmount { get; init; }
    public int? ApprovedById { get; init; }
    public DateTimeOffset? ApprovedAt { get; init; }
    public string? CancellationReason { get; init; }
    public List<ReceiptVoucherLineDto> Lines { get; init; } = [];
    public List<CheckDto> Checks { get; init; } = [];
    public byte[] RowVersion { get; init; } = [];
    public DateTimeOffset Created { get; init; }
    public string? CreatedBy { get; init; }
}

public class ReceiptVoucherLineDto
{
    public int Id { get; init; }
    public int ReceiptVoucherId { get; init; }
    public int RevenueAccountId { get; init; }
    public string RevenueAccountCode { get; init; } = string.Empty;
    public string RevenueAccountName { get; init; } = string.Empty;
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
    public int? DepositSlip48Id { get; init; }
    public string? DepositSlip48Number { get; init; }
    public DateTimeOffset? ClearedAt { get; init; }
    public DateTimeOffset? BouncedAt { get; init; }
    public int? ReplacementVoucherId { get; init; }
    public byte[] RowVersion { get; init; } = [];
    public DateTimeOffset Created { get; init; }
}

public class CreateCheckDto
{
    public string BankName { get; init; } = string.Empty;
    public string CheckNumber { get; init; } = string.Empty;
    public DateOnly CheckDate { get; init; }
    public decimal Amount { get; init; }
}

public class DepositSlip47Dto
{
    public int Id { get; init; }
    public string SlipNumber { get; init; } = string.Empty;
    public DateOnly SlipDate { get; init; }
    public decimal TotalAmount { get; init; }
    public int? ApprovedById { get; init; }
    public DateTimeOffset? ApprovedAt { get; init; }
    public List<ReceiptVoucherDto> ReceiptVouchers { get; init; } = [];
    public byte[] RowVersion { get; init; } = [];
    public DateTimeOffset Created { get; init; }
    public string? CreatedBy { get; init; }
}

public class DepositSlip48Dto
{
    public int Id { get; init; }
    public string SlipNumber { get; init; } = string.Empty;
    public DateOnly SlipDate { get; init; }
    public decimal TotalAmount { get; init; }
    public int? ApprovedById { get; init; }
    public DateTimeOffset? ApprovedAt { get; init; }
    public List<CheckDto> Checks { get; init; } = [];
    public byte[] RowVersion { get; init; } = [];
    public DateTimeOffset Created { get; init; }
    public string? CreatedBy { get; init; }
}
