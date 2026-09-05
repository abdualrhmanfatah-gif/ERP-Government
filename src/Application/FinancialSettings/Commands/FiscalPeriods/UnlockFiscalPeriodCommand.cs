using ERP_Government.Application.Common.Security;
using ERP_Government.Domain.FinancialSettings.Enums;

namespace ERP_Government.Application.FinancialSettings.Commands.FiscalPeriods;

[Authorize(Policy = PermissionCodes.FiscalPeriodsUnlock)]
public class UnlockFiscalPeriodCommand : IRequest<Result>
{
    public int Id { get; init; }
}

public class UnlockFiscalPeriodCommandHandler(
    IApplicationDbContext context) : IRequestHandler<UnlockFiscalPeriodCommand, Result>
{
    public async Task<Result> Handle(
        UnlockFiscalPeriodCommand request,
        CancellationToken cancellationToken)
    {
        var entity = await context.FiscalPeriods
            .FindAsync(request.Id, cancellationToken);

        if (entity is null)
            return Result.Failure(["Fiscal period not found."]);

        var fiscalYear = await context.FiscalYears
            .FindAsync(entity.FiscalYearId, cancellationToken);

        if (fiscalYear?.Status == FiscalYearStatus.HardClosed)
            return Result.Failure(["Cannot unlock period in a hard-closed fiscal year."]);

        if (!entity.IsLockedForPosting)
            return Result.Failure(["Fiscal period is not locked."]);

        entity.IsLockedForPosting = false;

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
