using ERP_Government.Application.Common.Security;
using ERP_Government.Domain.Accounting.Entities;
using ERP_Government.Domain.Accounting.Enums;

namespace ERP_Government.Application.Accounting.Commands.Accounts.CreateAccount;

[Authorize(Policy = PermissionCodes.ChartOfAccountsCreate)]
public class CreateAccountCommand : IRequest<Result>
{
    public string Code { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public int AccountGroupId { get; init; }
    public int? ParentId { get; init; }
    public NormalBalanceType NormalBalance { get; init; }
    public bool IsPostable { get; init; } = true;
    public bool IsReconcilable { get; init; }
    public int? CurrencyId { get; init; }
}

public class CreateAccountCommandHandler(
    IApplicationDbContext context) : IRequestHandler<CreateAccountCommand, Result>
{
    public async Task<Result> Handle(
        CreateAccountCommand request,
        CancellationToken cancellationToken)
    {
        var exists = await context.Accounts
            .AnyAsync(x => x.Code == request.Code, cancellationToken);

        if (exists)
            return Result.Failure(["Account code already exists."]);

        var accountGroup = await context.AccountGroups
            .FindAsync(request.AccountGroupId, cancellationToken);

        if (accountGroup is null)
            return Result.Failure(["Account group not found."]);

        if (request.ParentId.HasValue)
        {
            var parent = await context.Accounts
                .FindAsync(request.ParentId.Value, cancellationToken);

            if (parent is null)
                return Result.Failure(["Parent account not found."]);
        }

        var entity = new Account
        {
            Code = request.Code,
            Name = request.Name,
            Description = request.Description,
            AccountGroupId = request.AccountGroupId,
            ParentId = request.ParentId,
            Level = request.ParentId.HasValue ? (byte)1 : (byte)0,
            NormalBalance = request.NormalBalance,
            IsPostable = request.IsPostable,
            IsReconcilable = request.IsReconcilable,
            CurrencyId = request.CurrencyId,
            IsActive = true
        };

        context.Accounts.Add(entity);
        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

public class CreateAccountCommandValidator : AbstractValidator<CreateAccountCommand>
{
    public CreateAccountCommandValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Code is required.")
            .MaximumLength(20).WithMessage("Code must not exceed 20 characters.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(200).WithMessage("Name must not exceed 200 characters.");

        RuleFor(x => x.AccountGroupId)
            .GreaterThan(0).WithMessage("Account group is required.");

        RuleFor(x => x.NormalBalance)
            .IsInEnum().WithMessage("Invalid normal balance type.");
    }
}
