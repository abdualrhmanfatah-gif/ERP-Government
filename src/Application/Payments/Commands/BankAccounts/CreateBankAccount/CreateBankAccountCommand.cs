using ERP_Government.Application.Common.Security;
using ERP_Government.Domain.Payments.Entities;

namespace ERP_Government.Application.Payments.Commands.BankAccounts.CreateBankAccount;

[Authorize(Policy = PermissionCodes.BankAccountsCreate)]
public class CreateBankAccountCommand : IRequest<Result>
{
    public string Name { get; init; } = string.Empty;
    public string BankName { get; init; } = string.Empty;
    public string AccountNumber { get; init; } = string.Empty;
    public string? Iban { get; init; }
    public string? SwiftCode { get; init; }
    public string? BranchName { get; init; }
    public string? BranchCode { get; init; }
    public int CurrencyId { get; init; }
    public int? FundId { get; init; }
    public int? GlAccountId { get; init; }
    public bool IsDefault { get; init; }
    public decimal? MaxDailyLimit { get; init; }
    public decimal? MaxTransactionLimit { get; init; }
    public bool RequiresDualApproval { get; init; }
    public decimal? OpeningBalance { get; init; }
}

public class CreateBankAccountCommandHandler(
    IApplicationDbContext context) : IRequestHandler<CreateBankAccountCommand, Result>
{
    public async Task<Result> Handle(
        CreateBankAccountCommand request,
        CancellationToken cancellationToken)
    {
        // Validate IBAN uniqueness
        if (!string.IsNullOrEmpty(request.Iban) &&
            await context.BankAccounts.AnyAsync(x => x.Iban == request.Iban, cancellationToken))
            return Result.Failure(["IBAN already exists."]);

        // Validate Currency exists and is active
        var currency = await context.Currencies.FindAsync(request.CurrencyId, cancellationToken);
        if (currency is null || !currency.IsActive)
            return Result.Failure(["Invalid or inactive currency."]);

        // Validate Fund exists (if provided)
        if (request.FundId.HasValue)
        {
            var fund = await context.Funds.FindAsync(request.FundId.Value, cancellationToken);
            if (fund is null || !fund.IsActive)
                return Result.Failure(["Invalid or inactive fund."]);
        }

        // Validate limits
        if (request.MaxDailyLimit.HasValue && request.MaxDailyLimit < 0)
            return Result.Failure(["Daily limit must be non-negative."]);

        if (request.MaxTransactionLimit.HasValue && request.MaxTransactionLimit < 0)
            return Result.Failure(["Transaction limit must be non-negative."]);

        var entity = new BankAccount
        {
            Name = request.Name,
            BankName = request.BankName,
            AccountNumber = request.AccountNumber,
            Iban = request.Iban,
            SwiftCode = request.SwiftCode,
            BranchName = request.BranchName,
            BranchCode = request.BranchCode,
            CurrencyId = request.CurrencyId,
            FundId = request.FundId,
            GlAccountId = request.GlAccountId,
            IsDefault = request.IsDefault,
            MaxDailyLimit = request.MaxDailyLimit,
            MaxTransactionLimit = request.MaxTransactionLimit,
            RequiresDualApproval = request.RequiresDualApproval,
            OpeningBalance = request.OpeningBalance,
            CurrentBalance = request.OpeningBalance,
            IsActive = true
        };

        context.BankAccounts.Add(entity);

        if (request.IsDefault)
        {
            var otherDefaults = await context.BankAccounts
                .Where(b => b.IsDefault && b.Id != entity.Id)
                .ToListAsync(cancellationToken);
            foreach (var other in otherDefaults)
                other.IsDefault = false;
        }

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

public class CreateBankAccountCommandValidator : AbstractValidator<CreateBankAccountCommand>
{
    public CreateBankAccountCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(200).WithMessage("Name must not exceed 200 characters.");

        RuleFor(x => x.BankName)
            .NotEmpty().WithMessage("Bank name is required.")
            .MaximumLength(200).WithMessage("Bank name must not exceed 200 characters.");

        RuleFor(x => x.AccountNumber)
            .NotEmpty().WithMessage("Account number is required.")
            .MaximumLength(100).WithMessage("Account number must not exceed 100 characters.");

        RuleFor(x => x.CurrencyId)
            .GreaterThan(0).WithMessage("Currency is required.");
    }
}
