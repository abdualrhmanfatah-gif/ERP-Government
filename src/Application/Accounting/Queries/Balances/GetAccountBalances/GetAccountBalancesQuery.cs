using ERP_Government.Application.Accounting.Common;
using ERP_Government.Application.Common.Security;
using ERP_Government.Domain.Accounting.Entities;

namespace ERP_Government.Application.Accounting.Queries.Balances.GetAccountBalances;

[Authorize(Policy = PermissionCodes.BalancesRead)]
public class GetAccountBalancesQuery : IRequest<List<AccountBalanceDto>>
{
    public int FiscalYearId { get; init; }
    public int FiscalPeriodId { get; init; }
    public int? AccountId { get; init; }
    public int? CurrencyId { get; init; }
}

public class GetAccountBalancesQueryHandler(
    IApplicationDbContext context,
    IMapper mapper) : IRequestHandler<GetAccountBalancesQuery, List<AccountBalanceDto>>
{
    public async Task<List<AccountBalanceDto>> Handle(
        GetAccountBalancesQuery request,
        CancellationToken cancellationToken)
    {
        IQueryable<AccountBalance> query = context.AccountBalances
            .Include(x => x.Account)
            .Include(x => x.FiscalYear)
            .Include(x => x.FiscalPeriod)
            .Include(x => x.Currency)
            .Where(x => x.FiscalYearId == request.FiscalYearId
                     && x.FiscalPeriodId == request.FiscalPeriodId);

        if (request.AccountId.HasValue)
            query = query.Where(x => x.AccountId == request.AccountId.Value);

        if (request.CurrencyId.HasValue)
            query = query.Where(x => x.CurrencyId == request.CurrencyId.Value);

        var items = await query
            .OrderBy(x => x.Account.Code)
            .ToListAsync(cancellationToken);

        return mapper.Map<List<AccountBalanceDto>>(items);
    }
}

public class GetAccountBalancesQueryValidator : AbstractValidator<GetAccountBalancesQuery>
{
    public GetAccountBalancesQueryValidator()
    {
        RuleFor(x => x.FiscalYearId)
            .GreaterThan(0).WithMessage("Invalid fiscal year ID.");

        RuleFor(x => x.FiscalPeriodId)
            .GreaterThan(0).WithMessage("Invalid fiscal period ID.");
    }
}
