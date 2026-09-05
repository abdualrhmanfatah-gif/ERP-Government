using ERP_Government.Application.Common.Security;
using ERP_Government.Application.FinancialSettings.Common.Services;
using ERP_Government.Application.Revenue.Common.DTOs;
using ERP_Government.Domain.Revenue.Entities;
using ERP_Government.Domain.Revenue.Enums;
using ERP_Government.Domain.Security.Entities;

namespace ERP_Government.Application.Revenue.Commands.ReceiptVouchers.CreateReceiptVoucher;

[Authorize(Policy = PermissionCodes.ReceiptVouchersCreate)]
public class CreateReceiptVoucherCommand : IRequest<Result<int>>
{
    public DateOnly VoucherDate { get; init; }
    public int PartyId { get; init; }
    public PaymentMethod PaymentMethod { get; init; }
    public string ReceivedFrom { get; init; } = string.Empty;
    public string? Notes { get; init; }
    public List<CreateReceiptVoucherLineDto> Lines { get; init; } = [];
    public List<CreateCheckDto> Checks { get; init; } = [];
}

public class CreateReceiptVoucherCommandHandler(
    IApplicationDbContext context,
    IDocumentSequenceService sequenceService,
    IUser user) : IRequestHandler<CreateReceiptVoucherCommand, Result<int>>
{
    public async Task<Result<int>> Handle(
        CreateReceiptVoucherCommand request,
        CancellationToken cancellationToken)
    {
        if (user.Id is not int userId)
            return Result<int>.Failure(new[] { "User identity is required for this operation." });

        if (request.Lines.Count == 0)
            return Result<int>.Failure(new[] { "At least one line is required." });

        if (request.Lines.Any(l => l.Amount <= 0))
            return Result<int>.Failure(new[] { "Each line amount must be greater than zero." });

        var party = await context.Parties.FindAsync(request.PartyId, cancellationToken);
        if (party is null || !party.IsActive)
            return Result<int>.Failure(new[] { "Party not found or inactive." });

        if (request.PaymentMethod == PaymentMethod.Check && request.Checks.Count == 0)
            return Result<int>.Failure(new[] { "At least one check is required for check payments." });

        if (request.PaymentMethod == PaymentMethod.Cash && request.Checks.Count > 0)
            return Result<int>.Failure(new[] { "Check details are not allowed for cash payments." });

        if (request.Checks.Count > 0)
        {
            foreach (var check in request.Checks)
            {
                if (check.Amount <= 0)
                    return Result<int>.Failure(new[] { "Check amount must be greater than zero." });
                if (string.IsNullOrWhiteSpace(check.BankName))
                    return Result<int>.Failure(new[] { "Bank name is required for checks." });
                if (string.IsNullOrWhiteSpace(check.CheckNumber))
                    return Result<int>.Failure(new[] { "Check number is required." });
            }
        }

        var voucherNumber = await sequenceService.GenerateNextNumberAsync("ReceiptVoucher", cancellationToken);

        var voucher = new ReceiptVoucher
        {
            VoucherNumber = voucherNumber,
            VoucherDate = request.VoucherDate,
            PartyId = request.PartyId,
            PaymentMethod = request.PaymentMethod,
            ReceivedFrom = request.ReceivedFrom,
            Notes = request.Notes,
            Status = ReceiptVoucherStatus.Draft
        };

        context.ReceiptVouchers.Add(voucher);
        await context.SaveChangesAsync(cancellationToken);

        foreach (var lineDto in request.Lines)
        {
            var line = new ReceiptVoucherLine
            {
                ReceiptVoucherId = voucher.Id,
                RevenueAccountId = lineDto.RevenueAccountId,
                Amount = lineDto.Amount,
                Description = lineDto.Description
            };
            context.ReceiptVoucherLines.Add(line);
        }

        foreach (var checkDto in request.Checks)
        {
            var check = new Check
            {
                ReceiptVoucherId = voucher.Id,
                BankName = checkDto.BankName,
                CheckNumber = checkDto.CheckNumber,
                CheckDate = checkDto.CheckDate,
                Amount = checkDto.Amount,
                Status = CheckStatus.UnderCollection
            };
            context.Checks.Add(check);
        }

        context.DocumentStatusLogs.Add(new DocumentStatusLog
        {
            EntityName = nameof(ReceiptVoucher),
            DocumentId = voucher.Id,
            FromStatus = null!,
            ToStatus = ReceiptVoucherStatus.Draft.ToString(),
            ChangedById = userId,
            ChangedAt = DateTimeOffset.UtcNow
        });

        try
        {
            await context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            return Result<int>.Failure(new[] { "Voucher was modified by another user. Please refresh and try again." });
        }

        return Result<int>.Success(voucher.Id);
    }
}

public class CreateReceiptVoucherCommandValidator : AbstractValidator<CreateReceiptVoucherCommand>
{
    public CreateReceiptVoucherCommandValidator(IApplicationDbContext context)
    {
        RuleFor(x => x.VoucherDate)
            .NotEmpty().WithMessage("Voucher date is required.");

        RuleFor(x => x.PartyId)
            .GreaterThan(0).WithMessage("Party is required.")
            .MustAsync(async (partyId, ct) =>
                await context.Parties.AnyAsync(p => p.Id == partyId && p.IsActive, ct))
            .WithMessage("Party does not exist or is inactive.");

        RuleFor(x => x.PaymentMethod)
            .IsInEnum().WithMessage("Invalid payment method.");

        RuleFor(x => x.ReceivedFrom)
            .NotEmpty().WithMessage("Received from is required.")
            .MaximumLength(200).WithMessage("Received from must not exceed 200 characters.");

        RuleFor(x => x.Notes)
            .MaximumLength(500).WithMessage("Notes must not exceed 500 characters.");

        RuleFor(x => x.Lines)
            .NotEmpty().WithMessage("At least one line is required.");

        RuleForEach(x => x.Lines)
            .SetValidator(new CreateReceiptVoucherLineDtoValidator(context));

        RuleFor(x => x.Checks)
            .NotEmpty().WithMessage("At least one check is required for check payments.")
            .When(x => x.PaymentMethod == PaymentMethod.Check);

        RuleForEach(x => x.Checks)
            .SetValidator(new CreateCheckDtoValidator())
            .When(x => x.Checks.Count > 0);
    }
}

public class CreateReceiptVoucherLineDtoValidator : AbstractValidator<CreateReceiptVoucherLineDto>
{
    public CreateReceiptVoucherLineDtoValidator(IApplicationDbContext context)
    {
        RuleFor(x => x.RevenueAccountId)
            .GreaterThan(0).WithMessage("Revenue account is required.")
            .MustAsync(async (accountId, ct) =>
                await context.Accounts.AnyAsync(a => a.Id == accountId && a.IsActive, ct))
            .WithMessage("Revenue account does not exist or is inactive.");

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Amount must be greater than zero.");

        RuleFor(x => x.Description)
            .MaximumLength(200).WithMessage("Description must not exceed 200 characters.");
    }
}

public class CreateCheckDtoValidator : AbstractValidator<CreateCheckDto>
{
    public CreateCheckDtoValidator()
    {
        RuleFor(x => x.BankName)
            .NotEmpty().WithMessage("Bank name is required.")
            .MaximumLength(100).WithMessage("Bank name must not exceed 100 characters.");

        RuleFor(x => x.CheckNumber)
            .NotEmpty().WithMessage("Check number is required.")
            .MaximumLength(50).WithMessage("Check number must not exceed 50 characters.");

        RuleFor(x => x.CheckDate)
            .NotEmpty().WithMessage("Check date is required.");

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Check amount must be greater than zero.");
    }
}
