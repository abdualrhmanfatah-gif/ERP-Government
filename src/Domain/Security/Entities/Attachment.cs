using ERP_Government.Domain.Common;

namespace ERP_Government.Domain.Security.Entities;

public class Attachment : BaseEntity
{
    public string EntityName { get; set; } = string.Empty;
    public int DocumentId { get; set; }
    public string DocumentType { get; set; } = string.Empty;
    public string AttachmentTypeCode { get; set; } = string.Empty;
    public bool IsRequired { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string MimeType { get; set; } = string.Empty;
    public string StoragePath { get; set; } = string.Empty;
    public int SizeBytes { get; set; }
    public string? FileHash { get; set; }
    public int UploadedById { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public byte[] RowVersion { get; set; } = [];

    public User UploadedBy { get; set; } = null!;
}
