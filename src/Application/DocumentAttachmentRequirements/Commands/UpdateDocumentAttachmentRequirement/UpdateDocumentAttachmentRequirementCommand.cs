using ERP_Government.Application.Common.Security;
using ERP_Government.Domain.Security.Entities;

namespace ERP_Government.Application.DocumentAttachmentRequirements.Commands.UpdateDocumentAttachmentRequirement;

[Authorize(Policy = PermissionCodes.PartiesUpdate)]
public record UpdateDocumentAttachmentRequirementCommand(
    int Id,
    string TitleAr,
    bool IsMandatory) : IRequest<Result>;

public class UpdateDocumentAttachmentRequirementCommandHandler(
    IApplicationDbContext context) : IRequestHandler<UpdateDocumentAttachmentRequirementCommand, Result>
{
    public async Task<Result> Handle(
        UpdateDocumentAttachmentRequirementCommand request,
        CancellationToken cancellationToken)
    {
        var entity = await context.DocumentAttachmentRequirements.FindAsync(request.Id, cancellationToken);
        if (entity is null)
            return Result.Failure(["Document attachment requirement not found."]);

        entity.TitleAr = request.TitleAr;
        entity.IsMandatory = request.IsMandatory;
        entity.LastModified = DateTimeOffset.UtcNow;

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

public class UpdateDocumentAttachmentRequirementCommandValidator
    : AbstractValidator<UpdateDocumentAttachmentRequirementCommand>
{
    public UpdateDocumentAttachmentRequirementCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Invalid requirement ID.");

        RuleFor(x => x.TitleAr)
            .NotEmpty().WithMessage("Title (Arabic) is required.")
            .MaximumLength(500).WithMessage("Title must not exceed 500 characters.");
    }
}
