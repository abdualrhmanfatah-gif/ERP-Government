namespace ERP_Government.Application.Reporting.TrialBalance.GetLedgerMovement;

public record LedgerMovementDto
{
    public int AccountId { get; init; }
    public string AccountCode { get; init; } = string.Empty;
    public string AccountName { get; init; } = string.Empty;
    public List<LedgerMovementLineDto> Entries { get; init; } = [];
    public LedgerMovementTotalDto Totals { get; init; } = new();
}

public record LedgerMovementLineDto
{
    public int JournalEntryId { get; init; }
    public string EntryNumber { get; init; } = string.Empty;
    public DateOnly EntryDate { get; init; }
    public string? Description { get; init; }
    public decimal Debit { get; init; }
    public decimal Credit { get; init; }
    public decimal RunningBalance { get; init; }
}

public record LedgerMovementTotalDto
{
    public decimal TotalDebits { get; init; }
    public decimal TotalCredits { get; init; }
    public decimal FinalBalance { get; init; }
}
