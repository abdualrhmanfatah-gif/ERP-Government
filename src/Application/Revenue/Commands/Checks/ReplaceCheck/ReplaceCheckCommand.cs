using ERP_Government.Application.Common.Security;
using ERP_Government.Application.FinancialSettings.Common.Services;
using ERP_Government.Application.Revenue.Common.DTOs;
using ERP_Government.Domain.Revenue.Entities;
using ERP_Government.Domain.Revenue.Enums;
using ERP_Government.Domain.Security.Entities;

namespace ERP_Government.Application.Revenue.Commands.Checks.ReplaceCheck;

[Authorize(Policy = PermissionCodes.ChecksReplace)]
public class ReplaceCheckCommand : IRequest<Result>
{
    public int CheckId { get; init; }
    public PaymentMethod PaymentMethod { get; init; }
    public DateOnly VoucherDate { get; init; }
    public CreateCheckDto? CheckDetails { get; init; }
    public byte[] RowVersion { get; init; } = [];
}

public class ReplaceCheckCommandHandler(
    IApplicationDbContext context,
    IDocumentSequenceService sequenceService,
    IUser user) : IRequestHandler<ReplaceCheckCommand, Result>
{
    public async Task<Result> Handle(
        ReplaceCheckCommand request,
        CancellationToken cancellationToken)
    {
        var originalCheck = await context.Checks
            .Include(c => c.ReceiptVoucher)
            .FirstOrDefaultAsync(c => c.Id == request.CheckId, cancellationToken);
        if (originalCheck is null)
            return Result.Failure(new[] { "Original check not found." });

        if (originalCheck.Status != CheckStatus.Bounced)
            return Result.Failure(new[] { "Only bounced checks can be replaced." });

        if (originalCheck.ReplacementVoucherId.HasValue)
            return Result.Failure(new[] { "This check has already been replaced. Double replace is not allowed." });

        if (originalCheck.ReceiptVoucher is null)
            return Result.Failure(new[] { "Source voucher not found." });

        if (originalCheck.ReceiptVoucher.Status == ReceiptVoucherStatus.Cancelled)
            return Result.Failure(new[] { "Cannot replace a check whose source voucher is cancelled." });

        if (request.PaymentMethod == PaymentMethod.Check && request.CheckDetails is null)
            return Result.Failure(new[] { "Check details are required for check payments." });

        var sourceVoucher = originalCheck.ReceiptVoucher;
        var voucherNumber = await sequenceService.GenerateNextNumberAsync("ReceiptVoucher", cancellationToken);

        var replacementVoucher = new ReceiptVoucher
        {
            VoucherNumber = voucherNumber,
            VoucherDate = request.VoucherDate,
            PartyId = sourceVoucher.PartyId,
            PaymentMethod = request.PaymentMethod,
            ReceivedFrom = sourceVoucher.ReceivedFrom,
            Notes = $"Replacement for bounced check {originalCheck.CheckNumber}",
            Status = ReceiptVoucherStatus.Draft
        };

        context.ReceiptVouchers.Add(replacementVoucher);
        await context.SaveChangesAsync(cancellationToken);

        originalCheck.ReplacementVoucherId = replacementVoucher.Id;
        originalCheck.RowVersion = request.RowVersion;

        if (user.Id is int userId)
        {
            context.DocumentStatusLogs.Add(new DocumentStatusLog
            {
                EntityName = nameof(Check),
                DocumentId = originalCheck.Id,
                FromStatus = CheckStatus.Bounced.ToString(),
                ToStatus = "Replaced",
                ChangedById = userId,
                ChangedAt = DateTimeOffset.UtcNow,
                Reason = $"Replaced with voucher {replacementVoucher.VoucherNumber}"
            });
        }

        if (request.PaymentMethod == PaymentMethod.Check && request.CheckDetails is not null)
        {
            var newCheck = new Check
            {
                ReceiptVoucherId = replacementVoucher.Id,
                BankName = request.CheckDetails.BankName,
                CheckNumber = request.CheckDetails.CheckNumber,
                CheckDate = request.CheckDetails.CheckDate,
                Amount = originalCheck.Amount,
                Status = CheckStatus.UnderCollection
            };

            context.Checks.Add(newCheck);
        }

        try
        {
            await context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            return Result.Failure(new[] { "Check was modified by another user. Please refresh and try again." });
        }

        return Result.Success();
    }
}

public class ReplaceCheckCommandValidator : AbstractValidator<ReplaceCheckCommand>
{
    public ReplaceCheckCommandValidator()
    {
        RuleFor(x => x.CheckId)
            .GreaterThan(0).WithMessage("Check ID is required.");

        RuleFor(x => x.PaymentMethod)
            .IsInEnum().WithMessage("Invalid payment method.");

        RuleFor(x => x.VoucherDate)
            .NotEmpty().WithMessage("Voucher date is required.");

        RuleFor(x => x.CheckDetails)
            .NotNull().WithMessage("Check details are required for check payments.")
            .When(x => x.PaymentMethod == PaymentMethod.Check);

        RuleFor(x => x.RowVersion)
            .NotEmpty().WithMessage("Row version is required for concurrency control.");
    }
}
