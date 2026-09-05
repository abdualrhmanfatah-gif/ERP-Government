using ERP_Government.Application.Revenue.Common.DTOs;
using ERP_Government.Application.Revenue.Commands.Checks.ClearCheck;
using ERP_Government.Application.Revenue.Commands.Checks.BounceCheck;
using ERP_Government.Application.Revenue.Commands.Checks.ReplaceCheck;
using ERP_Government.Application.Common.Security;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ERP_Government.Web.Endpoints.Revenue;

public class Checks : IEndpointGroup
{
    public static void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapPost("/{id:int}/clear", ClearCheck)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .RequireAuthorization(PermissionCodes.ChecksClear);

        groupBuilder.MapPost("/{id:int}/bounce", BounceCheck)
            .Produces<int>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .RequireAuthorization(PermissionCodes.ChecksBounce);

        groupBuilder.MapPost("/{id:int}/replace", ReplaceCheck)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .RequireAuthorization(PermissionCodes.ChecksClear);
    }

    [EndpointSummary("Clear a check (bank confirmation)")]
    public static async Task<IResult> ClearCheck(
        [FromServices] ISender sender,
        int id,
        [FromBody] ClearCheckCommand command)
    {
        if (id != command.Id)
            return Results.BadRequest("ID mismatch.");

        var result = await sender.Send(command);
        if (!result.Succeeded)
            return Results.BadRequest(result.Errors);
        return Results.NoContent();
    }

    [EndpointSummary("Bounce a check (bank notification)")]
    public static async Task<IResult> BounceCheck(
        [FromServices] ISender sender,
        int id,
        [FromBody] BounceCheckCommand command)
    {
        if (id != command.Id)
            return Results.BadRequest("ID mismatch.");

        var result = await sender.Send(command);
        if (!result.Succeeded)
            return Results.BadRequest(result.Errors);
        return Results.Ok(result.Value);
    }

    [EndpointSummary("Replace a bounced check with new payment")]
    public static async Task<IResult> ReplaceCheck(
        [FromServices] ISender sender,
        int id,
        [FromBody] ReplaceCheckCommand command)
    {
        if (id != command.CheckId)
            return Results.BadRequest("ID mismatch.");

        var result = await sender.Send(command);
        if (!result.Succeeded)
            return Results.BadRequest(result.Errors);
        return Results.NoContent();
    }
}
