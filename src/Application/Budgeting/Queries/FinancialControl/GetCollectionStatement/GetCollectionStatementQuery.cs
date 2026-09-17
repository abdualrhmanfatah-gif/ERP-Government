using ERP_Government.Application.Common.Interfaces;
using ERP_Government.Application.Common.Security;
using ERP_Government.Domain.Revenue.Enums;
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
    string VoucherNumber,
    string PartyName,
    DateOnly Date,
    decimal Amount,
    string PaymentMethod);

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

        var receipts = await context.ReceiptVouchers
            .Where(r => r.VoucherDate >= fiscalYear.StartDate
                && r.VoucherDate <= fiscalYear.EndDate
                && r.Status == ReceiptVoucherStatus.Approved)
            .Select(r => new CollectionStatementLineResult(
                r.VoucherNumber,
                r.ReceivedFrom,
                r.VoucherDate,
                r.Lines.Sum(l => l.Amount),
                r.PaymentMethod.ToString()))
            .OrderBy(c => c.Date)
            .ToListAsync(cancellationToken);

        var subtotals = receipts
            .GroupBy(c => c.PaymentMethod)
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
