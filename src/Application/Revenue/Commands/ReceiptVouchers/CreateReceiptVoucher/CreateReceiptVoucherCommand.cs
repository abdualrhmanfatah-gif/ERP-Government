using ERP_Government.Application.Common.Security;
using ERP_Government.Application.FinancialSettings.Common.Services;
using ERP_Government.Application.Revenue.Common.DTOs;
using ERP_Government.Domain.Revenue.Entities;
using ERP_Government.Domain.Revenue.Enums;
using ERP_Government.Domain.Security.Entities;

namespace ERP_Government.Application.Revenue.Commands.ReceiptVouchers.CreateReceiptVoucher;

[Authorize(Policy = PermissionCodes.ReceiptVouchersCreate)]
public class CreateReceiptVoucherCommand : IRequest<Result<ReceiptVoucherDto>>
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
    IUser user) : IRequestHandler<CreateReceiptVoucherCommand, Result<ReceiptVoucherDto>>
{
    public async Task<Result<ReceiptVoucherDto>> Handle(
        CreateReceiptVoucherCommand request,
        CancellationToken cancellationToken)
    {
        if (user.Id is not int userId)
            return Result<ReceiptVoucherDto>.Failure(new[] { "User identity is required for this operation." });

        if (request.Lines.Count == 0)
            return Result<ReceiptVoucherDto>.Failure(new[] { "At least one line is required." });

        if (request.Lines.Any(l => l.Amount <= 0))
            return Result<ReceiptVoucherDto>.Failure(new[] { "Each line amount must be greater than zero." });

        var party = await context.Parties.FindAsync(request.PartyId, cancellationToken);
        if (party is null || !party.IsActive)
            return Result<ReceiptVoucherDto>.Failure(new[] { "Party not found or inactive." });

        if (request.PaymentMethod == PaymentMethod.Check && request.Checks.Count == 0)
            return Result<ReceiptVoucherDto>.Failure(new[] { "At least one check is required for check payments." });

        if (request.PaymentMethod == PaymentMethod.Cash && request.Checks.Count > 0)
            return Result<ReceiptVoucherDto>.Failure(new[] { "Check details are not allowed for cash payments." });

        var totalAmount = request.Lines.Sum(l => l.Amount);
        if (request.Checks.Sum(c => c.Amount) > totalAmount)
            return Result<ReceiptVoucherDto>.Failure(new[] { "The sum of check amounts exceeds the voucher total." });

        if (request.Checks.Count > 0)
        {
            foreach (var check in request.Checks)
            {
                if (check.Amount <= 0)
                    return Result<ReceiptVoucherDto>.Failure(new[] { "Check amount must be greater than zero." });
                if (string.IsNullOrWhiteSpace(check.BankName))
                    return Result<ReceiptVoucherDto>.Failure(new[] { "Bank name is required for checks." });
                if (string.IsNullOrWhiteSpace(check.CheckNumber))
                    return Result<ReceiptVoucherDto>.Failure(new[] { "Check number is required." });
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
            return Result<ReceiptVoucherDto>.Failure(new[] { "Voucher was modified by another user. Please refresh and try again." });
        }

        return Result<ReceiptVoucherDto>.Success(await MapVoucherAsync(voucher.Id, cancellationToken));
    }

    private async Task<ReceiptVoucherDto> MapVoucherAsync(int voucherId, CancellationToken cancellationToken)
    {
        var saved = await context.ReceiptVouchers
            .Include(v => v.Party)
            .Include(v => v.Lines)
            .Include(v => v.Checks)
            .FirstAsync(v => v.Id == voucherId, cancellationToken);

        return new ReceiptVoucherDto
        {
            Id = saved.Id,
            VoucherNumber = saved.VoucherNumber,
            VoucherDate = saved.VoucherDate,
            PartyId = saved.PartyId,
            PartyName = saved.Party?.NameAr ?? string.Empty,
            PaymentMethod = saved.PaymentMethod,
            PaymentMethodName = saved.PaymentMethod.ToString(),
            ReceivedFrom = saved.ReceivedFrom,
            Notes = saved.Notes,
            DepositSlipId = saved.DepositSlipId,
            DepositSlipNumber = saved.DepositSlip?.SlipNumber,
            Status = saved.Status,
            StatusName = saved.Status.ToString(),
            TotalAmount = saved.Lines.Sum(l => l.Amount),
            SubmittedById = saved.SubmittedById,
            SubmittedAt = saved.SubmittedAt,
            ReviewedById = saved.ReviewedById,
            ReviewedAt = saved.ReviewedAt,
            CancellationReason = saved.CancellationReason,
            RowVersion = saved.RowVersion,
            Created = saved.Created,
            CreatedBy = saved.CreatedBy,
            LastModified = saved.LastModified,
            LastModifiedBy = saved.LastModifiedBy,
            Lines = saved.Lines.Select(l => new ReceiptVoucherLineDto
            {
                Id = l.Id,
                ReceiptVoucherId = l.ReceiptVoucherId,
                RevenueAccountId = l.RevenueAccountId,
                Amount = l.Amount,
                Description = l.Description
            }).ToList(),
            Checks = saved.Checks.Select(c => new CheckDto
            {
                Id = c.Id,
                ReceiptVoucherId = c.ReceiptVoucherId,
                BankName = c.BankName,
                CheckNumber = c.CheckNumber,
                CheckDate = c.CheckDate,
                Amount = c.Amount,
                Status = c.Status,
                StatusName = c.Status.ToString(),
                ClearedAt = c.ClearedAt,
                BouncedAt = c.BouncedAt,
                ReplacementVoucherId = c.ReplacementVoucherId
            }).ToList()
        };
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
