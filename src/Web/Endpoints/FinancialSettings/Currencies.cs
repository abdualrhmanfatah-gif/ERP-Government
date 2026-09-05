using ERP_Government.Application.FinancialSettings.Common.DTOs;
using ERP_Government.Application.FinancialSettings.Commands.Currencies;
using ERP_Government.Application.FinancialSettings.Queries.Currencies;
using ERP_Government.Application.Common.Security;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ERP_Government.Web.Endpoint.FinancialSettings;

public class Currencies : IEndpointGroup
{
    public static void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapGet("/", GetCurrencies)
            .Produces<List<CurrencyDto>>()
            .RequireAuthorization(PermissionCodes.CurrenciesView);

        groupBuilder.MapGet("/{id:int}", GetCurrencyById)
            .Produces<CurrencyDto?>()
            .RequireAuthorization(PermissionCodes.CurrenciesView);

        groupBuilder.MapGet("/iso4217", GetIso4217Codes)
            .Produces<List<Iso4217CodeDto>>()
            .RequireAuthorization(PermissionCodes.CurrenciesView);

        groupBuilder.MapPost("/", CreateCurrency)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .RequireAuthorization(PermissionCodes.CurrenciesCreate);

        groupBuilder.MapPut("/{id:int}", UpdateCurrency)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .RequireAuthorization(PermissionCodes.CurrenciesUpdate);

        groupBuilder.MapPost("/{id:int}/activate", ActivateCurrency)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .RequireAuthorization(PermissionCodes.CurrenciesActivate);

        groupBuilder.MapPost("/{id:int}/deactivate", DeactivateCurrency)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .RequireAuthorization(PermissionCodes.CurrenciesDeactivate);
    }

    [EndpointSummary("Get all currencies")]
    public static async Task<List<CurrencyDto>> GetCurrencies(
        [FromServices] ISender sender,
        [AsParameters] GetCurrenciesQuery query)
    {
        return await sender.Send(query);
    }

    [EndpointSummary("Get currency by ID")]
    public static async Task<CurrencyDto?> GetCurrencyById(
        [FromServices] ISender sender,
        int id)
    {
        return await sender.Send(new GetCurrencyByIdQuery { Id = id });
    }

    [EndpointSummary("Get ISO 4217 reference codes")]
    public static async Task<List<Iso4217CodeDto>> GetIso4217Codes(
        [FromServices] ISender sender,
        [FromQuery] string? query = null)
    {
        return await sender.Send(new GetIso4217CodesQuery { Query = query });
    }

    [EndpointSummary("Create a new currency")]
    public static async Task<IResult> CreateCurrency(
        [FromServices] ISender sender,
        [FromBody] CreateCurrencyCommand command)
    {
        var result = await sender.Send(command);
        if (!result.Succeeded)
            return Results.BadRequest(result.Errors);
        return Results.NoContent();
    }

    [EndpointSummary("Update a currency")]
    public static async Task<IResult> UpdateCurrency(
        [FromServices] ISender sender,
        int id,
        [FromBody] UpdateCurrencyCommand command)
    {
        if (id != command.Id)
            return Results.BadRequest("ID mismatch.");

        var result = await sender.Send(command);
        if (!result.Succeeded)
            return Results.BadRequest(result.Errors);
        return Results.NoContent();
    }

    [EndpointSummary("Activate a currency")]
    public static async Task<IResult> ActivateCurrency(
        [FromServices] ISender sender,
        int id,
        [FromBody] ActivateCurrencyCommand command)
    {
        if (id != command.Id)
            return Results.BadRequest("ID mismatch.");

        var result = await sender.Send(command);
        if (!result.Succeeded)
            return Results.BadRequest(result.Errors);
        return Results.NoContent();
    }

    [EndpointSummary("Deactivate a currency")]
    public static async Task<IResult> DeactivateCurrency(
        [FromServices] ISender sender,
        int id,
        [FromBody] DeactivateCurrencyCommand command)
    {
        if (id != command.Id)
            return Results.BadRequest("ID mismatch.");

        var result = await sender.Send(command);
        if (!result.Succeeded)
            return Results.BadRequest(result.Errors);
        return Results.NoContent();
    }
}
