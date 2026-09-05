using ERP_Government.Application.Common.Interfaces;
using ERP_Government.Application.Common.Security;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ERP_Government.Application.Budgeting.Queries.FinancialControl.GetCollectionStatement;

[Authorize(Policy = PermissionCodes.FinancialControlLapseYear)]
public record GetCollectionStatementQuery(int FiscalYearId)
    : IRequest<CollectionStatementResult>;

public record CollectionStatementResult(
    int FiscalYearId,
    string FiscalYearName,
    List<CollectionStatementLineResult> Collections,
    Dictionary<string, decimal> Subtotals,
    decimal GrandTotal,
    bool IsClosed);

public record CollectionStatementLineResult(
    string FundCode,
    string FundName,
    DateOnly Date,
    decimal Amount,
    string Source);

public class GetCollectionStatementQueryHandler(
    IApplicationDbContext context) : IRequestHandler<GetCollectionStatementQuery, CollectionStatementResult>
{
    public async Task<CollectionStatementResult> Handle(
        GetCollectionStatementQuery request,
        CancellationToken cancellationToken)
    {
        var fiscalYear = await context.FiscalYears.FindAsync(request.FiscalYearId, cancellationToken);
        if (fiscalYear is null)
            return new CollectionStatementResult(request.FiscalYearId, string.Empty, [], new(), 0m, false);

        var receipts = await context.RevenueReceipts
            .Where(r => r.ReceiptDate >= fiscalYear.StartDate
                && r.ReceiptDate <= fiscalYear.EndDate
                && r.Status == Domain.Revenue.Enums.RevenueReceiptStatus.Posted)
            .Join(context.Funds,
                r => r.FundId,
                f => f.Id,
                (r, f) => new { Receipt = r, Fund = f })
            .Select(x => new CollectionStatementLineResult(
                x.Fund.FundNumber,
                x.Fund.FundName,
                x.Receipt.ReceiptDate,
                x.Receipt.AmountTotal,
                x.Receipt.ReceiptNumber))
            .OrderBy(c => c.Date)
            .ToListAsync(cancellationToken);

        var subtotals = receipts
            .GroupBy(c => c.FundCode)
            .ToDictionary(g => g.Key, g => g.Sum(c => c.Amount));

        var grandTotal = receipts.Sum(c => c.Amount);

        return new CollectionStatementResult(
            request.FiscalYearId,
            fiscalYear.Name,
            receipts,
            subtotals,
            grandTotal,
            fiscalYear.IsClosed);
    }
}
