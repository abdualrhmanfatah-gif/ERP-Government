using ERP_Government.Application.Committees.Common.DTOs;
using ERP_Government.Application.Committees.Commands.Committees.CreateCommittee;
using ERP_Government.Application.Committees.Commands.Committees.UpdateCommittee;
using ERP_Government.Application.Committees.Commands.Committees.ActivateCommittee;
using ERP_Government.Application.Committees.Commands.Committees.DeactivateCommittee;
using ERP_Government.Application.Committees.Commands.Committees.DissolveCommittee;
using ERP_Government.Application.Committees.Queries.Committees.GetCommitteeById;
using ERP_Government.Application.Committees.Queries.Committees.GetCommittees;
using ERP_Government.Application.Common.Security;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ERP_Government.Web.Endpoints.Committees;

public class Committees : IEndpointGroup
{
    public static void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapGet("/", GetCommittees)
            .Produces<List<CommitteeDto>>()
            .RequireAuthorization(PermissionCodes.CommitteesView);

        groupBuilder.MapGet("/{id:int}", GetCommitteeById)
            .Produces<CommitteeDto?>()
            .RequireAuthorization(PermissionCodes.CommitteesView);

        groupBuilder.MapPost("/", CreateCommittee)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .RequireAuthorization(PermissionCodes.CommitteesCreate);

        groupBuilder.MapPut("/{id:int}", UpdateCommittee)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .RequireAuthorization(PermissionCodes.CommitteesUpdate);

        groupBuilder.MapPost("/{id:int}/activate", ActivateCommittee)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .RequireAuthorization(PermissionCodes.CommitteesActivate);

        groupBuilder.MapPost("/{id:int}/deactivate", DeactivateCommittee)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .RequireAuthorization(PermissionCodes.CommitteesDeactivate);

        groupBuilder.MapPost("/{id:int}/dissolve", DissolveCommittee)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .RequireAuthorization(PermissionCodes.CommitteesDissolve);
    }

    [EndpointSummary("Get all committees")]
    public static async Task<List<CommitteeDto>> GetCommittees(
        [FromServices] ISender sender,
        [AsParameters] GetCommitteesQuery query)
    {
        return await sender.Send(query);
    }

    [EndpointSummary("Get committee by ID")]
    public static async Task<IResult> GetCommitteeById(
        [FromServices] ISender sender,
        int id)
    {
        var result = await sender.Send(new GetCommitteeByIdQuery { Id = id });
        return result.Succeeded ? Results.Ok(result.Value!) : result.ToProblemDetails();
    }

    [EndpointSummary("Create a new committee")]
    public static async Task<IResult> CreateCommittee(
        [FromServices] ISender sender,
        [FromBody] CreateCommitteeCommand command)
    {
        var result = await sender.Send(command);
        if (!result.Succeeded)
            return result.ToProblemDetails();
        return Results.NoContent();
    }

    [EndpointSummary("Update a committee")]
    public static async Task<IResult> UpdateCommittee(
        [FromServices] ISender sender,
        int id,
        [FromBody] UpdateCommitteeCommand command)
    {
        if (id != command.Id)
            return Results.Problem(
                detail: "ID mismatch.",
                statusCode: StatusCodes.Status400BadRequest,
                title: "Bad Request",
                type: "about:blank");

        var result = await sender.Send(command);
        if (!result.Succeeded)
            return result.ToProblemDetails();
        return Results.NoContent();
    }

    [EndpointSummary("Activate a committee")]
    public static async Task<IResult> ActivateCommittee(
        [FromServices] ISender sender,
        int id,
        [FromBody] ActivateCommitteeCommand command)
    {
        if (id != command.Id)
            return Results.Problem(
                detail: "ID mismatch.",
                statusCode: StatusCodes.Status400BadRequest,
                title: "Bad Request",
                type: "about:blank");

        var result = await sender.Send(command);
        if (!result.Succeeded)
            return result.ToProblemDetails();
        return Results.NoContent();
    }

    [EndpointSummary("Deactivate a committee")]
    public static async Task<IResult> DeactivateCommittee(
        [FromServices] ISender sender,
        int id,
        [FromBody] DeactivateCommitteeCommand command)
    {
        if (id != command.Id)
            return Results.Problem(
                detail: "ID mismatch.",
                statusCode: StatusCodes.Status400BadRequest,
                title: "Bad Request",
                type: "about:blank");

        var result = await sender.Send(command);
        if (!result.Succeeded)
            return result.ToProblemDetails();
        return Results.NoContent();
    }

    [EndpointSummary("Dissolve a committee")]
    public static async Task<IResult> DissolveCommittee(
        [FromServices] ISender sender,
        int id,
        [FromBody] DissolveCommitteeCommand command)
    {
        if (id != command.Id)
            return Results.Problem(
                detail: "ID mismatch.",
                statusCode: StatusCodes.Status400BadRequest,
                title: "Bad Request",
                type: "about:blank");

        var result = await sender.Send(command);
        if (!result.Succeeded)
            return result.ToProblemDetails();
        return Results.NoContent();
    }
}
