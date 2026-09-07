using ERP_Government.Domain.Revenue.Entities;

namespace ERP_Government.Application.Revenue.Common;

public static class CheckDateValidator
{
    public static (bool IsValid, string? Error) ValidateClearedAt(DateOnly checkDate, DateTimeOffset clearedAt)
    {
        if (clearedAt.Date < checkDate.ToDateTime(TimeOnly.MinValue))
            return (false, "Clearing date cannot be before the check date.");

        if (clearedAt > DateTimeOffset.UtcNow)
            return (false, "Clearing date cannot be in the future.");

        return (true, null);
    }

    public static (bool IsValid, string? Error) ValidateBouncedAt(DateOnly checkDate, DateTimeOffset bouncedAt)
    {
        if (bouncedAt.Date < checkDate.ToDateTime(TimeOnly.MinValue))
            return (false, "Bounce date cannot be before the check date.");

        if (bouncedAt > DateTimeOffset.UtcNow)
            return (false, "Bounce date cannot be in the future.");

        return (true, null);
    }
}
