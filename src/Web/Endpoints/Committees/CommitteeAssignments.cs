using ERP_Government.Application.Committees.Common.DTOs;
using ERP_Government.Application.Committees.Commands.CommitteeAssignments.CreateCommitteeAssignment;
using ERP_Government.Application.Committees.Commands.CommitteeAssignments.UpdateAssignmentStatus;
using ERP_Government.Application.Committees.Commands.CommitteeAssignments.RecordSignature;
using ERP_Government.Application.Committees.Queries.CommitteeAssignments.GetCommitteeAssignmentById;
using ERP_Government.Application.Committees.Queries.CommitteeAssignments.GetCommitteeAssignments;
using ERP_Government.Application.Common.Security;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ERP_Government.Web.Endpoints.Committees;

public class CommitteeAssignments : IEndpointGroup
{
    public static void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapGet("/", GetCommitteeAssignments)
            .Produces<List<CommitteeAssignmentDto>>()
            .RequireAuthorization(PermissionCodes.CommitteeAssignmentsView);

        groupBuilder.MapGet("/{id:int}", GetCommitteeAssignmentById)
            .Produces<CommitteeAssignmentDto?>()
            .RequireAuthorization(PermissionCodes.CommitteeAssignmentsView);

        groupBuilder.MapPost("/", CreateCommitteeAssignment)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .RequireAuthorization(PermissionCodes.CommitteeAssignmentsCreate);

        groupBuilder.MapPut("/{id:int}", UpdateAssignmentStatus)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .RequireAuthorization(PermissionCodes.CommitteeAssignmentsComplete);

        groupBuilder.MapPost("/{id:int}/signature", RecordSignature)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .RequireAuthorization(PermissionCodes.CommitteeAssignmentsComplete);
    }

    [EndpointSummary("Get committee assignments")]
    public static async Task<List<CommitteeAssignmentDto>> GetCommitteeAssignments(
        [FromServices] ISender sender,
        [AsParameters] GetCommitteeAssignmentsQuery query)
    {
        return await sender.Send(query);
    }

    [EndpointSummary("Get committee assignment by ID")]
    public static async Task<CommitteeAssignmentDto?> GetCommitteeAssignmentById(
        [FromServices] ISender sender,
        int id)
    {
        return await sender.Send(new GetCommitteeAssignmentByIdQuery { Id = id });
    }

    [EndpointSummary("Create a committee assignment")]
    public static async Task<IResult> CreateCommitteeAssignment(
        [FromServices] ISender sender,
        [FromBody] CreateCommitteeAssignmentCommand command)
    {
        var result = await sender.Send(command);
        if (!result.Succeeded)
            return Results.BadRequest(result.Errors);
        return Results.NoContent();
    }

    [EndpointSummary("Update assignment status")]
    public static async Task<IResult> UpdateAssignmentStatus(
        [FromServices] ISender sender,
        int id,
        [FromBody] UpdateAssignmentStatusCommand command)
    {
        if (id != command.Id)
            return Results.BadRequest("ID mismatch.");

        var result = await sender.Send(command);
        if (!result.Succeeded)
            return Results.BadRequest(result.Errors);
        return Results.NoContent();
    }

    [EndpointSummary("Record a signature on an assignment")]
    public static async Task<IResult> RecordSignature(
        [FromServices] ISender sender,
        int id,
        [FromBody] RecordSignatureCommand command)
    {
        if (id != command.Id)
            return Results.BadRequest("ID mismatch.");

        var result = await sender.Send(command);
        if (!result.Succeeded)
            return Results.BadRequest(result.Errors);
        return Results.NoContent();
    }
}
