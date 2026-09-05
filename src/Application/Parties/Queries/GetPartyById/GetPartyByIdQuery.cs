using ERP_Government.Domain.Parties.Entities;

namespace ERP_Government.Application.Parties.Queries.GetPartyById;

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
