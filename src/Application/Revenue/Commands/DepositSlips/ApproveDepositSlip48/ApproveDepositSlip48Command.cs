using ERP_Government.Application.Common.Security;
using ERP_Government.Domain.Events.Revenue;
using ERP_Government.Domain.Revenue.Entities;
using ERP_Government.Domain.Revenue.Enums;
using ERP_Government.Domain.Security.Entities;

namespace ERP_Government.Application.Revenue.Commands.DepositSlips.ApproveDepositSlip48;

[Authorize(Policy = PermissionCodes.DepositSlipsApprove)]
public class ApproveDepositSlip48Command : IRequest<Result>
{
    public int Id { get; init; }
    public string? Reason { get; init; }
    public byte[] RowVersion { get; init; } = [];
}

public class ApproveDepositSlip48CommandHandler(
    IApplicationDbContext context,
    IUser user) : IRequestHandler<ApproveDepositSlip48Command, Result>
{
    public async Task<Result> Handle(
        ApproveDepositSlip48Command request,
        CancellationToken cancellationToken)
    {
        if (user.Id is not int userId)
            return Result.Failure(["User identity is required."]);

        var slip = await context.DepositSlips48
            .Include(s => s.Checks)
            .FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken);

        if (slip is null)
            return Result.Failure(["DepositSlip48 not found."]);

        if (slip.ApprovedAt.HasValue)
            return Result.Failure(["DepositSlip48 is already approved."]);

        slip.ApprovedById = userId;
        slip.ApprovedAt = DateTimeOffset.UtcNow;
        slip.RowVersion = request.RowVersion;
        slip.LastModified = DateTimeOffset.UtcNow;
        slip.LastModifiedBy = userId.ToString();

        foreach (var check in slip.Checks)
        {
            if (check.Status == CheckStatus.Received)
            {
                check.Status = CheckStatus.UnderCollection;
                check.LastModified = DateTimeOffset.UtcNow;
                check.LastModifiedBy = userId.ToString();
            }
        }

        slip.AddDomainEvent(new DepositSlip48ApprovedEvent
        {
            SourceEntityId = slip.Id,
            OccurredAt = DateTimeOffset.UtcNow,
            SlipNumber = slip.SlipNumber,
            TotalAmount = slip.TotalAmount
        });

        context.DocumentStatusLogs.Add(new DocumentStatusLog
        {
            EntityName = nameof(DepositSlip48),
            DocumentId = slip.Id,
            FromStatus = "Draft",
            ToStatus = "Approved",
            ChangedById = userId,
            ChangedAt = DateTimeOffset.UtcNow,
            Reason = request.Reason
        });

        await context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

public class ApproveDepositSlip48CommandValidator : AbstractValidator<ApproveDepositSlip48Command>
{
    public ApproveDepositSlip48CommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0).WithMessage("Slip ID is required.");
        RuleFor(x => x.RowVersion).NotEmpty().WithMessage("Row version is required for concurrency control.");
    }
}
