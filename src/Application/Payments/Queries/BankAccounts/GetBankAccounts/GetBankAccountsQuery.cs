using ERP_Government.Application.Common.Security;
using ERP_Government.Application.Payments.Common.DTOs;

namespace ERP_Government.Application.Payments.Queries.BankAccounts.GetBankAccounts;

[Authorize(Policy = PermissionCodes.BankAccountsView)]
public class GetBankAccountsQuery : IRequest<List<BankAccountDto>>
{
    public bool? IsActive { get; init; }
    public int? CurrencyId { get; init; }
}

public class GetBankAccountsQueryHandler(
    IApplicationDbContext context) : IRequestHandler<GetBankAccountsQuery, List<BankAccountDto>>
{
    public async Task<List<BankAccountDto>> Handle(
        GetBankAccountsQuery request,
        CancellationToken cancellationToken)
    {
        var query = context.BankAccounts.AsQueryable();

        if (request.IsActive.HasValue)
            query = query.Where(x => x.IsActive == request.IsActive.Value);

        if (request.CurrencyId.HasValue)
            query = query.Where(x => x.CurrencyId == request.CurrencyId.Value);

        return await query
            .Select(x => new BankAccountDto
            {
                Id = x.Id,
                Name = x.Name,
                BankName = x.BankName,
                AccountNumber = x.AccountNumber,
                Iban = x.Iban,
                SwiftCode = x.SwiftCode,
                BranchName = x.BranchName,
                BranchCode = x.BranchCode,
                CurrencyId = x.CurrencyId,
                FundId = x.FundId,
                GlAccountId = x.GlAccountId,
                IsDefault = x.IsDefault,
                MaxDailyLimit = x.MaxDailyLimit,
                MaxTransactionLimit = x.MaxTransactionLimit,
                RequiresDualApproval = x.RequiresDualApproval,
                LastReconciliationDate = x.LastReconciliationDate,
                OpeningBalance = x.OpeningBalance,
                CurrentBalance = x.CurrentBalance,
                IsActive = x.IsActive
            })
            .ToListAsync(cancellationToken);
    }
}
