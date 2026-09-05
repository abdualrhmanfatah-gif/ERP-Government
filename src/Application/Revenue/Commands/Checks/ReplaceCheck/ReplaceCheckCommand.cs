using ERP_Government.Application.Common.Security;
using ERP_Government.Application.Revenue.Common.DTOs;
using ERP_Government.Domain.Revenue.Entities;
using ERP_Government.Domain.Revenue.Enums;

namespace ERP_Government.Application.Revenue.Commands.Checks.ReplaceCheck;

[Authorize(Policy = PermissionCodes.ChecksClear)]
public class ReplaceCheckCommand : IRequest<Result>
{
    public int CheckId { get; init; }
    public PaymentMethod PaymentMethod { get; init; }
    public CreateCheckDto? CheckDetails { get; init; }
    public byte[] RowVersion { get; init; } = [];
}

public class ReplaceCheckCommandHandler(
    IApplicationDbContext context) : IRequestHandler<ReplaceCheckCommand, Result>
{
    public async Task<Result> Handle(
        ReplaceCheckCommand request,
        CancellationToken cancellationToken)
    {
        var originalCheck = await context.Checks.FindAsync(request.CheckId, cancellationToken);
        if (originalCheck is null)
            return Result.Failure(new[] { "Original check not found."});

        if (originalCheck.Status != CheckStatus.Bounced)
            return Result.Failure(new[] { "Only bounced checks can be replaced."});

        if (!originalCheck.ReplacementVoucherId.HasValue)
            return Result.Failure(new[] { "No replacement voucher found for this check."});

        var replacementVoucher = await context.ReceiptVouchers
            .FirstOrDefaultAsync(v => v.Id == originalCheck.ReplacementVoucherId.Value, cancellationToken);

        if (replacementVoucher is null)
            return Result.Failure(new[] { "Replacement voucher not found."});

        if (request.PaymentMethod == PaymentMethod.Check)
        {
            if (request.CheckDetails is null)
                return Result.Failure(new[] { "Check details are required for check payments."});

            var newCheck = new Check
            {
                ReceiptVoucherId = replacementVoucher.Id,
                BankName = request.CheckDetails.BankName,
                CheckNumber = request.CheckDetails.CheckNumber,
                CheckDate = request.CheckDetails.CheckDate,
                Amount = request.CheckDetails.Amount,
                Status = CheckStatus.UnderCollection
            };

            context.Checks.Add(newCheck);
        }

        replacementVoucher.PaymentMethod = request.PaymentMethod;
        replacementVoucher.RowVersion = request.RowVersion;

        try
        {
            await context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            return Result.Failure(new[] { "Voucher was modified by another user. Please refresh and try again."});
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

        RuleFor(x => x.CheckDetails)
            .NotNull().WithMessage("Check details are required for check payments.")
            .When(x => x.PaymentMethod == PaymentMethod.Check);

        RuleFor(x => x.RowVersion)
            .NotEmpty().WithMessage("Row version is required for concurrency control.");
    }
}
