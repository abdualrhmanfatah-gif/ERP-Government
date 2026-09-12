namespace ERP_Government.Domain.Accounting.Enums;

/// <summary>
/// Journal entry types. Standard, Reversing, Adjusting, Closing, Opening.
/// </summary>
public enum MoveEntryType
{
    Standard = 0,
    Reversing = 1,
    Adjusting = 2,
    Closing = 3,
    Opening = 4,
    SystemGenerated = 5,
    Accrual = 6
}
