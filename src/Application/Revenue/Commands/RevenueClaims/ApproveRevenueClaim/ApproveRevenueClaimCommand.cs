using ERP_Government.Application.Common.Security;
using ERP_Government.Domain.Revenue.Entities;
using ERP_Government.Domain.Revenue.Enums;
using ERP_Government.Domain.Security.Entities;

namespace ERP_Government.Application.Revenue.Commands.RevenueClaims.ApproveRevenueClaim;

[Authorize(Policy = PermissionCodes.RevenueClaimsApprove)]
public class ApproveRevenueClaimCommand : IRequest<Result>
{
    public int Id { get; init; }
    public string? Reason { get; init; }
    public byte[] RowVersion { get; init; } = [];
}

public class ApproveRevenueClaimCommandHandler(
    IApplicationDbContext context,
    IUser user) : IRequestHandler<ApproveRevenueClaimCommand, Result>
{
    public async Task<Result> Handle(
        ApproveRevenueClaimCommand request,
        CancellationToken cancellationToken)
    {
        if (user.Id is not int userId)
            return Result.Failure(["User identity is required."]);

        var claim = await context.RevenueClaims.FindAsync(request.Id, cancellationToken);
        if (claim is null)
            return Result.Failure(["Revenue claim not found."]);

        if (claim.Status != ClaimStatus.Draft && claim.Status != ClaimStatus.PendingApproval)
            return Result.Failure(["Only Draft or PendingApproval claims can be approved."]);

        var previousStatus = claim.Status;
        claim.Status = ClaimStatus.Open;
        claim.RowVersion = request.RowVersion;
        claim.LastModified = DateTimeOffset.UtcNow;
        claim.LastModifiedBy = userId.ToString();

        context.DocumentStatusLogs.Add(new DocumentStatusLog
        {
            EntityName = nameof(RevenueClaim),
            DocumentId = claim.Id,
            FromStatus = previousStatus.ToString(),
            ToStatus = ClaimStatus.Open.ToString(),
            ChangedById = userId,
            ChangedAt = DateTimeOffset.UtcNow,
            Reason = request.Reason
        });

        await context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

public class ApproveRevenueClaimCommandValidator : AbstractValidator<ApproveRevenueClaimCommand>
{
    public ApproveRevenueClaimCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0).WithMessage("Claim ID is required.");
        RuleFor(x => x.RowVersion).NotEmpty().WithMessage("Row version is required for concurrency control.");
    }
}
