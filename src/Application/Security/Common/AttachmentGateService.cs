using ERP_Government.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ERP_Government.Application.Security.Common;

public class AttachmentGateService(
    IApplicationDbContext context) : IAttachmentGateService
{
    public async Task<IReadOnlyList<string>> CheckMandatoryAttachmentsAsync(
        string documentType,
        int documentId,
        CancellationToken ct)
    {
        var requirements = await context.DocumentAttachmentRequirements
            .Where(r => r.DocumentType == documentType && r.IsMandatory && r.IsActive)
            .ToListAsync(ct);

        if (requirements.Count == 0)
            return [];

        var attachedTypeCodes = await context.Attachments
            .Where(a => a.DocumentType == documentType && a.DocumentId == documentId)
            .Select(a => a.AttachmentTypeCode)
            .Distinct()
            .ToListAsync(ct);

        var missing = requirements
            .Where(r => !attachedTypeCodes.Contains(r.AttachmentTypeCode))
            .Select(r => r.AttachmentTypeCode)
            .ToList();

        return missing;
    }
}
