namespace ERP_Government.Application.Security.Common;

public interface IAttachmentGateService
{
    Task<IReadOnlyList<string>> CheckMandatoryAttachmentsAsync(
        string documentType,
        int documentId,
        CancellationToken ct = default);
}
