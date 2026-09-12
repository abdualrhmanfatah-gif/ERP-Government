using ERP_Government.Application.Common.Security;
using ERP_Government.Application.FinancialSettings.Common.Services;
using ERP_Government.Application.Parties.Common;
using ERP_Government.Domain.Accounting.Enums;
using ERP_Government.Domain.Payments.Entities;
using ERP_Government.Domain.Payments.Enums;

namespace ERP_Government.Application.Payments.Commands.PaymentOrders.CreatePaymentOrder;

[Authorize(Policy = PermissionCodes.PaymentOrdersCreate)]
public class CreatePaymentOrderCommand : IRequest<Result>
{
    public DateTime PaymentOrderDate { get; init; }
    public DateTime? DueDate { get; init; }
    public string PaymentOrderType { get; init; } = string.Empty;
    public int FundId { get; init; }
    public int? FiscalYearId { get; init; }
    public int? BudgetItemAllocationId { get; init; }
    public int? BudgetClassificationId { get; init; }
    public int? CostCenterId { get; init; }
    public int? AccountId { get; init; }
    public int? PurchaseOrderId { get; init; }
    public int? EncumbranceId { get; init; }
    public int CurrencyId { get; init; }
    public decimal? ExchangeRate { get; init; }
    public decimal AmountGross { get; init; }
    public decimal DeductionAmount { get; init; }
    public PaymentMethod PaymentMethod { get; init; }
    public int? BankAccountId { get; init; }
    public string BeneficiaryName { get; init; } = string.Empty;
    public string? BeneficiaryAccountNumber { get; init; }
    public string? BeneficiaryBankName { get; init; }
    public string? Notes { get; init; }
    public int? AccrualJournalEntryId { get; init; }
    public List<CreatePaymentOrderDeductionDto> Deductions { get; init; } = [];
}

public class CreatePaymentOrderDeductionDto
{
    public DeductionType DeductionType { get; init; }
    public string? DeductionCode { get; init; }
    public string? Description { get; init; }
    public int AccountId { get; init; }
    public decimal Amount { get; init; }
    public decimal? DeductionPercent { get; init; }
    public bool IsMandatory { get; init; }
    public bool IsTaxDeduction { get; init; }
    public int? TaxAuthorityId { get; init; }
    public string? ReferenceNumber { get; init; }
}

public class CreatePaymentOrderCommandHandler(
    IApplicationDbContext context,
    IDocumentSequenceService sequenceService,
    IDocumentStatusLogger statusLogger) : IRequestHandler<CreatePaymentOrderCommand, Result>
{
    public async Task<Result> Handle(
        CreatePaymentOrderCommand request,
        CancellationToken cancellationToken)
    {
        var fund = await context.Funds.FindAsync(request.FundId, cancellationToken);
        if (fund is null || !fund.IsActive)
            return Result.Failure(["Invalid or inactive fund."]);

        // Auto-determine fiscal year from date if not provided
        var orderDate = DateOnly.FromDateTime(request.PaymentOrderDate);
        ERP_Government.Domain.FinancialSettings.Entities.FiscalYear? fiscalYear;
        if (request.FiscalYearId.HasValue)
        {
            fiscalYear = await context.FiscalYears.FindAsync(request.FiscalYearId.Value, cancellationToken);
        }
        else
        {
            fiscalYear = await context.FiscalYears
                .FirstOrDefaultAsync(fy => fy.StartDate <= orderDate && fy.EndDate >= orderDate, cancellationToken);
        }
        if (fiscalYear is null || fiscalYear.Status == ERP_Government.Domain.FinancialSettings.Enums.FiscalYearStatus.HardClosed)
            return Result.Failure(["Invalid or closed fiscal year."]);

        if (request.BudgetItemAllocationId.HasValue)
        {
            var allocation = await context.BudgetItemAllocations
                .Include(a => a.Budget)
                .FirstOrDefaultAsync(a => a.Id == request.BudgetItemAllocationId.Value, cancellationToken);
            if (allocation is null)
                return Result.Failure(["المخصص غير موجود"]);
            if (allocation.Budget.Status != ERP_Government.Domain.Budgeting.Enums.BudgetStatus.Active)
                return Result.Failure(["الموازنة غير مفعلة"]);
        }

        if (request.PaymentMethod == PaymentMethod.Check && !request.BankAccountId.HasValue)
            return Result.Failure(["Bank account is required for check payment method."]);

        if (request.BankAccountId.HasValue)
        {
            var bankAccount = await context.BankAccounts.FindAsync(request.BankAccountId.Value, cancellationToken);
            if (bankAccount is null || !bankAccount.IsActive)
                return Result.Failure(["Invalid or inactive bank account."]);
        }

        if (string.IsNullOrWhiteSpace(request.BeneficiaryName))
            return Result.Failure(["Beneficiary name is required."]);

        if (request.AmountGross <= 0)
            return Result.Failure(["Gross amount must be greater than zero."]);

        if (request.DeductionAmount < 0)
            return Result.Failure(["Deduction amount cannot be negative."]);

        var deductionsSum = request.Deductions.Sum(d => d.Amount);
        if (Math.Abs(deductionsSum - request.DeductionAmount) > 0.01m)
            return Result.Failure(["Deductions must sum to the deduction amount."]);

        if (request.AccrualJournalEntryId.HasValue)
        {
            var accrualEntry = await context.JournalEntries
                .FirstOrDefaultAsync(j => j.Id == request.AccrualJournalEntryId.Value, cancellationToken);
            if (accrualEntry is null)
                return Result.Failure(["Accrual journal entry not found."]);

            if (accrualEntry.EntryType != MoveEntryType.Accrual)
                return Result.Failure(["Payment order must be linked to an accrual journal entry."]);

            var hasLiabilityCreditLine = await context.JournalEntryLines
                .AnyAsync(l => l.JournalEntryId == accrualEntry.Id && l.Credit > 0, cancellationToken);
            if (!hasLiabilityCreditLine)
                return Result.Failure(["Accrual journal entry must contain a liability credit line."]);

            var accrualAlreadyUsed = await context.PaymentOrders
                .AnyAsync(o => o.AccrualJournalEntryId == accrualEntry.Id
                    && o.Status != PaymentOrderStatus.Cancelled
                    && o.Status != PaymentOrderStatus.Voided,
                    cancellationToken);
            if (accrualAlreadyUsed)
                return Result.Failure(["A payment order already exists for this accrual journal entry."]);
        }

        string paymentOrderNumber;
        try
        {
            paymentOrderNumber = await sequenceService.GenerateNextNumberAsync("PaymentOrder", cancellationToken);
        }
        catch (DocumentSequenceException ex)
        {
            return Result.Failure([ex.Message]);
        }

        var entity = new PaymentOrder
        {
            PaymentOrderNumber = paymentOrderNumber,
            PaymentOrderDate = DateOnly.FromDateTime(request.PaymentOrderDate),
            DueDate = request.DueDate.HasValue ? DateOnly.FromDateTime(request.DueDate.Value) : null,
            PaymentOrderType = request.PaymentOrderType,
            FundId = request.FundId,
            FiscalYearId = fiscalYear.Id,
            BudgetItemAllocationId = request.BudgetItemAllocationId,
            BudgetClassificationId = request.BudgetClassificationId,
            CostCenterId = request.CostCenterId,
            AccountId = request.AccountId,
            PurchaseOrderId = request.PurchaseOrderId,
            EncumbranceId = request.EncumbranceId,
            CurrencyId = request.CurrencyId,
            ExchangeRate = request.ExchangeRate,
            AmountGross = request.AmountGross,
            DeductionAmount = request.DeductionAmount,
            PaymentMethod = request.PaymentMethod,
            BankAccountId = request.BankAccountId,
            BeneficiaryName = request.BeneficiaryName,
            BeneficiaryAccountNumber = request.BeneficiaryAccountNumber,
            BeneficiaryBankName = request.BeneficiaryBankName,
            Status = PaymentOrderStatus.Draft,
            Notes = request.Notes,
            AccrualJournalEntryId = request.AccrualJournalEntryId
        };

        context.PaymentOrders.Add(entity);
        await context.SaveChangesAsync(cancellationToken);

        int deductionLine = 1;
        foreach (var deduction in request.Deductions)
        {
            if (deduction.IsTaxDeduction && !deduction.TaxAuthorityId.HasValue)
                return Result.Failure(["Tax authority is required for tax deductions."]);

            context.PaymentOrderDeductions.Add(new PaymentOrderDeduction
            {
                PaymentOrderId = entity.Id,
                LineNumber = deductionLine++,
                DeductionType = deduction.DeductionType,
                DeductionCode = deduction.DeductionCode,
                Description = deduction.Description,
                AccountId = deduction.AccountId,
                Amount = deduction.Amount,
                DeductionPercent = deduction.DeductionPercent,
                IsMandatory = deduction.IsMandatory,
                IsTaxDeduction = deduction.IsTaxDeduction,
                TaxAuthorityId = deduction.TaxAuthorityId,
                ReferenceNumber = deduction.ReferenceNumber
            });
        }

        await context.SaveChangesAsync(cancellationToken);

        await statusLogger.LogAsync(
            "paymentorders",
            entity.Id,
            "—",
            PaymentOrderStatus.Draft.ToString(),
            0,
            null,
            cancellationToken);

        return Result.Success();
    }
}

public class CreatePaymentOrderCommandValidator : AbstractValidator<CreatePaymentOrderCommand>
{
    public CreatePaymentOrderCommandValidator()
    {
        RuleFor(x => x.PaymentOrderDate)
            .NotEmpty().WithMessage("Payment order date is required.");

        RuleFor(x => x.PaymentOrderType)
            .NotEmpty().WithMessage("Payment order type is required.")
            .MaximumLength(20).WithMessage("Type must not exceed 20 characters.");

        RuleFor(x => x.FundId)
            .GreaterThan(0).WithMessage("Fund is required.");

        RuleFor(x => x.CurrencyId)
            .GreaterThan(0).WithMessage("Currency is required.");

        RuleFor(x => x.BeneficiaryName)
            .NotEmpty().WithMessage("Beneficiary name is required.")
            .MaximumLength(200).WithMessage("Beneficiary name must not exceed 200 characters.");

        RuleFor(x => x.AmountGross)
            .GreaterThan(0).WithMessage("Gross amount must be greater than zero.");

        RuleFor(x => x.AccrualJournalEntryId)
            .GreaterThan(0).When(x => x.AccrualJournalEntryId.HasValue)
            .WithMessage("Invalid accrual journal entry ID.");
    }
}
