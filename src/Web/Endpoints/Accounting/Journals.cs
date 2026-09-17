using ERP_Government.Application.Accounting.Commands.Journals.CreateJournal;
using ERP_Government.Application.Accounting.Commands.Journals.UpdateJournal;
using ERP_Government.Application.Accounting.Common;
using ERP_Government.Application.Accounting.Queries.Journals.GetJournalById;
using ERP_Government.Application.Accounting.Queries.Journals.GetJournalsList;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ERP_Government.Web.Endpoints.Accounting;

public class Journals : IEndpointGroup
{
    public static void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapGet("/", GetJournalsList)
            .Produces<List<JournalDto>>()
            .RequireAuthorization("Accounting.Journals.Read");

        groupBuilder.MapGet("/{id:int}", GetJournalById)
            .Produces<JournalDto?>()
            .RequireAuthorization("Accounting.Journals.Read");

        groupBuilder.MapPost("/", CreateJournal)
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .RequireAuthorization("Accounting.Journals.Create");

        groupBuilder.MapPut("/{id:int}", UpdateJournal)
            .Produces(StatusCodes.Status204NoContent)
            .RequireAuthorization("Accounting.Journals.Edit");
    }

    [EndpointSummary("Get all journals")]
    public static async Task<List<JournalDto>> GetJournalsList(
        [FromServices] ISender sender,
        [AsParameters] GetJournalsListQuery query)
    {
        return await sender.Send(query);
    }

    [EndpointSummary("Get journal by ID")]
    public static async Task<IResult> GetJournalById(
        [FromServices] ISender sender,
        int id)
    {
        var result = await sender.Send(new GetJournalByIdQuery { Id = id });
        return result.Succeeded ? Results.Ok(result.Value!) : result.ToProblemDetails();
    }

    [EndpointSummary("Create a new journal")]
    public static async Task<IResult> CreateJournal(
        [FromServices] ISender sender,
        [FromBody] CreateJournalCommand command)
    {
        var result = await sender.Send(command);
        if (!result.Succeeded)
            return result.ToProblemDetails();
        return Results.Ok();
    }

    [EndpointSummary("Update an existing journal")]
    public static async Task<IResult> UpdateJournal(
        [FromServices] ISender sender,
        int id,
        [FromBody] UpdateJournalCommand command)
    {
        if (id != command.Id)
            return Results.Problem(
                detail: "ID mismatch.",
                statusCode: StatusCodes.Status400BadRequest,
                title: "Bad Request",
                type: "about:blank");

        await sender.Send(command);
        return Results.NoContent();
    }
}
