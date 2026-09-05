using ERP_Government.Application.Accounting.Commands.Accounts.CreateAccount;
using ERP_Government.Application.Accounting.Commands.Accounts.UpdateAccount;
using ERP_Government.Application.Accounting.Common;
using ERP_Government.Application.Accounting.Queries.Accounts.GetAccountById;
using ERP_Government.Application.Accounting.Queries.Accounts.GetAccountsList;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ERP_Government.Web.Endpoints.Accounting;

public class Accounts : IEndpointGroup
{
    public static void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapGet("/", GetAccountsList)
            .Produces<List<AccountDto>>()
            .RequireAuthorization("Accounting.ChartOfAccounts.Read");

        groupBuilder.MapGet("/{id:int}", GetAccountById)
            .Produces<AccountDto?>()
            .RequireAuthorization("Accounting.ChartOfAccounts.Read");

        groupBuilder.MapPost("/", CreateAccount)
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .RequireAuthorization("Accounting.ChartOfAccounts.Create");

        groupBuilder.MapPut("/{id:int}", UpdateAccount)
            .Produces(StatusCodes.Status204NoContent)
            .RequireAuthorization("Accounting.ChartOfAccounts.Edit");
    }

    [EndpointSummary("Get all accounts")]
    public static async Task<List<AccountDto>> GetAccountsList(
        [FromServices] ISender sender,
        [AsParameters] GetAccountsListQuery query)
    {
        return await sender.Send(query);
    }

    [EndpointSummary("Get account by ID")]
    public static async Task<AccountDto?> GetAccountById(
        [FromServices] ISender sender,
        int id)
    {
        return await sender.Send(new GetAccountByIdQuery { Id = id });
    }

    [EndpointSummary("Create a new account")]
    public static async Task<IResult> CreateAccount(
        [FromServices] ISender sender,
        [FromBody] CreateAccountCommand command)
    {
        var result = await sender.Send(command);
        if (!result.Succeeded)
            return Results.BadRequest(result.Errors);
        return Results.Ok();
    }

    [EndpointSummary("Update an existing account")]
    public static async Task<IResult> UpdateAccount(
        [FromServices] ISender sender,
        int id,
        [FromBody] UpdateAccountCommand command)
    {
        if (id != command.Id)
            return Results.BadRequest("ID mismatch.");

        await sender.Send(command);
        return Results.NoContent();
    }
}
