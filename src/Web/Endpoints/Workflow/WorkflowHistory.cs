using ERP_Government.Application.Common.Security;
using ERP_Government.Application.Workflow.Queries.GetWorkflowHistory;
using ERP_Government.Shared.Workflow;
using ERP_Government.Web.Infrastructure;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ERP_Government.Web.Endpoints.Workflow;

public class WorkflowHistory : IEndpointGroup
{
    public static void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapGet("/instances/{instanceId:int}/history", GetWorkflowHistory)
            .Produces<List<WorkflowHistoryDto>>()
            .RequireAuthorization(PermissionCodes.WorkflowHistoryView);
    }

    [EndpointSummary("Get workflow history for an instance")]
    public static async Task<IResult> GetWorkflowHistory(
        [FromServices] ISender sender,
        int instanceId)
    {
        var result = await sender.Send(new GetWorkflowHistoryQuery { InstanceId = instanceId });
        return result.ToProblemDetails();
    }
}