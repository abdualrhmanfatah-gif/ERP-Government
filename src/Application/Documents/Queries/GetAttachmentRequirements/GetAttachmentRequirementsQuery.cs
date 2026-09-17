using ERP_Government.Domain.Security.Entities;
using ERP_Government.Application.Common.Security;

namespace ERP_Government.Application.Documents.Queries.GetAttachmentRequirements;

[Authorize]
public record GetAttachmentRequirementsQuery(
    string DocumentType) : IRequest<IReadOnlyList<DocumentAttachmentRequirement>>;

public class GetAttachmentRequirementsQueryHandler(
    IApplicationDbContext context) : IRequestHandler<GetAttachmentRequirementsQuery, IReadOnlyList<DocumentAttachmentRequirement>>
{
    public async Task<IReadOnlyList<DocumentAttachmentRequirement>> Handle(
        GetAttachmentRequirementsQuery request,
        CancellationToken cancellationToken)
    {
        return await context.DocumentAttachmentRequirements
            .Where(r => r.DocumentType == request.DocumentType && r.IsActive)
            .OrderBy(r => r.AttachmentTypeCode)
            .ToListAsync(cancellationToken);
    }
}
