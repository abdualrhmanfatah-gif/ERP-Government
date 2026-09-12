using ERP_Government.Application.Common.Interfaces;
using ERP_Government.Application.Parties.Common;
using ERP_Government.Domain.Security.Entities;

namespace ERP_Government.Application.Security.Common;

public class DocumentStatusLogger : IDocumentStatusLogger
{
    private readonly IApplicationDbContext _context;

    public DocumentStatusLogger(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task LogAsync(
        string entityName,
        int documentId,
        string fromStatus,
        string toStatus,
        int changedById,
        string? reason,
        CancellationToken ct = default)
    {
        var log = new DocumentStatusLog
        {
            EntityName = entityName,
            DocumentId = documentId,
            FromStatus = fromStatus,
            ToStatus = toStatus,
            ChangedById = changedById,
            ChangedAt = DateTimeOffset.UtcNow,
            Reason = reason
        };

        _context.DocumentStatusLogs.Add(log);
    }
}
