using ERP_Government.Application.Common.Security;
using ERP_Government.Application.FinancialSettings.Common.DTOs;
using ERP_Government.Application.FinancialSettings.Common.Models;

namespace ERP_Government.Application.FinancialSettings.Queries.Currencies;

// Q-F003 — GetIso4217CodesQuery
[Authorize(Policy = PermissionCodes.CurrenciesView)]
public class GetIso4217CodesQuery : IRequest<List<Iso4217CodeDto>>
{
    public string? Query { get; init; }
}

public class GetIso4217CodesQueryHandler
    : IRequestHandler<GetIso4217CodesQuery, List<Iso4217CodeDto>>
{
    public Task<List<Iso4217CodeDto>> Handle(
        GetIso4217CodesQuery request,
        CancellationToken cancellationToken)
    {
        var codes = Iso4217Codes.GetAll(request.Query);

        var result = codes.Select(c => new Iso4217CodeDto
        {
            Code = c.Code,
            Name = c.Name,
            DecimalPlaces = c.DecimalPlaces
        }).ToList();

        return Task.FromResult(result);
    }
}
