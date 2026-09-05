using ERP_Government.Application.Common.Security;
using ERP_Government.Domain.Accounting.Entities;

namespace ERP_Government.Application.Accounting.Commands.Balances.UnfinalizePeriod;

[Authorize(Policy = PermissionCodes.BalancesUnfinalize)]
public class UnfinalizePeriodCommand : IRequest<Result>
{
    public int FiscalYearId { get; init; }
    public int FiscalPeriodId { get; init; }
}

public class UnfinalizePeriodCommandHandler(
    IApplicationDbContext context) : IRequestHandler<UnfinalizePeriodCommand, Result>
{
    public async Task<Result> Handle(
        UnfinalizePeriodCommand request,
        CancellationToken cancellationToken)
    {
        // Get all balances for the period
        var balances = await context.AccountBalances
            .Where(x => x.FiscalYearId == request.FiscalYearId
                     && x.FiscalPeriodId == request.FiscalPeriodId)
            .ToListAsync(cancellationToken);

        if (balances.Count == 0)
            return Result.Failure(["No account balances found for this period."]);

        // Check if already unfinalized
        if (balances.All(x => !x.IsFinalized))
            return Result.Failure(["Period is already unfinalized."]);

        // Unfinalize all balances
        foreach (var balance in balances)
        {
            balance.IsFinalized = false;
            balance.FinalizedAt = null;
        }

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

public class UnfinalizePeriodCommandValidator : AbstractValidator<UnfinalizePeriodCommand>
{
    public UnfinalizePeriodCommandValidator()
    {
        RuleFor(x => x.FiscalYearId)
            .GreaterThan(0).WithMessage("Invalid fiscal year ID.");

        RuleFor(x => x.FiscalPeriodId)
            .GreaterThan(0).WithMessage("Invalid fiscal period ID.");
    }
}
