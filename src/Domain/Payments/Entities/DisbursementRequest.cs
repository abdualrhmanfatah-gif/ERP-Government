using ERP_Government.Domain.Common;

namespace ERP_Government.Domain.Payments.Entities;

public class DisbursementRequest : BaseAuditableEntity
{
    public string RequestNumber { get; set; } = string.Empty;
    public int RequestedById { get; set; }
    public string RequestedByName { get; set; } = string.Empty;
    public string BeneficiaryName { get; set; } = string.Empty;
    public decimal RequestedAmount { get; set; }
    public int CurrencyId { get; set; }
    public string Purpose { get; set; } = string.Empty;
    public int FinancialYearId { get; set; }
    public DateOnly RequestDate { get; set; }
    public Enums.DisbursementRequestStatus Status { get; set; }
    public string? Notes { get; set; }
    public int? AccrualJournalEntryId { get; set; }
    public DateTimeOffset? PaymentDate { get; set; }
    public byte[] RowVersion { get; set; } = [];
}
