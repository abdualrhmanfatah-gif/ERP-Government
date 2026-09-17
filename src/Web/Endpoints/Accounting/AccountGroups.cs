using ERP_Government.Application.Accounting.Commands.AccountGroups.CreateAccountGroup;
using ERP_Government.Application.Accounting.Commands.AccountGroups.ToggleAccountGroupActive;
using ERP_Government.Application.Accounting.Commands.AccountGroups.UpdateAccountGroup;
using ERP_Government.Application.Accounting.Common;
using ERP_Government.Application.Accounting.Queries.AccountGroups.GetAccountGroupById;
using ERP_Government.Application.Accounting.Queries.AccountGroups.GetAccountGroupDetail;
using ERP_Government.Application.Accounting.Queries.AccountGroups.GetAccountGroupsList;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ERP_Government.Web.Endpoints.Accounting;

public class AccountGroups : IEndpointGroup
{
    public static void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapGet("/", GetAccountGroupsList)
            .Produces<PaginatedAccountGroupsResponse>()
            .RequireAuthorization("Accounting.ChartOfAccounts.Read");

        groupBuilder.MapGet("/{id:int}", GetAccountGroupById)
            .Produces<AccountGroupDto?>()
            .RequireAuthorization("Accounting.ChartOfAccounts.Read");

        groupBuilder.MapGet("/{id:int}/detail", GetAccountGroupDetail)
            .Produces<AccountGroupDetailResponse>()
            .RequireAuthorization("Accounting.ChartOfAccounts.Read");

        groupBuilder.MapPost("/", CreateAccountGroup)
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .RequireAuthorization("Accounting.ChartOfAccounts.Create");

        groupBuilder.MapPut("/{id:int}", UpdateAccountGroup)
            .Produces(StatusCodes.Status204NoContent)
            .RequireAuthorization("Accounting.ChartOfAccounts.Edit");

        groupBuilder.MapPost("/{id:int}/toggle-active", ToggleAccountGroupActive)
            .Produces(StatusCodes.Status204NoContent)
            .RequireAuthorization("Accounting.ChartOfAccounts.Edit");
    }

    [EndpointSummary("Get all account groups")]
    public static async Task<PaginatedAccountGroupsResponse> GetAccountGroupsList(
        [FromServices] ISender sender,
        [AsParameters] GetAccountGroupsListQuery query)
    {
        return await sender.Send(query);
    }

    [EndpointSummary("Get account group by ID")]
    public static async Task<IResult> GetAccountGroupById(
        [FromServices] ISender sender,
        int id)
    {
        var result = await sender.Send(new GetAccountGroupByIdQuery { Id = id });
        return result.Succeeded ? Results.Ok(result.Value!) : result.ToProblemDetails();
    }

    [EndpointSummary("Create a new account group")]
    public static async Task<IResult> CreateAccountGroup(
        [FromServices] ISender sender,
        [FromBody] CreateAccountGroupCommand command)
    {
        var result = await sender.Send(command);
        if (!result.Succeeded)
            return result.ToProblemDetails();
        return Results.Ok(new { id = result.Value });
    }

    [EndpointSummary("Update an existing account group")]
    public static async Task<IResult> UpdateAccountGroup(
        [FromServices] ISender sender,
        int id,
        [FromBody] UpdateAccountGroupCommand command)
    {
        if (id != command.Id)
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

    [EndpointSummary("Get account group detail")]
    public static async Task<IResult> GetAccountGroupDetail(
        [FromServices] ISender sender,
        int id)
    {
        var result = await sender.Send(new GetAccountGroupDetailQuery { Id = id });
        return result.Succeeded
            ? Results.Ok(result.Value!)
            : result.ToProblemDetails();
    }

    [EndpointSummary("Toggle account group active")]
    public static async Task<IResult> ToggleAccountGroupActive(
        [FromServices] ISender sender,
        int id,
        [FromBody] ToggleAccountGroupActiveCommand command)
    {
        if (id != command.Id)
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
}
