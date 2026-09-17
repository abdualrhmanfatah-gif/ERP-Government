namespace ERP_Government.Domain.Assets.Enums;

public enum AssetTransactionStatus
{
    Draft = 0,
    Approved = 1,
    Executed = 2,
    Posting = 3,
    Posted = 4,
    PostingFailed = 5,
    Reversed = 6,
    Cancelled = 7
}
