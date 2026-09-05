using ERP_Government.Domain.Budgeting.Entities;

namespace ERP_Government.Infrastructure.Data.Seeds;

/// <summary>
/// BudgetClassification seed data — 3-level hierarchy.
/// Note: ParentId references must be set after insert since IDs are database-generated.
/// Hierarchy: 1000 Revenue, 2000 Expenditure > 2100 Operational, 2200 Capital > 2110 Salaries.
/// </summary>
public static class BudgetClassificationSeedData
{
    public static List<BudgetClassification> GetClassifications() =>
    [
        // Level 1
        new() { Code = "1000", Name = "Revenue", IsActive = true },
        new() { Code = "2000", Name = "Expenditure", IsActive = true },

        // Level 2 (ParentId must be set after insert to reference Code "2000")
        new() { Code = "2100", Name = "Operational Expenses", IsActive = true },
        new() { Code = "2200", Name = "Capital Expenses", IsActive = true },

        // Level 3 (ParentId must be set after insert to reference Code "2100")
        new() { Code = "2110", Name = "Salaries", IsActive = true }
    ];
}
