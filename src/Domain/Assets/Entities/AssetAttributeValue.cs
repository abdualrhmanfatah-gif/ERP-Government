using ERP_Government.Domain.Common;

namespace ERP_Government.Domain.Assets.Entities;

public class AssetAttributeValue : BaseAuditableEntity
{
    public int AssetId { get; set; }
    public int AssetAttributeDefinitionId { get; set; }
    public string? TextValue { get; set; }
    public int? IntegerValue { get; set; }
    public decimal? DecimalValue { get; set; }
    public DateOnly? DateValue { get; set; }
    public bool? BooleanValue { get; set; }
    public byte[] RowVersion { get; set; } = [];

    public Asset? Asset { get; set; }
    public AssetAttributeDefinition? AssetAttributeDefinition { get; set; }
}
