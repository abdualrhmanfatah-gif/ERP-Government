using ERP_Government.Domain.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace ERP_Government.Infrastructure.Data.Interceptors;

/// <summary>
/// Prevents UPDATE and DELETE operations on entities implementing IImmutableEntity.
/// Throws InvalidOperationException before SaveChanges reaches the database.
/// </summary>
public class ImmutableEntityConstraint : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        ValidateImmutability(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        ValidateImmutability(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private static void ValidateImmutability(DbContext? context)
    {
        if (context is null) return;

        foreach (var entry in context.ChangeTracker.Entries())
        {
            if (entry.State is not (EntityState.Modified or EntityState.Deleted)) continue;

            if (!typeof(IImmutableEntity).IsAssignableFrom(entry.Entity.GetType())) continue;

            var entityType = entry.Entity.GetType().Name;
            throw new InvalidOperationException(
                $"Entity of type '{entityType}' implements IImmutableEntity and cannot be updated or deleted.");
        }
    }
}
