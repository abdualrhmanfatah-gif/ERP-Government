using ERP_Government.Application.Common.Interfaces;
using ERP_Government.Application.Common.Security;
using ERP_Government.Domain.Budgeting.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ERP_Government.Application.Budgeting.Queries.FinancialControl.GetFinalAccount;

[Authorize(Policy = PermissionCodes.FinancialControlApproveFinalAccount)]
public record GetFinalAccountQuery(int FinalAccountId)
    : IRequest<FinalAccountDetailResult?>;

public record FinalAccountDetailResult(
    int Id,
    int FiscalYearId,
    string FiscalYearName,
    FinalAccountStatus Status,
    DateTimeOffset GeneratedAt,
    int GeneratedById,
    DateTimeOffset? IssuedAt,
    int? IssuedById,
    List<FinalAccountLineDetailResult> Lines);

public record FinalAccountLineDetailResult(
    FinalAccountLineDimension Dimension,
    int DimensionId,
    string DimensionCode,
    string DimensionName,
    decimal BudgetedAmount,
    decimal ActualAmount,
    decimal Variance);

public class GetFinalAccountQueryHandler(
    IApplicationDbContext context) : IRequestHandler<GetFinalAccountQuery, FinalAccountDetailResult?>
{
    public async Task<FinalAccountDetailResult?> Handle(
        GetFinalAccountQuery request,
        CancellationToken cancellationToken)
    {
        var finalAccount = await context.FinalAccounts
            .Include(f => f.FiscalYear)
            .FirstOrDefaultAsync(f => f.Id == request.FinalAccountId, cancellationToken);

        if (finalAccount is null)
            return null;

        var lines = await context.FinalAccountLines
            .Where(l => l.FinalAccountId == request.FinalAccountId)
            .Select(l => new FinalAccountLineDetailResult(
                l.Dimension,
                l.DimensionId,
                l.DimensionCode,
                l.DimensionName,
                l.BudgetedAmount,
                l.ActualAmount,
                l.Variance))
            .ToListAsync(cancellationToken);

        return new FinalAccountDetailResult(
            finalAccount.Id,
            finalAccount.FiscalYearId,
            finalAccount.FiscalYear.Name,
            finalAccount.Status,
            finalAccount.GeneratedAt,
            finalAccount.GeneratedById,
            finalAccount.IssuedAt,
            finalAccount.IssuedById,
            lines);
    }
}
