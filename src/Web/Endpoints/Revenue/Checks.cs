using ERP_Government.Application.Revenue.Common.DTOs;
using ERP_Government.Application.Revenue.Queries.Checks.GetChecks;
using ERP_Government.Application.Revenue.Queries.Checks.GetCheckById;
using ERP_Government.Application.Revenue.Commands.Checks.ClearCheck;
using ERP_Government.Application.Revenue.Commands.Checks.BounceCheck;
using ERP_Government.Application.Revenue.Commands.Checks.ReplaceCheck;
using ERP_Government.Application.Common.Security;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ERP_Government.Web.Endpoints.Revenue;

public class Checks : IEndpointGroup
{
    public static string? RoutePrefix => "/api/Revenue/Checks";

    public static void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapGet("/", GetChecks)
            .Produces<List<CheckDto>>()
            .RequireAuthorization(PermissionCodes.ChecksView);

        groupBuilder.MapGet("/{id:int}", GetCheckById)
            .Produces<CheckDetailDto>()
            .RequireAuthorization(PermissionCodes.ChecksView);

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
            .RequireAuthorization(PermissionCodes.ChecksReplace);
    }

    [EndpointSummary("Get under-collection checks list")]
    public static async Task<IResult> GetChecks(
        [FromServices] ISender sender,
        [AsParameters] GetChecksQuery query)
    {
        var result = await sender.Send(query);
        if (!result.Succeeded)
            return Results.BadRequest(result.Errors);
        return Results.Ok(result.Value);
    }

    [EndpointSummary("Get single check detail")]
    public static async Task<IResult> GetCheckById(
        [FromServices] ISender sender,
        int id)
    {
        var result = await sender.Send(new GetCheckByIdQuery { Id = id });
        if (!result.Succeeded)
            return Results.BadRequest(result.Errors);
        return Results.Ok(result.Value);
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
