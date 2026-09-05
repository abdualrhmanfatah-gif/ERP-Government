using ERP_Government.Domain.Common;

namespace ERP_Government.Domain.Inventory.Entities;

public class ItemCategory : BaseAuditableEntity
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? NameEn { get; set; }
    public string? Description { get; set; }
    public int? ParentItemCategoryId { get; set; }
    public int? Level { get; set; }
    public string? Breadcrumb { get; set; }
    public int? ExpenseAccountId { get; set; }
    public int? InventoryAccountId { get; set; }
    public int? TaxAccountId { get; set; }
    public string? TaxClass { get; set; }
    public bool IsActive { get; set; } = true;
    public byte[] RowVersion { get; set; } = [];

    public ItemCategory? ParentItemCategory { get; set; }
}
