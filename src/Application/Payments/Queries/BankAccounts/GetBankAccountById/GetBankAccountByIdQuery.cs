using ERP_Government.Application.Common.Errors;
using ERP_Government.Application.Common.Models;
using ERP_Government.Application.Common.Security;
using ERP_Government.Application.Payments.Common.DTOs;

namespace ERP_Government.Application.Payments.Queries.BankAccounts.GetBankAccountById;

[Authorize(Policy = PermissionCodes.BankAccountsView)]
public class GetBankAccountByIdQuery : IRequest<Result<BankAccountDto>>
{
    public int Id { get; init; }
}

public class GetBankAccountByIdQueryHandler(
    IApplicationDbContext context) : IRequestHandler<GetBankAccountByIdQuery, Result<BankAccountDto>>
{
    public async Task<Result<BankAccountDto>> Handle(
        GetBankAccountByIdQuery request,
        CancellationToken cancellationToken)
    {
        var dto = await context.BankAccounts
            .Where(x => x.Id == request.Id)
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
            .FirstOrDefaultAsync(cancellationToken);

        if (dto is null)
            return Result<BankAccountDto>.Failure(ErrorCodes.Payments.BankAccountNotFound, ErrorCategory.NotFound, $"Bank account with ID {request.Id} not found.");

        return Result<BankAccountDto>.Success(dto);
    }
}
