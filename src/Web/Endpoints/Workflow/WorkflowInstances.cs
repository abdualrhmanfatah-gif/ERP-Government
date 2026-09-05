using ERP_Government.Application.Common.Security;
using ERP_Government.Application.Workflow.Commands.AdvanceWorkflowInstance;
using ERP_Government.Application.Workflow.Commands.CancelWorkflowInstance;
using ERP_Government.Application.Workflow.Commands.StartWorkflowInstance;
using ERP_Government.Application.Workflow.Queries.GetWorkflowInstanceById;
using ERP_Government.Application.Workflow.Queries.GetWorkflowInstances;
using ERP_Government.Domain.Workflow.Enums;
using ERP_Government.Shared.Workflow;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ERP_Government.Web.Endpoints.Workflow;

public class WorkflowInstances : IEndpointGroup
{
    public static void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapGet("/", GetWorkflowInstances)
            .Produces<List<WorkflowInstanceDto>>()
            .RequireAuthorization(PermissionCodes.WorkflowInstancesView);

        groupBuilder.MapGet("/{id:int}", GetWorkflowInstanceById)
            .Produces<WorkflowInstanceDto?>()
            .RequireAuthorization(PermissionCodes.WorkflowInstancesView);

        groupBuilder.MapPost("/", StartWorkflowInstance)
            .Produces<int>()
            .Produces(StatusCodes.Status400BadRequest)
            .RequireAuthorization(PermissionCodes.WorkflowInstancesExecute);

        groupBuilder.MapPost("/{id:int}/steps/{stepId:int}/approve", ApproveStep)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .RequireAuthorization(PermissionCodes.WorkflowInstancesExecute);

        groupBuilder.MapPost("/{id:int}/steps/{stepId:int}/reject", RejectStep)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .RequireAuthorization(PermissionCodes.WorkflowInstancesExecute);

        groupBuilder.MapPost("/{id:int}/cancel", CancelWorkflowInstance)
            .Produces(StatusCodes.Status204NoContent)
            .RequireAuthorization(PermissionCodes.WorkflowInstancesExecute);
    }

    [EndpointSummary("Get all workflow instances")]
    public static async Task<List<WorkflowInstanceDto>> GetWorkflowInstances(
        [FromServices] ISender sender,
        [AsParameters] GetWorkflowInstancesQuery query)
    {
        return await sender.Send(query);
    }

    [EndpointSummary("Get workflow instance by ID")]
    public static async Task<WorkflowInstanceDto?> GetWorkflowInstanceById(
        [FromServices] ISender sender,
        int id)
    {
        return await sender.Send(new GetWorkflowInstanceByIdQuery { Id = id });
    }

    [EndpointSummary("Start a new workflow instance")]
    public static async Task<IResult> StartWorkflowInstance(
        [FromServices] ISender sender,
        [FromBody] StartWorkflowInstanceCommand command)
    {
        var id = await sender.Send(command);
        return Results.Ok(id);
    }

    [EndpointSummary("Approve a workflow step")]
    public static async Task<IResult> ApproveStep(
        [FromServices] ISender sender,
        int id,
        int stepId,
        [FromBody] StepDecisionRequest? request)
    {
        await sender.Send(new AdvanceWorkflowInstanceCommand
        {
            InstanceId = id,
            StepId = stepId,
            Decision = WorkflowDecision.Approved,
            Reason = request?.Reason
        });
        return Results.NoContent();
    }

    [EndpointSummary("Reject a workflow step")]
    public static async Task<IResult> RejectStep(
        [FromServices] ISender sender,
        int id,
        int stepId,
        [FromBody] StepDecisionRequest? request)
    {
        await sender.Send(new AdvanceWorkflowInstanceCommand
        {
            InstanceId = id,
            StepId = stepId,
            Decision = WorkflowDecision.Rejected,
            Reason = request?.Reason
        });
        return Results.NoContent();
    }

    [EndpointSummary("Cancel a workflow instance")]
    public static async Task<IResult> CancelWorkflowInstance(
        [FromServices] ISender sender,
        int id,
        [FromBody] CancelWorkflowInstanceRequest? request)
    {
        await sender.Send(new CancelWorkflowInstanceCommand
        {
            InstanceId = id,
            Reason = request?.Reason
        });
        return Results.NoContent();
    }
}

public class StepDecisionRequest
{
    public string? Reason { get; init; }
}

public class CancelWorkflowInstanceRequest
{
    public string? Reason { get; init; }
}