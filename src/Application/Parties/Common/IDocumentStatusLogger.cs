namespace ERP_Government.Application.Parties.Common;

public interface IDocumentStatusLogger
{
    Task LogAsync(
        string entityName,
        int documentId,
        string fromStatus,
        string toStatus,
        int changedById,
        string? reason,
        CancellationToken ct = default);
}
