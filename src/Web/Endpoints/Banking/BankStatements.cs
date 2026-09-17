using ERP_Government.Application.Banking.Common.DTOs;
using ERP_Government.Application.Banking.Commands.BankStatements;
using ERP_Government.Application.Banking.Queries.BankStatements;
using ERP_Government.Application.Common.Security;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ERP_Government.Web.Endpoint.Banking;

public class BankStatements : IEndpointGroup
{
    public static void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapGet("/", GetBankStatements)
            .Produces<List<BankStatementDto>>()
            .RequireAuthorization(PermissionCodes.BankStatementsView);

        groupBuilder.MapGet("/{id:int}", GetBankStatementById)
            .Produces<BankStatementDto?>()
            .RequireAuthorization(PermissionCodes.BankStatementsView);

        groupBuilder.MapPost("/", CreateBankStatement)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .RequireAuthorization(PermissionCodes.BankStatementsCreate);

        groupBuilder.MapPost("/import", ImportBankStatement)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .RequireAuthorization(PermissionCodes.BankStatementsImport);

        groupBuilder.MapPost("/{id:int}/reconcile", ReconcileBankStatement)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .RequireAuthorization(PermissionCodes.BankStatementsCreate);

        groupBuilder.MapPost("/{id:int}/cancel", CancelBankStatement)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .RequireAuthorization(PermissionCodes.BankStatementsCreate);

        groupBuilder.MapGet("/{id:int}/lines", GetBankStatementLines)
            .Produces<List<BankStatementLineDto>>()
            .RequireAuthorization(PermissionCodes.BankStatementsView);

        groupBuilder.MapPost("/{id:int}/lines", CreateBankStatementLine)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .RequireAuthorization(PermissionCodes.BankStatementsCreate);
    }

    [EndpointSummary("Get all bank statements")]
    public static async Task<List<BankStatementDto>> GetBankStatements(
        [FromServices] ISender sender)
    {
        return await sender.Send(new GetBankStatementsQuery());
    }

    [EndpointSummary("Get bank statement by ID")]
    public static async Task<IResult> GetBankStatementById(
        [FromServices] ISender sender,
        int id)
    {
        var result = await sender.Send(new GetBankStatementByIdQuery { Id = id });
        return result.Succeeded ? Results.Ok(result.Value!) : result.ToProblemDetails();
    }

    [EndpointSummary("Create a new bank statement")]
    public static async Task<IResult> CreateBankStatement(
        [FromServices] ISender sender,
        [FromBody] CreateBankStatementCommand command)
    {
        var result = await sender.Send(command);
        if (!result.Succeeded)
            return result.ToProblemDetails();
        return Results.NoContent();
    }

    [EndpointSummary("Import bank statement from file")]
    public static async Task<IResult> ImportBankStatement(
        [FromServices] ISender sender,
        [FromBody] ImportBankStatementCommand command)
    {
        var result = await sender.Send(command);
        if (!result.Succeeded)
            return result.ToProblemDetails();
        return Results.NoContent();
    }

    [EndpointSummary("Reconcile bank statement (Imported -> Reconciled)")]
    public static async Task<IResult> ReconcileBankStatement(
        [FromServices] ISender sender,
        int id)
    {
        var result = await sender.Send(new ReconcileBankStatementCommand { Id = id });
        if (!result.Succeeded)
            return result.ToProblemDetails();
        return Results.NoContent();
    }

    [EndpointSummary("Cancel bank statement (Draft -> Cancelled)")]
    public static async Task<IResult> CancelBankStatement(
        [FromServices] ISender sender,
        int id)
    {
        var result = await sender.Send(new CancelBankStatementCommand { Id = id });
        if (!result.Succeeded)
            return result.ToProblemDetails();
        return Results.NoContent();
    }

    [EndpointSummary("Get bank statement lines")]
    public static async Task<List<BankStatementLineDto>> GetBankStatementLines(
        [FromServices] ISender sender,
        int id)
    {
        return await sender.Send(new Application.Banking.Queries.BankStatementLines.GetBankStatementLinesQuery { StatementId = id });
    }

    [EndpointSummary("Create a bank statement line")]
    public static async Task<IResult> CreateBankStatementLine(
        [FromServices] ISender sender,
        int id,
        [FromBody] CreateBankStatementLineCommand command)
    {
        var result = await sender.Send(new CreateBankStatementLineCommand
        {
            StatementId = id,
            TransactionDate = command.TransactionDate,
            Description = command.Description,
            Debit = command.Debit,
            Credit = command.Credit,
            Balance = command.Balance,
            Reference = command.Reference
        });
        if (!result.Succeeded)
            return result.ToProblemDetails();
        return Results.NoContent();
    }
}
