using ERP_Government.Domain.Common;
using ERP_Government.Domain.FinancialSettings.Enums;

namespace ERP_Government.Domain.FinancialSettings.Entities;

public class DocumentSequence : BaseAuditableEntity
{
    public string Name { get; set; } = string.Empty;
    public string DocumentType { get; set; } = string.Empty;
    public int? FiscalYearId { get; set; }
    public FiscalYear? FiscalYear { get; set; }
    public int CurrentNumber { get; set; } = 1;
    public ResetPolicy ResetPolicy { get; set; } = ResetPolicy.Yearly;
    public bool IsActive { get; set; } = true;
    public byte[] RowVersion { get; set; } = [];
}
