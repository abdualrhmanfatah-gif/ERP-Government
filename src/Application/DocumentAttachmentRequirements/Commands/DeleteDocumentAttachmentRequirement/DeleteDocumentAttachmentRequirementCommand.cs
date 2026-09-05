using ERP_Government.Application.Common.Security;

namespace ERP_Government.Application.DocumentAttachmentRequirements.Commands.DeleteDocumentAttachmentRequirement;

[Authorize(Policy = PermissionCodes.PartiesUpdate)]
public record DeleteDocumentAttachmentRequirementCommand(int Id) : IRequest<Result>;

public class DeleteDocumentAttachmentRequirementCommandHandler(
    IApplicationDbContext context) : IRequestHandler<DeleteDocumentAttachmentRequirementCommand, Result>
{
    public async Task<Result> Handle(
        DeleteDocumentAttachmentRequirementCommand request,
        CancellationToken cancellationToken)
    {
        var entity = await context.DocumentAttachmentRequirements.FindAsync(request.Id, cancellationToken);
        if (entity is null)
            return Result.Failure(["Document attachment requirement not found."]);

        entity.IsActive = false;
        entity.LastModified = DateTimeOffset.UtcNow;

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
