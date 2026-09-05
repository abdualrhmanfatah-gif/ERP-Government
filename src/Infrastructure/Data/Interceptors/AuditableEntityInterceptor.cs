using ERP_Government.Application.Common.Interfaces;
using ERP_Government.Domain.Common;
using ERP_Government.Domain.Security.Entities;
using ERP_Government.Domain.Security.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace ERP_Government.Infrastructure.Data.Interceptors;

public class AuditableEntityInterceptor : SaveChangesInterceptor
{
    private readonly IUser _user;
    private readonly TimeProvider _dateTime;
    private readonly IRequestContext _requestContext;

    public AuditableEntityInterceptor(
        IUser user,
        TimeProvider dateTime,
        IRequestContext requestContext)
    {
        _user = user;
        _dateTime = dateTime;
        _requestContext = requestContext;
    }

    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        UpdateEntities(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        UpdateEntities(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    public void UpdateEntities(DbContext? context)
    {
        if (context is null) return;

        var utcNow = _dateTime.GetUtcNow();

        // Collect entries first to avoid "collection was modified" during enumeration
        var entries = context.ChangeTracker.Entries<BaseAuditableEntity>().ToList();

        foreach (var entry in entries)
        {
            // Skip immutable entities — handled by ImmutableEntityConstraint
            if (!ShouldAudit(entry)) continue;

            // Stamp audit fields (existing behavior)
            if (entry.State is EntityState.Added or EntityState.Modified || entry.HasChangedOwnedEntities())
            {
                if (entry.State == EntityState.Added)
                {
                    entry.Entity.CreatedBy = GetUserId();
                    entry.Entity.Created = utcNow;
                }
                entry.Entity.LastModifiedBy = GetUserId();
                entry.Entity.LastModified = utcNow;
            }

            // Capture change-tracking audit record
            CaptureAuditRecord(context, entry, utcNow);
        }
    }

    private void CaptureAuditRecord(DbContext context, EntityEntry<BaseAuditableEntity> entry, DateTimeOffset utcNow)
    {
        switch (entry.State)
        {
            case EntityState.Added:
                CaptureInsert(context, entry, utcNow);
                break;
            case EntityState.Modified:
                CaptureUpdate(context, entry, utcNow);
                break;
            case EntityState.Deleted:
                CaptureDelete(context, entry, utcNow);
                break;
        }
    }

    private void CaptureInsert(DbContext context, EntityEntry<BaseAuditableEntity> entry, DateTimeOffset utcNow)
    {
        var newValues = AuditTrailChangeTracker.CaptureValues(entry);
        var userId = GetUserIdAsInt();

        // Skip audit trail when no valid user context (e.g. during seeding)
        if (userId is null) return;

        context.Set<AuditTrail>().Add(new AuditTrail
        {
            EventCategory = "EntityChange",
            DocumentType = entry.Entity.GetType().Name,
            DocumentId = GetEntityId(entry),
            Action = AuditAction.Create,
            UserId = userId,
            Success = true,
            Timestamp = utcNow,
            NewValues = newValues,
            IpAddress = _requestContext.IpAddress
        });
    }

    private void CaptureUpdate(DbContext context, EntityEntry<BaseAuditableEntity> entry, DateTimeOffset utcNow)
    {
        var changes = AuditTrailChangeTracker.CaptureChanges(entry);

        // No-op optimization: skip if no actual property changes
        if (changes is null) return;

        var userId = GetUserIdAsInt();
        if (userId is null) return;

        var (oldValues, newValues, fieldChanges, changeSummary) = changes.Value;

        context.Set<AuditTrail>().Add(new AuditTrail
        {
            EventCategory = "EntityChange",
            DocumentType = entry.Entity.GetType().Name,
            DocumentId = GetEntityId(entry),
            Action = AuditAction.Update,
            UserId = userId,
            Success = true,
            Timestamp = utcNow,
            OldValues = oldValues,
            NewValues = newValues,
            FieldChanges = fieldChanges,
            ChangeSummary = changeSummary,
            IpAddress = _requestContext.IpAddress
        });
    }

    private void CaptureDelete(DbContext context, EntityEntry<BaseAuditableEntity> entry, DateTimeOffset utcNow)
    {
        var oldValues = AuditTrailChangeTracker.CaptureValues(entry);
        var userId = GetUserIdAsInt();

        if (userId is null) return;

        context.Set<AuditTrail>().Add(new AuditTrail
        {
            EventCategory = "EntityChange",
            DocumentType = entry.Entity.GetType().Name,
            DocumentId = GetEntityId(entry),
            Action = AuditAction.Delete,
            UserId = userId,
            Success = true,
            Timestamp = utcNow,
            OldValues = oldValues,
            IpAddress = _requestContext.IpAddress
        });
    }

    private static bool ShouldAudit(EntityEntry entry)
    {
        // Skip IImmutableEntity implementations (AuditTrail, SecurityAuditLog)
        // These are INSERT-only, handled by ImmutableEntityConstraint for protection
        if (typeof(IImmutableEntity).IsAssignableFrom(entry.Entity.GetType())) return false;

        return true;
    }

    private static int? GetEntityId(EntityEntry entry)
    {
        var pkProperty = entry.Properties.FirstOrDefault(p => p.Metadata.IsPrimaryKey());
        return pkProperty?.CurrentValue as int?;
    }

    private string GetUserId() => _user.Id?.ToString() ?? string.Empty;

    private int? GetUserIdAsInt() => _user.Id;
}

public static class Extensions
{
    public static bool HasChangedOwnedEntities(this EntityEntry entry) =>
        entry.References.Any(r =>
            r.TargetEntry != null &&
            r.TargetEntry.Metadata.IsOwned() &&
            (r.TargetEntry.State == EntityState.Added || r.TargetEntry.State == EntityState.Modified));
}
