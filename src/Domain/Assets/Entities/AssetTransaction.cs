using ERP_Government.Domain.Accounting.Entities;
using ERP_Government.Domain.Assets.Enums;
using ERP_Government.Domain.Common;
using ERP_Government.Domain.FinancialSettings.Entities;

namespace ERP_Government.Domain.Assets.Entities;

public class AssetTransaction : BaseAuditableEntity
{
    public string TransactionNumber { get; set; } = string.Empty;
    public int AssetId { get; set; }
    public AssetTransactionType TransactionType { get; set; }
    public DateOnly TransactionDate { get; set; }
    public AssetTransactionStatus Status { get; set; } = AssetTransactionStatus.Draft;
    public int CurrencyId { get; set; }
    public int? JournalEntryId { get; set; }
    public bool IsPosted { get; set; }
    public string? ReferenceType { get; set; }
    public int? ReferenceId { get; set; }
    public string? Notes { get; set; }
    public byte[] RowVersion { get; set; } = [];

    public Asset? Asset { get; set; }
    public Currency? Currency { get; set; }
    public JournalEntry? JournalEntry { get; set; }
}
