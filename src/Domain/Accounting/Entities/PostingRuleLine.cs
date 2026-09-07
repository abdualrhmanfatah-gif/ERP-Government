using ERP_Government.Domain.Common;
using ERP_Government.Domain.Accounting.Enums;

namespace ERP_Government.Domain.Accounting.Entities;

/// <summary>
/// Posting rule lines per schema table 42.
/// </summary>
public class PostingRuleLine : BaseAuditableEntity
{
    public int PostingRuleId { get; set; }
    public int Sequence { get; set; }
    public AccountSource AccountSource { get; set; }
    public int? FixedAccountId { get; set; }
    public DebitOrCredit DebitOrCredit { get; set; }
    public AmountSource AmountSource { get; set; }
    public bool FundDimensionRequired { get; set; }
    public bool CostCenterDimensionRequired { get; set; }
    public bool ProjectDimensionRequired { get; set; }
    public bool IsActive { get; set; } = true;
    public byte[] RowVersion { get; set; } = [];

    public PostingRule PostingRule { get; set; } = null!;
    public Account? FixedAccount { get; set; }
}
