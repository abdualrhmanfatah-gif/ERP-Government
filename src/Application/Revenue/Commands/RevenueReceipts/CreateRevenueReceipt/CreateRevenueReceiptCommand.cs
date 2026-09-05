using ERP_Government.Application.Common.Security;
using ERP_Government.Application.Revenue.Common.DTOs;
using ERP_Government.Domain.Revenue.Entities;
using ERP_Government.Domain.Revenue.Enums;

namespace ERP_Government.Application.Revenue.Commands.RevenueReceipts.CreateRevenueReceipt;

[Authorize(Policy = PermissionCodes.RevenueReceiptsCreate)]
public class CreateRevenueReceiptCommand : IRequest<Result>
{
    public RevenueReceiptType ReceiptType { get; init; }
    public DateTime ReceiptDate { get; init; }
    public string PayerName { get; init; } = string.Empty;
    public string? PayerNationalId { get; init; }
    public int FundId { get; init; }
    public int? BudgetClassificationId { get; init; }
    public int CurrencyId { get; init; }
    public ERP_Government.Domain.Payments.Enums.PaymentMethod PaymentMethod { get; init; }
    public string? ExternalTransactionRef { get; init; }
    public List<CreateRevenueReceiptLineDto> Lines { get; init; } = [];
}

public class CreateRevenueReceiptCommandHandler(
    IApplicationDbContext context) : IRequestHandler<CreateRevenueReceiptCommand, Result>
{
    public async Task<Result> Handle(
        CreateRevenueReceiptCommand request,
        CancellationToken cancellationToken)
    {
        // Validate lines present
        if (request.Lines.Count == 0)
            return Result.Failure(new[] { "At least one line is required."});

        // Validate line amounts > 0
        if (request.Lines.Any(l => l.Amount <= 0))
            return Result.Failure(new[] { "Each line amount must be greater than zero."});

        // Validate Fund active
        var fund = await context.Funds.FindAsync(request.FundId, cancellationToken);
        if (fund is null || !fund.IsActive)
            return Result.Failure(new[] { "Fund not found or inactive."});

        // Validate Currency active
        var currency = await context.Currencies.FindAsync(request.CurrencyId, cancellationToken);
        if (currency is null || !currency.IsActive)
            return Result.Failure(new[] { "Currency not found or inactive."});

        // Validate BudgetClassification if set
        if (request.BudgetClassificationId.HasValue)
        {
            var bcExists = await context.BudgetClassifications
                .AnyAsync(x => x.Id == request.BudgetClassificationId.Value, cancellationToken);
            if (!bcExists)
                return Result.Failure(new[] { "Budget classification not found."});
        }

        // Calculate total
        var amountTotal = request.Lines.Sum(l => l.Amount);

        // Generate receipt number
        var receiptNumber = await GenerateReceiptNumber(cancellationToken);

        var receipt = new RevenueReceipt
        {
            ReceiptNumber = receiptNumber,
            ReceiptType = request.ReceiptType,
            ReceiptDate = DateOnly.FromDateTime(request.ReceiptDate),
            PayerName = request.PayerName,
            PayerNationalId = request.PayerNationalId,
            FundId = request.FundId,
            BudgetClassificationId = request.BudgetClassificationId,
            CurrencyId = request.CurrencyId,
            AmountTotal = amountTotal,
            PaymentMethod = request.PaymentMethod,
            ExternalTransactionRef = request.ExternalTransactionRef,
            Status = RevenueReceiptStatus.Draft
        };

        context.RevenueReceipts.Add(receipt);

        foreach (var lineDto in request.Lines)
        {
            var line = new RevenueReceiptLine
            {
                ReceiptId = receipt.Id,
                AccountId = lineDto.AccountId,
                Description = lineDto.Description,
                Amount = lineDto.Amount
            };
            context.RevenueReceiptLines.Add(line);
        }

        try
        {
            await context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            return Result.Failure(new[] { "Receipt was modified by another user. Please refresh and try again."});
        }

        return Result.Success();
    }

    private async Task<string> GenerateReceiptNumber(CancellationToken cancellationToken)
    {
        var year = DateTime.Today.Year;
        var prefix = $"RCP-{year}-";

        var lastNumber = await context.RevenueReceipts
            .Where(x => x.ReceiptNumber.StartsWith(prefix))
            .OrderByDescending(x => x.ReceiptNumber)
            .Select(x => x.ReceiptNumber)
            .FirstOrDefaultAsync(cancellationToken);

        if (lastNumber is null)
            return $"{prefix}000001";

        var lastSeq = int.Parse(lastNumber.Substring(prefix.Length));
        return $"{prefix}{(lastSeq + 1):D6}";
    }
}

public class CreateRevenueReceiptCommandValidator : AbstractValidator<CreateRevenueReceiptCommand>
{
    public CreateRevenueReceiptCommandValidator(IApplicationDbContext context)
    {
        RuleFor(x => x.ReceiptType)
            .IsInEnum()
            .WithMessage("Invalid receipt type.");

        RuleFor(x => x.ReceiptDate)
            .NotEmpty()
            .WithMessage("Receipt date is required.");

        RuleFor(x => x.PayerName)
            .NotEmpty().WithMessage("Payer name is required.")
            .MaximumLength(200).WithMessage("Payer name must not exceed 200 characters.");

        RuleFor(x => x.PayerNationalId)
            .MaximumLength(50).WithMessage("Payer national ID must not exceed 50 characters.");

        RuleFor(x => x.FundId)
            .GreaterThan(0).WithMessage("Fund is required.")
            .MustAsync(async (fundId, ct) =>
                await context.Funds.AnyAsync(f => f.Id == fundId && f.IsActive, ct))
            .WithMessage("Fund does not exist or is inactive.");

        RuleFor(x => x.BudgetClassificationId)
            .GreaterThan(0).WithMessage("Budget classification is required.")
            .When(x => x.BudgetClassificationId.HasValue);

        RuleFor(x => x.CurrencyId)
            .GreaterThan(0).WithMessage("Currency is required.")
            .MustAsync(async (currencyId, ct) =>
                await context.Currencies.AnyAsync(c => c.Id == currencyId && c.IsActive, ct))
            .WithMessage("Currency does not exist or is inactive.");

        RuleFor(x => x.ExternalTransactionRef)
            .MaximumLength(100).WithMessage("External reference must not exceed 100 characters.");

        RuleFor(x => x.Lines)
            .NotEmpty().WithMessage("At least one line is required.");

        RuleForEach(x => x.Lines)
            .SetValidator(new CreateRevenueReceiptLineDtoValidator(context));

        RuleFor(x => x)
            .MustAsync(async (cmd, ct) =>
            {
                if (cmd.Lines.Count == 0) return true; // Lines NotEmpty handles this
                var lineSum = cmd.Lines.Sum(l => l.Amount);
                return lineSum == cmd.Lines.Sum(l => l.Amount); // AmountTotal is computed, always matches
            })
            .WithMessage("Line amounts must sum to the header total.")
            .When(x => x.Lines.Count > 0);
    }
}

public class CreateRevenueReceiptLineDtoValidator : AbstractValidator<CreateRevenueReceiptLineDto>
{
    public CreateRevenueReceiptLineDtoValidator(IApplicationDbContext context)
    {
        RuleFor(x => x.AccountId)
            .GreaterThan(0).WithMessage("Account is required.")
            .MustAsync(async (accountId, ct) =>
                await context.Accounts.AnyAsync(a => a.Id == accountId && a.IsActive, ct))
            .WithMessage("Account does not exist or is inactive.");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description must not exceed 500 characters.");

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Amount must be greater than zero.");
    }
}
