using ERP_Government.Application.Common.Interfaces;
using ERP_Government.Application.Common.Security;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ERP_Government.Application.Budgeting.Queries.FinancialControl.GetDisbursementStatement;

[Authorize(Policy = PermissionCodes.FinancialControlLapseYear)]
public record GetDisbursementStatementQuery(int FiscalYearId)
    : IRequest<DisbursementStatementResult>;

public record DisbursementStatementResult(
    int FiscalYearId,
    string FiscalYearName,
    List<DisbursementStatementLineResult> Disbursements,
    Dictionary<string, decimal> Subtotals,
    decimal GrandTotal,
    bool IsClosed);

public record DisbursementStatementLineResult(
    string FundCode,
    string FundName,
    DateOnly Date,
    decimal Amount,
    string Payee);

public class GetDisbursementStatementQueryHandler(
    IApplicationDbContext context) : IRequestHandler<GetDisbursementStatementQuery, DisbursementStatementResult>
{
    public async Task<DisbursementStatementResult> Handle(
        GetDisbursementStatementQuery request,
        CancellationToken cancellationToken)
    {
        var fiscalYear = await context.FiscalYears.FindAsync(request.FiscalYearId, cancellationToken);
        if (fiscalYear is null)
            return new DisbursementStatementResult(request.FiscalYearId, string.Empty, [], new(), 0m, false);

        var payments = await context.PaymentOrders
            .Where(po => po.FiscalYearId == request.FiscalYearId
                && po.Status == Domain.Payments.Enums.PaymentOrderStatus.Paid)
            .Join(context.Funds,
                po => po.FundId,
                f => f.Id,
                (po, f) => new { PaymentOrder = po, Fund = f })
            .Select(x => new DisbursementStatementLineResult(
                x.Fund.FundNumber,
                x.Fund.FundName,
                DateOnly.FromDateTime(x.PaymentOrder.PaidAt!.Value.Date),
                x.PaymentOrder.AmountGross - x.PaymentOrder.DeductionAmount,
                x.PaymentOrder.BeneficiaryName))
            .OrderBy(d => d.Date)
            .ToListAsync(cancellationToken);

        var subtotals = payments
            .GroupBy(d => d.FundCode)
            .ToDictionary(g => g.Key, g => g.Sum(d => d.Amount));

        var grandTotal = payments.Sum(d => d.Amount);

        return new DisbursementStatementResult(
            request.FiscalYearId,
            fiscalYear.Name,
            payments,
            subtotals,
            grandTotal,
            fiscalYear.IsClosed);
    }
}
