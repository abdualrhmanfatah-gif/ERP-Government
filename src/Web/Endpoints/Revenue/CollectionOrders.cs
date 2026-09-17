using ERP_Government.Application.Common.Models;
using ERP_Government.Application.Common.Security;
using ERP_Government.Application.Revenue.Commands.CollectionOrders.ApproveCollectionOrder;
using ERP_Government.Application.Revenue.Commands.CollectionOrders.CreateCollectionOrder;
using ERP_Government.Application.Revenue.Common.DTOs;
using ERP_Government.Application.Revenue.Queries.CollectionOrders.GetCollectionOrders;
using ERP_Government.Domain.Revenue.Enums;
using ERP_Government.Web.Infrastructure;
using MediatR;

namespace ERP_Government.Web.Endpoints.Revenue;

public class CollectionOrders : IEndpointGroup
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapGet("/", HandleGetAll)
            .RequireAuthorization(PermissionCodes.CollectionOrdersView)
            .Produces<List<CollectionOrderDto>>();
        group.MapPost("/", HandleCreate)
            .RequireAuthorization(PermissionCodes.CollectionOrdersCreate)
            .Produces<Result<CollectionOrderDto>>();
        group.MapPost("/{id:int}/approve", HandleApprove)
            .RequireAuthorization(PermissionCodes.CollectionOrdersApprove)
            .Produces<Result>();
    }

    private static async Task<IResult> HandleGetAll(
        ISender sender,
        int? revenueClaimId = null,
        CollectionOrderStatus? status = null)
    {
        var result = await sender.Send(new GetCollectionOrdersQuery { RevenueClaimId = revenueClaimId, Status = status });
        return Results.Ok(result);
    }

    private static async Task<IResult> HandleCreate(
        ISender sender,
        CreateCollectionOrderCommand command)
    {
        var result = await sender.Send(command);
        return result.Succeeded ? Results.Ok(result) : Results.BadRequest(result);
    }

    private static async Task<IResult> HandleApprove(
        ISender sender,
        int id,
        ApproveCollectionOrderCommand command)
    {
        if (id != command.Id)
            return Results.BadRequest(Result.Failure(["Route ID does not match command ID."]));

        var result = await sender.Send(command);
        return result.Succeeded ? Results.Ok(result) : Results.BadRequest(result);
    }
}
