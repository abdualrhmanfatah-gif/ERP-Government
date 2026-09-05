using ERP_Government.Application.Common.Security;

namespace ERP_Government.Application.Payments.Commands.BankAccounts.DeactivateBankAccount;

[Authorize(Policy = PermissionCodes.BankAccountsDeactivate)]
public class DeactivateBankAccountCommand : IRequest<Result>
{
    public int Id { get; init; }
    public byte[] RowVersion { get; init; } = [];
}

public class DeactivateBankAccountCommandHandler(
    IApplicationDbContext context) : IRequestHandler<DeactivateBankAccountCommand, Result>
{
    public async Task<Result> Handle(
        DeactivateBankAccountCommand request,
        CancellationToken cancellationToken)
    {
        var entity = await context.BankAccounts
            .FindAsync(request.Id, cancellationToken);

        if (entity is null)
            return Result.Failure(["Bank account not found."]);

        entity.IsActive = false;

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

public class DeactivateBankAccountCommandValidator : AbstractValidator<DeactivateBankAccountCommand>
{
    public DeactivateBankAccountCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Invalid bank account ID.");
    }
}
