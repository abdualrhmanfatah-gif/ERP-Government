using ERP_Government.Application.Common.Security;
using ERP_Government.Application.Revenue.Common;
using ERP_Government.Domain.Revenue.Entities;
using ERP_Government.Domain.Revenue.Enums;
using ERP_Government.Domain.Security.Entities;

namespace ERP_Government.Application.Revenue.Commands.Checks.BounceCheck;

[Authorize(Policy = PermissionCodes.ChecksBounce)]
public class BounceCheckCommand : IRequest<Result<int>>
{
    public int Id { get; init; }
    public DateTimeOffset BouncedAt { get; init; }
    public string? Reason { get; init; }
    public byte[] RowVersion { get; init; } = [];
}

public class BounceCheckCommandHandler(
    IApplicationDbContext context,
    IUser user) : IRequestHandler<BounceCheckCommand, Result<int>>
{
    public async Task<Result<int>> Handle(
        BounceCheckCommand request,
        CancellationToken cancellationToken)
    {
        if (user.Id is not int userId)
            return Result<int>.Failure(new[] { "User identity is required for this operation." });

        var check = await context.Checks
            .Include(c => c.ReceiptVoucher)
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

        if (check is null)
            return Result<int>.Failure(new[] { "Check not found." });

        if (check.Status != CheckStatus.UnderCollection)
            return Result<int>.Failure(new[] { "Only Under-Collection checks can be bounced." });

        if (check.ReceiptVoucher is null)
            return Result<int>.Failure(new[] { "Source voucher not found." });

        if (check.ReceiptVoucher.Status == ReceiptVoucherStatus.Cancelled)
            return Result<int>.Failure(new[] { "Cannot bounce a check whose source voucher is cancelled." });

        var (dateValid, dateError) = CheckDateValidator.ValidateBouncedAt(check.CheckDate, request.BouncedAt);
        if (!dateValid)
            return Result<int>.Failure(new[] { dateError! });

        check.Status = CheckStatus.Bounced;
        check.BouncedAt = request.BouncedAt;
        check.RowVersion = request.RowVersion;

        context.DocumentStatusLogs.Add(new DocumentStatusLog
        {
            EntityName = nameof(Check),
            DocumentId = check.Id,
            FromStatus = CheckStatus.UnderCollection.ToString(),
            ToStatus = CheckStatus.Bounced.ToString(),
            ChangedById = userId,
            ChangedAt = DateTimeOffset.UtcNow,
            Reason = request.Reason
        });

        try
        {
            await context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            return Result<int>.Failure(new[] { "Check was modified by another user. Please refresh and try again." });
        }

        return Result<int>.Success(check.Id);
    }
}

public class BounceCheckCommandValidator : AbstractValidator<BounceCheckCommand>
{
    public BounceCheckCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Check ID is required.");

        RuleFor(x => x.BouncedAt)
            .NotEmpty().WithMessage("Bounce date is required.");

        RuleFor(x => x.RowVersion)
            .NotEmpty().WithMessage("Row version is required for concurrency control.");
    }
}
