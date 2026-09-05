using ERP_Government.Application.Common.Security;
using ERP_Government.Domain.Events.FinancialSettings;

namespace ERP_Government.Application.FinancialSettings.Commands.ExchangeRates;

// C-F007 — DeactivateExchangeRateCommand
[Authorize(Policy = PermissionCodes.ExchangeRatesDeactivate)]
public class DeactivateExchangeRateCommand : IRequest<Result>
{
    public int Id { get; init; }
    public byte[] RowVersion { get; init; } = [];
}

public class DeactivateExchangeRateCommandHandler(
    IApplicationDbContext context) : IRequestHandler<DeactivateExchangeRateCommand, Result>
{
    public async Task<Result> Handle(
        DeactivateExchangeRateCommand request,
        CancellationToken cancellationToken)
    {
        var entity = await context.ExchangeRates
            .FindAsync(request.Id, cancellationToken);

        if (entity is null)
            return Result.Failure(["Exchange rate not found."]);

        if (!entity.RowVersion.SequenceEqual(request.RowVersion))
            return Result.Failure(["Exchange rate has been modified by another user. Please reload and try again."]);

        if (!entity.IsActive)
            return Result.Failure(["Exchange rate is already inactive."]);

        entity.IsActive = false;
        entity.LastModified = DateTimeOffset.UtcNow;

        entity.AddDomainEvent(new ExchangeRateDeactivated
        {
            ExchangeRateId = entity.Id,
            OccurredAt = DateTimeOffset.UtcNow
        });

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

public class DeactivateExchangeRateCommandValidator : AbstractValidator<DeactivateExchangeRateCommand>
{
    public DeactivateExchangeRateCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Invalid exchange rate ID.");
    }
}
