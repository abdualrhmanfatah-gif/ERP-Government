using ERP_Government.Application.Common.Security;

namespace ERP_Government.Application.FinancialSettings.Commands.Currencies;

// C-F003 — ActivateCurrencyCommand
[Authorize(Policy = PermissionCodes.CurrenciesActivate)]
public class ActivateCurrencyCommand : IRequest<Result>
{
    public int Id { get; init; }
    public byte[] RowVersion { get; init; } = [];
}

public class ActivateCurrencyCommandHandler(
    IApplicationDbContext context) : IRequestHandler<ActivateCurrencyCommand, Result>
{
    public async Task<Result> Handle(
        ActivateCurrencyCommand request,
        CancellationToken cancellationToken)
    {
        var entity = await context.Currencies
            .FindAsync(request.Id, cancellationToken);

        if (entity is null)
            return Result.Failure(["Currency not found."]);

        if (entity.IsActive)
            return Result.Failure(["Currency is already active."]);

        if (!entity.RowVersion.SequenceEqual(request.RowVersion))
            return Result.Failure(["Currency has been modified by another user. Please reload and try again."]);

        entity.IsActive = true;

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

public class ActivateCurrencyCommandValidator : AbstractValidator<ActivateCurrencyCommand>
{
    public ActivateCurrencyCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Invalid currency ID.");
    }
}
