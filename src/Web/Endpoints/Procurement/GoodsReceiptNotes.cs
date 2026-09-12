using ERP_Government.Application.Common.Models;
using ERP_Government.Application.Procurement.Commands.GoodsReceiptNotes.ConfirmGRN;

using ERP_Government.Application.Procurement.Commands.GoodsReceiptNotes.CreateGRN;
using ERP_Government.Application.Procurement.Commands.GoodsReceiptNotes.RejectGRN;
using ERP_Government.Application.Procurement.Queries.GoodsReceiptNotes.GetGRNById;
using ERP_Government.Application.Procurement.Queries.GoodsReceiptNotes.GetGRNs;
using ERP_Government.Web.Infrastructure;

namespace ERP_Government.Web.Endpoints.Procurement;

public class GoodsReceiptNotes : IEndpointGroup
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapGet("/", HandleGetAll)
            .Produces<object>();
        group.MapGet("/{id:int}", HandleGetById)
            .Produces<GRNDetailResponse>();
        group.MapPost("/", HandleCreate)
            .Produces<int>();
        group.MapPatch("/{id:int}/confirm", HandleConfirm)
            .Produces<Result>();
        group.MapPatch("/{id:int}/reject", HandleReject)
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
        return result.Succeeded ? Results.Ok(result.Value) : Results.BadRequest(result.Errors);
    }

    private static async Task<IResult> HandleGetById(ISender sender, int id)
    {
        var result = await sender.Send(new GetGRNByIdQuery(id));
        return result.Succeeded ? Results.Ok(result.Value) : Results.NotFound();
    }

    private static async Task<IResult> HandleCreate(ISender sender, CreateGRNCommand command)
    {
        var result = await sender.Send(command);
        return result.Succeeded
            ? Results.Created($"/api/GoodsReceiptNotes/{result.Value}", result.Value)
            : Results.BadRequest(result.Errors);
    }

    private static async Task<IResult> HandleConfirm(ISender sender, int id)
    {
        var result = await sender.Send(new ConfirmGRNCommand(id));
        return result.Succeeded ? Results.Ok() : Results.BadRequest(result.Errors);
    }

    private static async Task<IResult> HandleReject(ISender sender, int id, RejectGRNCommand command)
    {
        var result = await sender.Send(command with { Id = id });
        return result.Succeeded ? Results.Ok() : Results.BadRequest(result.Errors);
    }
}
