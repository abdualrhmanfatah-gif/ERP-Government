using ERP_Government.Domain.Common;

namespace ERP_Government.Domain.Payments.Entities;

public class BankAccount : BaseAuditableEntity
{
    public string Name { get; set; } = string.Empty;
    public string BankName { get; set; } = string.Empty;
    public string AccountNumber { get; set; } = string.Empty;
    public string? Iban { get; set; }
    public string? SwiftCode { get; set; }
    public string? BranchName { get; set; }
    public string? BranchCode { get; set; }
    public int CurrencyId { get; set; }
    public int? FundId { get; set; }
    public int? GlAccountId { get; set; }
    public bool IsDefault { get; set; }
    public decimal? MaxDailyLimit { get; set; }
    public decimal? MaxTransactionLimit { get; set; }
    public bool RequiresDualApproval { get; set; }
    public DateOnly? LastReconciliationDate { get; set; }
    public decimal? OpeningBalance { get; set; }
    public decimal? CurrentBalance { get; set; }
    public bool IsActive { get; set; } = true;
    public byte[] RowVersion { get; set; } = [];
}
