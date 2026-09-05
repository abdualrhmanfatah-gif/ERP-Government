using ERP_Government.Application.Common.Security;
using ERP_Government.Application.Security.ApprovalDelegations.Commands.CreateApprovalDelegation;
using ERP_Government.Application.Security.ApprovalDelegations.Commands.RevokeApprovalDelegation;
using ERP_Government.Application.Security.ApprovalDelegations.Queries.GetApprovalDelegations;
using ERP_Government.Application.Security.Common;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ERP_Government.Web.Endpoints.Security;

public class ApprovalDelegations : IEndpointGroup
{
    public static void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapGet("/", GetApprovalDelegations)
            .Produces<List<ApprovalDelegationDto>>()
            .RequireAuthorization(PermissionCodes.ApprovalDelegationsView);

        groupBuilder.MapPost("/", CreateApprovalDelegation)
            .Produces<int>()
            .Produces(StatusCodes.Status400BadRequest)
            .RequireAuthorization(PermissionCodes.ApprovalDelegationsManage);

        groupBuilder.MapPatch("/{id:int}/revoke", RevokeApprovalDelegation)
            .Produces(StatusCodes.Status204NoContent)
            .RequireAuthorization(PermissionCodes.ApprovalDelegationsManage);
    }

    [EndpointSummary("Get all approval delegations")]
    public static async Task<List<ApprovalDelegationDto>> GetApprovalDelegations(
        [FromServices] ISender sender,
        [AsParameters] GetApprovalDelegationsQuery query)
    {
        return await sender.Send(query);
    }

    [EndpointSummary("Create a new approval delegation")]
    public static async Task<IResult> CreateApprovalDelegation(
        [FromServices] ISender sender,
        [FromBody] CreateApprovalDelegationCommand command)
    {
        var id = await sender.Send(command);
        return Results.Ok(id);
    }

    [EndpointSummary("Revoke an approval delegation")]
    public static async Task<IResult> RevokeApprovalDelegation(
        [FromServices] ISender sender,
        int id)
    {
        await sender.Send(new RevokeApprovalDelegationCommand { Id = id });
        return Results.NoContent();
    }
}
