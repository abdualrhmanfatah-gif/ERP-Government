using ERP_Government.Application.Common.Security;
using ERP_Government.Application.Documents.Common;
using ERP_Government.Domain.Security.Entities;

namespace ERP_Government.Application.Documents.Commands.UploadAttachment;

[Authorize(Policy = PermissionCodes.PartiesCreate)]
public record UploadAttachmentCommand(
    string DocumentType,
    int DocumentId,
    string AttachmentTypeCode,
    string FileName,
    Stream Content) : IRequest<Result<int>>;

public class UploadAttachmentCommandHandler(
    IApplicationDbContext context,
    IFileStorageService fileStorageService) : IRequestHandler<UploadAttachmentCommand, Result<int>>
{
    public async Task<Result<int>> Handle(
        UploadAttachmentCommand request,
        CancellationToken cancellationToken)
    {
        var filePath = await fileStorageService.SaveStreamAsync(request.FileName, request.Content, cancellationToken);

        var entity = new Attachment
        {
            DocumentType = request.DocumentType,
            DocumentId = request.DocumentId,
            FileName = request.FileName,
            StoragePath = filePath,
            MimeType = "application/octet-stream",
            SizeBytes = (int)request.Content.Length,
            AttachmentTypeCode = request.AttachmentTypeCode,
            IsRequired = false,
            CreatedAt = DateTimeOffset.UtcNow
        };

        context.Attachments.Add(entity);
        await context.SaveChangesAsync(cancellationToken);

        return Result<int>.Success(entity.Id);
    }
}
