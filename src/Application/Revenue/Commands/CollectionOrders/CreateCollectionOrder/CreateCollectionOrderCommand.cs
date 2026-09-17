using ERP_Government.Application.Common.Security;
using ERP_Government.Application.FinancialSettings.Common.Services;
using ERP_Government.Application.Revenue.Common.DTOs;
using ERP_Government.Application.Revenue.Common.Services;
using ERP_Government.Domain.Revenue.Entities;
using ERP_Government.Domain.Revenue.Enums;
using ERP_Government.Domain.Security.Entities;

namespace ERP_Government.Application.Revenue.Commands.CollectionOrders.CreateCollectionOrder;

[Authorize(Policy = PermissionCodes.CollectionOrdersCreate)]
public class CreateCollectionOrderCommand : IRequest<Result<CollectionOrderDto>>
{
    public int RevenueClaimId { get; init; }
    public DateOnly OrderDate { get; init; }
    public decimal AuthorizedAmount { get; init; }
    public string? Notes { get; init; }
}

public class CreateCollectionOrderCommandHandler(
    IApplicationDbContext context,
    IDocumentSequenceService sequenceService,
    IUser user) : IRequestHandler<CreateCollectionOrderCommand, Result<CollectionOrderDto>>
{
    public async Task<Result<CollectionOrderDto>> Handle(
        CreateCollectionOrderCommand request,
        CancellationToken cancellationToken)
    {
        if (user.Id is not int userId)
            return Result<CollectionOrderDto>.Failure(["User identity is required."]);

        var claim = await context.RevenueClaims
            .Include(c => c.CollectionOrders)
                .ThenInclude(o => o.ReceiptVouchers)
                    .ThenInclude(v => v.Lines)
            .Include(c => c.CollectionOrders)
                .ThenInclude(o => o.ReceiptVouchers)
                    .ThenInclude(v => v.Checks)
            .FirstOrDefaultAsync(c => c.Id == request.RevenueClaimId, cancellationToken);

        if (claim is null)
            return Result<CollectionOrderDto>.Failure(["Revenue claim not found."]);

        if (claim.Status != ClaimStatus.Open && claim.Status != ClaimStatus.PartiallySettled)
            return Result<CollectionOrderDto>.Failure(["Collection orders can only be created for Open or PartiallySettled claims."]);

        if (request.AuthorizedAmount <= 0)
            return Result<CollectionOrderDto>.Failure(["Authorized amount must be greater than zero."]);

        var (_, _, _, availableOnClaim) = RevenueMetricsCalculator.CalculateClaimMetrics(claim.TotalAmount, claim.CollectionOrders);

        if (request.AuthorizedAmount > availableOnClaim)
            return Result<CollectionOrderDto>.Failure([$"Authorized amount ({request.AuthorizedAmount:N2}) exceeds available amount on claim ({availableOnClaim:N2})."]);

        var orderNumber = await sequenceService.GenerateNextNumberAsync("CollectionOrder", cancellationToken);

        var order = new CollectionOrder
        {
            RevenueClaimId = request.RevenueClaimId,
            OrderNumber = orderNumber,
            OrderDate = request.OrderDate,
            AuthorizedAmount = request.AuthorizedAmount,
            Notes = request.Notes,
            Status = CollectionOrderStatus.Draft,
            Created = DateTimeOffset.UtcNow,
            CreatedBy = userId.ToString()
        };

        context.CollectionOrders.Add(order);

        context.DocumentStatusLogs.Add(new DocumentStatusLog
        {
            EntityName = nameof(CollectionOrder),
            DocumentId = order.Id,
            FromStatus = CollectionOrderStatus.Draft.ToString(),
            ToStatus = CollectionOrderStatus.Draft.ToString(),
            ChangedById = userId,
            ChangedAt = DateTimeOffset.UtcNow
        });

        await context.SaveChangesAsync(cancellationToken);

        return Result<CollectionOrderDto>.Success(new CollectionOrderDto
        {
            Id = order.Id,
            RevenueClaimId = order.RevenueClaimId,
            RevenueClaimNumber = claim.ClaimNumber,
            OrderNumber = order.OrderNumber,
            OrderDate = order.OrderDate,
            AuthorizedAmount = order.AuthorizedAmount,
            CollectedAmount = 0,
            UnderCollectionAmount = 0,
            OutstandingAmount = order.AuthorizedAmount,
            AvailableAmount = order.AuthorizedAmount,
            Notes = order.Notes,
            Status = order.Status,
            StatusName = order.Status.ToString(),
            RowVersion = order.RowVersion,
            Created = order.Created,
            CreatedBy = order.CreatedBy
        });
    }
}

public class CreateCollectionOrderCommandValidator : AbstractValidator<CreateCollectionOrderCommand>
{
    public CreateCollectionOrderCommandValidator()
    {
        RuleFor(x => x.RevenueClaimId).GreaterThan(0).WithMessage("Revenue claim is required.");
        RuleFor(x => x.OrderDate).NotEmpty().WithMessage("Order date is required.");
        RuleFor(x => x.AuthorizedAmount).GreaterThan(0).WithMessage("Authorized amount must be greater than zero.");
    }
}
