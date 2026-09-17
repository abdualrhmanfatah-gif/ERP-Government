using ERP_Government.Domain.Parties.Entities;
using ERP_Government.Application.Common.Security;

namespace ERP_Government.Application.Parties.Queries.GetPartyById;

[Authorize(Policy = PermissionCodes.PartiesView)]
public record GetPartyByIdQuery(int Id) : IRequest<Party?>;

public class GetPartyByIdQueryHandler(
    IApplicationDbContext context) : IRequestHandler<GetPartyByIdQuery, Party?>
{
    public async Task<Party?> Handle(
        GetPartyByIdQuery request,
        CancellationToken cancellationToken)
    {
        return await context.Parties.FindAsync(request.Id, cancellationToken);
    }
}
