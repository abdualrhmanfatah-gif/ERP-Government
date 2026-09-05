using ERP_Government.Domain.Common;
using ERP_Government.Domain.Events.Common;

namespace ERP_Government.Domain.Events.FinancialSettings;

public class FiscalYearClosed : BaseEvent, IHasSourceEntity
{
    public int SourceEntityId { get; init; }
    public string SourceEntityType => "FiscalYear";
    public DateTimeOffset OccurredAt { get; init; }
    public int FiscalYearId { get; init; }
    public int YearNumber { get; init; }
}
