using ERP_Government.Application.Common.Security;
using ERP_Government.Domain.Budgeting.Enums;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace ERP_Government.Application.Budgeting.Commands.Encumbrances;

[Authorize(Policy = PermissionCodes.EncumbrancesUpdate)]
public record UpdateEncumbranceCommand(
    int Id,
    string? Description,
    DateOnly? EncumbranceDate,
    int? PurchaseOrderId,
    byte[] RowVersion) : IRequest<Result>;

public class UpdateEncumbranceCommandHandler(
    IApplicationDbContext context) : IRequestHandler<UpdateEncumbranceCommand, Result>
{
    public async Task<Result> Handle(UpdateEncumbranceCommand request, CancellationToken cancellationToken)
    {
        var entity = await context.Encumbrances.FindAsync(request.Id, cancellationToken);
        if (entity is null)
            return Result.Failure(["Encumbrance not found."]);

        if (entity.Status != EncumbranceStatus.Draft)
            return Result.Failure(["Only Draft encumbrances can be updated."]);

        if (!request.RowVersion.SequenceEqual(entity.RowVersion))
            return Result.Failure(["Concurrency conflict."]);

        if (request.Description is not null) entity.Description = request.Description;
        if (request.EncumbranceDate.HasValue) entity.EncumbranceDate = request.EncumbranceDate.Value;
        if (request.PurchaseOrderId.HasValue) entity.PurchaseOrderId = request.PurchaseOrderId;

        await context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

[Authorize(Policy = PermissionCodes.EncumbrancesDelete)]
public record DeleteEncumbranceCommand(
    int Id,
    byte[] RowVersion) : IRequest<Result>;

public class DeleteEncumbranceCommandHandler(
    IApplicationDbContext context) : IRequestHandler<DeleteEncumbranceCommand, Result>
{
    public async Task<Result> Handle(DeleteEncumbranceCommand request, CancellationToken cancellationToken)
    {
        var entity = await context.Encumbrances.FindAsync(request.Id, cancellationToken);
        if (entity is null)
            return Result.Failure(["Encumbrance not found."]);

        if (entity.Status != EncumbranceStatus.Draft)
            return Result.Failure(["Only Draft encumbrances can be deleted."]);

        if (!request.RowVersion.SequenceEqual(entity.RowVersion))
            return Result.Failure(["Concurrency conflict."]);

        context.Encumbrances.Remove(entity);
        await context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
