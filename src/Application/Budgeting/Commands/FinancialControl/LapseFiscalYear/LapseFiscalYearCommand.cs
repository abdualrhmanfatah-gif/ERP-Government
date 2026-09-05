using ERP_Government.Application.Common.Interfaces;
using ERP_Government.Application.Common.Models;
using ERP_Government.Application.Common.Security;
using ERP_Government.Domain.Budgeting.Entities;
using ERP_Government.Domain.Budgeting.Enums;
using ERP_Government.Domain.Common;
using MediatR;

namespace ERP_Government.Application.Budgeting.Commands.FinancialControl.LapseFiscalYear;

[Authorize(Policy = PermissionCodes.FinancialControlLapseYear)]
public record LapseFiscalYearCommand(int FiscalYearId) : IRequest<Result<LapseFiscalYearResult>>;

public record LapseFiscalYearResult(
    int YearClosingRunId,
    int FiscalYearId,
    string FiscalYearName,
    decimal LapsedAppropriationTotal,
    decimal LapsedEncumbranceTotal);

public class LapseFiscalYearCommandHandler(
    IApplicationDbContext context,
    IUser user) : IRequestHandler<LapseFiscalYearCommand, Result<LapseFiscalYearResult>>
{
    public async Task<Result<LapseFiscalYearResult>> Handle(
        LapseFiscalYearCommand request,
        CancellationToken cancellationToken)
    {
        var fiscalYear = await context.FiscalYears.FindAsync(request.FiscalYearId, cancellationToken);
        if (fiscalYear is null)
            return Result<LapseFiscalYearResult>.Failure(new[] { "Fiscal year not found." });

        var existingRun = await context.YearClosingRuns
            .Where(r => r.FiscalYearId == request.FiscalYearId && r.Status == YearClosingRunStatus.Completed)
            .FirstOrDefaultAsync(cancellationToken);

        if (existingRun is not null)
            return Result<LapseFiscalYearResult>.Failure(new[]
                { $"Fiscal year {fiscalYear.Name} has already been lapsed. Run ID: {existingRun.Id}." });

        var appropriations = await context.Appropriations
            .Where(a => a.BudgetItem.Budget.FiscalYearId == request.FiscalYearId
                && a.Status == AppropriationStatus.Active)
            .ToListAsync(cancellationToken);

        var encumbrances = await context.Encumbrances
            .Where(e => appropriations.Any(a => a.Id == e.AppropriationId)
                && (e.Status == EncumbranceStatus.Active
                    || e.Status == EncumbranceStatus.PartiallyReleased
                    || e.Status == EncumbranceStatus.PartiallyLiquidated)
                && e.ReversalOfId == null)
            .ToListAsync(cancellationToken);

        var lapsedAppropriationTotal = 0m;
        var lapsedEncumbranceTotal = 0m;

        foreach (var appropriation in appropriations)
        {
            lapsedAppropriationTotal += appropriation.AppropriationType == AppropriationType.Original ? appropriation.Amount :
                appropriation.AppropriationType == AppropriationType.Supplement ? appropriation.Amount :
                appropriation.AppropriationType == AppropriationType.Reduction ? -appropriation.Amount :
                appropriation.AppropriationType == AppropriationType.Adjustment ? appropriation.Amount : 0m;

            appropriation.Status = AppropriationStatus.Cancelled;
        }

        foreach (var encumbrance in encumbrances)
        {
            lapsedEncumbranceTotal += encumbrance.Amount;
            encumbrance.Status = EncumbranceStatus.Cancelled;
        }

        fiscalYear.IsClosed = true;

        var closingRun = new YearClosingRun
        {
            FiscalYearId = request.FiscalYearId,
            RunAt = DateTimeOffset.UtcNow,
            RunById = user.Id ?? 0,
            RunType = YearClosingRunType.Lapse,
            LapsedAppropriationTotal = lapsedAppropriationTotal,
            LapsedEncumbranceTotal = lapsedEncumbranceTotal,
            Status = YearClosingRunStatus.Completed
        };

        context.YearClosingRuns.Add(closingRun);
        await context.SaveChangesAsync(cancellationToken);

        return Result<LapseFiscalYearResult>.Success(new LapseFiscalYearResult(
            closingRun.Id,
            request.FiscalYearId,
            fiscalYear.Name,
            lapsedAppropriationTotal,
            lapsedEncumbranceTotal));
    }
}
