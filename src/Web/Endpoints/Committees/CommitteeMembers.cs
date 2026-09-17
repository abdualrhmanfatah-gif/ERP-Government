using ERP_Government.Application.Committees.Common.DTOs;
using ERP_Government.Application.Committees.Commands.CommitteeMembers.AddCommitteeMember;
using ERP_Government.Application.Committees.Commands.CommitteeMembers.UpdateCommitteeMember;
using ERP_Government.Application.Committees.Commands.CommitteeMembers.RemoveCommitteeMember;
using ERP_Government.Application.Committees.Commands.CommitteeMembers.ActivateMember;
using ERP_Government.Application.Committees.Queries.CommitteeMembers.GetCommitteeMemberById;
using ERP_Government.Application.Committees.Queries.CommitteeMembers.GetCommitteeMembers;
using ERP_Government.Application.Common.Security;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ERP_Government.Web.Endpoints.Committees;

public class CommitteeMembers : IEndpointGroup
{
    public static void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapGet("/", GetCommitteeMembers)
            .Produces<List<CommitteeMemberDto>>()
            .RequireAuthorization(PermissionCodes.CommitteeMembersView);

        groupBuilder.MapGet("/{id:int}", GetCommitteeMemberById)
            .Produces<CommitteeMemberDto?>()
            .RequireAuthorization(PermissionCodes.CommitteeMembersView);

        groupBuilder.MapPost("/", AddCommitteeMember)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .RequireAuthorization(PermissionCodes.CommitteeMembersAdd);

        groupBuilder.MapPut("/{id:int}", UpdateCommitteeMember)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .RequireAuthorization(PermissionCodes.CommitteeMembersAdd);

        groupBuilder.MapPost("/{id:int}/deactivate", RemoveCommitteeMember)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .RequireAuthorization(PermissionCodes.CommitteeMembersRemove);

        groupBuilder.MapPost("/{id:int}/activate", ActivateMember)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .RequireAuthorization(PermissionCodes.CommitteeMembersAdd);
    }

    [EndpointSummary("Get committee members")]
    public static async Task<List<CommitteeMemberDto>> GetCommitteeMembers(
        [FromServices] ISender sender,
        [AsParameters] GetCommitteeMembersQuery query)
    {
        return await sender.Send(query);
    }

    [EndpointSummary("Get committee member by ID")]
    public static async Task<IResult> GetCommitteeMemberById(
        [FromServices] ISender sender,
        int id)
    {
        var result = await sender.Send(new GetCommitteeMemberByIdQuery { Id = id });
        return result.Succeeded ? Results.Ok(result.Value!) : result.ToProblemDetails();
    }

    [EndpointSummary("Add a member to a committee")]
    public static async Task<IResult> AddCommitteeMember(
        [FromServices] ISender sender,
        [FromBody] AddCommitteeMemberCommand command)
    {
        var result = await sender.Send(command);
        if (!result.Succeeded)
            return result.ToProblemDetails();
        return Results.NoContent();
    }

    [EndpointSummary("Update a committee member")]
    public static async Task<IResult> UpdateCommitteeMember(
        [FromServices] ISender sender,
        int id,
        [FromBody] UpdateCommitteeMemberCommand command)
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

    [EndpointSummary("Deactivate a committee member")]
    public static async Task<IResult> RemoveCommitteeMember(
        [FromServices] ISender sender,
        int id,
        [FromBody] RemoveCommitteeMemberCommand command)
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

    [EndpointSummary("Activate a committee member")]
    public static async Task<IResult> ActivateMember(
        [FromServices] ISender sender,
        int id,
        [FromBody] ActivateMemberCommand command)
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
