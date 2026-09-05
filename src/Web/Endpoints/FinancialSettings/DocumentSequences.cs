using ERP_Government.Application.FinancialSettings.Common.DTOs;
using ERP_Government.Application.FinancialSettings.Commands.DocumentSequences.CreateDocumentSequence;
using ERP_Government.Application.FinancialSettings.Commands.DocumentSequences.UpdateDocumentSequence;
using ERP_Government.Application.FinancialSettings.Queries.DocumentSequences.GetDocumentSequenceById;
using ERP_Government.Application.FinancialSettings.Queries.DocumentSequences.GetDocumentSequences;
using ERP_Government.Application.Common.Security;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ERP_Government.Web.Endpoint.FinancialSettings;

public class DocumentSequences : IEndpointGroup
{
    public static void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapGet("/", GetDocumentSequences)
            .Produces<List<DocumentSequenceDto>>()
            .RequireAuthorization(PermissionCodes.DocumentSequencesView);

        groupBuilder.MapGet("/{id:int}", GetDocumentSequenceById)
            .Produces<DocumentSequenceDto?>()
            .RequireAuthorization(PermissionCodes.DocumentSequencesView);

        groupBuilder.MapPost("/", CreateDocumentSequence)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .RequireAuthorization(PermissionCodes.DocumentSequencesCreate);

        groupBuilder.MapPut("/{id:int}", UpdateDocumentSequence)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .RequireAuthorization(PermissionCodes.DocumentSequencesCreate);

        groupBuilder.MapPost("/{id:int}/deactivate", DeactivateDocumentSequence)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .RequireAuthorization(PermissionCodes.DocumentSequencesCreate);
    }

    [EndpointSummary("Get all document sequences")]
    public static async Task<List<DocumentSequenceDto>> GetDocumentSequences(
        [FromServices] ISender sender,
        [AsParameters] GetDocumentSequencesQuery query)
    {
        return await sender.Send(query);
    }

    [EndpointSummary("Get document sequence by ID")]
    public static async Task<DocumentSequenceDto?> GetDocumentSequenceById(
        [FromServices] ISender sender,
        int id)
    {
        return await sender.Send(new GetDocumentSequenceByIdQuery { Id = id });
    }

    [EndpointSummary("Create a new document sequence")]
    public static async Task<IResult> CreateDocumentSequence(
        [FromServices] ISender sender,
        [FromBody] CreateDocumentSequenceCommand command)
    {
        var result = await sender.Send(command);
        if (!result.Succeeded)
            return Results.BadRequest(result.Errors);
        return Results.NoContent();
    }

    [EndpointSummary("Update a document sequence")]
    public static async Task<IResult> UpdateDocumentSequence(
        [FromServices] ISender sender,
        int id,
        [FromBody] UpdateDocumentSequenceCommand command)
    {
        if (id != command.Id)
            return Results.BadRequest("ID mismatch.");

        var result = await sender.Send(command);
        if (!result.Succeeded)
            return Results.BadRequest(result.Errors);
        return Results.NoContent();
    }

    [EndpointSummary("Deactivate a document sequence")]
    public static async Task<IResult> DeactivateDocumentSequence(
        [FromServices] ISender sender,
        int id)
    {
        var result = await sender.Send(new UpdateDocumentSequenceCommand { Id = id, IsActive = false });
        if (!result.Succeeded)
            return Results.BadRequest(result.Errors);
        return Results.NoContent();
    }
}
