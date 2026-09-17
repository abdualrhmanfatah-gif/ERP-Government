using ERP_Government.Domain.Procurement.Enums;
using ERP_Government.Application.Common.Security;

namespace ERP_Government.Application.Procurement.Queries.PurchaseRequests.GetPurchaseRequestById;

[Authorize(Policy = PermissionCodes.PurchaseRequestsView)]
public record GetPurchaseRequestByIdQuery(int Id) : IRequest<Result<PurchaseRequestDetailResponse>>;

public record PurchaseRequestDetailResponse(
    int Id,
    string RequestNumber,
    DateTime RequestDate,
    DateOnly? RequiredDate,
    int? DepartmentId,
    int? CostCenterId,
    string RequesterName,
    PurchaseRequestPriority Priority,
    PurchaseRequestStatus Status,
    decimal? TotalEstimatedCost,
    string? Notes,
    IReadOnlyList<PurchaseRequestDetailLineResponse> Lines);

public record PurchaseRequestDetailLineResponse(
    int Id,
    int ItemId,
    int UnitId,
    decimal RequestedQuantity,
    decimal? ApprovedQuantity,
    decimal? UnitCostEstimate,
    decimal? TotalCostEstimate,
    string? Notes);

public class GetPurchaseRequestByIdQueryHandler(
    IApplicationDbContext context) : IRequestHandler<GetPurchaseRequestByIdQuery, Result<PurchaseRequestDetailResponse>>
{
    public async Task<Result<PurchaseRequestDetailResponse>> Handle(
        GetPurchaseRequestByIdQuery request,
        CancellationToken cancellationToken)
    {
        var entity = await context.PurchaseRequests
            .Include(p => p.Details)
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

        if (entity is null)
            return Result<PurchaseRequestDetailResponse>.Failure(["Purchase request not found."]);

        var response = new PurchaseRequestDetailResponse(
            entity.Id,
            entity.RequestNumber,
            entity.RequestDate,
            entity.RequiredDate,
            entity.DepartmentId,
            entity.CostCenterId,
            entity.RequesterName,
            entity.Priority,
            entity.Status,
            entity.TotalEstimatedCost,
            entity.Notes,
            entity.Details.Select(d => new PurchaseRequestDetailLineResponse(
                d.Id,
                d.ItemId,
                d.UnitId,
                d.RequestedQuantity,
                d.ApprovedQuantity,
                d.UnitCostEstimate,
                d.TotalCostEstimate,
                d.Notes)).ToList());

        return Result<PurchaseRequestDetailResponse>.Success(response);
    }
}
