using ERP_Government.Domain.Security.Entities;
using ERP_Government.Application.Common.Security;

namespace ERP_Government.Application.Documents.Queries.GetDocumentStatusLog;

[Authorize]
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
