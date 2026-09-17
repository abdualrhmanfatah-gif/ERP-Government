using ERP_Government.Application.FinancialSettings.Common.DTOs;
using ERP_Government.Application.FinancialSettings.Commands.FiscalYears;
using ERP_Government.Application.FinancialSettings.Queries.FiscalYears;
using ERP_Government.Application.Common.Security;
using MediatR;
using ERP_Government.Web.Infrastructure;
using Microsoft.AspNetCore.Mvc;

namespace ERP_Government.Web.Endpoint.FinancialSettings;

public class FiscalYears : IEndpointGroup
{
    public static void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapGet("/", GetFiscalYears)
            .Produces<List<FiscalYearDto>>()
            .RequireAuthorization(PermissionCodes.FiscalYearsView);

        groupBuilder.MapGet("/{id:int}", GetFiscalYearById)
            .Produces<FiscalYearDto?>()
            .RequireAuthorization(PermissionCodes.FiscalYearsView);

        groupBuilder.MapPost("/", CreateFiscalYear)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .RequireAuthorization(PermissionCodes.FiscalYearsCreate);

        groupBuilder.MapPut("/{id:int}", UpdateFiscalYear)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .RequireAuthorization(PermissionCodes.FiscalYearsUpdate);

        groupBuilder.MapPost("/{id:int}/open", OpenFiscalYear)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .RequireAuthorization(PermissionCodes.FiscalYearsOpen);

        groupBuilder.MapPost("/{id:int}/close", CloseFiscalYear)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .RequireAuthorization(PermissionCodes.FiscalYearsClose);

        groupBuilder.MapGet("/by-date", GetFiscalYearPeriodByDate)
            .Produces<GetFiscalYearPeriodByDateResult?>()
            .RequireAuthorization(PermissionCodes.FiscalYearsView);
    }

    [EndpointSummary("Get all fiscal years")]
    public static async Task<List<FiscalYearDto>> GetFiscalYears(
        [FromServices] ISender sender,
        [AsParameters] GetFiscalYearsQuery query)
    {
        return await sender.Send(query);
    }

    [EndpointSummary("Get fiscal year by ID")]
    public static async Task<IResult> GetFiscalYearById(
        [FromServices] ISender sender,
        int id)
    {
        var result = await sender.Send(new GetFiscalYearByIdQuery { Id = id });
        return result.Succeeded ? Results.Ok(result.Value!) : result.ToProblemDetails();
    }

    [EndpointSummary("Create a new fiscal year")]
    public static async Task<IResult> CreateFiscalYear(
        [FromServices] ISender sender,
        [FromBody] CreateFiscalYearCommand command)
    {
        var result = await sender.Send(command);
        if (!result.Succeeded)
            return result.ToProblemDetails();
        return Results.NoContent();
    }

    [EndpointSummary("Update a fiscal year")]
    public static async Task<IResult> UpdateFiscalYear(
        [FromServices] ISender sender,
        int id,
        [FromBody] UpdateFiscalYearCommand command)
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

    [EndpointSummary("Open a draft fiscal year")]
    public static async Task<IResult> OpenFiscalYear(
        [FromServices] ISender sender,
        int id)
    {
        var result = await sender.Send(new OpenFiscalYearCommand { Id = id });
        if (!result.Succeeded)
            return result.ToProblemDetails();
        return Results.NoContent();
    }

    [EndpointSummary("Close an open fiscal year")]
    public static async Task<IResult> CloseFiscalYear(
        [FromServices] ISender sender,
        int id)
    {
        var result = await sender.Send(new CloseFiscalYearCommand { Id = id });
        if (!result.Succeeded)
            return result.ToProblemDetails();
        return Results.NoContent();
    }

    [EndpointSummary("Get fiscal year and period by date")]
    public static async Task<IResult> GetFiscalYearPeriodByDate(
        [FromServices] ISender sender,
        [FromQuery] DateTime date)
    {
        var result = await sender.Send(new GetFiscalYearPeriodByDateQuery { Date = date });
        return result.Succeeded ? Results.Ok(result.Value!) : result.ToProblemDetails();
    }
}
