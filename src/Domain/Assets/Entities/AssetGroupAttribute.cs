using ERP_Government.Domain.Common;

namespace ERP_Government.Domain.Assets.Entities;

public class AssetGroupAttribute : BaseEntity
{
    public int AssetGroupId { get; set; }
    public int AssetAttributeDefinitionId { get; set; }
    public bool IsRequired { get; set; }
    public int? SortOrder { get; set; }

    public AssetGroup? AssetGroup { get; set; }
    public AssetAttributeDefinition? AssetAttributeDefinition { get; set; }
}
