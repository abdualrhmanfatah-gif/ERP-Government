using ERP_Government.Application.Common.Security;

namespace ERP_Government.Application.FinancialSettings.Commands.Currencies;

// C-F002 — UpdateCurrencyCommand
[Authorize(Policy = PermissionCodes.CurrenciesUpdate)]
public class UpdateCurrencyCommand : IRequest<Result>
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Symbol { get; init; } = string.Empty;
    public int DecimalPlaces { get; init; }
    public decimal RoundingPrecision { get; init; }
    public byte[] RowVersion { get; init; } = [];
}

public class UpdateCurrencyCommandHandler(
    IApplicationDbContext context) : IRequestHandler<UpdateCurrencyCommand, Result>
{
    public async Task<Result> Handle(
        UpdateCurrencyCommand request,
        CancellationToken cancellationToken)
    {
        var entity = await context.Currencies
            .FindAsync(request.Id, cancellationToken);

        if (entity is null)
            return Result.Failure(["Currency not found."]);

        if (!entity.IsActive)
            return Result.Failure(["Cannot update inactive currency."]);

        if (!entity.RowVersion.SequenceEqual(request.RowVersion))
            return Result.Failure(["Currency has been modified by another user. Please reload and try again."]);

        entity.Name = request.Name;
        entity.Symbol = request.Symbol;
        entity.DecimalPlaces = request.DecimalPlaces;
        entity.RoundingPrecision = request.RoundingPrecision;

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

public class UpdateCurrencyCommandValidator : AbstractValidator<UpdateCurrencyCommand>
{
    public UpdateCurrencyCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Invalid currency ID.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(100).WithMessage("Name must not exceed 100 characters.");

        RuleFor(x => x.Symbol)
            .NotEmpty().WithMessage("Symbol is required.")
            .MaximumLength(10).WithMessage("Symbol must not exceed 10 characters.");

        RuleFor(x => x.DecimalPlaces)
            .InclusiveBetween(0, 6).WithMessage("Decimal places must be between 0 and 6.");

        RuleFor(x => x.RoundingPrecision)
            .GreaterThan(0).WithMessage("Rounding precision must be greater than 0.");
    }
}
