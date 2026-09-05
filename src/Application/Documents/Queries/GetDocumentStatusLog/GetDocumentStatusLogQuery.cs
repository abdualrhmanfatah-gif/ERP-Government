using ERP_Government.Domain.Security.Entities;

namespace ERP_Government.Application.Documents.Queries.GetDocumentStatusLog;

public record GetDocumentStatusLogQuery(
    string EntityName,
    int DocumentId) : IRequest<IReadOnlyList<DocumentStatusLog>>;

public class GetDocumentStatusLogQueryHandler(
    IApplicationDbContext context) : IRequestHandler<GetDocumentStatusLogQuery, IReadOnlyList<DocumentStatusLog>>
{
    public async Task<IReadOnlyList<DocumentStatusLog>> Handle(
        GetDocumentStatusLogQuery request,
        CancellationToken cancellationToken)
    {
        return await context.DocumentStatusLogs
            .Where(l => l.EntityName == request.EntityName && l.DocumentId == request.DocumentId)
            .OrderBy(l => l.ChangedAt)
            .ToListAsync(cancellationToken);
    }
}
