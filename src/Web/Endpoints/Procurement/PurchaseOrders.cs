using ERP_Government.Application.Common.Models;
using ERP_Government.Application.Common.Security;
using ERP_Government.Application.Procurement.Commands.PurchaseOrders.ApprovePurchaseOrder;
using ERP_Government.Application.Procurement.Commands.PurchaseOrders.CancelPurchaseOrder;
using ERP_Government.Application.Procurement.Commands.PurchaseOrders.ClosePurchaseOrder;
using ERP_Government.Application.Procurement.Commands.PurchaseOrders.CreatePurchaseOrder;
using ERP_Government.Application.Procurement.Commands.PurchaseOrders.IssuePurchaseOrder;
using ERP_Government.Application.Procurement.Commands.PurchaseOrders.SubmitPurchaseOrder;
using ERP_Government.Application.Procurement.Commands.PurchaseOrders.UpdatePurchaseOrder;
using ERP_Government.Application.Procurement.Queries.PurchaseOrders.GetPurchaseOrderById;
using ERP_Government.Application.Procurement.Queries.PurchaseOrders.GetPurchaseOrders;
using ERP_Government.Domain.Procurement.Enums;
using ERP_Government.Web.Infrastructure;

namespace ERP_Government.Web.Endpoints.Procurement;

public class PurchaseOrders : IEndpointGroup
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapGet("/", HandleGetList)
            .RequireAuthorization(PermissionCodes.PurchaseOrdersView)
            .Produces<Result<PaginatedList<PurchaseOrderListItem>>>();
        group.MapGet("/{id:int}", HandleGetById)
            .RequireAuthorization(PermissionCodes.PurchaseOrdersView)
            .Produces<Result<PurchaseOrderDetailResponse>>();
        group.MapPost("/", HandleCreate)
            .RequireAuthorization(PermissionCodes.PurchaseOrdersCreate)
            .Produces<int>();
        group.MapPut("/{id:int}", HandleUpdate)
            .RequireAuthorization(PermissionCodes.PurchaseOrdersCreate)
            .Produces<Result>();
        group.MapPatch("/{id:int}/submit", HandleSubmit)
            .RequireAuthorization(PermissionCodes.PurchaseOrdersSubmit)
            .Produces<Result>();
        group.MapPatch("/{id:int}/approve", HandleApprove)
            .RequireAuthorization(PermissionCodes.PurchaseOrdersApprove)
            .Produces<Result>();
        group.MapPatch("/{id:int}/issue", HandleIssue)
            .RequireAuthorization(PermissionCodes.PurchaseOrdersIssue)
            .Produces<Result>();
        group.MapPatch("/{id:int}/cancel", HandleCancel)
            .RequireAuthorization(PermissionCodes.PurchaseOrdersCancel)
            .Produces<Result>();
        group.MapPatch("/{id:int}/close", HandleClose)
            .RequireAuthorization(PermissionCodes.PurchaseOrdersClose)
            .Produces<Result>();
    }

    private static async Task<IResult> HandleGetList(ISender sender, int? purchaseRequestId, int? quotationId, int? supplierPartyId, PurchaseOrderStatus? status, string? search, DateTime? expectedDeliveryDateFrom, DateTime? expectedDeliveryDateTo, int page = 1, int pageSize = 20)
    {
        var result = await sender.Send(new GetPurchaseOrdersQuery(
            purchaseRequestId,
            quotationId,
            supplierPartyId,
            status,
            search,
            expectedDeliveryDateFrom,
            expectedDeliveryDateTo,
            page,
            pageSize));
        return result.Succeeded ? Results.Ok(result.Value) : result.ToProblemDetails();
    }

    private static async Task<IResult> HandleGetById(ISender sender, int id)
    {
        var result = await sender.Send(new GetPurchaseOrderByIdQuery(id));
        return result.ToProblemDetails();
    }

    private static async Task<IResult> HandleCreate(ISender sender, CreatePurchaseOrderCommand command)
    {
        var result = await sender.Send(command);
        return result.Succeeded
            ? Results.Created($"/api/PurchaseOrders/{result.Value}", result.Value)
            : result.ToProblemDetails();
    }

    private static async Task<IResult> HandleUpdate(ISender sender, int id, UpdatePurchaseOrderCommand command)
    {
        var result = await sender.Send(command with { Id = id });
        return result.Succeeded ? Results.Ok(result) : result.ToProblemDetails();
    }

    private static async Task<IResult> HandleSubmit(ISender sender, int id)
    {
        var result = await sender.Send(new SubmitPurchaseOrderCommand(id));
        return result.Succeeded ? Results.Ok(result) : result.ToProblemDetails();
    }

    private static async Task<IResult> HandleApprove(ISender sender, int id)
    {
        var result = await sender.Send(new ApprovePurchaseOrderCommand(id));
        return result.Succeeded ? Results.Ok() : result.ToProblemDetails();
    }

    private static async Task<IResult> HandleIssue(ISender sender, int id)
    {
        var result = await sender.Send(new IssuePurchaseOrderCommand(id));
        return result.Succeeded ? Results.Ok() : result.ToProblemDetails();
    }

    private static async Task<IResult> HandleCancel(ISender sender, int id, CancelPurchaseOrderCommand command)
    {
        var result = await sender.Send(command with { Id = id });
        return result.Succeeded ? Results.Ok() : result.ToProblemDetails();
    }

    private static async Task<IResult> HandleClose(ISender sender, int id, ClosePurchaseOrderCommand command)
    {
        var result = await sender.Send(command with { Id = id });
        return result.Succeeded ? Results.Ok() : result.ToProblemDetails();
    }
}
