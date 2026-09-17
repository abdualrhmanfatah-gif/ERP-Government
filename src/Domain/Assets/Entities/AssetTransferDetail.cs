using ERP_Government.Domain.Common;
using ERP_Government.Domain.Inventory.Entities;
using ERP_Government.Domain.Organization.Entities;

namespace ERP_Government.Domain.Assets.Entities;

public class AssetTransferDetail : BaseEntity
{
    public int AssetTransactionId { get; set; }
    public DateTimeOffset OccurredAt { get; set; }
    public int? FromLocationId { get; set; }
    public int? ToLocationId { get; set; }
    public int? FromEmployeeId { get; set; }
    public int? ToEmployeeId { get; set; }
    public int? FromDepartmentId { get; set; }
    public int? ToDepartmentId { get; set; }

    public AssetTransaction? AssetTransaction { get; set; }
    public Location? FromLocation { get; set; }
    public Location? ToLocation { get; set; }
    public Employee? FromEmployee { get; set; }
    public Employee? ToEmployee { get; set; }
    public OrganizationalUnit? FromDepartment { get; set; }
    public OrganizationalUnit? ToDepartment { get; set; }
}
