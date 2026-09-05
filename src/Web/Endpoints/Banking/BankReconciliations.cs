using ERP_Government.Application.Banking.Common.DTOs;
using ERP_Government.Application.Banking.Commands.BankReconciliations;
using ERP_Government.Application.Banking.Queries.BankReconciliations;
using ERP_Government.Application.Common.Security;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ERP_Government.Web.Endpoint.Banking;

public class BankReconciliations : IEndpointGroup
{
    public static void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapGet("/", GetBankReconciliations)
            .Produces<List<BankReconciliationDto>>()
            .RequireAuthorization(PermissionCodes.BankReconciliationView);

        groupBuilder.MapGet("/{id:int}", GetBankReconciliationById)
            .Produces<BankReconciliationDto?>()
            .RequireAuthorization(PermissionCodes.BankReconciliationView);

        groupBuilder.MapPost("/", CreateBankReconciliation)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .RequireAuthorization(PermissionCodes.BankReconciliationCreate);

        groupBuilder.MapPost("/{id:int}/approve", ApproveBankReconciliation)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .RequireAuthorization(PermissionCodes.BankReconciliationApprove);

        groupBuilder.MapPost("/{id:int}/complete", CompleteBankReconciliation)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .RequireAuthorization(PermissionCodes.BankReconciliationCreate);

        groupBuilder.MapPost("/{id:int}/reject", RejectBankReconciliation)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .RequireAuthorization(PermissionCodes.BankReconciliationApprove);

        groupBuilder.MapGet("/{id:int}/lines", GetBankReconciliationLines)
            .Produces<List<BankReconciliationLineDto>>()
            .RequireAuthorization(PermissionCodes.BankReconciliationView);

        groupBuilder.MapPost("/{id:int}/lines", CreateBankReconciliationLine)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .RequireAuthorization(PermissionCodes.BankReconciliationCreate);
    }

    [EndpointSummary("Get all bank reconciliations")]
    public static async Task<List<BankReconciliationDto>> GetBankReconciliations(
        [FromServices] ISender sender)
    {
        return await sender.Send(new GetBankReconciliationsQuery());
    }

    [EndpointSummary("Get bank reconciliation by ID")]
    public static async Task<BankReconciliationDto?> GetBankReconciliationById(
        [FromServices] ISender sender,
        int id)
    {
        return await sender.Send(new GetBankReconciliationByIdQuery { Id = id });
    }

    [EndpointSummary("Create a new bank reconciliation")]
    public static async Task<IResult> CreateBankReconciliation(
        [FromServices] ISender sender,
        [FromBody] CreateBankReconciliationCommand command)
    {
        var result = await sender.Send(command);
        if (!result.Succeeded)
            return Results.BadRequest(result.Errors);
        return Results.NoContent();
    }

    [EndpointSummary("Approve a bank reconciliation")]
    public static async Task<IResult> ApproveBankReconciliation(
        [FromServices] ISender sender,
        int id)
    {
        var result = await sender.Send(new ApproveBankReconciliationCommand { Id = id });
        if (!result.Succeeded)
            return Results.BadRequest(result.Errors);
        return Results.NoContent();
    }

    [EndpointSummary("Complete bank reconciliation (Draft -> Completed)")]
    public static async Task<IResult> CompleteBankReconciliation(
        [FromServices] ISender sender,
        int id)
    {
        var result = await sender.Send(new CompleteBankReconciliationCommand { Id = id });
        if (!result.Succeeded)
            return Results.BadRequest(result.Errors);
        return Results.NoContent();
    }

    [EndpointSummary("Reject bank reconciliation (Completed -> Rejected, terminal)")]
    public static async Task<IResult> RejectBankReconciliation(
        [FromServices] ISender sender,
        int id,
        [FromBody] RejectBankReconciliationCommand command)
    {
        var result = await sender.Send(new RejectBankReconciliationCommand
        {
            Id = id,
            Reason = command.Reason
        });
        if (!result.Succeeded)
            return Results.BadRequest(result.Errors);
        return Results.NoContent();
    }

    [EndpointSummary("Get bank reconciliation lines")]
    public static async Task<List<BankReconciliationLineDto>> GetBankReconciliationLines(
        [FromServices] ISender sender,
        int id)
    {
        return await sender.Send(new Application.Banking.Queries.BankReconciliationLines.GetBankReconciliationLinesQuery { ReconciliationId = id });
    }

    [EndpointSummary("Create a bank reconciliation line")]
    public static async Task<IResult> CreateBankReconciliationLine(
        [FromServices] ISender sender,
        int id,
        [FromBody] CreateBankReconciliationLineCommand command)
    {
        var result = await sender.Send(new CreateBankReconciliationLineCommand
        {
            ReconciliationId = id,
            LineType = command.LineType,
            BankStatementLineId = command.BankStatementLineId,
            JournalEntryLineId = command.JournalEntryLineId,
            Amount = command.Amount,
            Description = command.Description
        });
        if (!result.Succeeded)
            return Results.BadRequest(result.Errors);
        return Results.NoContent();
    }
}
