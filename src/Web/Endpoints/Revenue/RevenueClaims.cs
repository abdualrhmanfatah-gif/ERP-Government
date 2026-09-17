using ERP_Government.Application.Common.Models;
using ERP_Government.Application.Common.Security;
using ERP_Government.Application.Revenue.Commands.RevenueClaims.ApproveRevenueClaim;
using ERP_Government.Application.Revenue.Commands.RevenueClaims.CreateRevenueClaim;
using ERP_Government.Application.Revenue.Commands.RevenueClaims.WriteOffRevenueClaim;
using ERP_Government.Application.Revenue.Common.DTOs;
using ERP_Government.Application.Revenue.Queries.RevenueClaims.GetRevenueClaims;
using ERP_Government.Domain.Revenue.Enums;
using ERP_Government.Web.Infrastructure;
using MediatR;

namespace ERP_Government.Web.Endpoints.Revenue;

public class RevenueClaims : IEndpointGroup
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapGet("/", HandleGetAll)
            .RequireAuthorization(PermissionCodes.RevenueClaimsView)
            .Produces<List<RevenueClaimDto>>();
        group.MapPost("/", HandleCreate)
            .RequireAuthorization(PermissionCodes.RevenueClaimsCreate)
            .Produces<Result<RevenueClaimDto>>();
        group.MapPost("/{id:int}/approve", HandleApprove)
            .RequireAuthorization(PermissionCodes.RevenueClaimsApprove)
            .Produces<Result>();
        group.MapPost("/{id:int}/writeoff", HandleWriteOff)
            .RequireAuthorization(PermissionCodes.RevenueClaimsWriteOff)
            .Produces<Result>();
    }

    private static async Task<IResult> HandleGetAll(
        ISender sender,
        int? partyId = null,
        ClaimStatus? status = null)
    {
        var result = await sender.Send(new GetRevenueClaimsQuery { PartyId = partyId, Status = status });
        return Results.Ok(result);
    }

    private static async Task<IResult> HandleCreate(
        ISender sender,
        CreateRevenueClaimCommand command)
    {
        var result = await sender.Send(command);
        return result.Succeeded ? Results.Ok(result) : Results.BadRequest(result);
    }

    private static async Task<IResult> HandleApprove(
        ISender sender,
        int id,
        ApproveRevenueClaimCommand command)
    {
        if (id != command.Id)
            return Results.BadRequest(Result.Failure(["Route ID does not match command ID."]));

        var result = await sender.Send(command);
        return result.Succeeded ? Results.Ok(result) : Results.BadRequest(result);
    }

    private static async Task<IResult> HandleWriteOff(
        ISender sender,
        int id,
        WriteOffRevenueClaimCommand command)
    {
        if (id != command.Id)
            return Results.BadRequest(Result.Failure(["Route ID does not match command ID."]));

        var result = await sender.Send(command);
        return result.Succeeded ? Results.Ok(result) : Results.BadRequest(result);
    }
}
