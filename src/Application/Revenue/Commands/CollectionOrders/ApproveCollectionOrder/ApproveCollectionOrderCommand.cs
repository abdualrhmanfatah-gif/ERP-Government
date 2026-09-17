using ERP_Government.Application.Common.Security;
using ERP_Government.Domain.Revenue.Entities;
using ERP_Government.Domain.Revenue.Enums;
using ERP_Government.Domain.Security.Entities;

namespace ERP_Government.Application.Revenue.Commands.CollectionOrders.ApproveCollectionOrder;

[Authorize(Policy = PermissionCodes.CollectionOrdersApprove)]
public class ApproveCollectionOrderCommand : IRequest<Result>
{
    public int Id { get; init; }
    public string? Reason { get; init; }
    public byte[] RowVersion { get; init; } = [];
}

public class ApproveCollectionOrderCommandHandler(
    IApplicationDbContext context,
    IUser user) : IRequestHandler<ApproveCollectionOrderCommand, Result>
{
    public async Task<Result> Handle(
        ApproveCollectionOrderCommand request,
        CancellationToken cancellationToken)
    {
        if (user.Id is not int userId)
            return Result.Failure(["User identity is required."]);

        var order = await context.CollectionOrders.FindAsync(request.Id, cancellationToken);
        if (order is null)
            return Result.Failure(["Collection order not found."]);

        if (order.Status != CollectionOrderStatus.Draft && order.Status != CollectionOrderStatus.PendingApproval)
            return Result.Failure(["Only Draft or PendingApproval collection orders can be approved."]);

        var previousStatus = order.Status;
        order.Status = CollectionOrderStatus.Approved;
        order.RowVersion = request.RowVersion;
        order.LastModified = DateTimeOffset.UtcNow;
        order.LastModifiedBy = userId.ToString();

        context.DocumentStatusLogs.Add(new DocumentStatusLog
        {
            EntityName = nameof(CollectionOrder),
            DocumentId = order.Id,
            FromStatus = previousStatus.ToString(),
            ToStatus = CollectionOrderStatus.Approved.ToString(),
            ChangedById = userId,
            ChangedAt = DateTimeOffset.UtcNow,
            Reason = request.Reason
        });

        await context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

public class ApproveCollectionOrderCommandValidator : AbstractValidator<ApproveCollectionOrderCommand>
{
    public ApproveCollectionOrderCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0).WithMessage("Order ID is required.");
        RuleFor(x => x.RowVersion).NotEmpty().WithMessage("Row version is required for concurrency control.");
    }
}
