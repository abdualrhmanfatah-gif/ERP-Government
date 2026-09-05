using ERP_Government.Application.Common.Security;
using ERP_Government.Domain.Payments.Enums;

namespace ERP_Government.Application.Payments.Commands.DisbursementRequests.SubmitDisbursementRequest;

[Authorize(Policy = PermissionCodes.DisbursementRequestsSubmit)]
public class SubmitDisbursementRequestCommand : IRequest<Result>
{
    public int Id { get; init; }
    public byte[] RowVersion { get; init; } = [];
}

public class SubmitDisbursementRequestCommandHandler(
    IApplicationDbContext context,
    IUser user) : IRequestHandler<SubmitDisbursementRequestCommand, Result>
{
    public async Task<Result> Handle(
        SubmitDisbursementRequestCommand request,
        CancellationToken cancellationToken)
    {
        if (user.Id is not int userId)
            return Result.Failure(new[] { "User identity is required for this operation." });

        var entity = await context.DisbursementRequests
            .FindAsync(request.Id, cancellationToken);

        if (entity is null)
            return Result.Failure(["Disbursement request not found."]);

        if (entity.Status != DisbursementRequestStatus.Draft)
            return Result.Failure(["Only draft disbursement requests can be submitted."]);

        entity.Status = DisbursementRequestStatus.PendingApproval;
        entity.LastModified = DateTimeOffset.UtcNow;
        entity.LastModifiedBy = userId.ToString();

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

public class SubmitDisbursementRequestCommandValidator : AbstractValidator<SubmitDisbursementRequestCommand>
{
    public SubmitDisbursementRequestCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Invalid disbursement request ID.");
    }
}
