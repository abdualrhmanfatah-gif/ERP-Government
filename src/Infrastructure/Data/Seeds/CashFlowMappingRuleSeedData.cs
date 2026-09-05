using ERP_Government.Domain.Accounting.Entities;
using ERP_Government.Domain.Accounting.Enums;

namespace ERP_Government.Infrastructure.Data.Seeds;

public static class CashFlowMappingRuleSeedData
{
    public static List<CashFlowMappingRule> GetRules()
    {
        return
        [
            // Operating — Revenue and Expense root groups
            new() { AccountGroupId = 3, Section = CashFlowSectionType.Operating, Description = "Expense accounts (root group 3)", IsActive = true },
            new() { AccountGroupId = 4, Section = CashFlowSectionType.Operating, Description = "Revenue accounts (root group 4)", IsActive = true },

            // Investing — Asset root group (fixed assets)
            new() { AccountGroupId = 1, Section = CashFlowSectionType.Investing, Description = "Asset accounts — fixed asset acquisitions/disposals", IsActive = true },

            // Financing — Equity root group (equity, long-term debt)
            new() { AccountGroupId = 2, Section = CashFlowSectionType.Financing, Description = "Equity accounts — equity, long-term liabilities", IsActive = true },
        ];
    }
}
