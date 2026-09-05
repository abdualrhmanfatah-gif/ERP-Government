using ERP_Government.Application.Common.Security;
using ERP_Government.Domain.FinancialSettings.Enums;

namespace ERP_Government.Application.FinancialSettings.Commands.FiscalPeriods;

[Authorize(Policy = PermissionCodes.FiscalPeriodsLock)]
public class LockFiscalPeriodCommand : IRequest<Result>
{
    public int Id { get; init; }
}

public class LockFiscalPeriodCommandHandler(
    IApplicationDbContext context) : IRequestHandler<LockFiscalPeriodCommand, Result>
{
    public async Task<Result> Handle(
        LockFiscalPeriodCommand request,
        CancellationToken cancellationToken)
    {
        var entity = await context.FiscalPeriods
            .FindAsync(request.Id, cancellationToken);

        if (entity is null)
            return Result.Failure(["Fiscal period not found."]);

        var fiscalYear = await context.FiscalYears
            .FindAsync(entity.FiscalYearId, cancellationToken);

        if (fiscalYear?.Status == FiscalYearStatus.HardClosed)
            return Result.Failure(["Cannot lock period in a hard-closed fiscal year."]);

        if (entity.IsLockedForPosting)
            return Result.Failure(["Fiscal period is already locked."]);

        entity.IsLockedForPosting = true;

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
