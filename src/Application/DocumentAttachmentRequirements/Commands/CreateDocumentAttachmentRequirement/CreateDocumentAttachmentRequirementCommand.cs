using ERP_Government.Application.Common.Security;
using ERP_Government.Domain.Security.Entities;

namespace ERP_Government.Application.DocumentAttachmentRequirements.Commands.CreateDocumentAttachmentRequirement;

[Authorize(Policy = PermissionCodes.PartiesCreate)]
public record CreateDocumentAttachmentRequirementCommand(
    string DocumentType,
    string AttachmentTypeCode,
    string TitleAr,
    bool IsMandatory) : IRequest<Result<int>>;

public class CreateDocumentAttachmentRequirementCommandHandler(
    IApplicationDbContext context) : IRequestHandler<CreateDocumentAttachmentRequirementCommand, Result<int>>
{
    public async Task<Result<int>> Handle(
        CreateDocumentAttachmentRequirementCommand request,
        CancellationToken cancellationToken)
    {
        var exists = await context.DocumentAttachmentRequirements
            .AnyAsync(r => r.DocumentType == request.DocumentType
                && r.AttachmentTypeCode == request.AttachmentTypeCode, cancellationToken);

        if (exists)
            return Result<int>.Failure(["A requirement with this document type and attachment type code already exists."]);

        var entity = new DocumentAttachmentRequirement
        {
            DocumentType = request.DocumentType,
            AttachmentTypeCode = request.AttachmentTypeCode,
            TitleAr = request.TitleAr,
            IsMandatory = request.IsMandatory,
            IsActive = true,
            Created = DateTimeOffset.UtcNow,
            LastModified = DateTimeOffset.UtcNow
        };

        context.DocumentAttachmentRequirements.Add(entity);
        await context.SaveChangesAsync(cancellationToken);

        return Result<int>.Success(entity.Id);
    }
}

public class CreateDocumentAttachmentRequirementCommandValidator
    : AbstractValidator<CreateDocumentAttachmentRequirementCommand>
{
    public CreateDocumentAttachmentRequirementCommandValidator()
    {
        RuleFor(x => x.DocumentType)
            .NotEmpty().WithMessage("Document type is required.")
            .MaximumLength(100).WithMessage("Document type must not exceed 100 characters.");

        RuleFor(x => x.AttachmentTypeCode)
            .NotEmpty().WithMessage("Attachment type code is required.")
            .MaximumLength(50).WithMessage("Attachment type code must not exceed 50 characters.");

        RuleFor(x => x.TitleAr)
            .NotEmpty().WithMessage("Title (Arabic) is required.")
            .MaximumLength(500).WithMessage("Title must not exceed 500 characters.");
    }
}
