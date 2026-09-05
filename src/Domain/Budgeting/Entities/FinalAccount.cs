using ERP_Government.Domain.Common;

namespace ERP_Government.Domain.Budgeting.Entities;

public class FinalAccount : BaseAuditableEntity
{
    public int FiscalYearId { get; set; }
    public DateTimeOffset GeneratedAt { get; set; }
    public int GeneratedById { get; set; }
    public Enums.FinalAccountStatus Status { get; set; } = Enums.FinalAccountStatus.Draft;
    public DateTimeOffset? IssuedAt { get; set; }
    public int? IssuedById { get; set; }
    public byte[] RowVersion { get; set; } = [];

    public FinancialSettings.Entities.FiscalYear FiscalYear { get; set; } = null!;

    private readonly List<FinalAccountLine> _lines = [];
    public IReadOnlyCollection<FinalAccountLine> Lines => _lines.AsReadOnly();

    public void AddLine(FinalAccountLine line)
    {
        _lines.Add(line);
    }
}
