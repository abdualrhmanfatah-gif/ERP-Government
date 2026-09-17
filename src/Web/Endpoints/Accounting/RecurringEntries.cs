using ERP_Government.Application.Accounting.Commands.RecurringEntries.CancelRecurringEntry;
using ERP_Government.Application.Accounting.Commands.RecurringEntries.CreateRecurringEntry;
using ERP_Government.Application.Accounting.Commands.RecurringEntries.PauseRecurringEntry;
using ERP_Government.Application.Accounting.Commands.RecurringEntries.ResumeRecurringEntry;
using ERP_Government.Application.Accounting.Common;
using ERP_Government.Application.Accounting.Queries.RecurringEntries.GetRecurringEntryById;
using ERP_Government.Application.Accounting.Queries.RecurringEntries.GetRecurringEntriesList;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ERP_Government.Web.Endpoints.Accounting;

public class RecurringEntries : IEndpointGroup
{
    public static void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapGet("/", GetRecurringEntriesList)
            .Produces<List<RecurringEntryDto>>()
            .RequireAuthorization("Accounting.RecurringEntries.Read");

        groupBuilder.MapGet("/{id:int}", GetRecurringEntryById)
            .Produces<RecurringEntryDto?>()
            .RequireAuthorization("Accounting.RecurringEntries.Read");

        groupBuilder.MapPost("/", CreateRecurringEntry)
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .RequireAuthorization("Accounting.RecurringEntries.Create");

        groupBuilder.MapPost("/{id:int}/pause", PauseRecurringEntry)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .RequireAuthorization("Accounting.RecurringEntries.Pause");

        groupBuilder.MapPost("/{id:int}/resume", ResumeRecurringEntry)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .RequireAuthorization("Accounting.RecurringEntries.Resume");

        groupBuilder.MapPost("/{id:int}/cancel", CancelRecurringEntry)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .RequireAuthorization("Accounting.RecurringEntries.Cancel");
    }

    [EndpointSummary("Get all recurring entries")]
    public static async Task<List<RecurringEntryDto>> GetRecurringEntriesList(
        [FromServices] ISender sender,
        [AsParameters] GetRecurringEntriesListQuery query)
    {
        return await sender.Send(query);
    }

    [EndpointSummary("Get recurring entry by ID")]
    public static async Task<IResult> GetRecurringEntryById(
        [FromServices] ISender sender,
        int id)
    {
        var result = await sender.Send(new GetRecurringEntryByIdQuery { Id = id });
        return result.Succeeded ? Results.Ok(result.Value!) : result.ToProblemDetails();
    }

    [EndpointSummary("Create a new recurring entry")]
    public static async Task<IResult> CreateRecurringEntry(
        [FromServices] ISender sender,
        [FromBody] CreateRecurringEntryCommand command)
    {
        var result = await sender.Send(command);
        if (!result.Succeeded)
            return result.ToProblemDetails();
        return Results.Ok();
    }

    [EndpointSummary("Pause an active recurring entry")]
    public static async Task<IResult> PauseRecurringEntry(
        [FromServices] ISender sender,
        int id,
        [FromBody] PauseRecurringEntryCommand command)
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

    [EndpointSummary("Resume a paused recurring entry")]
    public static async Task<IResult> ResumeRecurringEntry(
        [FromServices] ISender sender,
        int id,
        [FromBody] ResumeRecurringEntryCommand command)
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

    [EndpointSummary("Cancel a recurring entry")]
    public static async Task<IResult> CancelRecurringEntry(
        [FromServices] ISender sender,
        int id,
        [FromBody] CancelRecurringEntryCommand command)
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
