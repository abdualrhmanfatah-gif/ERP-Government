using ERP_Government.Domain.Security.Entities;

namespace ERP_Government.Application.DocumentAttachmentRequirements.Queries.GetDocumentAttachmentRequirementById;

public record GetDocumentAttachmentRequirementByIdQuery(int Id)
    : IRequest<DocumentAttachmentRequirement?>;

public class GetDocumentAttachmentRequirementByIdQueryHandler(
    IApplicationDbContext context) : IRequestHandler<GetDocumentAttachmentRequirementByIdQuery, DocumentAttachmentRequirement?>
{
    public async Task<DocumentAttachmentRequirement?> Handle(
        GetDocumentAttachmentRequirementByIdQuery request,
        CancellationToken cancellationToken)
    {
        return await context.DocumentAttachmentRequirements.FindAsync(request.Id, cancellationToken);
    }
}
