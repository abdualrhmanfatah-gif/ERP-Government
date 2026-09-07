using ERP_Government.Application.Accounting.Common;
using ERP_Government.Application.Common.Interfaces;
using ERP_Government.Application.Common.Security;
using ERP_Government.Domain.Accounting.Enums;

namespace ERP_Government.Application.Accounting.Commands.AccountBalances.Finalize;

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
        // FR-002: Reject if any AccountingEvents with Status=Pending exist (global check)
        // Pending events lack a JournalEntry link, so period-level filtering is not possible.
        var hasPendingEvents = await context.AccountingEvents
            .AnyAsync(e => e.Status == EventStatus.Pending,
                      cancellationToken);

        if (hasPendingEvents)
            return Result.Failure(["هناك أحداث معلقة، أكمل الترحيل أولاً"]);

        // Find all balances for this period
        var balances = await context.AccountBalances
            .Where(b => b.FiscalYearId == request.FiscalYearId
                     && b.FiscalPeriodId == request.FiscalPeriodId)
            .ToListAsync(cancellationToken);

        if (balances.Count == 0)
            return Result.Failure(["No account balances found for this period."]);

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
