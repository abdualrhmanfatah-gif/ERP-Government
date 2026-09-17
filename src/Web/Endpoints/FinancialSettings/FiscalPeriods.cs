using ERP_Government.Application.FinancialSettings.Common.DTOs;
using ERP_Government.Application.FinancialSettings.Commands.FiscalPeriods;
using ERP_Government.Application.FinancialSettings.Queries.FiscalPeriods;
using ERP_Government.Application.Common.Security;
using MediatR;
using ERP_Government.Web.Infrastructure;
using Microsoft.AspNetCore.Mvc;

namespace ERP_Government.Web.Endpoint.FinancialSettings;

public class FiscalPeriods : IEndpointGroup
{
    public static void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapGet("/", GetFiscalPeriods)
            .Produces<List<FiscalPeriodDto>>()
            .RequireAuthorization(PermissionCodes.FiscalPeriodsView);

        groupBuilder.MapGet("/{id:int}", GetFiscalPeriodById)
            .Produces<FiscalPeriodDto?>()
            .RequireAuthorization(PermissionCodes.FiscalPeriodsView);

        groupBuilder.MapPost("/", CreateFiscalPeriod)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .RequireAuthorization(PermissionCodes.FiscalPeriodsCreate);

        groupBuilder.MapPost("/{id:int}/lock", LockFiscalPeriod)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .RequireAuthorization(PermissionCodes.FiscalPeriodsLock);

        groupBuilder.MapPost("/{id:int}/unlock", UnlockFiscalPeriod)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .RequireAuthorization(PermissionCodes.FiscalPeriodsLock);

        groupBuilder.MapPut("/{id:int}", UpdateFiscalPeriod)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .RequireAuthorization(PermissionCodes.FiscalPeriodsCreate);

        groupBuilder.MapPost("/bulk-generate", BulkGeneratePeriods)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .RequireAuthorization(PermissionCodes.FiscalPeriodsCreate);
    }

    [EndpointSummary("Get fiscal periods for a year")]
    public static async Task<List<FiscalPeriodDto>> GetFiscalPeriods(
        [FromServices] ISender sender,
        [AsParameters] GetFiscalPeriodsQuery query)
    {
        return await sender.Send(query);
    }

    [EndpointSummary("Get fiscal period by ID")]
    public static async Task<IResult> GetFiscalPeriodById(
        [FromServices] ISender sender,
        int id)
    {
        var result = await sender.Send(new GetFiscalPeriodByIdQuery { Id = id });
        return result.Succeeded ? Results.Ok(result.Value!) : result.ToProblemDetails();
    }

    [EndpointSummary("Create a new fiscal period")]
    public static async Task<IResult> CreateFiscalPeriod(
        [FromServices] ISender sender,
        [FromBody] CreateFiscalPeriodCommand command)
    {
        var result = await sender.Send(command);
        if (!result.Succeeded)
            return result.ToProblemDetails();
        return Results.NoContent();
    }

    [EndpointSummary("Lock a fiscal period for posting")]
    public static async Task<IResult> LockFiscalPeriod(
        [FromServices] ISender sender,
        int id)
    {
        var result = await sender.Send(new LockFiscalPeriodCommand { Id = id });
        if (!result.Succeeded)
            return result.ToProblemDetails();
        return Results.NoContent();
    }

    [EndpointSummary("Unlock a fiscal period for posting")]
    public static async Task<IResult> UnlockFiscalPeriod(
        [FromServices] ISender sender,
        int id)
    {
        var result = await sender.Send(new UnlockFiscalPeriodCommand { Id = id });
        if (!result.Succeeded)
            return result.ToProblemDetails();
        return Results.NoContent();
    }

    [EndpointSummary("Update a fiscal period")]
    public static async Task<IResult> UpdateFiscalPeriod(
        [FromServices] ISender sender,
        int id,
        [FromBody] UpdateFiscalPeriodCommand command)
    {
        var result = await sender.Send(new UpdateFiscalPeriodCommand
        {
            Id = id,
            Name = command.Name,
            StartDate = command.StartDate,
            EndDate = command.EndDate
        });
        if (!result.Succeeded)
            return result.ToProblemDetails();
        return Results.NoContent();
    }

    [EndpointSummary("Bulk generate 12 monthly periods for a fiscal year")]
    public static async Task<IResult> BulkGeneratePeriods(
        [FromServices] ISender sender,
        [FromBody] BulkGeneratePeriodsCommand command)
    {
        var result = await sender.Send(command);
        if (!result.Succeeded)
            return result.ToProblemDetails();
        return Results.NoContent();
    }
}
