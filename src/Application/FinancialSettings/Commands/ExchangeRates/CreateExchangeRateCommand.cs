using ERP_Government.Application.Common.Security;
using ERP_Government.Domain.FinancialSettings.Enums;

namespace ERP_Government.Application.FinancialSettings.Commands.ExchangeRates;

// C-F004 — CreateExchangeRateCommand
[Authorize(Policy = PermissionCodes.ExchangeRatesCreate)]
public class CreateExchangeRateCommand : IRequest<Result>
{
    public int BaseCurrencyId { get; init; }
    public int CurrencyId { get; init; }
    public DateOnly RateDate { get; init; }
    public ExchangeRateType RateType { get; init; }
    public decimal Rate { get; init; }
}

public class CreateExchangeRateCommandHandler(
    IApplicationDbContext context) : IRequestHandler<CreateExchangeRateCommand, Result>
{
    public async Task<Result> Handle(
        CreateExchangeRateCommand request,
        CancellationToken cancellationToken)
    {
        // Validate BaseCurrencyId ≠ CurrencyId
        if (request.BaseCurrencyId == request.CurrencyId)
            return Result.Failure(["Base currency and target currency cannot be the same."]);

        // Validate both currencies exist and are active
        var baseCurrencyActive = await context.Currencies
            .AnyAsync(c => c.Id == request.BaseCurrencyId && c.IsActive, cancellationToken);
        if (!baseCurrencyActive)
            return Result.Failure(["Base currency not found or inactive."]);

        var currencyActive = await context.Currencies
            .AnyAsync(c => c.Id == request.CurrencyId && c.IsActive, cancellationToken);
        if (!currencyActive)
            return Result.Failure(["Target currency not found or inactive."]);

        // Validate unique combination
        var exists = await context.ExchangeRates
            .AnyAsync(x => x.BaseCurrencyId == request.BaseCurrencyId
                && x.CurrencyId == request.CurrencyId
                && x.RateDate == request.RateDate
                && x.RateType == request.RateType,
                cancellationToken);
        if (exists)
            return Result.Failure(["Exchange rate already exists for this currency pair, date, and type."]);

        var entity = new ExchangeRate
        {
            BaseCurrencyId = request.BaseCurrencyId,
            CurrencyId = request.CurrencyId,
            RateDate = request.RateDate,
            RateType = request.RateType,
            Rate = request.Rate,
            IsActive = true,
            Created = DateTimeOffset.UtcNow
        };

        context.ExchangeRates.Add(entity);
        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

public class CreateExchangeRateCommandValidator : AbstractValidator<CreateExchangeRateCommand>
{
    public CreateExchangeRateCommandValidator()
    {
        RuleFor(x => x.BaseCurrencyId)
            .GreaterThan(0).WithMessage("Base currency is required.");

        RuleFor(x => x.CurrencyId)
            .GreaterThan(0).WithMessage("Target currency is required.");

        RuleFor(x => x.RateDate)
            .NotEmpty().WithMessage("Rate date is required.");

        RuleFor(x => x.RateType)
            .IsInEnum().WithMessage("Rate type must be Official or Market.");

        RuleFor(x => x.Rate)
            .GreaterThan(0).WithMessage("Rate must be greater than 0.");
    }
}
