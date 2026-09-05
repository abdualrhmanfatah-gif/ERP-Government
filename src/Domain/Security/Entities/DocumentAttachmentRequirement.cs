using ERP_Government.Domain.Common;

namespace ERP_Government.Domain.Security.Entities;

public class DocumentAttachmentRequirement : BaseAuditableEntity
{
    public string DocumentType { get; set; } = string.Empty;
    public string AttachmentTypeCode { get; set; } = string.Empty;
    public string TitleAr { get; set; } = string.Empty;
    public bool IsMandatory { get; set; }
    public bool IsActive { get; set; } = true;
    public byte[] RowVersion { get; set; } = [];
}
