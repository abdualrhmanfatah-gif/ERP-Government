using ERP_Government.Domain.Common;
using ERP_Government.Domain.Parties.Enums;

namespace ERP_Government.Domain.Parties.Entities;

public class Party : BaseAuditableEntity
{
    public string PartyCode { get; set; } = string.Empty;
    public PartyType PartyType { get; set; }
    public string NameAr { get; set; } = string.Empty;
    public string? NameEn { get; set; }
    public string? TaxNumber { get; set; }
    public string? NationalId { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public string? Notes { get; set; }
    public bool IsActive { get; set; } = true;
    public byte[] RowVersion { get; set; } = [];
}
