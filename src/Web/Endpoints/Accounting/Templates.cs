using ERP_Government.Application.Accounting.Commands.Templates.CreateTemplate;
using ERP_Government.Application.Accounting.Commands.Templates.UpdateTemplate;
using ERP_Government.Application.Accounting.Common;
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
            return Results.BadRequest(result.Errors);
        return Results.Ok();
    }

    [EndpointSummary("Update an existing template")]
    public static async Task<IResult> UpdateTemplate(
        [FromServices] ISender sender,
        int id,
        [FromBody] UpdateTemplateCommand command)
    {
        if (id != command.Id)
            return Results.BadRequest("ID mismatch.");

        await sender.Send(command);
        return Results.NoContent();
    }
}
