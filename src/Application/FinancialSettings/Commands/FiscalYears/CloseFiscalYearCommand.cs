using ERP_Government.Application.Common.Security;
using ERP_Government.Domain.FinancialSettings.Enums;
using ERP_Government.Domain.Events.FinancialSettings;

namespace ERP_Government.Application.FinancialSettings.Commands.FiscalYears;

// C-F004 — CloseFiscalYearCommand
[Authorize(Policy = PermissionCodes.FiscalYearsClose)]
public class CloseFiscalYearCommand : IRequest<Result>
{
    public int Id { get; init; }
}

public class CloseFiscalYearCommandHandler(
    IApplicationDbContext context) : IRequestHandler<CloseFiscalYearCommand, Result>
{
    public async Task<Result> Handle(
        CloseFiscalYearCommand request,
        CancellationToken cancellationToken)
    {
        var entity = await context.FiscalYears
            .FindAsync(request.Id, cancellationToken);

        if (entity is null)
            return Result.Failure(["Fiscal year not found."]);

        if (entity.Status != FiscalYearStatus.Open)
            return Result.Failure(["Only open fiscal years can be closed."]);

        entity.Status = FiscalYearStatus.HardClosed;
        entity.IsClosed = true;

        entity.AddDomainEvent(new FiscalYearClosed
        {
            FiscalYearId = entity.Id,
            YearNumber = entity.YearNumber,
            OccurredAt = DateTimeOffset.UtcNow
        });

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
