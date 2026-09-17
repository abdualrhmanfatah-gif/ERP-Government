using ERP_Government.Application.Common.Security;
using ERP_Government.Domain.Revenue.Entities;
using ERP_Government.Domain.Revenue.Enums;
using ERP_Government.Domain.Security.Entities;

namespace ERP_Government.Application.Revenue.Commands.RevenueClaims.WriteOffRevenueClaim;

[Authorize(Policy = PermissionCodes.RevenueClaimsWriteOff)]
public class WriteOffRevenueClaimCommand : IRequest<Result>
{
    public int Id { get; init; }
    public string Reason { get; init; } = string.Empty;
    public byte[] RowVersion { get; init; } = [];
}

public class WriteOffRevenueClaimCommandHandler(
    IApplicationDbContext context,
    IUser user) : IRequestHandler<WriteOffRevenueClaimCommand, Result>
{
    public async Task<Result> Handle(
        WriteOffRevenueClaimCommand request,
        CancellationToken cancellationToken)
    {
        if (user.Id is not int userId)
            return Result.Failure(["User identity is required."]);

        var claim = await context.RevenueClaims.FindAsync(request.Id, cancellationToken);
        if (claim is null)
            return Result.Failure(["Revenue claim not found."]);

        if (claim.Status == ClaimStatus.Settled || claim.Status == ClaimStatus.WrittenOff)
            return Result.Failure(["Settled or already WrittenOff claims cannot be written off."]);

        var previousStatus = claim.Status;
        claim.Status = ClaimStatus.WrittenOff;
        claim.RowVersion = request.RowVersion;
        claim.LastModified = DateTimeOffset.UtcNow;
        claim.LastModifiedBy = userId.ToString();

        context.DocumentStatusLogs.Add(new DocumentStatusLog
        {
            EntityName = nameof(RevenueClaim),
            DocumentId = claim.Id,
            FromStatus = previousStatus.ToString(),
            ToStatus = ClaimStatus.WrittenOff.ToString(),
            ChangedById = userId,
            ChangedAt = DateTimeOffset.UtcNow,
            Reason = request.Reason
        });

        await context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

public class WriteOffRevenueClaimCommandValidator : AbstractValidator<WriteOffRevenueClaimCommand>
{
    public WriteOffRevenueClaimCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0).WithMessage("Claim ID is required.");
        RuleFor(x => x.Reason).NotEmpty().WithMessage("Write-off reason is required.");
        RuleFor(x => x.RowVersion).NotEmpty().WithMessage("Row version is required for concurrency control.");
    }
}
