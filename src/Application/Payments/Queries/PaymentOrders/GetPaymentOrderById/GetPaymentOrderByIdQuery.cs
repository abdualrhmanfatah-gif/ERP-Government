using ERP_Government.Application.Common.Security;
using ERP_Government.Application.Payments.Common.DTOs;

namespace ERP_Government.Application.Payments.Queries.PaymentOrders.GetPaymentOrderById;

[Authorize(Policy = PermissionCodes.PaymentOrdersView)]
public class GetPaymentOrderByIdQuery : IRequest<PaymentOrderDto?>
{
    public int Id { get; init; }
}

public class GetPaymentOrderByIdQueryHandler(
    IApplicationDbContext context) : IRequestHandler<GetPaymentOrderByIdQuery, PaymentOrderDto?>
{
    public async Task<PaymentOrderDto?> Handle(
        GetPaymentOrderByIdQuery request,
        CancellationToken cancellationToken)
    {
        var order = await context.PaymentOrders
            .Where(x => x.Id == request.Id)
            .Select(x => new PaymentOrderDto
            {
                Id = x.Id,
                PaymentOrderNumber = x.PaymentOrderNumber,
                PaymentOrderDate = x.PaymentOrderDate.ToDateTime(TimeOnly.MinValue),
                DueDate = x.DueDate.HasValue ? x.DueDate.Value.ToDateTime(TimeOnly.MinValue) : null,
                PaymentOrderType = x.PaymentOrderType,
                VendorId = x.VendorId,
                FundId = x.FundId,
                FiscalYearId = x.FiscalYearId,
                AppropriationId = x.AppropriationId,
                BudgetClassificationId = x.BudgetClassificationId,
                CostCenterId = x.CostCenterId,
                ProjectId = x.ProjectId,
                PurchaseOrderId = x.PurchaseOrderId,
                EncumbranceId = x.EncumbranceId,
                CurrencyId = x.CurrencyId,
                ExchangeRate = x.ExchangeRate,
                AmountGross = x.AmountGross,
                DeductionAmount = x.DeductionAmount,
                PaymentMethod = x.PaymentMethod,
                BankAccountId = x.BankAccountId,
                BeneficiaryName = x.BeneficiaryName,
                BeneficiaryIban = x.BeneficiaryIban,
                BeneficiaryAccountNumber = x.BeneficiaryAccountNumber,
                BeneficiaryBankName = x.BeneficiaryBankName,
                Status = x.Status,
                BudgetCheckStatus = x.BudgetCheckStatus,
                TreasuryStatus = x.TreasuryStatus,
                TreasuryReference = x.TreasuryReference,
                TreasurySentAt = x.TreasurySentAt,
                PaidAt = x.PaidAt,
                JournalEntryId = x.JournalEntryId,
                Notes = x.Notes
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (order is null) return null;

        // Load lines
        order.Lines = await context.PaymentOrderLines
            .Where(l => l.PaymentOrderId == request.Id)
            .Select(l => new PaymentOrderLineDto
            {
                Id = l.Id,
                LineNumber = l.LineNumber,
                LineType = l.LineType,
                Description = l.Description,
                AccountId = l.AccountId,
                Amount = l.Amount,
                TaxAmount = l.TaxAmount,
                FundId = l.FundId,
                AppropriationId = l.AppropriationId,
                OrganizationUnitId = l.OrganizationUnitId,
                CostCenterId = l.CostCenterId,
                ProjectId = l.ProjectId
            })
            .ToListAsync(cancellationToken);

        // Load deductions
        order.Deductions = await context.PaymentOrderDeductions
            .Where(d => d.PaymentOrderId == request.Id)
            .Select(d => new PaymentOrderDeductionDto
            {
                Id = d.Id,
                LineNumber = d.LineNumber,
                DeductionType = d.DeductionType,
                DeductionCode = d.DeductionCode,
                Description = d.Description,
                AccountId = d.AccountId,
                Amount = d.Amount,
                DeductionPercent = d.DeductionPercent,
                IsMandatory = d.IsMandatory,
                IsTaxDeduction = d.IsTaxDeduction,
                TaxAuthorityId = d.TaxAuthorityId,
                ReferenceNumber = d.ReferenceNumber
            })
            .ToListAsync(cancellationToken);

        return order;
    }
}
