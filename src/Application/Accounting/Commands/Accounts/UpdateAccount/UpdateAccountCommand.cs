using ERP_Government.Application.Common.Security;
using ERP_Government.Domain.Accounting.Enums;

namespace ERP_Government.Application.Accounting.Commands.Accounts.UpdateAccount;

[Authorize(Policy = PermissionCodes.ChartOfAccountsEdit)]
public class UpdateAccountCommand : IRequest<Result>
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public int AccountGroupId { get; init; }
    public NormalBalanceType NormalBalance { get; init; }
    public bool IsPostable { get; init; }
    public bool IsReconcilable { get; init; }
    public byte[] RowVersion { get; init; } = [];
}

public class UpdateAccountCommandHandler(
    IApplicationDbContext context) : IRequestHandler<UpdateAccountCommand, Result>
{
    public async Task<Result> Handle(
        UpdateAccountCommand request,
        CancellationToken cancellationToken)
    {
        var entity = await context.Accounts
            .FindAsync(request.Id, cancellationToken);

        if (entity is null)
            return Result.Failure(["Account not found."]);

        var accountGroup = await context.AccountGroups
            .FindAsync(request.AccountGroupId, cancellationToken);

        if (accountGroup is null)
            return Result.Failure(["Account group not found."]);

        entity.Name = request.Name;
        entity.Description = request.Description;
        entity.AccountGroupId = request.AccountGroupId;
        entity.NormalBalance = request.NormalBalance;
        entity.IsPostable = request.IsPostable;
        entity.IsReconcilable = request.IsReconcilable;

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

public class UpdateAccountCommandValidator : AbstractValidator<UpdateAccountCommand>
{
    public UpdateAccountCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Invalid account ID.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(200).WithMessage("Name must not exceed 200 characters.");

        RuleFor(x => x.AccountGroupId)
            .GreaterThan(0).WithMessage("Account group is required.");

        RuleFor(x => x.NormalBalance)
            .IsInEnum().WithMessage("Invalid normal balance type.");

        RuleFor(x => x.RowVersion)
            .NotEmpty().WithMessage("RowVersion is required for concurrency control.");
    }
}
