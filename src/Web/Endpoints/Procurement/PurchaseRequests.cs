using ERP_Government.Application.Common.Models;
using ERP_Government.Application.Procurement.Commands.PurchaseRequests.ApprovePurchaseRequest;
using ERP_Government.Application.Procurement.Commands.PurchaseRequests.CancelPurchaseRequest;
using ERP_Government.Application.Procurement.Commands.PurchaseRequests.CreatePurchaseRequest;
using ERP_Government.Application.Procurement.Commands.PurchaseRequests.RejectPurchaseRequest;
using ERP_Government.Application.Procurement.Commands.PurchaseRequests.SubmitPurchaseRequest;
using ERP_Government.Application.Procurement.Commands.PurchaseRequests.UpdatePurchaseRequest;
using ERP_Government.Application.Procurement.Queries.PurchaseRequests.GetPurchaseRequestById;
using ERP_Government.Application.Procurement.Queries.PurchaseRequests.GetPurchaseRequests;
using ERP_Government.Web.Infrastructure;

namespace ERP_Government.Web.Endpoints.Procurement;

public class PurchaseRequests : IEndpointGroup
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapGet("/", HandleGetAll)
            .Produces<IReadOnlyList<PurchaseRequestListItem>>();
        group.MapGet("/{id:int}", HandleGetById)
            .Produces<PurchaseRequestDetailResponse?>();
        group.MapPost("/", HandleCreate)
            .Produces<int>();
        group.MapPut("/{id:int}", HandleUpdate)
            .Produces<Result>();
        group.MapPatch("/{id:int}/submit", HandleSubmit)
            .Produces<Result>();
        group.MapPatch("/{id:int}/approve", HandleApprove)
            .Produces<Result>();
        group.MapPatch("/{id:int}/reject", HandleReject)
            .Produces<Result>();
        group.MapPatch("/{id:int}/cancel", HandleCancel)
            .Produces<Result>();
    }

    private static async Task<IResult> HandleGetAll(
        ISender sender,
        ERP_Government.Domain.Procurement.Enums.PurchaseRequestStatus? status = null,
        ERP_Government.Domain.Procurement.Enums.PurchaseRequestPriority? priority = null,
        string? search = null,
        int page = 1,
        int pageSize = 20)
    {
        var result = await sender.Send(new GetPurchaseRequestsQuery(status, priority, search, page, pageSize));
        return result.Succeeded ? Results.Ok(result.Value) : Results.BadRequest(result.Errors);
    }

    private static async Task<IResult> HandleGetById(ISender sender, int id)
    {
        var result = await sender.Send(new GetPurchaseRequestByIdQuery(id));
        return result.Succeeded ? Results.Ok(result.Value) : Results.NotFound();
    }

    private static async Task<IResult> HandleCreate(ISender sender, CreatePurchaseRequestCommand command)
    {
        var result = await sender.Send(command);
        return result.Succeeded
            ? Results.Created($"/api/PurchaseRequests/{result.Value}", result.Value)
            : Results.BadRequest(result.Errors);
    }

    private static async Task<IResult> HandleUpdate(ISender sender, int id, UpdatePurchaseRequestCommand command)
    {
        if (id != command.Id) return Results.BadRequest(new[] { "ID mismatch." });
        var result = await sender.Send(command);
        return result.Succeeded ? Results.Ok() : Results.BadRequest(result.Errors);
    }

    private static async Task<IResult> HandleSubmit(ISender sender, int id)
    {
        var result = await sender.Send(new SubmitPurchaseRequestCommand(id));
        return result.Succeeded ? Results.Ok() : Results.BadRequest(result.Errors);
    }

    private static async Task<IResult> HandleApprove(ISender sender, int id)
    {
        var result = await sender.Send(new ApprovePurchaseRequestCommand(id));
        return result.Succeeded ? Results.Ok() : Results.BadRequest(result.Errors);
    }

    private static async Task<IResult> HandleReject(ISender sender, int id, RejectPurchaseRequestRequest request)
    {
        var result = await sender.Send(new RejectPurchaseRequestCommand(id, request.Reason));
        return result.Succeeded ? Results.Ok() : Results.BadRequest(result.Errors);
    }

    private static async Task<IResult> HandleCancel(ISender sender, int id, CancelPurchaseRequestRequest request)
    {
        var result = await sender.Send(new CancelPurchaseRequestCommand(id, request.Reason));
        return result.Succeeded ? Results.Ok() : Results.BadRequest(result.Errors);
    }
}

public record RejectPurchaseRequestRequest(string Reason);
public record CancelPurchaseRequestRequest(string? Reason);
