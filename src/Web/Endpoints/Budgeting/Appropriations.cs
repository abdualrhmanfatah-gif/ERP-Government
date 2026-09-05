using ERP_Government.Application.Budgeting.Commands.Appropriations;
using ERP_Government.Application.Budgeting.Common;
using ERP_Government.Application.Budgeting.Queries.Appropriations;
using ERP_Government.Application.Common.Security;
using ERP_Government.Domain.Budgeting.Enums;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ERP_Government.Web.Endpoint.Budgeting;

public class Appropriations : IEndpointGroup
{
    public static void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapGet("/", GetAppropriations)
            .Produces<List<AppropriationDto>>()
            .RequireAuthorization(PermissionCodes.AppropriationsView);

        groupBuilder.MapGet("/{id:int}", GetAppropriationById)
            .Produces<AppropriationDto>()
            .Produces(StatusCodes.Status404NotFound)
            .RequireAuthorization(PermissionCodes.AppropriationsView);

        groupBuilder.MapGet("/{id:int}/availability", GetAvailability)
            .Produces<BudgetAvailabilitySummary>()
            .Produces(StatusCodes.Status404NotFound)
            .RequireAuthorization(PermissionCodes.AppropriationsView);

        groupBuilder.MapPost("/", CreateAppropriation)
            .Produces<int>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .RequireAuthorization(PermissionCodes.AppropriationsCreate);

        groupBuilder.MapPost("/transfers", CreateTransfer)
            .Produces<TransferPairResult>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .RequireAuthorization(PermissionCodes.AppropriationsCreate);

        groupBuilder.MapPut("/{id:int}", UpdateAppropriation)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .RequireAuthorization(PermissionCodes.AppropriationsUpdate);

        groupBuilder.MapDelete("/{id:int}", DeleteAppropriation)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .RequireAuthorization(PermissionCodes.AppropriationsDelete);

        groupBuilder.MapPatch("/{id:int}/submit", SubmitAppropriation)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .RequireAuthorization(PermissionCodes.AppropriationsSubmit);

        groupBuilder.MapPatch("/{id:int}/approve", ApproveAppropriation)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .RequireAuthorization(PermissionCodes.AppropriationsApprove);

        groupBuilder.MapPatch("/{id:int}/activate", ActivateAppropriation)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .RequireAuthorization(PermissionCodes.AppropriationsActivate);

        groupBuilder.MapPatch("/{id:int}/suspend", SuspendAppropriation)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .RequireAuthorization(PermissionCodes.AppropriationsSuspend);

        groupBuilder.MapPatch("/{id:int}/close", CloseAppropriation)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .RequireAuthorization(PermissionCodes.AppropriationsClose);

        groupBuilder.MapPatch("/{id:int}/cancel", CancelAppropriation)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .RequireAuthorization(PermissionCodes.AppropriationsCancel);

        groupBuilder.MapPatch("/{id:int}/reverse", ReverseAppropriation)
            .Produces<int>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .RequireAuthorization(PermissionCodes.AppropriationsReverse);
    }

    [EndpointSummary("Get all appropriations with optional filters")]
    public static async Task<List<AppropriationDto>> GetAppropriations(
        [FromServices] ISender sender,
        [AsParameters] GetAppropriationsListQuery query)
    {
        return await sender.Send(query);
    }

    [EndpointSummary("Get appropriation by ID")]
    public static async Task<AppropriationDto> GetAppropriationById(
        [FromServices] ISender sender,
        int id)
    {
        return await sender.Send(new GetAppropriationByIdQuery(id));
    }

    [EndpointSummary("Create a new appropriation")]
    public static async Task<IResult> CreateAppropriation(
        [FromServices] ISender sender,
        [FromBody] CreateAppropriationRequest body)
    {
        var result = await sender.Send(new CreateAppropriationCommand(
            body.BudgetId, body.BudgetItemId, body.AppropriationType,
            body.DocumentType, body.DocumentId, body.Amount));
        if (!result.Succeeded)
            return Results.BadRequest(result.Errors);
        return Results.Created($"/api/Appropriations/{result.Value}", result.Value);
    }

    [EndpointSummary("Update an appropriation (Draft only)")]
    public static async Task<IResult> UpdateAppropriation(
        [FromServices] ISender sender,
        int id,
        [FromBody] UpdateAppropriationRequest body)
    {
        var result = await sender.Send(new UpdateAppropriationCommand(
            id, body.AppropriationType, body.DocumentType, body.DocumentId, body.Amount, body.RowVersion));
        if (!result.Succeeded)
            return Results.BadRequest(result.Errors);
        return Results.NoContent();
    }

    [EndpointSummary("Delete an appropriation (Draft only)")]
    public static async Task<IResult> DeleteAppropriation(
        [FromServices] ISender sender,
        int id)
    {
        var result = await sender.Send(new DeleteAppropriationCommand(id));
        if (!result.Succeeded)
            return Results.BadRequest(result.Errors);
        return Results.NoContent();
    }

    [EndpointSummary("Submit an appropriation for approval")]
    public static async Task<IResult> SubmitAppropriation(
        [FromServices] ISender sender,
        int id,
        [FromBody] AppropriationActionRequest body)
    {
        var result = await sender.Send(new SubmitAppropriationCommand(id, body.RowVersion));
        if (!result.Succeeded)
            return Results.BadRequest(result.Errors);
        return Results.NoContent();
    }

    [EndpointSummary("Approve a submitted appropriation")]
    public static async Task<IResult> ApproveAppropriation(
        [FromServices] ISender sender,
        int id,
        [FromBody] AppropriationActionRequest body)
    {
        var result = await sender.Send(new ApproveAppropriationCommand(id, body.RowVersion));
        if (!result.Succeeded)
            return Results.BadRequest(result.Errors);
        return Results.NoContent();
    }

    [EndpointSummary("Activate an approved appropriation")]
    public static async Task<IResult> ActivateAppropriation(
        [FromServices] ISender sender,
        int id,
        [FromBody] AppropriationActionRequest body)
    {
        var result = await sender.Send(new ActivateAppropriationCommand(id, body.RowVersion));
        if (!result.Succeeded)
            return Results.BadRequest(result.Errors);
        return Results.NoContent();
    }

    [EndpointSummary("Suspend an active appropriation")]
    public static async Task<IResult> SuspendAppropriation(
        [FromServices] ISender sender,
        int id,
        [FromBody] AppropriationActionRequest body)
    {
        var result = await sender.Send(new SuspendAppropriationCommand(id, body.RowVersion));
        if (!result.Succeeded)
            return Results.BadRequest(result.Errors);
        return Results.NoContent();
    }

    [EndpointSummary("Close an active or suspended appropriation")]
    public static async Task<IResult> CloseAppropriation(
        [FromServices] ISender sender,
        int id,
        [FromBody] AppropriationActionRequest body)
    {
        var result = await sender.Send(new CloseAppropriationCommand(id, body.RowVersion));
        if (!result.Succeeded)
            return Results.BadRequest(result.Errors);
        return Results.NoContent();
    }

    [EndpointSummary("Cancel a draft, pending, or suspended appropriation")]
    public static async Task<IResult> CancelAppropriation(
        [FromServices] ISender sender,
        int id,
        [FromBody] AppropriationActionRequest body)
    {
        var result = await sender.Send(new CancelAppropriationCommand(id, body.RowVersion));
        if (!result.Succeeded)
            return Results.BadRequest(result.Errors);
        return Results.NoContent();
    }

    [EndpointSummary("Reverse an active appropriation (creates Adjustment with negative amount)")]
    public static async Task<IResult> ReverseAppropriation(
        [FromServices] ISender sender,
        int id,
        [FromBody] AppropriationActionRequest body)
    {
        var result = await sender.Send(new ReverseAppropriationCommand(id, body.RowVersion));
        if (!result.Succeeded)
            return Results.BadRequest(result.Errors);
        return Results.Ok(result.Value);
    }

    [EndpointSummary("Get availability for the appropriation's budget item")]
    public static async Task<IResult> GetAvailability(
        [FromServices] ISender sender,
        int id)
    {
        var result = await sender.Send(new GetAppropriationAvailabilityQuery(id));
        return result is not null ? Results.Ok(result) : Results.NotFound();
    }

    [EndpointSummary("Create a transfer pair (negative source + positive target)")]
    public static async Task<IResult> CreateTransfer(
        [FromServices] ISender sender,
        [FromBody] CreateTransferRequest body)
    {
        var result = await sender.Send(new CreateTransferAppropriationCommand(
            body.BudgetId, body.SourceBudgetItemId, body.TargetBudgetItemId, body.Amount));
        if (!result.Succeeded)
            return Results.BadRequest(result.Errors);
        return Results.Ok(result.Value);
    }
}

// ─── Request DTOs ────────────────────────────────────────────────

public record CreateAppropriationRequest(
    int BudgetId,
    int BudgetItemId,
    ERP_Government.Domain.Budgeting.Enums.AppropriationType AppropriationType,
    string DocumentType,
    int DocumentId,
    decimal Amount);

public record UpdateAppropriationRequest(
    ERP_Government.Domain.Budgeting.Enums.AppropriationType AppropriationType,
    string DocumentType,
    int DocumentId,
    decimal Amount,
    byte[] RowVersion);

public record AppropriationActionRequest(byte[] RowVersion);

public record CreateTransferRequest(
    int BudgetId,
    int SourceBudgetItemId,
    int TargetBudgetItemId,
    decimal Amount);

public record TransferPairResult(int SourceId, int TargetId);
