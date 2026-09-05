using System.Text.Json;
using ERP_Government.Domain.Common;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace ERP_Government.Infrastructure.Data.Interceptors;

/// <summary>
/// Static helper for capturing property-level change tracking data from EF Core entity entries.
/// </summary>
public static class AuditTrailChangeTracker
{
    private const int MaxJsonPayloadSize = 16 * 1024; // 16KB
    private const string TruncatedMarker = "[truncated]";

    private static readonly HashSet<string> ExcludedProperties = new()
    {
        nameof(BaseAuditableEntity.Created),
        nameof(BaseAuditableEntity.CreatedBy),
        nameof(BaseAuditableEntity.LastModified),
        nameof(BaseAuditableEntity.LastModifiedBy)
    };

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
        WriteIndented = false
    };

    /// <summary>
    /// Captures all property values from an entity entry as a JSON string.
    /// Used for INSERT (NewValues) and DELETE (OldValues).
    /// Excludes navigation properties, RowVersion, and audit fields.
    /// </summary>
    public static string? CaptureValues(EntityEntry entry)
    {
        var properties = GetAuditableProperties(entry);
        return SerializeProperties(properties);
    }

    /// <summary>
    /// Compares OriginalValues vs CurrentValues for a Modified entry.
    /// Returns (oldValues, newValues, fieldChanges, changeSummary) for changed properties only.
    /// Returns nulls if no actual changes detected.
    /// </summary>
    public static (string? OldValues, string? NewValues, string? FieldChanges, string? ChangeSummary)?
        CaptureChanges(EntityEntry entry)
    {
        var oldDict = new Dictionary<string, object?>();
        var newDict = new Dictionary<string, object?>();
        var fieldChanges = new Dictionary<string, Dictionary<string, object?>>();

        foreach (var property in entry.Properties)
        {
            if (ShouldExcludeProperty(property)) continue;

            var oldVal = property.OriginalValue;
            var newVal = property.CurrentValue;

            if (Equals(oldVal, newVal)) continue;

            var propName = property.Metadata.Name;
            oldDict[propName] = oldVal;
            newDict[propName] = newVal;
            fieldChanges[propName] = new Dictionary<string, object?> { ["Old"] = oldVal, ["New"] = newVal };
        }

        if (oldDict.Count == 0) return null;

        var oldJson = SerializeDictionary(oldDict);
        var newJson = SerializeDictionary(newDict);
        var fieldChangesJson = JsonSerializer.Serialize(fieldChanges, JsonOptions);
        var changeSummary = BuildChangeSummary(fieldChanges);

        return (oldJson, newJson, fieldChangesJson, changeSummary);
    }

    /// <summary>
    /// Builds a human-readable change summary: "Field1: Old1 → New1, Field2: Old2 → New2"
    /// </summary>
    public static string BuildChangeSummary(Dictionary<string, Dictionary<string, object?>> fieldChanges)
    {
        return string.Join(", ", fieldChanges.Select(kvp =>
        {
            var oldVal = kvp.Value["Old"];
            var newVal = kvp.Value["New"];
            return $"{kvp.Key}: {oldVal} → {newVal}";
        }));
    }

    private static IEnumerable<PropertyEntry> GetAuditableProperties(EntityEntry entry)
    {
        return entry.Properties.Where(p => !ShouldExcludeProperty(p));
    }

    private static bool ShouldExcludeProperty(PropertyEntry property)
    {
        // Exclude RowVersion (concurrency token)
        if (property.Metadata.IsConcurrencyToken) return true;

        // Exclude audit fields managed by the interceptor itself
        if (ExcludedProperties.Contains(property.Metadata.Name)) return true;

        return false;
    }

    private static string? SerializeProperties(IEnumerable<PropertyEntry> properties)
    {
        var dict = new Dictionary<string, object?>();
        foreach (var prop in properties)
        {
            dict[prop.Metadata.Name] = prop.CurrentValue;
        }

        if (dict.Count == 0) return null;

        return SerializeDictionary(dict);
    }

    private static string SerializeDictionary(Dictionary<string, object?> dict)
    {
        var json = JsonSerializer.Serialize(dict, JsonOptions);

        if (json.Length > MaxJsonPayloadSize)
        {
            json = json[..MaxJsonPayloadSize] + TruncatedMarker;
        }

        return json;
    }
}
