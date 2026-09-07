using ERP_Government.Application.Payments.Common.DTOs;
using ERP_Government.Application.Payments.Commands.PaymentOrders.CreatePaymentOrder;
using ERP_Government.Application.Payments.Commands.PaymentOrders.UpdatePaymentOrder;
using ERP_Government.Application.Payments.Commands.PaymentOrders.SubmitPaymentOrder;
using ERP_Government.Application.Payments.Commands.PaymentOrders.ApprovePaymentOrder;
using ERP_Government.Application.Payments.Commands.PaymentOrders.RejectPaymentOrder;
using ERP_Government.Application.Payments.Commands.PaymentOrders.CancelPaymentOrder;
using ERP_Government.Application.Payments.Commands.PaymentOrders.SendToTreasury;
using ERP_Government.Application.Payments.Commands.PaymentOrders.VoidPaymentOrder;
using ERP_Government.Application.Payments.Queries.GetPaymentOrderTotals;
using ERP_Government.Application.Payments.Queries.PaymentOrders.GetPaymentOrderById;
using ERP_Government.Application.Payments.Queries.PaymentOrders.GetPaymentOrders;
using ERP_Government.Application.Common.Security;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ERP_Government.Web.Endpoints.Payments;

public class PaymentOrders : IEndpointGroup
{
    public static void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapGet("/", GetPaymentOrders)
            .Produces<List<PaymentOrderDto>>()
            .RequireAuthorization(PermissionCodes.PaymentOrdersView);

        groupBuilder.MapGet("/{id:int}", GetPaymentOrderById)
            .Produces<PaymentOrderDto?>()
            .RequireAuthorization(PermissionCodes.PaymentOrdersView);

        groupBuilder.MapGet("/{id:int}/totals", GetPaymentOrderTotals)
            .Produces<PaymentOrderTotalsDto?>()
            .RequireAuthorization(PermissionCodes.PaymentOrdersView);

        groupBuilder.MapPost("/", CreatePaymentOrder)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .RequireAuthorization(PermissionCodes.PaymentOrdersCreate);

        groupBuilder.MapPost("/{id:int}/submit", SubmitPaymentOrder)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .RequireAuthorization(PermissionCodes.PaymentOrdersSubmit);

        groupBuilder.MapPost("/{id:int}/approve", ApprovePaymentOrder)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .RequireAuthorization(PermissionCodes.PaymentOrdersApprove);

        groupBuilder.MapPost("/{id:int}/reject", RejectPaymentOrder)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .RequireAuthorization(PermissionCodes.PaymentOrdersReject);

        groupBuilder.MapPost("/{id:int}/cancel", CancelPaymentOrder)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .RequireAuthorization(PermissionCodes.PaymentOrdersCancel);

        groupBuilder.MapPost("/{id:int}/send-to-treasury", SendToTreasury)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .RequireAuthorization(PermissionCodes.PaymentOrdersSendToTreasury);

        groupBuilder.MapPost("/{id:int}/void", VoidPaymentOrder)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .RequireAuthorization(PermissionCodes.PaymentOrdersVoid);

        groupBuilder.MapPut("/{id:int}", UpdatePaymentOrder)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .RequireAuthorization(PermissionCodes.PaymentOrdersUpdate);
    }

    [EndpointSummary("Get all payment orders")]
    public static async Task<List<PaymentOrderDto>> GetPaymentOrders(
        [FromServices] ISender sender,
        [AsParameters] GetPaymentOrdersQuery query)
    {
        return await sender.Send(query);
    }

    [EndpointSummary("Get payment order by ID")]
    public static async Task<PaymentOrderDto?> GetPaymentOrderById(
        [FromServices] ISender sender,
        int id)
    {
        return await sender.Send(new GetPaymentOrderByIdQuery { Id = id });
    }

    [EndpointSummary("Get computed totals for a payment order")]
    public static async Task<PaymentOrderTotalsDto?> GetPaymentOrderTotals(
        [FromServices] ISender sender,
        int id)
    {
        return await sender.Send(new GetPaymentOrderTotalsQuery { Id = id });
    }

    [EndpointSummary("Create a new payment order")]
    public static async Task<IResult> CreatePaymentOrder(
        [FromServices] ISender sender,
        [FromBody] CreatePaymentOrderCommand command)
    {
        var result = await sender.Send(command);
        if (!result.Succeeded)
            return Results.BadRequest(result.Errors);
        return Results.NoContent();
    }

    [EndpointSummary("Update a draft payment order (header, lines and deductions are replaced)")]
    public static async Task<IResult> UpdatePaymentOrder(
        [FromServices] ISender sender,
        int id,
        [FromBody] UpdatePaymentOrderCommand command)
    {
        if (id != command.Id)
            return Results.BadRequest("ID mismatch.");

        var result = await sender.Send(command);
        if (!result.Succeeded)
            return Results.BadRequest(result.Errors);
        return Results.NoContent();
    }

    [EndpointSummary("Submit payment order for approval")]
    public static async Task<IResult> SubmitPaymentOrder(
        [FromServices] ISender sender,
        int id,
        [FromBody] SubmitPaymentOrderCommand command)
    {
        if (id != command.Id)
            return Results.BadRequest("ID mismatch.");

        var result = await sender.Send(command);
        if (!result.Succeeded)
            return Results.BadRequest(result.Errors);
        return Results.NoContent();
    }

    [EndpointSummary("Approve a submitted payment order")]
    public static async Task<IResult> ApprovePaymentOrder(
        [FromServices] ISender sender,
        int id,
        [FromBody] ApprovePaymentOrderCommand command)
    {
        if (id != command.Id)
            return Results.BadRequest("ID mismatch.");

        var result = await sender.Send(command);
        if (!result.Succeeded)
            return Results.BadRequest(result.Errors);
        return Results.NoContent();
    }

    [EndpointSummary("Reject a submitted payment order")]
    public static async Task<IResult> RejectPaymentOrder(
        [FromServices] ISender sender,
        int id,
        [FromBody] RejectPaymentOrderCommand command)
    {
        if (id != command.Id)
            return Results.BadRequest("ID mismatch.");

        var result = await sender.Send(command);
        if (!result.Succeeded)
            return Results.BadRequest(result.Errors);
        return Results.NoContent();
    }

    [EndpointSummary("Cancel a payment order")]
    public static async Task<IResult> CancelPaymentOrder(
        [FromServices] ISender sender,
        int id,
        [FromBody] CancelPaymentOrderCommand command)
    {
        if (id != command.Id)
            return Results.BadRequest("ID mismatch.");

        var result = await sender.Send(command);
        if (!result.Succeeded)
            return Results.BadRequest(result.Errors);
        return Results.NoContent();
    }

    [EndpointSummary("Send payment order to treasury")]
    public static async Task<IResult> SendToTreasury(
        [FromServices] ISender sender,
        int id,
        [FromBody] SendToTreasuryCommand command)
    {
        if (id != command.Id)
            return Results.BadRequest("ID mismatch.");

        var result = await sender.Send(command);
        if (!result.Succeeded)
            return Results.BadRequest(result.Errors);
        return Results.NoContent();
    }

    [EndpointSummary("Void a payment order")]
    public static async Task<IResult> VoidPaymentOrder(
        [FromServices] ISender sender,
        int id,
        [FromBody] VoidPaymentOrderCommand command)
    {
        if (id != command.Id)
            return Results.BadRequest("ID mismatch.");

        var result = await sender.Send(command);
        if (!result.Succeeded)
            return Results.BadRequest(result.Errors);
        return Results.NoContent();
    }
}
