using ERP_Government.Application.Common.Models;
using ERP_Government.Application.Procurement.Commands.GoodsReceiptNotes.ConfirmGRN;

using ERP_Government.Application.Procurement.Commands.GoodsReceiptNotes.CreateGRN;
using ERP_Government.Application.Procurement.Commands.GoodsReceiptNotes.RejectGRN;
using ERP_Government.Application.Procurement.Queries.GoodsReceiptNotes.GetGRNById;
using ERP_Government.Application.Procurement.Queries.GoodsReceiptNotes.GetGRNs;
using ERP_Government.Web.Infrastructure;
using ERP_Government.Application.Common.Security;

namespace ERP_Government.Web.Endpoints.Procurement;

public class GoodsReceiptNotes : IEndpointGroup
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapGet("/", HandleGetAll)
            .RequireAuthorization(PermissionCodes.GoodsReceiptsView)
            .Produces<object>();
        group.MapGet("/{id:int}", HandleGetById)
            .RequireAuthorization(PermissionCodes.GoodsReceiptsView)
            .Produces<GRNDetailResponse>();
        group.MapPost("/", HandleCreate)
            .RequireAuthorization(PermissionCodes.GoodsReceiptsCreate)
            .Produces<int>();
        group.MapPatch("/{id:int}/confirm", HandleConfirm)
            .RequireAuthorization(PermissionCodes.GoodsReceiptsConfirm)
            .Produces<Result>();
        group.MapPatch("/{id:int}/reject", HandleReject)
            .RequireAuthorization(PermissionCodes.GoodsReceiptsReject)
            .Produces<Result>();
    }

    private static async Task<IResult> HandleGetAll(
        ISender sender,
        int? purchaseOrderId = null,
        ERP_Government.Domain.Procurement.Enums.GRNStatus? status = null,
        string? search = null,
        int page = 1,
        int pageSize = 20)
    {
        var result = await sender.Send(new GetGRNsQuery(purchaseOrderId, status, search, page, pageSize));
        return result.Succeeded ? Results.Ok(result.Value) : result.ToProblemDetails();
    }

    private static async Task<IResult> HandleGetById(ISender sender, int id)
    {
        var result = await sender.Send(new GetGRNByIdQuery(id));
        return result.ToProblemDetails();
    }

    private static async Task<IResult> HandleCreate(ISender sender, CreateGRNCommand command)
    {
        var result = await sender.Send(command);
        return result.Succeeded
            ? Results.Created($"/api/GoodsReceiptNotes/{result.Value}", result.Value)
            : result.ToProblemDetails();
    }

    private static async Task<IResult> HandleConfirm(ISender sender, int id)
    {
        var result = await sender.Send(new ConfirmGRNCommand(id));
        return result.Succeeded ? Results.Ok() : result.ToProblemDetails();
    }

    private static async Task<IResult> HandleReject(ISender sender, int id, RejectGRNCommand command)
    {
        var result = await sender.Send(command with { Id = id });
        return result.Succeeded ? Results.Ok() : result.ToProblemDetails();
    }
}
