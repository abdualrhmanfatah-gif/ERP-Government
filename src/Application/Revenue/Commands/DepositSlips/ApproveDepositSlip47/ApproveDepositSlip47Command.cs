using ERP_Government.Application.Common.Security;
using ERP_Government.Domain.Events.Revenue;
using ERP_Government.Domain.Revenue.Entities;
using ERP_Government.Domain.Security.Entities;

namespace ERP_Government.Application.Revenue.Commands.DepositSlips.ApproveDepositSlip47;

[Authorize(Policy = PermissionCodes.DepositSlipsApprove)]
public class ApproveDepositSlip47Command : IRequest<Result>
{
    public int Id { get; init; }
    public string? Reason { get; init; }
    public byte[] RowVersion { get; init; } = [];
}

public class ApproveDepositSlip47CommandHandler(
    IApplicationDbContext context,
    IUser user) : IRequestHandler<ApproveDepositSlip47Command, Result>
{
    public async Task<Result> Handle(
        ApproveDepositSlip47Command request,
        CancellationToken cancellationToken)
    {
        if (user.Id is not int userId)
            return Result.Failure(["User identity is required."]);

        var slip = await context.DepositSlips47.FindAsync(request.Id, cancellationToken);
        if (slip is null)
            return Result.Failure(["DepositSlip47 not found."]);

        if (slip.ApprovedAt.HasValue)
            return Result.Failure(["DepositSlip47 is already approved."]);

        slip.ApprovedById = userId;
        slip.ApprovedAt = DateTimeOffset.UtcNow;
        slip.RowVersion = request.RowVersion;
        slip.LastModified = DateTimeOffset.UtcNow;
        slip.LastModifiedBy = userId.ToString();

        slip.AddDomainEvent(new DepositSlip47ApprovedEvent
        {
            SourceEntityId = slip.Id,
            OccurredAt = DateTimeOffset.UtcNow,
            SlipNumber = slip.SlipNumber,
            TotalAmount = slip.TotalAmount
        });

        context.DocumentStatusLogs.Add(new DocumentStatusLog
        {
            EntityName = nameof(DepositSlip47),
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

public class ApproveDepositSlip47CommandValidator : AbstractValidator<ApproveDepositSlip47Command>
{
    public ApproveDepositSlip47CommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0).WithMessage("Slip ID is required.");
        RuleFor(x => x.RowVersion).NotEmpty().WithMessage("Row version is required for concurrency control.");
    }
}
