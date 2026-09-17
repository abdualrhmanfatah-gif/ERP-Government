using ERP_Government.Application.Common.Errors;
using ERP_Government.Application.Common.Models;
using ERP_Government.Application.Common.Security;
using ERP_Government.Application.Payments.Common.DTOs;

namespace ERP_Government.Application.Payments.Queries.PaymentOrders.GetPaymentOrderById;

[Authorize(Policy = PermissionCodes.PaymentOrdersView)]
public class GetPaymentOrderByIdQuery : IRequest<Result<PaymentOrderDto>>
{
    public int Id { get; init; }
}

public class GetPaymentOrderByIdQueryHandler(
    IApplicationDbContext context) : IRequestHandler<GetPaymentOrderByIdQuery, Result<PaymentOrderDto>>
{
    public async Task<Result<PaymentOrderDto>> Handle(
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
                FundId = x.FundId,
                FiscalYearId = x.FiscalYearId,
                BudgetItemAllocationId = x.BudgetItemAllocationId,
                BudgetClassificationId = x.BudgetClassificationId,
                CostCenterId = x.CostCenterId,
                AccountId = x.AccountId,
                PurchaseOrderId = x.PurchaseOrderId,
                EncumbranceId = x.EncumbranceId,
                CurrencyId = x.CurrencyId,
                ExchangeRate = x.ExchangeRate,
                AmountGross = x.AmountGross,
                DeductionAmount = x.DeductionAmount,
                PaymentMethod = x.PaymentMethod,
                BankAccountId = x.BankAccountId,
                BeneficiaryName = x.BeneficiaryName,
                BeneficiaryAccountNumber = x.BeneficiaryAccountNumber,
                BeneficiaryBankName = x.BeneficiaryBankName,
                Status = x.Status,
                TreasurySentAt = x.TreasurySentAt,
                PaidAt = x.PaidAt,
                JournalEntryId = x.JournalEntryId,
                Notes = x.Notes,
                DisbursementRequestId = x.DisbursementRequestId,
                DisbursementRequestNumber = x.DisbursementRequest != null ? x.DisbursementRequest.RequestNumber : null,
                AccrualJournalEntryId = x.AccrualJournalEntryId,
                AccrualEntryNumber = x.AccrualJournalEntry != null ? x.AccrualJournalEntry.EntryNumber : null,
                RowVersion = x.RowVersion
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (order is null)
            return Result<PaymentOrderDto>.Failure(ErrorCodes.Payments.PaymentOrderNotFound, ErrorCategory.NotFound, $"Payment order with ID {request.Id} not found.");

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

        return Result<PaymentOrderDto>.Success(order);
    }
}
