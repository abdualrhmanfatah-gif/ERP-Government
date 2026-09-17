using ERP_Government.Application.Common.Security;
using ERP_Government.Application.FinancialSettings.Common.Services;
using ERP_Government.Application.Revenue.Common.DTOs;
using ERP_Government.Domain.Revenue.Entities;
using ERP_Government.Domain.Revenue.Enums;
using ERP_Government.Domain.Security.Entities;

namespace ERP_Government.Application.Revenue.Commands.RevenueClaims.CreateRevenueClaim;

[Authorize(Policy = PermissionCodes.RevenueClaimsCreate)]
public class CreateRevenueClaimCommand : IRequest<Result<RevenueClaimDto>>
{
    public DateOnly ClaimDate { get; init; }
    public int PartyId { get; init; }
    public decimal TotalAmount { get; init; }
    public string? Notes { get; init; }
}

public class CreateRevenueClaimCommandHandler(
    IApplicationDbContext context,
    IDocumentSequenceService sequenceService,
    IUser user) : IRequestHandler<CreateRevenueClaimCommand, Result<RevenueClaimDto>>
{
    public async Task<Result<RevenueClaimDto>> Handle(
        CreateRevenueClaimCommand request,
        CancellationToken cancellationToken)
    {
        if (user.Id is not int userId)
            return Result<RevenueClaimDto>.Failure(["User identity is required."]);

        var party = await context.Parties.FindAsync(request.PartyId, cancellationToken);
        if (party is null || !party.IsActive)
            return Result<RevenueClaimDto>.Failure(["Party not found or inactive."]);

        if (request.TotalAmount <= 0)
            return Result<RevenueClaimDto>.Failure(["Total amount must be greater than zero."]);

        var claimNumber = await sequenceService.GenerateNextNumberAsync("RevenueClaim", cancellationToken);

        var claim = new RevenueClaim
        {
            ClaimNumber = claimNumber,
            ClaimDate = request.ClaimDate,
            PartyId = request.PartyId,
            TotalAmount = request.TotalAmount,
            Notes = request.Notes,
            Status = ClaimStatus.Draft,
            Created = DateTimeOffset.UtcNow,
            CreatedBy = userId.ToString()
        };

        context.RevenueClaims.Add(claim);

        context.DocumentStatusLogs.Add(new DocumentStatusLog
        {
            EntityName = nameof(RevenueClaim),
            DocumentId = claim.Id,
            FromStatus = ClaimStatus.Draft.ToString(),
            ToStatus = ClaimStatus.Draft.ToString(),
            ChangedById = userId,
            ChangedAt = DateTimeOffset.UtcNow
        });

        await context.SaveChangesAsync(cancellationToken);

        return Result<RevenueClaimDto>.Success(new RevenueClaimDto
        {
            Id = claim.Id,
            ClaimNumber = claim.ClaimNumber,
            ClaimDate = claim.ClaimDate,
            PartyId = claim.PartyId,
            PartyName = party.NameAr,
            TotalAmount = claim.TotalAmount,
            CollectedAmount = 0,
            UnderCollectionAmount = 0,
            OutstandingAmount = claim.TotalAmount,
            AvailableAmount = claim.TotalAmount,
            Notes = claim.Notes,
            Status = claim.Status,
            StatusName = claim.Status.ToString(),
            RowVersion = claim.RowVersion,
            Created = claim.Created,
            CreatedBy = claim.CreatedBy
        });
    }
}

public class CreateRevenueClaimCommandValidator : AbstractValidator<CreateRevenueClaimCommand>
{
    public CreateRevenueClaimCommandValidator()
    {
        RuleFor(x => x.ClaimDate).NotEmpty().WithMessage("Claim date is required.");
        RuleFor(x => x.PartyId).GreaterThan(0).WithMessage("Party is required.");
        RuleFor(x => x.TotalAmount).GreaterThan(0).WithMessage("Total amount must be greater than zero.");
    }
}
