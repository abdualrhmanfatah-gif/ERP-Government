using ERP_Government.Application.Common.Security;
using ERP_Government.Domain.Accounting.Entities;

namespace ERP_Government.Application.Accounting.Commands.Balances.FinalizePeriod;

[Authorize(Policy = PermissionCodes.BalancesFinalize)]
public class FinalizePeriodCommand : IRequest<Result>
{
    public int FiscalYearId { get; init; }
    public int FiscalPeriodId { get; init; }
}

public class FinalizePeriodCommandHandler(
    IApplicationDbContext context) : IRequestHandler<FinalizePeriodCommand, Result>
{
    public async Task<Result> Handle(
        FinalizePeriodCommand request,
        CancellationToken cancellationToken)
    {
        // Get all balances for the period
        var balances = await context.AccountBalances
            .Where(x => x.FiscalYearId == request.FiscalYearId
                     && x.FiscalPeriodId == request.FiscalPeriodId)
            .ToListAsync(cancellationToken);

        if (balances.Count == 0)
            return Result.Failure(["No account balances found for this period."]);

        // Check if already finalized
        if (balances.All(x => x.IsFinalized))
            return Result.Failure(["Period is already finalized."]);

        // Finalize all balances
        var now = DateTimeOffset.UtcNow;
        foreach (var balance in balances)
        {
            balance.IsFinalized = true;
            balance.FinalizedAt = now;
        }

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

public class FinalizePeriodCommandValidator : AbstractValidator<FinalizePeriodCommand>
{
    public FinalizePeriodCommandValidator()
    {
        RuleFor(x => x.FiscalYearId)
            .GreaterThan(0).WithMessage("Invalid fiscal year ID.");

        RuleFor(x => x.FiscalPeriodId)
            .GreaterThan(0).WithMessage("Invalid fiscal period ID.");
    }
}
