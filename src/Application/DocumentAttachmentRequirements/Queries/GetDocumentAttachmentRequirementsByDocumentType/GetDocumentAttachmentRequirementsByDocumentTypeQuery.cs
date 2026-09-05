using ERP_Government.Domain.Security.Entities;

namespace ERP_Government.Application.DocumentAttachmentRequirements.Queries.GetDocumentAttachmentRequirementsByDocumentType;

public record GetDocumentAttachmentRequirementsByDocumentTypeQuery(string DocumentType)
    : IRequest<IReadOnlyList<DocumentAttachmentRequirement>>;

public class GetDocumentAttachmentRequirementsByDocumentTypeQueryHandler(
    IApplicationDbContext context) : IRequestHandler<GetDocumentAttachmentRequirementsByDocumentTypeQuery, IReadOnlyList<DocumentAttachmentRequirement>>
{
    public async Task<IReadOnlyList<DocumentAttachmentRequirement>> Handle(
        GetDocumentAttachmentRequirementsByDocumentTypeQuery request,
        CancellationToken cancellationToken)
    {
        return await context.DocumentAttachmentRequirements
            .Where(r => r.DocumentType == request.DocumentType && r.IsActive)
            .OrderBy(r => r.AttachmentTypeCode)
            .ToListAsync(cancellationToken);
    }
}
