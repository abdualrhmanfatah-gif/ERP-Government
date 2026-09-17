using ERP_Government.Domain.Security.Entities;
using ERP_Government.Application.Common.Security;

namespace ERP_Government.Application.Documents.Queries.GetDocumentApprovals;

[Authorize]
public record GetDocumentApprovalsQuery(
    string DocumentType,
    int DocumentId) : IRequest<IReadOnlyList<ApprovalHistory>>;

public class GetDocumentApprovalsQueryHandler(
    IApplicationDbContext context) : IRequestHandler<GetDocumentApprovalsQuery, IReadOnlyList<ApprovalHistory>>
{
    public async Task<IReadOnlyList<ApprovalHistory>> Handle(
        GetDocumentApprovalsQuery request,
        CancellationToken cancellationToken)
    {
        return await context.ApprovalHistory
            .Where(a => a.DocumentType == request.DocumentType && a.DocumentId == request.DocumentId)
            .OrderByDescending(a => a.DecisionAt)
            .ToListAsync(cancellationToken);
    }
}
