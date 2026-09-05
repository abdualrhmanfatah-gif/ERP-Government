using ERP_Government.Domain.Budgeting.Entities;
using ERP_Government.Domain.Budgeting.Enums;

namespace ERP_Government.Infrastructure.Data.Seeds;

/// <summary>
/// Fund seed data — 2 funds (General Operating, Capital Development).
/// </summary>
public static class FundSeedData
{
    public static List<Fund> GetFunds() =>
    [
        new()
        {
            FundNumber = "F001",
            FundName = "General Operating Fund",
            FundType = FundType.General,
            FundCategory = FundCategory.Operating,
            LegalAuthority = "General Fund Act",
            IsActive = true
        },
        new()
        {
            FundNumber = "F002",
            FundName = "Capital Development Fund",
            FundType = FundType.Project,
            FundCategory = FundCategory.Capital,
            LegalAuthority = "Capital Investment Act",
            IsActive = true
        }
    ];
}
