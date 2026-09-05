using ERP_Government.Application.Common.Security;
using ERP_Government.Application.FinancialSettings.Common.Models;

namespace ERP_Government.Application.FinancialSettings.Commands.Currencies;

// C-F001 — CreateCurrencyCommand
[Authorize(Policy = PermissionCodes.CurrenciesCreate)]
public class CreateCurrencyCommand : IRequest<Result>
{
    public string Code { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string Symbol { get; init; } = string.Empty;
    public int DecimalPlaces { get; init; } = 2;
    public decimal RoundingPrecision { get; init; } = 0.01m;
    public bool IsBase { get; init; }
}

public class CreateCurrencyCommandHandler(
    IApplicationDbContext context) : IRequestHandler<CreateCurrencyCommand, Result>
{
    public async Task<Result> Handle(
        CreateCurrencyCommand request,
        CancellationToken cancellationToken)
    {
        var exists = await context.Currencies
            .AnyAsync(x => x.Code == request.Code, cancellationToken);

        if (exists)
            return Result.Failure(["Currency code already exists."]);

        // FR-009: If IsBase=true, unset existing base currency before saving
        if (request.IsBase)
        {
            var currentBase = await context.Currencies
                .FirstOrDefaultAsync(x => x.IsBase, cancellationToken);

            if (currentBase is not null)
                currentBase.IsBase = false;
        }

        var entity = new Currency
        {
            Code = request.Code,
            Name = request.Name,
            Symbol = request.Symbol,
            DecimalPlaces = request.DecimalPlaces,
            RoundingPrecision = request.RoundingPrecision,
            IsBase = request.IsBase,
            IsActive = true
        };

        context.Currencies.Add(entity);
        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

public class CreateCurrencyCommandValidator : AbstractValidator<CreateCurrencyCommand>
{
    public CreateCurrencyCommandValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Code is required.")
            .MaximumLength(10).WithMessage("Code must not exceed 10 characters.")
            .Must(code => Iso4217Codes.IsValid(code))
            .WithMessage("Code must be a valid ISO 4217 currency code.");

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
