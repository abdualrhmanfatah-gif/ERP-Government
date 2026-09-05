namespace ERP_Government.Domain.Accounting.Enums;

/// <summary>
/// Journal types per schema line 1181.
/// </summary>
public enum JournalType
{
    General = 0,
    Purchase = 1,
    Sale = 2,
    Cash = 3,
    Bank = 4,
    Adjustment = 5,
    Closing = 6
}
