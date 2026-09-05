using ERP_Government.Application.Common.Security;
using ERP_Government.Application.Documents.Common;

namespace ERP_Government.Application.Documents.Commands.DeleteAttachment;

[Authorize(Policy = PermissionCodes.PartiesUpdate)]
public record DeleteAttachmentCommand(int Id) : IRequest<Result>;

public class DeleteAttachmentCommandHandler(
    IApplicationDbContext context,
    IFileStorageService fileStorageService) : IRequestHandler<DeleteAttachmentCommand, Result>
{
    public async Task<Result> Handle(
        DeleteAttachmentCommand request,
        CancellationToken cancellationToken)
    {
        var entity = await context.Attachments.FindAsync(request.Id, cancellationToken);
        if (entity is null)
            return Result.Failure(["Attachment not found."]);

        await fileStorageService.DeleteAsync(entity.StoragePath, cancellationToken);

        context.Attachments.Remove(entity);
        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
