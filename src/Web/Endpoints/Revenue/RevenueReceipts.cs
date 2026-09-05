using ERP_Government.Application.Revenue.Common.DTOs;
using ERP_Government.Application.Revenue.Commands.RevenueReceipts.CreateRevenueReceipt;
using ERP_Government.Application.Revenue.Commands.RevenueReceipts.ApproveRevenueReceipt;
using ERP_Government.Application.Revenue.Commands.RevenueReceipts.PostRevenueReceipt;
using ERP_Government.Application.Revenue.Commands.RevenueReceipts.CancelRevenueReceipt;
using ERP_Government.Application.Revenue.Queries.RevenueReceipts.GetRevenueReceiptById;
using ERP_Government.Application.Revenue.Queries.RevenueReceipts.GetRevenueReceipts;
using ERP_Government.Application.Common.Security;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ERP_Government.Web.Endpoints.Revenue;

public class RevenueReceipts : IEndpointGroup
{
    public static void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapGet("/", GetRevenueReceipts)
            .Produces<List<RevenueReceiptDto>>()
            .RequireAuthorization(PermissionCodes.RevenueReceiptsView);

        groupBuilder.MapGet("/{id:int}", GetRevenueReceiptById)
            .Produces<RevenueReceiptDto?>()
            .RequireAuthorization(PermissionCodes.RevenueReceiptsView);

        groupBuilder.MapPost("/", CreateRevenueReceipt)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .RequireAuthorization(PermissionCodes.RevenueReceiptsCreate);

        groupBuilder.MapPost("/{id:int}/approve", ApproveRevenueReceipt)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .RequireAuthorization(PermissionCodes.RevenueReceiptsApprove);

        groupBuilder.MapPost("/{id:int}/post", PostRevenueReceipt)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .RequireAuthorization(PermissionCodes.RevenueReceiptsPost);

        groupBuilder.MapPost("/{id:int}/cancel", CancelRevenueReceipt)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .RequireAuthorization(PermissionCodes.RevenueReceiptsCancel);
    }

    [EndpointSummary("Get all revenue receipts")]
    public static async Task<List<RevenueReceiptDto>> GetRevenueReceipts(
        [FromServices] ISender sender,
        [AsParameters] GetRevenueReceiptsQuery query)
    {
        return await sender.Send(query);
    }

    [EndpointSummary("Get revenue receipt by ID")]
    public static async Task<RevenueReceiptDto?> GetRevenueReceiptById(
        [FromServices] ISender sender,
        int id)
    {
        return await sender.Send(new GetRevenueReceiptByIdQuery { Id = id });
    }

    [EndpointSummary("Create a new revenue receipt")]
    public static async Task<IResult> CreateRevenueReceipt(
        [FromServices] ISender sender,
        [FromBody] CreateRevenueReceiptCommand command)
    {
        var result = await sender.Send(command);
        if (!result.Succeeded)
            return Results.BadRequest(result.Errors);
        return Results.NoContent();
    }

    [EndpointSummary("Approve a revenue receipt")]
    public static async Task<IResult> ApproveRevenueReceipt(
        [FromServices] ISender sender,
        int id,
        [FromBody] ApproveRevenueReceiptCommand command)
    {
        if (id != command.Id)
            return Results.BadRequest("ID mismatch.");

        var result = await sender.Send(command);
        if (!result.Succeeded)
            return Results.BadRequest(result.Errors);
        return Results.NoContent();
    }

    [EndpointSummary("Post a revenue receipt (creates accounting entry)")]
    public static async Task<IResult> PostRevenueReceipt(
        [FromServices] ISender sender,
        int id,
        [FromBody] PostRevenueReceiptCommand command)
    {
        if (id != command.Id)
            return Results.BadRequest("ID mismatch.");

        var result = await sender.Send(command);
        if (!result.Succeeded)
            return Results.BadRequest(result.Errors);
        return Results.NoContent();
    }

    [EndpointSummary("Cancel a revenue receipt")]
    public static async Task<IResult> CancelRevenueReceipt(
        [FromServices] ISender sender,
        int id,
        [FromBody] CancelRevenueReceiptCommand command)
    {
        if (id != command.Id)
            return Results.BadRequest("ID mismatch.");

        var result = await sender.Send(command);
        if (!result.Succeeded)
            return Results.BadRequest(result.Errors);
        return Results.NoContent();
    }
}
