using ERP_Government.Application.Revenue.Common.DTOs;
using ERP_Government.Application.Revenue.Commands.DepositSlips.CreateDepositSlip;
using ERP_Government.Application.Revenue.Commands.DepositSlips.AddVoucherToSlip;
using ERP_Government.Application.Revenue.Commands.DepositSlips.RemoveVoucherFromSlip;
using ERP_Government.Application.Revenue.Commands.DepositSlips.ApproveDepositSlip;
using ERP_Government.Application.Revenue.Queries.DepositSlips.GetDepositSlips;
using ERP_Government.Application.Revenue.Queries.DepositSlips.GetDepositSlipById;
using ERP_Government.Application.Revenue.Queries.Statements.GetMonthlyStatement;
using ERP_Government.Application.Common.Security;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ERP_Government.Web.Endpoints.Revenue;

public class DepositSlips : IEndpointGroup
{
    public static string? RoutePrefix => "/api/Revenue/DepositSlips";

    public static void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapGet("/", GetDepositSlips)
            .Produces<List<DepositSlipDto>>()
            .RequireAuthorization(PermissionCodes.DepositSlipsView);

        groupBuilder.MapGet("/{id:int}", GetDepositSlipById)
            .Produces<DepositSlipDto?>()
            .RequireAuthorization(PermissionCodes.DepositSlipsView);

        groupBuilder.MapGet("/monthly-statement", GetMonthlyStatement)
            .Produces<MonthlyStatementDto>()
            .RequireAuthorization(PermissionCodes.DepositSlipsView);

        groupBuilder.MapPost("/", CreateDepositSlip)
            .Produces<int>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .RequireAuthorization(PermissionCodes.DepositSlipsCreate);

        groupBuilder.MapPost("/{id:int}/add-voucher", AddVoucherToSlip)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .RequireAuthorization(PermissionCodes.DepositSlipsUpdate);

        groupBuilder.MapPost("/{id:int}/remove-voucher", RemoveVoucherFromSlip)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .RequireAuthorization(PermissionCodes.DepositSlipsUpdate);

        groupBuilder.MapPost("/{id:int}/approve", ApproveDepositSlip)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .RequireAuthorization(PermissionCodes.DepositSlipsApprove);
    }

    [EndpointSummary("Get all deposit slips")]
    public static async Task<IResult> GetDepositSlips(
        [FromServices] ISender sender,
        [AsParameters] GetDepositSlipsQuery query)
    {
        var result = await sender.Send(query);
        if (!result.Succeeded)
            return Results.BadRequest(result.Errors);
        return Results.Ok(result.Value);
    }

    [EndpointSummary("Get deposit slip by ID")]
    public static async Task<IResult> GetDepositSlipById(
        [FromServices] ISender sender,
        int id)
    {
        var result = await sender.Send(new GetDepositSlipByIdQuery { Id = id });
        if (!result.Succeeded)
            return Results.NotFound();
        return Results.Ok(result.Value);
    }

    [EndpointSummary("Get monthly collections statement")]
    public static async Task<IResult> GetMonthlyStatement(
        [FromServices] ISender sender,
        [AsParameters] GetMonthlyStatementQuery query)
    {
        var result = await sender.Send(query);
        if (!result.Succeeded)
            return Results.BadRequest(result.Errors);
        return Results.Ok(result.Value);
    }

    [EndpointSummary("Create a new deposit slip")]
    public static async Task<IResult> CreateDepositSlip(
        [FromServices] ISender sender,
        [FromBody] CreateDepositSlipCommand command)
    {
        var result = await sender.Send(command);
        if (!result.Succeeded)
            return Results.BadRequest(result.Errors);
        return Results.Created($"/api/DepositSlips/{result.Value}", result.Value);
    }

    [EndpointSummary("Add voucher to deposit slip")]
    public static async Task<IResult> AddVoucherToSlip(
        [FromServices] ISender sender,
        int id,
        [FromBody] AddVoucherToSlipCommand command)
    {
        if (id != command.SlipId)
            return Results.BadRequest("ID mismatch.");

        var result = await sender.Send(command);
        if (!result.Succeeded)
            return Results.BadRequest(result.Errors);
        return Results.NoContent();
    }

    [EndpointSummary("Remove voucher from deposit slip")]
    public static async Task<IResult> RemoveVoucherFromSlip(
        [FromServices] ISender sender,
        int id,
        [FromBody] RemoveVoucherFromSlipCommand command)
    {
        if (id != command.SlipId)
            return Results.BadRequest("ID mismatch.");

        var result = await sender.Send(command);
        if (!result.Succeeded)
            return Results.BadRequest(result.Errors);
        return Results.NoContent();
    }

    [EndpointSummary("Approve deposit slip (Treasury manager)")]
    public static async Task<IResult> ApproveDepositSlip(
        [FromServices] ISender sender,
        int id,
        [FromBody] ApproveDepositSlipCommand command)
    {
        if (id != command.Id)
            return Results.BadRequest("ID mismatch.");

        var result = await sender.Send(command);
        if (!result.Succeeded)
            return Results.BadRequest(result.Errors);
        return Results.NoContent();
    }
}
