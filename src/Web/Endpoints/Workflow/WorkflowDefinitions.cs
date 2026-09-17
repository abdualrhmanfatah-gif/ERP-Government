using ERP_Government.Application.Common.Security;
using ERP_Government.Application.Workflow.Commands.CreateWorkflowDefinition;
using ERP_Government.Application.Workflow.Commands.DeleteWorkflowDefinition;
using ERP_Government.Application.Workflow.Commands.UpdateWorkflowDefinition;
using ERP_Government.Application.Workflow.Queries.GetWorkflowDefinitionById;
using ERP_Government.Application.Workflow.Queries.GetWorkflowDefinitions;
using ERP_Government.Shared.Workflow;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ERP_Government.Web.Endpoints.Workflow;

public class WorkflowDefinitions : IEndpointGroup
{
    public static void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapGet("/", GetWorkflowDefinitions)
            .Produces<List<WorkflowDefinitionDto>>()
            .RequireAuthorization(PermissionCodes.WorkflowDefinitionsView);

        groupBuilder.MapGet("/{id:int}", GetWorkflowDefinitionById)
            .Produces<WorkflowDefinitionDto?>()
            .RequireAuthorization(PermissionCodes.WorkflowDefinitionsView);

        groupBuilder.MapPost("/", CreateWorkflowDefinition)
            .Produces<int>()
            .Produces(StatusCodes.Status400BadRequest)
            .RequireAuthorization(PermissionCodes.WorkflowDefinitionsManage);

        groupBuilder.MapPut("/{id:int}", UpdateWorkflowDefinition)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .RequireAuthorization(PermissionCodes.WorkflowDefinitionsManage);

        groupBuilder.MapDelete("/{id:int}", DeleteWorkflowDefinition)
            .Produces(StatusCodes.Status204NoContent)
            .RequireAuthorization(PermissionCodes.WorkflowDefinitionsManage);
    }

    [EndpointSummary("Get all workflow definitions")]
    public static async Task<List<WorkflowDefinitionDto>> GetWorkflowDefinitions(
        [FromServices] ISender sender,
        [AsParameters] GetWorkflowDefinitionsQuery query)
    {
        return await sender.Send(query);
    }

    [EndpointSummary("Get workflow definition by ID")]
    public static async Task<IResult> GetWorkflowDefinitionById(
        [FromServices] ISender sender,
        int id)
    {
        var result = await sender.Send(new GetWorkflowDefinitionByIdQuery { Id = id });
        return result.Succeeded ? Results.Ok(result.Value!) : result.ToProblemDetails();
    }

    [EndpointSummary("Create a new workflow definition")]
    public static async Task<IResult> CreateWorkflowDefinition(
        [FromServices] ISender sender,
        [FromBody] CreateWorkflowDefinitionCommand command)
    {
        var id = await sender.Send(command);
        return Results.Ok(id);
    }

    [EndpointSummary("Update an existing workflow definition")]
    public static async Task<IResult> UpdateWorkflowDefinition(
        [FromServices] ISender sender,
        int id,
        [FromBody] UpdateWorkflowDefinitionCommand command)
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

    [EndpointSummary("Delete a workflow definition (soft delete)")]
    public static async Task<IResult> DeleteWorkflowDefinition(
        [FromServices] ISender sender,
        int id)
    {
        await sender.Send(new DeleteWorkflowDefinitionCommand { Id = id });
        return Results.NoContent();
    }
}