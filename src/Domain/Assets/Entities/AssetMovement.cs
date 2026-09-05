using ERP_Government.Domain.Common;

namespace ERP_Government.Domain.Assets.Entities;

public class AssetMovement : BaseAuditableEntity
{
    public string MovementNumber { get; set; } = string.Empty;
    public int AssetId { get; set; }
    public string MovementType { get; set; } = string.Empty;
    public DateTime MovementDate { get; set; }
    public string? ReferenceType { get; set; }
    public int? ReferenceId { get; set; }
    public int? FromLocationId { get; set; }
    public int? ToLocationId { get; set; }
    public int? FromCustodianId { get; set; }
    public int? ToCustodianId { get; set; }
    public int? FromDepartmentId { get; set; }
    public int? ToDepartmentId { get; set; }
    public decimal? OldValue { get; set; }
    public decimal? NewValue { get; set; }
    public decimal? Amount { get; set; }
    public string? CurrencyCode { get; set; }
    public int? JournalEntryId { get; set; }
    public string? Notes { get; set; }
    public byte[] RowVersion { get; set; } = [];

    public Asset? Asset { get; set; }
}
