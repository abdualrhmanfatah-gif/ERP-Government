using ERP_Government.Application.Accounting.Commands.TemplateLines.CreateTemplateLine;
using ERP_Government.Application.Accounting.Commands.TemplateLines.RemoveTemplateLine;
using ERP_Government.Application.Accounting.Commands.TemplateLines.UpdateTemplateLine;
using ERP_Government.Application.Accounting.Commands.Templates.CreateTemplate;
using ERP_Government.Application.Accounting.Commands.Templates.UpdateTemplate;
using ERP_Government.Application.Accounting.Common;
using ERP_Government.Application.Accounting.Queries.TemplateLines.GetTemplateLines;
using ERP_Government.Application.Accounting.Queries.Templates.GetTemplateById;
using ERP_Government.Application.Accounting.Queries.Templates.GetTemplatesList;
using ERP_Government.Application.Common.Security;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ERP_Government.Web.Endpoints.Accounting;

public class Templates : IEndpointGroup
{
    public static void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapGet("/", GetTemplatesList)
            .Produces<List<JournalEntryTemplateDto>>()
            .RequireAuthorization(PermissionCodes.TemplatesRead);

        groupBuilder.MapGet("/{id:int}", GetTemplateById)
            .Produces<JournalEntryTemplateDto?>()
            .RequireAuthorization(PermissionCodes.TemplatesRead);

        groupBuilder.MapPost("/", CreateTemplate)
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .RequireAuthorization(PermissionCodes.TemplatesCreate);

        groupBuilder.MapPut("/{id:int}", UpdateTemplate)
            .Produces(StatusCodes.Status204NoContent)
            .RequireAuthorization(PermissionCodes.TemplatesUpdate);

        // TemplateLines — US4 (TemplatesUpdate permission, no separate lines code)
        groupBuilder.MapGet("/{id:int}/lines", GetTemplateLines)
            .Produces<List<JournalEntryTemplateLineDto>>()
            .RequireAuthorization(PermissionCodes.TemplatesRead);

        groupBuilder.MapPost("/{id:int}/lines", CreateTemplateLine)
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .RequireAuthorization(PermissionCodes.TemplatesUpdate);

        groupBuilder.MapPut("/{id:int}/lines/{lineId:int}", UpdateTemplateLine)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .RequireAuthorization(PermissionCodes.TemplatesUpdate);

        groupBuilder.MapDelete("/{id:int}/lines/{lineId:int}", RemoveTemplateLine)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .RequireAuthorization(PermissionCodes.TemplatesUpdate);
    }

    [EndpointSummary("Get all templates")]
    public static async Task<List<JournalEntryTemplateDto>> GetTemplatesList(
        [FromServices] ISender sender,
        [AsParameters] GetTemplatesListQuery query)
    {
        return await sender.Send(query);
    }

    [EndpointSummary("Get template by ID")]
    public static async Task<JournalEntryTemplateDto?> GetTemplateById(
        [FromServices] ISender sender,
        int id)
    {
        return await sender.Send(new GetTemplateByIdQuery { Id = id });
    }

    [EndpointSummary("Create a new template")]
    public static async Task<IResult> CreateTemplate(
        [FromServices] ISender sender,
        [FromBody] CreateTemplateCommand command)
    {
        var result = await sender.Send(command);
        if (!result.Succeeded)
            return result.ToProblemDetails();
        return Results.Ok();
    }

    [EndpointSummary("Update an existing template")]
    public static async Task<IResult> UpdateTemplate(
        [FromServices] ISender sender,
        int id,
        [FromBody] UpdateTemplateCommand command)
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

    [EndpointSummary("Get template lines")]
    public static async Task<List<JournalEntryTemplateLineDto>> GetTemplateLines(
        [FromServices] ISender sender,
        int id)
    {
        return await sender.Send(new GetTemplateLinesQuery { TemplateId = id });
    }

    [EndpointSummary("Create a template line")]
    public static async Task<IResult> CreateTemplateLine(
        [FromServices] ISender sender,
        int id,
        [FromBody] CreateTemplateLineCommand command)
    {
        if (id != command.TemplateId)
            return Results.BadRequest("Template ID mismatch.");

        var result = await sender.Send(command);
        if (!result.Succeeded)
            return result.ToProblemDetails();
        return Results.Ok(new { lineId = result.Value });
    }

    [EndpointSummary("Update a template line")]
    public static async Task<IResult> UpdateTemplateLine(
        [FromServices] ISender sender,
        int id,
        int lineId,
        [FromBody] UpdateTemplateLineCommand command)
    {
        if (id != command.TemplateId || lineId != command.Id)
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

    [EndpointSummary("Remove a template line")]
    public static async Task<IResult> RemoveTemplateLine(
        [FromServices] ISender sender,
        int id,
        int lineId)
    {
        var command = new RemoveTemplateLineCommand { Id = lineId, TemplateId = id };
        var result = await sender.Send(command);
        if (!result.Succeeded)
            return result.ToProblemDetails();
        return Results.NoContent();
    }
}
