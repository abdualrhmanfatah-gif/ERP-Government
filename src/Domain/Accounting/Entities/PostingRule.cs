using ERP_Government.Domain.Common;

namespace ERP_Government.Domain.Accounting.Entities;

/// <summary>
/// Posting rules for event types per schema table 41.
/// </summary>
public class PostingRule : BaseAuditableEntity
{
    public string Name { get; set; } = string.Empty;
    public string EventType { get; set; } = string.Empty;
    public int JournalId { get; set; }
    public int Priority { get; set; } = 100;
    public bool IsActive { get; set; } = true;
    public byte[] RowVersion { get; set; } = [];

    public Journal Journal { get; set; } = null!;
    public ICollection<PostingRuleLine> Lines { get; set; } = [];
}
