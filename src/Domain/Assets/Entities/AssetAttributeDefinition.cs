using ERP_Government.Domain.Assets.Enums;
using ERP_Government.Domain.Common;

namespace ERP_Government.Domain.Assets.Entities;

public class AssetAttributeDefinition : BaseAuditableEntity
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Unit { get; set; }
    public int SortOrder { get; set; }
    public AssetAttributeDataType AttributeDataType { get; set; }
    public bool IsActive { get; set; } = true;
    public byte[] RowVersion { get; set; } = [];
}
