using ERP_Government.Application.Common.Security;

namespace ERP_Government.Application.Payments.Commands.BankAccounts.UpdateBankAccount;

[Authorize(Policy = PermissionCodes.BankAccountsUpdate)]
public class UpdateBankAccountCommand : IRequest<Result>
{
    public int Id { get; init; }
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
    public byte[] RowVersion { get; init; } = [];
}

public class UpdateBankAccountCommandHandler(
    IApplicationDbContext context) : IRequestHandler<UpdateBankAccountCommand, Result>
{
    public async Task<Result> Handle(
        UpdateBankAccountCommand request,
        CancellationToken cancellationToken)
    {
        var entity = await context.BankAccounts
            .FindAsync(request.Id, cancellationToken);

        if (entity is null)
            return Result.Failure(["Bank account not found."]);

        if (!entity.IsActive)
            return Result.Failure(["Cannot update inactive bank account."]);

        // Validate IBAN uniqueness (if changed)
        if (!string.IsNullOrEmpty(request.Iban) && request.Iban != entity.Iban)
        {
            if (await context.BankAccounts.AnyAsync(x => x.Iban == request.Iban, cancellationToken))
                return Result.Failure(["IBAN already exists."]);
        }

        entity.Name = request.Name;
        entity.BankName = request.BankName;
        entity.AccountNumber = request.AccountNumber;
        entity.Iban = request.Iban;
        entity.SwiftCode = request.SwiftCode;
        entity.BranchName = request.BranchName;
        entity.BranchCode = request.BranchCode;
        entity.CurrencyId = request.CurrencyId;
        entity.FundId = request.FundId;
        entity.GlAccountId = request.GlAccountId;
        entity.IsDefault = request.IsDefault;
        entity.MaxDailyLimit = request.MaxDailyLimit;
        entity.MaxTransactionLimit = request.MaxTransactionLimit;
        entity.RequiresDualApproval = request.RequiresDualApproval;

        if (request.IsDefault && !entity.IsDefault)
        {
            var otherDefaults = await context.BankAccounts
                .Where(b => b.IsDefault && b.Id != entity.Id)
                .ToListAsync(cancellationToken);
            foreach (var other in otherDefaults)
                other.IsDefault = false;
        }
        entity.IsDefault = request.IsDefault;

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

public class UpdateBankAccountCommandValidator : AbstractValidator<UpdateBankAccountCommand>
{
    public UpdateBankAccountCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Invalid bank account ID.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.");

        RuleFor(x => x.BankName)
            .NotEmpty().WithMessage("Bank name is required.");

        RuleFor(x => x.AccountNumber)
            .NotEmpty().WithMessage("Account number is required.");

        RuleFor(x => x.CurrencyId)
            .GreaterThan(0).WithMessage("Currency is required.");
    }
}
