namespace ERP_Government.Domain.Common;

/// <summary>
/// Marker interface for entities that are INSERT-ONLY.
/// No UPDATE or DELETE operations should be performed on these entities.
/// </summary>
public interface IImmutableEntity
{
}
