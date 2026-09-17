using ERP_Government.Application.Common.Security;
using ERP_Government.Application.Security.ApprovalRules.Commands.CreateApprovalRule;
using ERP_Government.Application.Security.ApprovalRules.Commands.DeactivateApprovalRule;
using ERP_Government.Application.Security.ApprovalRules.Commands.UpdateApprovalRule;
using ERP_Government.Application.Security.ApprovalRules.Queries.GetApprovalRuleById;
using ERP_Government.Application.Security.ApprovalRules.Queries.GetApprovalRules;
using ERP_Government.Application.Security.Common;
using ERP_Government.Application.Security.PendingApprovals.Queries.GetPendingApprovals;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ERP_Government.Web.Endpoints.Security;

public class ApprovalRules : IEndpointGroup
{
    public static void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapGet("/", GetApprovalRules)
            .Produces<List<ApprovalRuleDto>>()
            .RequireAuthorization(PermissionCodes.ApprovalRulesView);

        groupBuilder.MapGet("/{id:int}", GetApprovalRuleById)
            .Produces<ApprovalRuleDto?>()
            .RequireAuthorization(PermissionCodes.ApprovalRulesView);

        groupBuilder.MapPost("/", CreateApprovalRule)
            .Produces<int>()
            .Produces(StatusCodes.Status400BadRequest)
            .RequireAuthorization(PermissionCodes.ApprovalRulesManage);

        groupBuilder.MapPut("/{id:int}", UpdateApprovalRule)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .RequireAuthorization(PermissionCodes.ApprovalRulesManage);

        groupBuilder.MapPatch("/{id:int}/deactivate", DeactivateApprovalRule)
            .Produces(StatusCodes.Status204NoContent)
            .RequireAuthorization(PermissionCodes.ApprovalRulesManage);

        groupBuilder.MapGet("/pending", GetPendingApprovals)
            .Produces<List<PendingApprovalDto>>()
            .RequireAuthorization(PermissionCodes.PurchaseOrdersView);
    }

    [EndpointSummary("Get all approval rules")]
    public static async Task<List<ApprovalRuleDto>> GetApprovalRules(
        [FromServices] ISender sender,
        [AsParameters] GetApprovalRulesQuery query)
    {
        return await sender.Send(query);
    }

    [EndpointSummary("Get approval rule by ID")]
    public static async Task<IResult> GetApprovalRuleById(
        [FromServices] ISender sender,
        int id)
    {
        var result = await sender.Send(new GetApprovalRuleByIdQuery { Id = id });
        return result.Succeeded ? Results.Ok(result.Value!) : result.ToProblemDetails();
    }

    [EndpointSummary("Create a new approval rule")]
    public static async Task<IResult> CreateApprovalRule(
        [FromServices] ISender sender,
        [FromBody] CreateApprovalRuleCommand command)
    {
        var id = await sender.Send(command);
        return Results.Ok(id);
    }

    [EndpointSummary("Update an existing approval rule")]
    public static async Task<IResult> UpdateApprovalRule(
        [FromServices] ISender sender,
        int id,
        [FromBody] UpdateApprovalRuleCommand command)
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

    [EndpointSummary("Deactivate an approval rule")]
    public static async Task<IResult> DeactivateApprovalRule(
        [FromServices] ISender sender,
        int id)
    {
        await sender.Send(new DeactivateApprovalRuleCommand { Id = id });
        return Results.NoContent();
    }

    [EndpointSummary("Get pending approvals requiring user action")]
    public static async Task<List<PendingApprovalDto>> GetPendingApprovals(
        [FromServices] ISender sender,
        [AsParameters] GetPendingApprovalsQuery query)
    {
        return await sender.Send(query);
    }
}
