using ERP_Government.Application.Common.Security;
using ERP_Government.Domain.Budgeting.Enums;

namespace ERP_Government.Application.Budgeting.Commands.Appropriations;

[Authorize(Policy = PermissionCodes.AppropriationsDelete)]
public record DeleteAppropriationCommand(
    int Id) : IRequest<Result>;

public class DeleteAppropriationCommandHandler(
    IApplicationDbContext context) : IRequestHandler<DeleteAppropriationCommand, Result>
{
    public async Task<Result> Handle(
        DeleteAppropriationCommand request,
        CancellationToken cancellationToken)
    {
        var entity = await context.Appropriations
            .FindAsync(request.Id, cancellationToken);

        if (entity is null)
            return Result.Failure(["Appropriation not found."]);

        if (entity.Status != AppropriationStatus.Draft)
            return Result.Failure(["Only Draft appropriations can be deleted."]);

        var hasEncumbrances = await context.Encumbrances
            .AnyAsync(x => x.AppropriationId == request.Id, cancellationToken);

        if (hasEncumbrances)
            return Result.Failure(["Cannot delete an appropriation that has encumbrances."]);

        context.Appropriations.Remove(entity);
        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
