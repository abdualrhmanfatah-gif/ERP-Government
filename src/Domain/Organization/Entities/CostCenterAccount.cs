namespace ERP_Government.Domain.Organization.Entities;

public class CostCenterAccount
{
    public int CostCenterId { get; set; }
    public int AccountId { get; set; }

    public CostCenter CostCenter { get; set; } = null!;
}
