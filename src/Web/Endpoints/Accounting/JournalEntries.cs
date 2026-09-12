using ERP_Government.Application.Accounting.Commands.JournalEntryLines.CreateJournalEntryLine;
using ERP_Government.Application.Accounting.Commands.JournalEntryLines.RemoveJournalEntryLine;
using ERP_Government.Application.Accounting.Commands.JournalEntryLines.UpdateJournalEntryLine;
using ERP_Government.Application.Accounting.Commands.JournalEntries.ApproveJournalEntry;
using ERP_Government.Application.Accounting.Commands.JournalEntries.CancelJournalEntry;
using ERP_Government.Application.Accounting.Commands.JournalEntries.CreateJournalEntry;
using ERP_Government.Application.Accounting.Commands.JournalEntries.PostJournalEntry;
using ERP_Government.Application.Accounting.Commands.JournalEntries.ReverseJournalEntry;
using ERP_Government.Application.Accounting.Commands.JournalEntries.SubmitJournalEntry;
using ERP_Government.Application.Accounting.Commands.JournalEntries.UpdateJournalEntry;
using ERP_Government.Application.Accounting.Common;
using ERP_Government.Application.Accounting.Queries.JournalEntries.GetJournalEntryById;
using ERP_Government.Application.Accounting.Queries.JournalEntries.GetJournalEntriesList;
using ERP_Government.Application.Common.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ERP_Government.Web.Endpoints.Accounting;

public class JournalEntries : IEndpointGroup
{
    public static void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapGet("/", GetJournalEntriesList)
            .Produces<List<JournalEntryDto>>()
            .RequireAuthorization("Accounting.JournalEntries.Read");

        groupBuilder.MapGet("/{id:int}", GetJournalEntryById)
            .Produces<JournalEntryDto?>()
            .RequireAuthorization("Accounting.JournalEntries.Read");

        groupBuilder.MapPost("/", CreateJournalEntry)
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .RequireAuthorization("Accounting.JournalEntries.Create");

        groupBuilder.MapPut("/{id:int}", UpdateJournalEntry)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .RequireAuthorization("Accounting.JournalEntries.Create");

        groupBuilder.MapPost("/{id:int}/cancel", CancelJournalEntry)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .RequireAuthorization("Accounting.JournalEntries.Cancel");

        groupBuilder.MapPost("/{id:int}/submit", SubmitJournalEntry)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .RequireAuthorization("Accounting.JournalEntries.Submit");

        groupBuilder.MapPost("/{id:int}/approve", ApproveJournalEntry)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .RequireAuthorization("Accounting.JournalEntries.Approve");

        groupBuilder.MapPost("/{id:int}/post", PostJournalEntry)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .RequireAuthorization("Accounting.JournalEntries.Post");

        groupBuilder.MapPost("/{id:int}/reverse", ReverseJournalEntry)
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .RequireAuthorization("Accounting.JournalEntries.Reverse");

        // JournalEntryLines — US1 line management (Draft only, UpdateLines permission)
        groupBuilder.MapPost("/{id:int}/lines", CreateJournalEntryLine)
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .RequireAuthorization("Accounting.JournalEntries.UpdateLines");

        groupBuilder.MapPut("/{id:int}/lines/{lineId:long}", UpdateJournalEntryLine)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .RequireAuthorization("Accounting.JournalEntries.UpdateLines");

        groupBuilder.MapDelete("/{id:int}/lines/{lineId:long}", RemoveJournalEntryLine)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .RequireAuthorization("Accounting.JournalEntries.UpdateLines");

        groupBuilder.MapGet("/export", ExportJournalEntries)
            .RequireAuthorization("Accounting.JournalEntries.Read");
    }

    [EndpointSummary("Get all journal entries")]
    public static async Task<List<JournalEntryDto>> GetJournalEntriesList(
        [FromServices] ISender sender,
        [AsParameters] GetJournalEntriesListQuery query)
    {
        return await sender.Send(query);
    }

    [EndpointSummary("Get journal entry by ID")]
    public static async Task<JournalEntryDto?> GetJournalEntryById(
        [FromServices] ISender sender,
        int id)
    {
        return await sender.Send(new GetJournalEntryByIdQuery { Id = id });
    }

    [EndpointSummary("Create a new journal entry")]
    public static async Task<IResult> CreateJournalEntry(
        [FromServices] ISender sender,
        [FromBody] CreateJournalEntryCommand command)
    {
        var result = await sender.Send(command);
        if (!result.Succeeded)
            return Results.BadRequest(result.Errors);
        return Results.Ok(new { id = result.Value });
    }

    [EndpointSummary("Update a journal entry header")]
    public static async Task<IResult> UpdateJournalEntry(
        [FromServices] ISender sender,
        int id,
        [FromBody] UpdateJournalEntryCommand command)
    {
        if (id != command.Id)
            return Results.BadRequest("ID mismatch.");

        var result = await sender.Send(command);
        if (!result.Succeeded)
            return Results.BadRequest(result.Errors);
        return Results.NoContent();
    }

    [EndpointSummary("Cancel a journal entry")]
    public static async Task<IResult> CancelJournalEntry(
        [FromServices] ISender sender,
        int id,
        [FromBody] CancelJournalEntryCommand command)
    {
        if (id != command.Id)
            return Results.BadRequest("ID mismatch.");

        var result = await sender.Send(command);
        if (!result.Succeeded)
            return Results.BadRequest(result.Errors);
        return Results.NoContent();
    }

    [EndpointSummary("Submit a journal entry for approval")]
    public static async Task<IResult> SubmitJournalEntry(
        [FromServices] ISender sender,
        int id,
        [FromBody] SubmitJournalEntryCommand command)
    {
        if (id != command.Id)
            return Results.BadRequest("ID mismatch.");

        var result = await sender.Send(command);
        if (!result.Succeeded)
            return Results.BadRequest(result.Errors);
        return Results.NoContent();
    }

    [EndpointSummary("Approve a submitted journal entry")]
    public static async Task<IResult> ApproveJournalEntry(
        [FromServices] ISender sender,
        int id,
        [FromBody] ApproveJournalEntryCommand command)
    {
        if (id != command.Id)
            return Results.BadRequest("ID mismatch.");

        var result = await sender.Send(command);
        if (!result.Succeeded)
            return Results.BadRequest(result.Errors);
        return Results.NoContent();
    }

    [EndpointSummary("Post an approved journal entry")]
    public static async Task<IResult> PostJournalEntry(
        [FromServices] ISender sender,
        int id,
        [FromBody] PostJournalEntryCommand command)
    {
        if (id != command.Id)
            return Results.BadRequest("ID mismatch.");

        var result = await sender.Send(command);
        if (!result.Succeeded)
            return Results.BadRequest(result.Errors);
        return Results.NoContent();
    }

    [EndpointSummary("Reverse a posted journal entry")]
    public static async Task<IResult> ReverseJournalEntry(
        [FromServices] ISender sender,
        int id,
        [FromBody] ReverseJournalEntryCommand command)
    {
        if (id != command.Id)
            return Results.BadRequest("ID mismatch.");

        var result = await sender.Send(command);
        if (!result.Succeeded)
            return Results.BadRequest(result.Errors);
        return Results.Ok(new { reversalId = result.Value });
    }

    [EndpointSummary("Create a journal entry line")]
    public static async Task<IResult> CreateJournalEntryLine(
        [FromServices] ISender sender,
        int id,
        [FromBody] CreateJournalEntryLineCommand command)
    {
        if (id != command.JournalEntryId)
            return Results.BadRequest("Journal entry ID mismatch.");

        var result = await sender.Send(command);
        if (!result.Succeeded)
            return Results.BadRequest(result.Errors);
        return Results.Ok(new { lineId = result.Value });
    }

    [EndpointSummary("Update a journal entry line")]
    public static async Task<IResult> UpdateJournalEntryLine(
        [FromServices] ISender sender,
        int id,
        long lineId,
        [FromBody] UpdateJournalEntryLineCommand command)
    {
        if (id != command.JournalEntryId || lineId != command.Id)
            return Results.BadRequest("ID mismatch.");

        var result = await sender.Send(command);
        if (!result.Succeeded)
            return Results.BadRequest(result.Errors);
        return Results.NoContent();
    }

    [EndpointSummary("Remove a journal entry line")]
    public static async Task<IResult> RemoveJournalEntryLine(
        [FromServices] ISender sender,
        int id,
        long lineId)
    {
        var command = new RemoveJournalEntryLineCommand { Id = lineId, JournalEntryId = id };
        var result = await sender.Send(command);
        if (!result.Succeeded)
            return Results.BadRequest(result.Errors);
        return Results.NoContent();
    }

    [EndpointSummary("Export journal entries to Excel or PDF")]
    public static async Task<IResult> ExportJournalEntries(
        [FromServices] ISender sender,
        [FromServices] IApplicationDbContext dbContext,
        [FromQuery] string format,
        [FromQuery] int? journalId,
        [FromQuery] int? entryId,
        [FromQuery] string? status,
        [FromQuery] DateOnly? fromDate,
        [FromQuery] DateOnly? toDate,
        [FromQuery] string? pageSize,
        [FromQuery] bool? isLandscape)
    {
        var query = new GetJournalEntriesListQuery
        {
            JournalId = journalId,
            EntryStatus = status,
            FromDate = fromDate,
            ToDate = toDate
        };
        var entries = await sender.Send(query);

        if (entryId.HasValue)
            entries = entries.Where(e => e.Id == entryId.Value).ToList();

        var entryIds = entries.Select(e => e.Id).ToList();
        var lines = await dbContext.JournalEntryLines
            .Include(l => l.Account)
            .Where(l => entryIds.Contains(l.JournalEntryId))
            .ToListAsync();

        var linesByEntry = lines.GroupBy(l => l.JournalEntryId)
            .ToDictionary(g => g.Key, g => g.ToList());

        var sections = entries.Select(e => new Application.Accounting.Reports.Common.ReportSection
        {
            Title = $"قيد رقم {e.EntryNumber}",
            TitleEn = $"Entry {e.EntryNumber}",
            Description = e.Narration,
            Lines = linesByEntry.TryGetValue(e.Id, out var entryLines)
                ? entryLines.Select(l => new Application.Accounting.Reports.Common.ReportLine
                {
                    AccountCode = l.Account?.Code ?? "",
                    AccountName = l.Account?.Name ?? "",
                    Description = l.Description,
                    Debit = l.Debit,
                    Credit = l.Credit,
                }).ToList()
                : [],
        }).ToList();

        var reportResult = new Application.Accounting.Reports.Common.ReportResult
        {
            Currency = "YER",
            GeneratedAt = DateTimeOffset.UtcNow,
            Sections = sections,
            PaperSize = pageSize ?? "A5",
            IsLandscape = isLandscape ?? true,
        };

        var stream = new MemoryStream();
        if (format?.ToLower() == "pdf")
        {
            var exporter = new ERP_Government.Infrastructure.Services.PdfReportExporter();
            await exporter.ExportPdfAsync(reportResult, "Journal Entries", stream);
        }
        else
        {
            var exporter = new ERP_Government.Infrastructure.Services.ExcelReportExporter();
            await exporter.ExportExcelAsync(reportResult, "Journal Entries", stream);
        }
        stream.Position = 0;

        var contentType = format?.ToLower() == "pdf" ? "application/pdf" : "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
        var extension = format?.ToLower() == "pdf" ? "pdf" : "xlsx";
        return Results.File(stream, contentType, $"JournalEntries-{DateTime.Now:yyyyMMdd}.{extension}");
    }
}
