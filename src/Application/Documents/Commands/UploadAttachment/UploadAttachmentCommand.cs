using ERP_Government.Application.Documents.Common;
using ERP_Government.Domain.Security.Entities;
using ERP_Government.Application.Common.Security;

namespace ERP_Government.Application.Documents.Commands.UploadAttachment;

[Authorize]
public record UploadAttachmentCommand(
    string DocumentType,
    int DocumentId,
    string AttachmentTypeCode,
    string FileName,
    Stream Content) : IRequest<Result<int>>;

public class UploadAttachmentCommandHandler(
    IApplicationDbContext context,
    IFileStorageService fileStorageService,
    IUser user) : IRequestHandler<UploadAttachmentCommand, Result<int>>
{
    public async Task<Result<int>> Handle(
        UploadAttachmentCommand request,
        CancellationToken cancellationToken)
    {
        if (user.Id is not int userId)
            return Result<int>.Failure(new[] { "User identity is required for this operation." });

        var filePath = await fileStorageService.SaveStreamAsync(request.FileName, request.Content, cancellationToken);

        var entity = new Attachment
        {
            EntityName = request.DocumentType,
            DocumentType = request.DocumentType,
            DocumentId = request.DocumentId,
            FileName = request.FileName,
            StoragePath = filePath,
            MimeType = MimeDetector.Detect(request.FileName),
            SizeBytes = (int)request.Content.Length,
            AttachmentTypeCode = request.AttachmentTypeCode,
            IsRequired = false,
            UploadedById = userId,
            CreatedAt = DateTimeOffset.UtcNow
        };

        context.Attachments.Add(entity);
        await context.SaveChangesAsync(cancellationToken);

        return Result<int>.Success(entity.Id);
    }
}
