namespace ERP_Government.Domain.Common;

public enum OutboxMessageStatus
{
    Pending = 0,
    Processing = 1,
    Processed = 2,
    Failed = 3
}
