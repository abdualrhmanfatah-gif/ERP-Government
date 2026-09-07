using ERP_Government.Application.Accounting.Common;
using ERP_Government.Application.Common.Interfaces;
using ERP_Government.Application.Common.Security;

namespace ERP_Government.Application.Accounting.Commands.AccountBalances.Unfinalize;

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
        var balances = await context.AccountBalances
            .Where(b => b.FiscalYearId == request.FiscalYearId
                     && b.FiscalPeriodId == request.FiscalPeriodId)
            .ToListAsync(cancellationToken);

        if (balances.Count == 0)
            return Result.Failure(["No account balances found for this period."]);

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
