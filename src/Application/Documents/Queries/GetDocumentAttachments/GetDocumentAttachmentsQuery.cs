using ERP_Government.Domain.Security.Entities;

namespace ERP_Government.Application.Documents.Queries.GetDocumentAttachments;

public record GetDocumentAttachmentsQuery(
    string DocumentType,
    int DocumentId) : IRequest<IReadOnlyList<Attachment>>;

public class GetDocumentAttachmentsQueryHandler(
    IApplicationDbContext context) : IRequestHandler<GetDocumentAttachmentsQuery, IReadOnlyList<Attachment>>
{
    public async Task<IReadOnlyList<Attachment>> Handle(
        GetDocumentAttachmentsQuery request,
        CancellationToken cancellationToken)
    {
        return await context.Attachments
            .Where(a => a.DocumentType == request.DocumentType && a.DocumentId == request.DocumentId)
            .OrderBy(a => a.FileName)
            .ToListAsync(cancellationToken);
    }
}
