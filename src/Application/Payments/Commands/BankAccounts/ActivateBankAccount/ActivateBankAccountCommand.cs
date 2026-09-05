using ERP_Government.Application.Common.Security;

namespace ERP_Government.Application.Payments.Commands.BankAccounts.ActivateBankAccount;

[Authorize(Policy = PermissionCodes.BankAccountsActivate)]
public class ActivateBankAccountCommand : IRequest<Result>
{
    public int Id { get; init; }
    public byte[] RowVersion { get; init; } = [];
}

public class ActivateBankAccountCommandHandler(
    IApplicationDbContext context) : IRequestHandler<ActivateBankAccountCommand, Result>
{
    public async Task<Result> Handle(
        ActivateBankAccountCommand request,
        CancellationToken cancellationToken)
    {
        var entity = await context.BankAccounts
            .FindAsync(request.Id, cancellationToken);

        if (entity is null)
            return Result.Failure(["Bank account not found."]);

        entity.IsActive = true;

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

public class ActivateBankAccountCommandValidator : AbstractValidator<ActivateBankAccountCommand>
{
    public ActivateBankAccountCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Invalid bank account ID.");
    }
}
