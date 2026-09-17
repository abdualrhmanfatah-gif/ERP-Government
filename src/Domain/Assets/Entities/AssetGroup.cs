using ERP_Government.Domain.Accounting.Entities;
using ERP_Government.Domain.Common;

namespace ERP_Government.Domain.Assets.Entities;

public class AssetGroup : BaseAuditableEntity
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int? ParentAssetGroupId { get; set; }
    public int? AssetAccountId { get; set; }
    public int? AccumulatedDepreciationAccountId { get; set; }
    public int? DepreciationExpenseAccountId { get; set; }
    public int? DisposalAccountId { get; set; }
    public string DepreciationMethod { get; set; } = string.Empty;
    public decimal? DepreciationRate { get; set; }
    public int? DefaultUsefulLifeYears { get; set; }
    public decimal? ResidualValuePercentage { get; set; }
    public bool IsDepreciable { get; set; } = true;
    public string AssetCategory { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public byte[] RowVersion { get; set; } = [];

    public AssetGroup? ParentAssetGroup { get; set; }
    public Account? AssetAccount { get; set; }
    public Account? AccumulatedDepreciationAccount { get; set; }
    public Account? DepreciationExpenseAccount { get; set; }
    public Account? DisposalAccount { get; set; }
}
