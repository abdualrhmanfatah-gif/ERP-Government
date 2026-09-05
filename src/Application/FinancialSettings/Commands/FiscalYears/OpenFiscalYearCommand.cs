using ERP_Government.Application.Common.Security;
using ERP_Government.Domain.FinancialSettings.Enums;
using ERP_Government.Domain.Events.FinancialSettings;

namespace ERP_Government.Application.FinancialSettings.Commands.FiscalYears;

// C-F003 — OpenFiscalYearCommand
[Authorize(Policy = PermissionCodes.FiscalYearsOpen)]
public class OpenFiscalYearCommand : IRequest<Result>
{
    public int Id { get; init; }
}

public class OpenFiscalYearCommandHandler(
    IApplicationDbContext context) : IRequestHandler<OpenFiscalYearCommand, Result>
{
    public async Task<Result> Handle(
        OpenFiscalYearCommand request,
        CancellationToken cancellationToken)
    {
        var entity = await context.FiscalYears
            .FindAsync(request.Id, cancellationToken);

        if (entity is null)
            return Result.Failure(["Fiscal year not found."]);

        if (entity.Status != FiscalYearStatus.Draft)
            return Result.Failure(["Only draft fiscal years can be opened."]);

        entity.Status = FiscalYearStatus.Open;
        entity.IsActive = true;

        entity.AddDomainEvent(new FiscalYearOpened
        {
            FiscalYearId = entity.Id,
            YearNumber = entity.YearNumber,
            OccurredAt = DateTimeOffset.UtcNow
        });

        // Yearly reset: reset CurrentNumber to 1 for all sequences with matching FiscalYearId
        var sequencesToReset = await context.DocumentSequences
            .Where(s => s.ResetPolicy == ResetPolicy.Yearly && s.FiscalYearId == request.Id)
            .ToListAsync(cancellationToken);

        foreach (var sequence in sequencesToReset)
        {
            sequence.CurrentNumber = 1;
        }

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
