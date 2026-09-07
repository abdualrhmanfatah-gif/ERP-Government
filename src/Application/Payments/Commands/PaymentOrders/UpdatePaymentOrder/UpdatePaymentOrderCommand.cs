using ERP_Government.Application.Common.Security;
using ERP_Government.Domain.Payments.Entities;
using ERP_Government.Domain.Payments.Enums;

namespace ERP_Government.Application.Payments.Commands.PaymentOrders.UpdatePaymentOrder;

[Authorize(Policy = PermissionCodes.PaymentOrdersUpdate)]
public class UpdatePaymentOrderCommand : IRequest<Result>
{
    public int Id { get; init; }
    public byte[] RowVersion { get; init; } = [];
    public DateTime PaymentOrderDate { get; init; }
    public DateTime? DueDate { get; init; }
    public string PaymentOrderType { get; init; } = string.Empty;
    public int VendorId { get; init; }
    public int FundId { get; init; }
    public int FiscalYearId { get; init; }
    public int AppropriationId { get; init; }
    public int? BudgetClassificationId { get; init; }
    public int? CostCenterId { get; init; }
    public int? ProjectId { get; init; }
    public int? PurchaseOrderId { get; init; }
    public int? EncumbranceId { get; init; }
    public int CurrencyId { get; init; }
    public decimal? ExchangeRate { get; init; }
    public decimal AmountGross { get; init; }
    public decimal DeductionAmount { get; init; }
    public ERP_Government.Domain.Payments.Enums.PaymentMethod PaymentMethod { get; init; }
    public int? BankAccountId { get; init; }
    public string BeneficiaryName { get; init; } = string.Empty;
    public string? BeneficiaryIban { get; init; }
    public string? BeneficiaryAccountNumber { get; init; }
    public string? BeneficiaryBankName { get; init; }
    public string? Notes { get; init; }
    public List<UpdatePaymentOrderLineDto> Lines { get; init; } = [];
    public List<UpdatePaymentOrderDeductionDto> Deductions { get; init; } = [];
}

public class UpdatePaymentOrderLineDto
{
    public PaymentOrderLineType LineType { get; init; }
    public string? Description { get; init; }
    public int AccountId { get; init; }
    public decimal Amount { get; init; }
    public decimal? TaxAmount { get; init; }
    public int? FundId { get; init; }
    public int? AppropriationId { get; init; }
    public int? OrganizationUnitId { get; init; }
    public int? CostCenterId { get; init; }
    public int? ProjectId { get; init; }
}

public class UpdatePaymentOrderDeductionDto
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

public class UpdatePaymentOrderCommandHandler(IApplicationDbContext context)
    : IRequestHandler<UpdatePaymentOrderCommand, Result>
{
    public async Task<Result> Handle(
        UpdatePaymentOrderCommand request,
        CancellationToken cancellationToken)
    {
        var entity = await context.PaymentOrders
            .FindAsync(request.Id, cancellationToken);

        if (entity is null)
            return Result.Failure(["Payment order not found."]);

        // BR-2: only Draft payment orders can be edited.
        if (entity.Status != PaymentOrderStatus.Draft)
            return Result.Failure(["Only draft payment orders can be updated."]);

        // Concurrency: reject stale updates.
        if (entity.RowVersion.Length > 0 && request.RowVersion.Length > 0
            && !entity.RowVersion.SequenceEqual(request.RowVersion))
            return Result.Failure(["RowVersion conflict — record modified by another user. Reload."]);

        // Validate Party (vendor) exists and is active
        var vendor = await context.Parties.FindAsync(request.VendorId, cancellationToken);
        if (vendor is null || !vendor.IsActive || vendor.PartyType != ERP_Government.Domain.Parties.Enums.PartyType.Supplier)
            return Result.Failure(["Invalid or inactive supplier."]);

        // Validate Fund exists and is active
        var fund = await context.Funds.FindAsync(request.FundId, cancellationToken);
        if (fund is null || !fund.IsActive)
            return Result.Failure(["Invalid or inactive fund."]);

        // Validate FiscalYear is open
        var fiscalYear = await context.FiscalYears.FindAsync(request.FiscalYearId, cancellationToken);
        if (fiscalYear is null || fiscalYear.Status == ERP_Government.Domain.FinancialSettings.Enums.FiscalYearStatus.HardClosed)
            return Result.Failure(["Invalid or closed fiscal year."]);

        // Validate Appropriation exists
        var appropriation = await context.Appropriations.FindAsync(request.AppropriationId, cancellationToken);
        if (appropriation is null)
            return Result.Failure(["Invalid appropriation."]);

        // Validate BankAccountId (if payment method requires bank)
        if (request.PaymentMethod == ERP_Government.Domain.Payments.Enums.PaymentMethod.BankTransfer && !request.BankAccountId.HasValue)
            return Result.Failure(["Bank account is required for bank transfer payment method."]);

        if (request.BankAccountId.HasValue)
        {
            var bankAccount = await context.BankAccounts.FindAsync(request.BankAccountId.Value, cancellationToken);
            if (bankAccount is null || !bankAccount.IsActive)
                return Result.Failure(["Invalid or inactive bank account."]);
        }

        // Validate BeneficiaryName
        if (string.IsNullOrWhiteSpace(request.BeneficiaryName))
            return Result.Failure(["Beneficiary name is required."]);

        // Validate amounts
        if (request.AmountGross <= 0)
            return Result.Failure(["Gross amount must be greater than zero."]);

        if (request.DeductionAmount < 0)
            return Result.Failure(["Deduction amount cannot be negative."]);

        // AmountNet = AmountGross - DeductionAmount
        var amountNet = request.AmountGross - request.DeductionAmount;

        // Validate lines sum
        var linesSum = request.Lines.Sum(l => l.Amount);
        if (Math.Abs(linesSum - amountNet) > 0.01m)
            return Result.Failure(["Lines must sum to the net amount."]);

        // Validate deductions sum
        var deductionsSum = request.Deductions.Sum(d => d.Amount);
        if (Math.Abs(deductionsSum - request.DeductionAmount) > 0.01m)
            return Result.Failure(["Deductions must sum to the deduction amount."]);

        // BR-4: mandatory deductions cannot be removed on update.
        var existingDeductions = await context.PaymentOrderDeductions
            .Where(d => d.PaymentOrderId == entity.Id)
            .ToListAsync(cancellationToken);

        foreach (var mandatory in existingDeductions.Where(d => d.IsMandatory))
        {
            var stillPresent = request.Deductions.Any(d =>
                d.DeductionType == mandatory.DeductionType
                && d.IsMandatory
                && string.Equals(d.DeductionCode, mandatory.DeductionCode, StringComparison.OrdinalIgnoreCase));
            if (!stillPresent)
                return Result.Failure(
                    [$"Mandatory deduction ({mandatory.DeductionType}) cannot be removed."]);
        }

        // Validate tax deductions (BR-037)
        foreach (var deduction in request.Deductions)
        {
            if (deduction.IsTaxDeduction && !deduction.TaxAuthorityId.HasValue)
                return Result.Failure(["Tax authority is required for tax deductions."]);
        }

        // Header
        entity.PaymentOrderDate = DateOnly.FromDateTime(request.PaymentOrderDate);
        entity.DueDate = request.DueDate.HasValue ? DateOnly.FromDateTime(request.DueDate.Value) : null;
        entity.PaymentOrderType = request.PaymentOrderType;
        entity.VendorId = request.VendorId;
        entity.FundId = request.FundId;
        entity.FiscalYearId = request.FiscalYearId;
        entity.AppropriationId = request.AppropriationId;
        entity.BudgetClassificationId = request.BudgetClassificationId;
        entity.CostCenterId = request.CostCenterId;
        entity.ProjectId = request.ProjectId;
        entity.PurchaseOrderId = request.PurchaseOrderId;
        entity.EncumbranceId = request.EncumbranceId;
        entity.CurrencyId = request.CurrencyId;
        entity.ExchangeRate = request.ExchangeRate;
        entity.AmountGross = request.AmountGross;
        entity.DeductionAmount = request.DeductionAmount;
        entity.PaymentMethod = request.PaymentMethod;
        entity.BankAccountId = request.BankAccountId;
        entity.BeneficiaryName = request.BeneficiaryName;
        entity.BeneficiaryIban = request.BeneficiaryIban;
        entity.BeneficiaryAccountNumber = request.BeneficiaryAccountNumber;
        entity.BeneficiaryBankName = request.BeneficiaryBankName;
        entity.Notes = request.Notes;

        // Replace lines
        var existingLines = await context.PaymentOrderLines
            .Where(l => l.PaymentOrderId == entity.Id)
            .ToListAsync(cancellationToken);
        context.PaymentOrderLines.RemoveRange(existingLines);
        int lineNumber = 1;
        foreach (var line in request.Lines)
        {
            context.PaymentOrderLines.Add(new PaymentOrderLine
            {
                PaymentOrderId = entity.Id,
                LineNumber = lineNumber++,
                LineType = line.LineType,
                Description = line.Description,
                AccountId = line.AccountId,
                Amount = line.Amount,
                TaxAmount = line.TaxAmount,
                FundId = line.FundId,
                AppropriationId = line.AppropriationId,
                OrganizationUnitId = line.OrganizationUnitId,
                CostCenterId = line.CostCenterId,
                ProjectId = line.ProjectId
            });
        }

        // Replace deductions
        context.PaymentOrderDeductions.RemoveRange(existingDeductions);

        int deductionLine = 1;
        foreach (var deduction in request.Deductions)
        {
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

        return Result.Success();
    }
}

public class UpdatePaymentOrderCommandValidator : AbstractValidator<UpdatePaymentOrderCommand>
{
    public UpdatePaymentOrderCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Invalid payment order ID.");

        RuleFor(x => x.PaymentOrderDate)
            .NotEmpty().WithMessage("Payment order date is required.");

        RuleFor(x => x.PaymentOrderType)
            .NotEmpty().WithMessage("Payment order type is required.")
            .MaximumLength(20).WithMessage("Type must not exceed 20 characters.");

        RuleFor(x => x.VendorId)
            .GreaterThan(0).WithMessage("Vendor is required.");

        RuleFor(x => x.FundId)
            .GreaterThan(0).WithMessage("Fund is required.");

        RuleFor(x => x.FiscalYearId)
            .GreaterThan(0).WithMessage("Fiscal year is required.");

        RuleFor(x => x.AppropriationId)
            .GreaterThan(0).WithMessage("Appropriation is required.");

        RuleFor(x => x.CurrencyId)
            .GreaterThan(0).WithMessage("Currency is required.");

        RuleFor(x => x.BeneficiaryName)
            .NotEmpty().WithMessage("Beneficiary name is required.")
            .MaximumLength(200).WithMessage("Beneficiary name must not exceed 200 characters.");

        RuleFor(x => x.AmountGross)
            .GreaterThan(0).WithMessage("Gross amount must be greater than zero.");
    }
}
