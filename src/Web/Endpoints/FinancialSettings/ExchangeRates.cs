using ERP_Government.Application.FinancialSettings.Common.DTOs;
using ERP_Government.Application.FinancialSettings.Commands.ExchangeRates;
using ERP_Government.Application.FinancialSettings.Queries.ExchangeRates;
using ERP_Government.Application.Common.Security;
using MediatR;
using ERP_Government.Web.Infrastructure;
using Microsoft.AspNetCore.Mvc;

namespace ERP_Government.Web.Endpoint.FinancialSettings;

public class ExchangeRates : IEndpointGroup
{
    public static void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapGet("/", GetExchangeRates)
            .Produces<List<ExchangeRateDto>>()
            .RequireAuthorization(PermissionCodes.ExchangeRatesView);

        groupBuilder.MapGet("/{id:int}", GetExchangeRateById)
            .Produces<ExchangeRateDto?>()
            .RequireAuthorization(PermissionCodes.ExchangeRatesView);

        groupBuilder.MapGet("/lookup", LookupExchangeRate)
            .Produces<ExchangeRateLookupDto?>()
            .Produces(StatusCodes.Status404NotFound)
            .RequireAuthorization(PermissionCodes.ExchangeRatesView);

        groupBuilder.MapPost("/", CreateExchangeRate)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .RequireAuthorization(PermissionCodes.ExchangeRatesCreate);

        groupBuilder.MapPut("/{id:int}", UpdateExchangeRate)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .RequireAuthorization(PermissionCodes.ExchangeRatesUpdate);

        groupBuilder.MapPost("/{id:int}/activate", ActivateExchangeRate)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .RequireAuthorization(PermissionCodes.ExchangeRatesActivate);

        groupBuilder.MapPost("/{id:int}/deactivate", DeactivateExchangeRate)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .RequireAuthorization(PermissionCodes.ExchangeRatesDeactivate);
    }

    [EndpointSummary("Get all exchange rates")]
    public static async Task<List<ExchangeRateDto>> GetExchangeRates(
        [FromServices] ISender sender,
        [AsParameters] GetExchangeRatesQuery query)
    {
        return await sender.Send(query);
    }

    [EndpointSummary("Get exchange rate by ID")]
    public static async Task<IResult> GetExchangeRateById(
        [FromServices] ISender sender,
        int id)
    {
        var result = await sender.Send(new GetExchangeRateByIdQuery { Id = id });
        return result.Succeeded ? Results.Ok(result.Value!) : result.ToProblemDetails();
    }

    [EndpointSummary("Lookup exchange rate for conversion")]
    public static async Task<IResult> LookupExchangeRate(
        [FromServices] ISender sender,
        [AsParameters] LookupExchangeRateQuery query)
    {
        var result = await sender.Send(query);
        return result.Succeeded
            ? Results.Ok(result.Value!)
            : result.ToProblemDetails();
    }

    [EndpointSummary("Create a new exchange rate")]
    public static async Task<IResult> CreateExchangeRate(
        [FromServices] ISender sender,
        [FromBody] CreateExchangeRateCommand command)
    {
        var result = await sender.Send(command);
        if (!result.Succeeded)
            return result.ToProblemDetails();
        return Results.NoContent();
    }

    [EndpointSummary("Update an exchange rate")]
    public static async Task<IResult> UpdateExchangeRate(
        [FromServices] ISender sender,
        int id,
        [FromBody] UpdateExchangeRateCommand command)
    {
        if (id != command.Id)
            return Results.Problem(
                detail: "ID mismatch.",
                statusCode: StatusCodes.Status400BadRequest,
                title: "Bad Request",
                type: "about:blank");

        var result = await sender.Send(command);
        if (!result.Succeeded)
            return result.ToProblemDetails();
        return Results.NoContent();
    }

    [EndpointSummary("Activate an exchange rate")]
    public static async Task<IResult> ActivateExchangeRate(
        [FromServices] ISender sender,
        int id,
        [FromBody] ActivateExchangeRateCommand command)
    {
        if (id != command.Id)
            return Results.Problem(
                detail: "ID mismatch.",
                statusCode: StatusCodes.Status400BadRequest,
                title: "Bad Request",
                type: "about:blank");

        var result = await sender.Send(command);
        if (!result.Succeeded)
            return result.ToProblemDetails();
        return Results.NoContent();
    }

    [EndpointSummary("Deactivate an exchange rate")]
    public static async Task<IResult> DeactivateExchangeRate(
        [FromServices] ISender sender,
        int id,
        [FromBody] DeactivateExchangeRateCommand command)
    {
        if (id != command.Id)
            return Results.Problem(
                detail: "ID mismatch.",
                statusCode: StatusCodes.Status400BadRequest,
                title: "Bad Request",
                type: "about:blank");

        var result = await sender.Send(command);
        if (!result.Succeeded)
            return result.ToProblemDetails();
        return Results.NoContent();
    }
}
