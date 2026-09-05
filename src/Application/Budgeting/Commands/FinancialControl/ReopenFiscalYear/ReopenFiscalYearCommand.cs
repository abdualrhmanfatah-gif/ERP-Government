using ERP_Government.Application.Common.Interfaces;
using ERP_Government.Application.Common.Models;
using ERP_Government.Application.Common.Security;
using ERP_Government.Domain.Budgeting.Entities;
using ERP_Government.Domain.Budgeting.Enums;
using MediatR;

namespace ERP_Government.Application.Budgeting.Commands.FinancialControl.ReopenFiscalYear;

[Authorize(Policy = PermissionCodes.FinancialControlLapseYear)]
public record ReopenFiscalYearCommand(int FiscalYearId) : IRequest<Result<ReopenFiscalYearResult>>;

public record ReopenFiscalYearResult(
    int YearClosingRunId,
    int FiscalYearId,
    decimal RestoredAppropriationTotal,
    decimal RestoredEncumbranceTotal);

public class ReopenFiscalYearCommandHandler(
    IApplicationDbContext context,
    IUser user) : IRequestHandler<ReopenFiscalYearCommand, Result<ReopenFiscalYearResult>>
{
    public async Task<Result<ReopenFiscalYearResult>> Handle(
        ReopenFiscalYearCommand request,
        CancellationToken cancellationToken)
    {
        var fiscalYear = await context.FiscalYears.FindAsync(request.FiscalYearId, cancellationToken);
        if (fiscalYear is null)
            return Result<ReopenFiscalYearResult>.Failure(new[] { "Fiscal year not found." });

        var finalAccount = await context.FinalAccounts
            .Where(f => f.FiscalYearId == request.FiscalYearId && f.Status == FinalAccountStatus.Issued)
            .FirstOrDefaultAsync(cancellationToken);

        if (finalAccount is not null)
            return Result<ReopenFiscalYearResult>.Failure(new[]
                { $"Cannot reopen fiscal year {fiscalYear.Name} — final account has been issued." });

        var lapsedRun = await context.YearClosingRuns
            .Where(r => r.FiscalYearId == request.FiscalYearId && r.Status == YearClosingRunStatus.Completed)
            .FirstOrDefaultAsync(cancellationToken);

        if (lapsedRun is null)
            return Result<ReopenFiscalYearResult>.Failure(new[]
                { $"Fiscal year {fiscalYear.Name} has not been lapsed." });

        var appropriations = await context.Appropriations
            .Where(a => a.BudgetItem.Budget.FiscalYearId == request.FiscalYearId
                && a.Status == AppropriationStatus.Cancelled)
            .ToListAsync(cancellationToken);

        var encumbrances = await context.Encumbrances
            .Where(e => appropriations.Any(a => a.Id == e.AppropriationId)
                && e.Status == EncumbranceStatus.Cancelled)
            .ToListAsync(cancellationToken);

        var restoredAppropriationTotal = 0m;
        var restoredEncumbranceTotal = 0m;

        foreach (var appropriation in appropriations)
        {
            restoredAppropriationTotal += appropriation.AppropriationType == AppropriationType.Original ? appropriation.Amount :
                appropriation.AppropriationType == AppropriationType.Supplement ? appropriation.Amount :
                appropriation.AppropriationType == AppropriationType.Reduction ? -appropriation.Amount :
                appropriation.AppropriationType == AppropriationType.Adjustment ? appropriation.Amount : 0m;

            appropriation.Status = AppropriationStatus.Active;
        }

        foreach (var encumbrance in encumbrances)
        {
            restoredEncumbranceTotal += encumbrance.Amount;
            encumbrance.Status = EncumbranceStatus.Active;
        }

        fiscalYear.IsClosed = false;

        lapsedRun.Status = YearClosingRunStatus.Reversed;
        lapsedRun.ReversedById = user.Id ?? 0;
        lapsedRun.ReversedAt = DateTimeOffset.UtcNow;

        var reopenRun = new YearClosingRun
        {
            FiscalYearId = request.FiscalYearId,
            RunAt = DateTimeOffset.UtcNow,
            RunById = user.Id ?? 0,
            RunType = YearClosingRunType.Reopen,
            LapsedAppropriationTotal = restoredAppropriationTotal,
            LapsedEncumbranceTotal = restoredEncumbranceTotal,
            Status = YearClosingRunStatus.Completed
        };

        context.YearClosingRuns.Add(reopenRun);
        await context.SaveChangesAsync(cancellationToken);

        return Result<ReopenFiscalYearResult>.Success(new ReopenFiscalYearResult(
            reopenRun.Id,
            request.FiscalYearId,
            restoredAppropriationTotal,
            restoredEncumbranceTotal));
    }
}
