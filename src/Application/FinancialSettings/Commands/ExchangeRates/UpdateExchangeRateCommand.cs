using ERP_Government.Application.Common.Security;
using ERP_Government.Domain.FinancialSettings.Enums;

namespace ERP_Government.Application.FinancialSettings.Commands.ExchangeRates;

// C-F005 — UpdateExchangeRateCommand
[Authorize(Policy = PermissionCodes.ExchangeRatesUpdate)]
public class UpdateExchangeRateCommand : IRequest<Result>
{
    public int Id { get; init; }
    public decimal Rate { get; init; }
    public ExchangeRateType RateType { get; init; }
    public byte[] RowVersion { get; init; } = [];
}

public class UpdateExchangeRateCommandHandler(
    IApplicationDbContext context) : IRequestHandler<UpdateExchangeRateCommand, Result>
{
    public async Task<Result> Handle(
        UpdateExchangeRateCommand request,
        CancellationToken cancellationToken)
    {
        var entity = await context.ExchangeRates
            .FindAsync(request.Id, cancellationToken);

        if (entity is null)
            return Result.Failure(["Exchange rate not found."]);

        if (!entity.RowVersion.SequenceEqual(request.RowVersion))
            return Result.Failure(["Exchange rate has been modified by another user. Please reload and try again."]);

        if (!entity.IsActive)
            return Result.Failure(["Cannot update inactive exchange rate."]);

        entity.Rate = request.Rate;
        entity.RateType = request.RateType;
        entity.LastModified = DateTimeOffset.UtcNow;

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

public class UpdateExchangeRateCommandValidator : AbstractValidator<UpdateExchangeRateCommand>
{
    public UpdateExchangeRateCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Invalid exchange rate ID.");

        RuleFor(x => x.Rate)
            .GreaterThan(0).WithMessage("Rate must be greater than 0.");

        RuleFor(x => x.RateType)
            .IsInEnum().WithMessage("Rate type must be Official or Market.");
    }
}
