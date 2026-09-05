using ERP_Government.Domain.Common;
using ERP_Government.Domain.Accounting.Enums;

namespace ERP_Government.Domain.Accounting.Entities;

/// <summary>
/// Reusable journal entry templates per schema table 43.
/// </summary>
public class JournalEntryTemplate : BaseAuditableEntity
{
    public string TemplateName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int JournalId { get; set; }
    public JournalEntryTemplateType TemplateType { get; set; }
    public bool IsSystemTemplate { get; set; }
    public bool IsActive { get; set; } = true;
    public byte[] RowVersion { get; set; } = [];

    public Journal Journal { get; set; } = null!;
}
