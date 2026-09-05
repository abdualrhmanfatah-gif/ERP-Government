using ERP_Government.Application.Common.Security;
using ERP_Government.Domain.Committees.Enums;

namespace ERP_Government.Application.Committees.Commands.CommitteeAssignments.RecordSignature;

[Authorize(Policy = PermissionCodes.CommitteeAssignmentsComplete)]
public class RecordSignatureCommand : IRequest<Result>
{
    public int Id { get; init; }
}

public class RecordSignatureCommandHandler(
    IApplicationDbContext context) : IRequestHandler<RecordSignatureCommand, Result>
{
    public async Task<Result> Handle(
        RecordSignatureCommand request,
        CancellationToken cancellationToken)
    {
        var entity = await context.CommitteeAssignments.FindAsync(request.Id, cancellationToken);
        if (entity is null)
            return Result.Failure(["Committee assignment not found."]);

        // Cannot record on completed/cancelled
        if (entity.Status == CommitteeAssignmentStatus.Completed
            || entity.Status == CommitteeAssignmentStatus.Cancelled)
            return Result.Failure(["Cannot record signature on completed or cancelled assignment."]);

        // ActualSignaturesCount <= RequiredSignaturesCount (BR-062)
        if (entity.ActualSignaturesCount >= entity.RequiredSignaturesCount)
            return Result.Failure([$"Signature limit reached: {entity.ActualSignaturesCount}/{entity.RequiredSignaturesCount}."]);

        entity.ActualSignaturesCount += 1;
        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

public class RecordSignatureCommandValidator : AbstractValidator<RecordSignatureCommand>
{
    public RecordSignatureCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Id must be greater than 0.");
    }
}
