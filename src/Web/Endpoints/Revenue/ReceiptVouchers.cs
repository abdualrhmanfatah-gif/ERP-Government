using ERP_Government.Application.Common.Models;
using ERP_Government.Application.Common.Security;
using ERP_Government.Application.Revenue.Commands.ReceiptVouchers.ApproveReceiptVoucher;
using ERP_Government.Application.Revenue.Commands.ReceiptVouchers.CancelReceiptVoucher;
using ERP_Government.Application.Revenue.Commands.ReceiptVouchers.CreateReceiptVoucher;
using ERP_Government.Application.Revenue.Commands.ReceiptVouchers.UpdateReceiptVoucher;
using ERP_Government.Application.Revenue.Common.DTOs;
using ERP_Government.Application.Revenue.Queries.ReceiptVouchers.GetReceiptVoucherById;
using ERP_Government.Application.Revenue.Queries.ReceiptVouchers.GetReceiptVouchers;
using ERP_Government.Domain.Revenue.Enums;
using ERP_Government.Web.Infrastructure;
using MediatR;

namespace ERP_Government.Web.Endpoints.Revenue;

public class ReceiptVouchers : IEndpointGroup
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapGet("/", HandleGetAll)
            .RequireAuthorization(PermissionCodes.ReceiptVouchersView)
            .Produces<List<ReceiptVoucherDto>>();
        group.MapGet("/{id:int}", HandleGetById)
            .RequireAuthorization(PermissionCodes.ReceiptVouchersView)
            .Produces<ReceiptVoucherDto>();
        group.MapPost("/", HandleCreate)
            .RequireAuthorization(PermissionCodes.ReceiptVouchersCreate)
            .Produces<Result<ReceiptVoucherDto>>();
        group.MapPut("/{id:int}", HandleUpdate)
            .RequireAuthorization(PermissionCodes.ReceiptVouchersUpdate)
            .Produces<Result<ReceiptVoucherDto>>();
        group.MapPost("/{id:int}/approve", HandleApprove)
            .RequireAuthorization(PermissionCodes.ReceiptVouchersApprove)
            .Produces<Result>();
        group.MapPost("/{id:int}/cancel", HandleCancel)
            .RequireAuthorization(PermissionCodes.ReceiptVouchersCancel)
            .Produces<Result>();
    }

    private static async Task<IResult> HandleGetAll(
        ISender sender,
        int? collectionOrderId = null,
        int? partyId = null,
        ReceiptVoucherStatus? status = null)
    {
        var result = await sender.Send(new GetReceiptVouchersQuery { CollectionOrderId = collectionOrderId, PartyId = partyId, Status = status });
        return Results.Ok(result);
    }

    private static async Task<IResult> HandleGetById(
        ISender sender,
        int id)
    {
        var result = await sender.Send(new GetReceiptVoucherByIdQuery { Id = id });
        return result.Succeeded
            ? Results.Ok(result.Value!)
            : result.ToProblemDetails();
    }

    private static async Task<IResult> HandleCreate(
        ISender sender,
        CreateReceiptVoucherCommand command)
    {
        var result = await sender.Send(command);
        return result.Succeeded ? Results.Ok(result) : Results.BadRequest(result);
    }

    private static async Task<IResult> HandleUpdate(
        ISender sender,
        int id,
        UpdateReceiptVoucherCommand command)
    {
        if (id != command.Id)
            return Results.BadRequest(Result.Failure(["Route ID does not match command ID."]));

        var result = await sender.Send(command);
        return result.Succeeded ? Results.Ok(result) : Results.BadRequest(result);
    }

    private static async Task<IResult> HandleApprove(
        ISender sender,
        int id,
        ApproveReceiptVoucherCommand command)
    {
        if (id != command.Id)
            return Results.BadRequest(Result.Failure(["Route ID does not match command ID."]));

        var result = await sender.Send(command);
        return result.Succeeded ? Results.Ok(result) : Results.BadRequest(result);
    }

    private static async Task<IResult> HandleCancel(
        ISender sender,
        int id,
        CancelReceiptVoucherCommand command)
    {
        if (id != command.Id)
            return Results.BadRequest(Result.Failure(["Route ID does not match command ID."]));

        var result = await sender.Send(command);
        return result.Succeeded ? Results.Ok(result) : Results.BadRequest(result);
    }
}
