using ERP_Government.Domain.Parties.Entities;
using ERP_Government.Domain.Parties.Enums;

namespace ERP_Government.Infrastructure.Data.Seeds;

/// <summary>
/// Party seed data — 3 parties (supplier, customer, government entity).
/// </summary>
public static class PartySeedData
{
    public static List<Party> GetParties() =>
    [
        new()
        {
            PartyCode = "PTY-000001",
            PartyType = PartyType.Supplier,
            NameAr = "شركة الأحمد للتجارة",
            NameEn = "Al-Ahmad Trading Co.",
            TaxNumber = "1001234567",
            Phone = "771234567",
            Email = "info@al-ahmad-trading.com",
            Address = "صنعاء، شارع الستين",
            IsActive = true,
        },
        new()
        {
            PartyCode = "PTY-000002",
            PartyType = PartyType.Customer,
            NameAr = "شركة النور للاستقدام",
            NameEn = "Al-Noor Recruitment Co.",
            TaxNumber = "1009876543",
            Phone = "779876543",
            Email = "contact@al-noor-recruitment.com",
            Address = "عدن، طريق كريتر",
            IsActive = true,
        },
        new()
        {
            PartyCode = "PTY-000003",
            PartyType = PartyType.GovEntity,
            NameAr = "وزارة المالية العامة",
            NameEn = "Ministry of Public Finance",
            TaxNumber = "3000000001",
            Phone = "1234567",
            Email = "info@mof.gov.ye",
            Address = "صنعاء",
            IsActive = true,
        },
    ];
}
