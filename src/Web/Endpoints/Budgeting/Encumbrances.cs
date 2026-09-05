using ERP_Government.Application.Budgeting.Commands.Encumbrances;
using ERP_Government.Application.Budgeting.Queries.Encumbrances;
using ERP_Government.Application.Common.Security;
using ERP_Government.Domain.Budgeting.Enums;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ERP_Government.Web.Endpoint.Budgeting;

public class Encumbrances : IEndpointGroup
{
    public static void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapGet("/", GetEncumbrances)
            .Produces<List<EncumbranceListItemDto>>()
            .RequireAuthorization(PermissionCodes.EncumbrancesView);

        groupBuilder.MapGet("/{id:int}", GetEncumbranceById)
            .Produces<EncumbranceDetailDto>()
            .Produces(StatusCodes.Status404NotFound)
            .RequireAuthorization(PermissionCodes.EncumbrancesView);

        groupBuilder.MapPost("/", CreateEncumbrance)
            .Produces<int>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .RequireAuthorization(PermissionCodes.EncumbrancesCreate);

        groupBuilder.MapPut("/{id:int}", UpdateEncumbrance)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .RequireAuthorization(PermissionCodes.EncumbrancesUpdate);

        groupBuilder.MapDelete("/{id:int}", DeleteEncumbrance)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .RequireAuthorization(PermissionCodes.EncumbrancesDelete);

        groupBuilder.MapPatch("/{id:int}/submit", SubmitEncumbrance)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .RequireAuthorization(PermissionCodes.EncumbrancesSubmit);

        groupBuilder.MapPatch("/{id:int}/approve", ApproveEncumbrance)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .RequireAuthorization(PermissionCodes.EncumbrancesApprove);

        groupBuilder.MapPatch("/{id:int}/activate", ActivateEncumbrance)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .RequireAuthorization(PermissionCodes.EncumbrancesActivate);

        groupBuilder.MapPatch("/{id:int}/suspend", SuspendEncumbrance)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .RequireAuthorization(PermissionCodes.EncumbrancesSuspend);

        groupBuilder.MapPatch("/{id:int}/close", CloseEncumbrance)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .RequireAuthorization(PermissionCodes.EncumbrancesClose);

        groupBuilder.MapPatch("/{id:int}/cancel", CancelEncumbrance)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .RequireAuthorization(PermissionCodes.EncumbrancesCancel);

        groupBuilder.MapPatch("/{id:int}/reverse", ReverseEncumbrance)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .RequireAuthorization(PermissionCodes.EncumbrancesReverse);
    }

    public static async Task<List<EncumbranceListItemDto>> GetEncumbrances(
        [FromServices] ISender sender,
        int? appropriationId,
        EncumbranceType? type,
        EncumbranceStatus? status)
    {
        return await sender.Send(new GetEncumbrancesListQuery(appropriationId, type, status));
    }

    public static async Task<IResult> GetEncumbranceById(
        [FromServices] ISender sender,
        int id)
    {
        var result = await sender.Send(new GetEncumbranceByIdQuery(id));
        return result is not null ? Results.Ok(result) : Results.NotFound();
    }

    public static async Task<IResult> CreateEncumbrance(
        [FromServices] ISender sender,
        [FromBody] CreateEncumbranceRequest body)
    {
        var result = await sender.Send(new CreateEncumbranceCommand(
            body.AppropriationId, body.EncumbranceType, body.VendorId, body.PurchaseOrderId,
            body.DocumentType, body.DocumentId, body.Description, body.EncumbranceDate, body.Amount));
        if (!result.Succeeded)
            return Results.BadRequest(result.Errors);
        return Results.Created($"/api/Encumbrances/{result.Value}", result.Value);
    }

    public static async Task<IResult> UpdateEncumbrance(
        [FromServices] ISender sender,
        int id,
        [FromBody] UpdateEncumbranceRequest body)
    {
        var result = await sender.Send(new UpdateEncumbranceCommand(
            id, body.Description, body.EncumbranceDate, body.Amount, body.VendorId, body.PurchaseOrderId, body.RowVersion));
        if (!result.Succeeded)
            return Results.BadRequest(result.Errors);
        return Results.NoContent();
    }

    public static async Task<IResult> DeleteEncumbrance(
        [FromServices] ISender sender,
        int id,
        [FromBody] EncumbranceActionRequest body)
    {
        var result = await sender.Send(new DeleteEncumbranceCommand(id, body.RowVersion));
        if (!result.Succeeded)
            return Results.BadRequest(result.Errors);
        return Results.NoContent();
    }

    public static async Task<IResult> SubmitEncumbrance(
        [FromServices] ISender sender,
        int id,
        [FromBody] EncumbranceActionRequest body)
    {
        var result = await sender.Send(new SubmitEncumbranceCommand(id, body.RowVersion));
        if (!result.Succeeded)
            return Results.BadRequest(result.Errors);
        return Results.NoContent();
    }

    public static async Task<IResult> ApproveEncumbrance(
        [FromServices] ISender sender,
        int id,
        [FromBody] EncumbranceActionRequest body)
    {
        var result = await sender.Send(new ApproveEncumbranceCommand(id, body.RowVersion, body.Reason));
        if (!result.Succeeded)
            return Results.BadRequest(result.Errors);
        return Results.NoContent();
    }

    public static async Task<IResult> ActivateEncumbrance(
        [FromServices] ISender sender,
        int id,
        [FromBody] EncumbranceActionRequest body)
    {
        var result = await sender.Send(new ActivateEncumbranceCommand(id, body.RowVersion));
        if (!result.Succeeded)
            return Results.BadRequest(result.Errors);
        return Results.NoContent();
    }

    public static async Task<IResult> SuspendEncumbrance(
        [FromServices] ISender sender,
        int id,
        [FromBody] EncumbranceActionRequest body)
    {
        var result = await sender.Send(new SuspendEncumbranceCommand(id, body.RowVersion, body.Reason));
        if (!result.Succeeded)
            return Results.BadRequest(result.Errors);
        return Results.NoContent();
    }

    public static async Task<IResult> CloseEncumbrance(
        [FromServices] ISender sender,
        int id,
        [FromBody] EncumbranceActionRequest body)
    {
        var result = await sender.Send(new CloseEncumbranceCommand(id, body.RowVersion, body.Reason));
        if (!result.Succeeded)
            return Results.BadRequest(result.Errors);
        return Results.NoContent();
    }

    public static async Task<IResult> CancelEncumbrance(
        [FromServices] ISender sender,
        int id,
        [FromBody] EncumbranceActionRequest body)
    {
        var result = await sender.Send(new CancelEncumbranceCommand(id, body.RowVersion, body.Reason));
        if (!result.Succeeded)
            return Results.BadRequest(result.Errors);
        return Results.NoContent();
    }

    public static async Task<IResult> ReverseEncumbrance(
        [FromServices] ISender sender,
        int id,
        [FromBody] ReverseEncumbranceRequest body)
    {
        var result = await sender.Send(new ReverseEncumbranceCommand(id, body.RowVersion, body.Reason));
        if (!result.Succeeded)
            return Results.BadRequest(result.Errors);
        return Results.NoContent();
    }
}

public record CreateEncumbranceRequest(
    int AppropriationId,
    EncumbranceType EncumbranceType,
    int? VendorId,
    int? PurchaseOrderId,
    string DocumentType,
    int DocumentId,
    string? Description,
    DateOnly EncumbranceDate,
    decimal Amount);

public record UpdateEncumbranceRequest(
    string? Description,
    DateOnly? EncumbranceDate,
    decimal? Amount,
    int? VendorId,
    int? PurchaseOrderId,
    byte[] RowVersion);

public record EncumbranceActionRequest(byte[] RowVersion, string? Reason = null);

public record ReverseEncumbranceRequest(byte[] RowVersion, string? Reason);
