namespace ERP_Government.Application.Payments.Common.DTOs;

public class BankAccountDto
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string BankName { get; init; } = string.Empty;
    public string AccountNumber { get; init; } = string.Empty;
    public string? Iban { get; init; }
    public string? SwiftCode { get; init; }
    public string? BranchName { get; init; }
    public string? BranchCode { get; init; }
    public int CurrencyId { get; init; }
    public int? FundId { get; init; }
    public int? GlAccountId { get; init; }
    public bool IsDefault { get; init; }
    public decimal? MaxDailyLimit { get; init; }
    public decimal? MaxTransactionLimit { get; init; }
    public bool RequiresDualApproval { get; init; }
    public DateOnly? LastReconciliationDate { get; init; }
    public decimal? OpeningBalance { get; init; }
    public decimal? CurrentBalance { get; init; }
    public bool IsActive { get; init; }
}
