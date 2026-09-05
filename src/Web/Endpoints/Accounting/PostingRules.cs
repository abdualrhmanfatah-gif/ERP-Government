using ERP_Government.Application.Accounting.Commands.PostingRules.CreatePostingRule;
using ERP_Government.Application.Accounting.Commands.PostingRules.DeletePostingRule;
using ERP_Government.Application.Accounting.Commands.PostingRules.UpdatePostingRule;
using ERP_Government.Application.Accounting.Common;
using ERP_Government.Application.Accounting.Queries.PostingRules.GetPostingRuleById;
using ERP_Government.Application.Accounting.Queries.PostingRules.GetPostingRulesList;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ERP_Government.Web.Endpoints.Accounting;

public class PostingRules : IEndpointGroup
{
    public static void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapGet("/", GetPostingRulesList)
            .Produces<List<PostingRuleDto>>()
            .RequireAuthorization("Accounting.PostingRules.Read");

        groupBuilder.MapGet("/{id:int}", GetPostingRuleById)
            .Produces<PostingRuleDto?>()
            .RequireAuthorization("Accounting.PostingRules.Read");

        groupBuilder.MapPost("/", CreatePostingRule)
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .RequireAuthorization("Accounting.PostingRules.Create");

        groupBuilder.MapPut("/{id:int}", UpdatePostingRule)
            .Produces(StatusCodes.Status204NoContent)
            .RequireAuthorization("Accounting.PostingRules.Edit");

        groupBuilder.MapDelete("/{id:int}", DeletePostingRule)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .RequireAuthorization("Accounting.PostingRules.Delete");
    }

    [EndpointSummary("Get all posting rules")]
    public static async Task<List<PostingRuleDto>> GetPostingRulesList(
        [FromServices] ISender sender,
        [AsParameters] GetPostingRulesListQuery query)
    {
        return await sender.Send(query);
    }

    [EndpointSummary("Get posting rule by ID")]
    public static async Task<PostingRuleDto?> GetPostingRuleById(
        [FromServices] ISender sender,
        int id)
    {
        return await sender.Send(new GetPostingRuleByIdQuery { Id = id });
    }

    [EndpointSummary("Create a new posting rule")]
    public static async Task<IResult> CreatePostingRule(
        [FromServices] ISender sender,
        [FromBody] CreatePostingRuleCommand command)
    {
        var result = await sender.Send(command);
        if (!result.Succeeded)
            return Results.BadRequest(result.Errors);
        return Results.Ok();
    }

    [EndpointSummary("Update an existing posting rule")]
    public static async Task<IResult> UpdatePostingRule(
        [FromServices] ISender sender,
        int id,
        [FromBody] UpdatePostingRuleCommand command)
    {
        if (id != command.Id)
            return Results.BadRequest("ID mismatch.");

        var result = await sender.Send(command);
        if (!result.Succeeded)
            return Results.BadRequest(result.Errors);
        return Results.NoContent();
    }

    [EndpointSummary("Delete a posting rule")]
    public static async Task<IResult> DeletePostingRule(
        [FromServices] ISender sender,
        int id)
    {
        var result = await sender.Send(new DeletePostingRuleCommand { Id = id });
        if (!result.Succeeded)
            return Results.BadRequest(result.Errors);
        return Results.NoContent();
    }
}
