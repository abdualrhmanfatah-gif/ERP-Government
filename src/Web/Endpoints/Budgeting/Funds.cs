using ERP_Government.Application.Budgeting.Commands.Funds;
using ERP_Government.Application.Budgeting.Common;
using ERP_Government.Application.Budgeting.Queries.Funds;
using ERP_Government.Application.Common.Security;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ERP_Government.Web.Endpoint.Budgeting;

public class Funds : IEndpointGroup
{
    public static void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapGet("/", GetFunds)
            .Produces<List<FundDto>>()
            .RequireAuthorization(PermissionCodes.FundsView);

        groupBuilder.MapGet("/{id:int}", GetFundById)
            .Produces<FundDto>()
            .Produces(StatusCodes.Status404NotFound)
            .RequireAuthorization(PermissionCodes.FundsView);

        groupBuilder.MapPost("/", CreateFund)
            .Produces<int>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .RequireAuthorization(PermissionCodes.FundsCreate);

        groupBuilder.MapPut("/{id:int}", UpdateFund)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .RequireAuthorization(PermissionCodes.FundsUpdate);

        groupBuilder.MapPatch("/{id:int}/toggle-active", ToggleFundActive)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .RequireAuthorization(PermissionCodes.FundsToggleActive);
    }

    [EndpointSummary("Get all funds")]
    public static async Task<List<FundDto>> GetFunds(
        [FromServices] ISender sender)
    {
        return await sender.Send(new GetFundsListQuery());
    }

    [EndpointSummary("Get fund by ID")]
    public static async Task<FundDto> GetFundById(
        [FromServices] ISender sender,
        int id)
    {
        return await sender.Send(new GetFundByIdQuery(id));
    }

    [EndpointSummary("Create a new fund")]
    public static async Task<IResult> CreateFund(
        [FromServices] ISender sender,
        [FromBody] CreateFundRequest body)
    {
        var result = await sender.Send(new CreateFundCommand(
            body.FundNumber, body.FundName, body.FundType, body.FundCategory,
            body.LegalAuthority, body.Description,
            body.DefaultRevenueAccountId, body.CurrencyId));
        if (!result.Succeeded)
            return Results.BadRequest(result.Errors);
        return Results.Created($"/api/Funds/{result.Value}", result.Value);
    }

    [EndpointSummary("Update a fund")]
    public static async Task<IResult> UpdateFund(
        [FromServices] ISender sender,
        int id,
        [FromBody] UpdateFundRequest body)
    {
        var result = await sender.Send(new UpdateFundCommand(
            id, body.FundNumber, body.FundName, body.FundType, body.FundCategory,
            body.LegalAuthority, body.Description,
            body.DefaultRevenueAccountId, body.CurrencyId, body.RowVersion));
        if (!result.Succeeded)
            return Results.BadRequest(result.Errors);
        return Results.NoContent();
    }

    [EndpointSummary("Toggle fund active status")]
    public static async Task<IResult> ToggleFundActive(
        [FromServices] ISender sender,
        int id,
        [FromBody] FundToggleActiveRequest body)
    {
        var result = await sender.Send(new ToggleFundActiveCommand(id, body.RowVersion));
        if (!result.Succeeded)
            return Results.BadRequest(result.Errors);
        return Results.NoContent();
    }
}

public record CreateFundRequest(
    string FundNumber,
    string FundName,
    ERP_Government.Domain.Budgeting.Enums.FundType FundType,
    ERP_Government.Domain.Budgeting.Enums.FundCategory FundCategory,
    string LegalAuthority,
    string? Description,
    int? DefaultRevenueAccountId,
    int? CurrencyId);

public record FundToggleActiveRequest(byte[] RowVersion);

public record UpdateFundRequest(
    string FundNumber,
    string FundName,
    ERP_Government.Domain.Budgeting.Enums.FundType FundType,
    ERP_Government.Domain.Budgeting.Enums.FundCategory FundCategory,
    string LegalAuthority,
    string? Description,
    int? DefaultRevenueAccountId,
    int? CurrencyId,
    byte[] RowVersion);
