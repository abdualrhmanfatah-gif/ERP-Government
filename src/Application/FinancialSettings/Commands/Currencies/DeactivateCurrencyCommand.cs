using ERP_Government.Application.Common.Security;

namespace ERP_Government.Application.FinancialSettings.Commands.Currencies;

// C-F004 — DeactivateCurrencyCommand
[Authorize(Policy = PermissionCodes.CurrenciesDeactivate)]
public class DeactivateCurrencyCommand : IRequest<Result>
{
    public int Id { get; init; }
    public byte[] RowVersion { get; init; } = [];
}

public class DeactivateCurrencyCommandHandler(
    IApplicationDbContext context) : IRequestHandler<DeactivateCurrencyCommand, Result>
{
    public async Task<Result> Handle(
        DeactivateCurrencyCommand request,
        CancellationToken cancellationToken)
    {
        var entity = await context.Currencies
            .FindAsync(request.Id, cancellationToken);

        if (entity is null)
            return Result.Failure(["Currency not found."]);

        if (!entity.IsActive)
            return Result.Failure(["Currency is already inactive."]);

        if (entity.IsBase)
            return Result.Failure(["Cannot deactivate the base currency."]);

        if (!entity.RowVersion.SequenceEqual(request.RowVersion))
            return Result.Failure(["Currency has been modified by another user. Please reload and try again."]);

        // FR-007: Block deactivation if currency has active exchange rates
        var hasActiveExchangeRates = await context.ExchangeRates
            .AnyAsync(x => (x.BaseCurrencyId == entity.Id || x.CurrencyId == entity.Id)
                           && x.IsActive, cancellationToken);

        if (hasActiveExchangeRates)
            return Result.Failure(["Cannot deactivate currency with active exchange rates. Deactivate exchange rates first."]);

        entity.IsActive = false;

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

public class DeactivateCurrencyCommandValidator : AbstractValidator<DeactivateCurrencyCommand>
{
    public DeactivateCurrencyCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Invalid currency ID.");
    }
}
