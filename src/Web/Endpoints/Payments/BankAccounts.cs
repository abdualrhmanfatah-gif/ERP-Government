using ERP_Government.Application.Payments.Common.DTOs;
using ERP_Government.Application.Payments.Commands.BankAccounts.CreateBankAccount;
using ERP_Government.Application.Payments.Commands.BankAccounts.UpdateBankAccount;
using ERP_Government.Application.Payments.Commands.BankAccounts.DeactivateBankAccount;
using ERP_Government.Application.Payments.Commands.BankAccounts.ActivateBankAccount;
using ERP_Government.Application.Payments.Queries.BankAccounts.GetBankAccountById;
using ERP_Government.Application.Payments.Queries.BankAccounts.GetBankAccounts;
using ERP_Government.Application.Common.Security;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ERP_Government.Web.Endpoints.Payments;

public class BankAccounts : IEndpointGroup
{
    public static void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapGet("/", GetBankAccounts)
            .Produces<List<BankAccountDto>>()
            .RequireAuthorization(PermissionCodes.BankAccountsView);

        groupBuilder.MapGet("/{id:int}", GetBankAccountById)
            .Produces<BankAccountDto?>()
            .RequireAuthorization(PermissionCodes.BankAccountsView);

        groupBuilder.MapPost("/", CreateBankAccount)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .RequireAuthorization(PermissionCodes.BankAccountsCreate);

        groupBuilder.MapPut("/{id:int}", UpdateBankAccount)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .RequireAuthorization(PermissionCodes.BankAccountsUpdate);

        groupBuilder.MapPost("/{id:int}/activate", ActivateBankAccount)
            .Produces(StatusCodes.Status204NoContent)
            .RequireAuthorization(PermissionCodes.BankAccountsActivate);

        groupBuilder.MapPost("/{id:int}/deactivate", DeactivateBankAccount)
            .Produces(StatusCodes.Status204NoContent)
            .RequireAuthorization(PermissionCodes.BankAccountsDeactivate);
    }

    [EndpointSummary("Get all bank accounts")]
    public static async Task<List<BankAccountDto>> GetBankAccounts(
        [FromServices] ISender sender)
    {
        return await sender.Send(new GetBankAccountsQuery());
    }

    [EndpointSummary("Get bank account by ID")]
    public static async Task<BankAccountDto?> GetBankAccountById(
        [FromServices] ISender sender,
        int id)
    {
        return await sender.Send(new GetBankAccountByIdQuery { Id = id });
    }

    [EndpointSummary("Create a new bank account")]
    public static async Task<IResult> CreateBankAccount(
        [FromServices] ISender sender,
        [FromBody] CreateBankAccountCommand command)
    {
        var result = await sender.Send(command);
        if (!result.Succeeded)
            return Results.BadRequest(result.Errors);
        return Results.NoContent();
    }

    [EndpointSummary("Update a bank account")]
    public static async Task<IResult> UpdateBankAccount(
        [FromServices] ISender sender,
        int id,
        [FromBody] UpdateBankAccountCommand command)
    {
        if (id != command.Id)
            return Results.BadRequest("ID mismatch.");

        var result = await sender.Send(command);
        if (!result.Succeeded)
            return Results.BadRequest(result.Errors);
        return Results.NoContent();
    }

    [EndpointSummary("Activate a bank account")]
    public static async Task<IResult> ActivateBankAccount(
        [FromServices] ISender sender,
        int id,
        [FromBody] ActivateBankAccountCommand command)
    {
        if (id != command.Id)
            return Results.BadRequest("ID mismatch.");

        await sender.Send(command);
        return Results.NoContent();
    }

    [EndpointSummary("Deactivate a bank account")]
    public static async Task<IResult> DeactivateBankAccount(
        [FromServices] ISender sender,
        int id,
        [FromBody] DeactivateBankAccountCommand command)
    {
        if (id != command.Id)
            return Results.BadRequest("ID mismatch.");

        await sender.Send(command);
        return Results.NoContent();
    }
}
