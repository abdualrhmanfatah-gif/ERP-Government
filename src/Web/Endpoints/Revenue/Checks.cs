using ERP_Government.Application.Common.Models;
using ERP_Government.Application.Common.Security;
using ERP_Government.Application.Revenue.Commands.Checks.BounceCheck;
using ERP_Government.Application.Revenue.Commands.Checks.ClearCheck;
using ERP_Government.Web.Infrastructure;
using MediatR;

namespace ERP_Government.Web.Endpoints.Revenue;

public class Checks : IEndpointGroup
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapPost("/{id:int}/clear", HandleClear)
            .RequireAuthorization(PermissionCodes.ChecksClear)
            .Produces<Result>();
        group.MapPost("/{id:int}/bounce", HandleBounce)
            .RequireAuthorization(PermissionCodes.ChecksBounce)
            .Produces<Result>();
    }

    private static async Task<IResult> HandleClear(
        ISender sender,
        int id,
        ClearCheckCommand command)
    {
        if (id != command.Id)
            return Results.BadRequest(Result.Failure(["Route ID does not match command ID."]));

        var result = await sender.Send(command);
        return result.Succeeded ? Results.Ok(result) : Results.BadRequest(result);
    }

    private static async Task<IResult> HandleBounce(
        ISender sender,
        int id,
        BounceCheckCommand command)
    {
        if (id != command.Id)
            return Results.BadRequest(Result.Failure(["Route ID does not match command ID."]));

        var result = await sender.Send(command);
        return result.Succeeded ? Results.Ok(result) : Results.BadRequest(result);
    }
}
