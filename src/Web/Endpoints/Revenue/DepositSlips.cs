using ERP_Government.Application.Common.Models;
using ERP_Government.Application.Common.Security;
using ERP_Government.Application.Revenue.Commands.DepositSlips.ApproveDepositSlip47;
using ERP_Government.Application.Revenue.Commands.DepositSlips.ApproveDepositSlip48;
using ERP_Government.Application.Revenue.Commands.DepositSlips.CreateDepositSlip47;
using ERP_Government.Application.Revenue.Commands.DepositSlips.CreateDepositSlip48;
using ERP_Government.Application.Revenue.Common.DTOs;
using ERP_Government.Web.Infrastructure;
using MediatR;

namespace ERP_Government.Web.Endpoints.Revenue;

public class DepositSlips : IEndpointGroup
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapPost("/slip47", HandleCreateSlip47)
            .RequireAuthorization(PermissionCodes.DepositSlipsCreate)
            .Produces<Result<DepositSlip47Dto>>();
        group.MapPost("/slip47/{id:int}/approve", HandleApproveSlip47)
            .RequireAuthorization(PermissionCodes.DepositSlipsApprove)
            .Produces<Result>();
        group.MapPost("/slip48", HandleCreateSlip48)
            .RequireAuthorization(PermissionCodes.DepositSlipsCreate)
            .Produces<Result<DepositSlip48Dto>>();
        group.MapPost("/slip48/{id:int}/approve", HandleApproveSlip48)
            .RequireAuthorization(PermissionCodes.DepositSlipsApprove)
            .Produces<Result>();
    }

    private static async Task<IResult> HandleCreateSlip47(
        ISender sender,
        CreateDepositSlip47Command command)
    {
        var result = await sender.Send(command);
        return result.Succeeded ? Results.Ok(result) : Results.BadRequest(result);
    }

    private static async Task<IResult> HandleApproveSlip47(
        ISender sender,
        int id,
        ApproveDepositSlip47Command command)
    {
        if (id != command.Id)
            return Results.BadRequest(Result.Failure(["Route ID does not match command ID."]));

        var result = await sender.Send(command);
        return result.Succeeded ? Results.Ok(result) : Results.BadRequest(result);
    }

    private static async Task<IResult> HandleCreateSlip48(
        ISender sender,
        CreateDepositSlip48Command command)
    {
        var result = await sender.Send(command);
        return result.Succeeded ? Results.Ok(result) : Results.BadRequest(result);
    }

    private static async Task<IResult> HandleApproveSlip48(
        ISender sender,
        int id,
        ApproveDepositSlip48Command command)
    {
        if (id != command.Id)
            return Results.BadRequest(Result.Failure(["Route ID does not match command ID."]));

        var result = await sender.Send(command);
        return result.Succeeded ? Results.Ok(result) : Results.BadRequest(result);
    }
}
