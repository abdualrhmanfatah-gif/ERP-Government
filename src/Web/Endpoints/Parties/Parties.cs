using ERP_Government.Application.Common.Interfaces;
using ERP_Government.Application.Common.Models;
using ERP_Government.Application.Parties.Commands.CreateParty;
using ERP_Government.Application.Parties.Commands.TogglePartyActive;
using ERP_Government.Application.Parties.Commands.UpdateParty;
using ERP_Government.Application.Parties.Queries.GetParties;
using ERP_Government.Application.Parties.Queries.GetPartyById;
using ERP_Government.Domain.Parties.Enums;
using ERP_Government.Web.Infrastructure;
using Microsoft.EntityFrameworkCore;
using ERP_Government.Application.Common.Security;

namespace ERP_Government.Web.Endpoints.Parties;

public class Parties : IEndpointGroup
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapGet("/", HandleGetAll)
            .RequireAuthorization(PermissionCodes.PartiesView)
            .Produces<IReadOnlyList<PartyResponse>>();
        group.MapGet("/{id:int}", HandleGetById)
            .RequireAuthorization(PermissionCodes.PartiesView)
            .Produces<PartyResponse?>();
        group.MapPost("/", HandleCreate)
            .RequireAuthorization(PermissionCodes.PartiesCreate)
            .Produces<int>();
        group.MapPut("/{id:int}", HandleUpdate)
            .RequireAuthorization(PermissionCodes.PartiesUpdate)
            .Produces<Result>();
        group.MapPatch("/{id:int}/toggle-active", HandleToggleActive)
            .RequireAuthorization(PermissionCodes.PartiesUpdate)
            .Produces<Result>();
        group.MapGet("/{id:int}/documents", HandleGetDocuments)
            .RequireAuthorization(PermissionCodes.PartiesView)
            .Produces<IReadOnlyList<PartyDocumentResponse>>();
        group.MapGet("/check-tax-number", HandleCheckTaxNumber)
            .RequireAuthorization(PermissionCodes.PartiesView)
            .Produces<TaxNumberCheckResponse>();
    }

    private static async Task<IResult> HandleGetAll(
        ISender sender,
        PartyType? partyType = null,
        bool? isActive = null,
        string? search = null)
    {
        var result = await sender.Send(new GetPartiesQuery(partyType, isActive, search));
        return Results.Ok(result.Select(p => p.ToResponse()).ToList());
    }

    private static async Task<IResult> HandleGetById(
        ISender sender,
        int id)
    {
        var result = await sender.Send(new GetPartyByIdQuery(id));
        return result is not null ? Results.Ok(result.ToResponse()) : Results.NotFound();
    }

    private static async Task<IResult> HandleCreate(
        ISender sender,
        CreatePartyRequest request)
    {
        var result = await sender.Send(request.ToCommand());
        return result.ToProblemDetails();
    }

    private static async Task<IResult> HandleUpdate(
        ISender sender,
        int id,
        UpdatePartyRequest request)
    {
        var result = await sender.Send(request.ToCommand(id));
        return result.ToProblemDetails();
    }

    private static async Task<IResult> HandleToggleActive(
        ISender sender,
        int id)
    {
        var result = await sender.Send(new TogglePartyActiveCommand(id));
        return result.ToProblemDetails();
    }

    private static async Task<IResult> HandleGetDocuments(
        IApplicationDbContext dbContext,
        int id)
    {
        var receipts = await dbContext.ReceiptVouchers
            .Where(r => r.PartyId == id)
            .ToListAsync();

        var encumbrances = await dbContext.Encumbrances
            .Where(e => e.VendorPartyId == id)
            .ToListAsync();

        var all = new List<PartyDocumentResponse>();

        all.AddRange(receipts.Select(r => new PartyDocumentResponse(
            "ReceiptVoucher", r.Id, r.VoucherNumber, r.Status.ToString(), r.VoucherDate, 0m)));

        all.AddRange(encumbrances.Select(e => new PartyDocumentResponse(
            "Encumbrance", e.Id, e.EncumbranceNumber, e.Status.ToString(), e.EncumbranceDate, e.TotalAmount)));

        return Results.Ok(all.OrderByDescending(d => d.Date).ToList());
    }

    private static async Task<IResult> HandleCheckTaxNumber(
        IApplicationDbContext dbContext,
        string taxNumber,
        int? excludeId = null)
    {
        if (string.IsNullOrEmpty(taxNumber))
            return Results.Ok(new TaxNumberCheckResponse(false));

        var exists = await dbContext.Parties
            .AnyAsync(p => p.TaxNumber == taxNumber && (excludeId == null || p.Id != excludeId));

        return Results.Ok(new TaxNumberCheckResponse(exists));
    }
}

public record PartyDocumentResponse(
    string DocumentType,
    int DocumentId,
    string DocumentNumber,
    string Status,
    DateOnly Date,
    decimal Amount);

public record PartyResponse(
    int Id,
    string PartyCode,
    PartyType PartyType,
    string NameAr,
    string? NameEn,
    string? TaxNumber,
    string? NationalId,
    string? Phone,
    string? Email,
    string? Address,
    string? Notes,
    bool IsActive);

public record CreatePartyRequest(
    PartyType PartyType,
    string NameAr,
    string? NameEn,
    string? TaxNumber,
    string? NationalId,
    string? Phone,
    string? Email,
    string? Address,
    string? Notes)
{
    public CreatePartyCommand ToCommand() => new(
        PartyType, NameAr, NameEn, TaxNumber, NationalId, Phone, Email, Address, Notes);
}

public record UpdatePartyRequest(
    PartyType PartyType,
    string NameAr,
    string? NameEn,
    string? TaxNumber,
    string? NationalId,
    string? Phone,
    string? Email,
    string? Address,
    string? Notes)
{
    public UpdatePartyCommand ToCommand(int id) => new(
        id, PartyType, NameAr, NameEn, TaxNumber, NationalId, Phone, Email, Address, Notes);
}

internal static class PartyMappingExtensions
{
    public static PartyResponse ToResponse(this ERP_Government.Domain.Parties.Entities.Party p) => new(
        p.Id,
        p.PartyCode,
        p.PartyType,
        p.NameAr,
        p.NameEn,
        p.TaxNumber,
        p.NationalId,
        p.Phone,
        p.Email,
        p.Address,
        p.Notes,
        p.IsActive);
}

public record TaxNumberCheckResponse(bool Exists);
