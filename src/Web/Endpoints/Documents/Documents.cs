using Microsoft.AspNetCore.Mvc;
using ERP_Government.Application.Common.Interfaces;
using ERP_Government.Application.Common.Models;
using ERP_Government.Application.Documents.Commands.DeleteAttachment;
using ERP_Government.Application.Documents.Commands.UploadAttachment;
using ERP_Government.Application.Documents.Common;
using ERP_Government.Application.Documents.Queries.GetAttachmentRequirements;
using ERP_Government.Application.Documents.Queries.GetDocumentApprovals;
using ERP_Government.Application.Documents.Queries.GetDocumentAttachments;
using ERP_Government.Application.Documents.Queries.GetDocumentStatusLog;
using ERP_Government.Application.Security.Common;
using ERP_Government.Web.Infrastructure;

namespace ERP_Government.Web.Endpoints.Documents;

public class Documents : IEndpointGroup
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapGet("/{documentType}/{documentId:int}/approvals", HandleGetApprovals)
            .RequireAuthorization();
        group.MapGet("/{documentType}/{documentId:int}/status-log", HandleGetStatusLog)
            .RequireAuthorization();
        group.MapGet("/{documentType}/{documentId:int}/attachments", HandleGetAttachments)
            .RequireAuthorization();
        group.MapGet("/{documentType}/attachment-requirements", HandleGetAttachmentRequirements)
            .RequireAuthorization();
        group.MapPost("/{documentType}/{documentId:int}/attachments", HandleUploadAttachment)
            .RequireAuthorization()
            .DisableAntiforgery();
        group.MapDelete("/attachments/{id:int}", HandleDeleteAttachment)
            .RequireAuthorization();
        group.MapGet("/{documentType}/{documentId:int}/attachment-gate-check", HandleAttachmentGateCheck)
            .RequireAuthorization();
        group.MapGet("/attachments/{id:int}/download", HandleDownloadAttachment)
            .RequireAuthorization();
    }

    private static async Task<IResult> HandleGetApprovals(
        ISender sender,
        string documentType,
        int documentId)
    {
        var result = await sender.Send(new GetDocumentApprovalsQuery(documentType, documentId));
        return Results.Ok(result);
    }

    private static async Task<IResult> HandleGetStatusLog(
        ISender sender,
        string documentType,
        int documentId)
    {
        var result = await sender.Send(new GetDocumentStatusLogQuery(documentType, documentId));
        return Results.Ok(result);
    }

    private static async Task<IResult> HandleGetAttachments(
        ISender sender,
        string documentType,
        int documentId)
    {
        var result = await sender.Send(new GetDocumentAttachmentsQuery(documentType, documentId));
        return Results.Ok(result);
    }

    private static async Task<IResult> HandleGetAttachmentRequirements(
        ISender sender,
        string documentType)
    {
        var result = await sender.Send(new GetAttachmentRequirementsQuery(documentType));
        return Results.Ok(result);
    }

    private static async Task<IResult> HandleUploadAttachment(
        ISender sender,
        string documentType,
        int documentId,
        IFormFile file,
        string? attachmentTypeCode = null)
    {
        await using var stream = file.OpenReadStream();
        var result = await sender.Send(new UploadAttachmentCommand(
            documentType, documentId, attachmentTypeCode ?? "GENERAL", file.FileName, stream));
        return result.Succeeded ? Results.Created($"/api/Documents/{documentType}/{documentId}/attachments", result.Value) : result.ToProblemDetails();
    }

    private static async Task<IResult> HandleDeleteAttachment(
        ISender sender,
        int id)
    {
        var result = await sender.Send(new DeleteAttachmentCommand(id));
        return result.Succeeded ? Results.Ok() : result.ToProblemDetails();
    }

    private static async Task<IResult> HandleAttachmentGateCheck(
        IAttachmentGateService gateService,
        string documentType,
        int documentId)
    {
        var missing = await gateService.CheckMandatoryAttachmentsAsync(documentType, documentId);
        return Results.Ok(missing);
    }

    private static async Task<IResult> HandleDownloadAttachment(
        IFileStorageService fileStorage,
        IApplicationDbContext dbContext,
        int id)
    {
        var attachment = await dbContext.Attachments.FindAsync(id);
        if (attachment is null) return Results.Problem(
                detail: "Attachment not found",
                statusCode: StatusCodes.Status404NotFound,
                title: "Not Found",
                type: "about:blank");

        var stream = await fileStorage.OpenReadAsync(attachment.StoragePath);
        if (stream is null) return Results.Problem(
                detail: "File not found",
                statusCode: StatusCodes.Status404NotFound,
                title: "Not Found",
                type: "about:blank");

        return Results.File(stream, attachment.MimeType, attachment.FileName);
    }
}
