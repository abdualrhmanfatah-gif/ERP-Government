using ERP_Government.Domain.Budgeting.Entities;
using ERP_Government.Domain.Budgeting.Enums;

namespace ERP_Government.Infrastructure.Data.Seeds;

/// <summary>
/// BudgetType seed data — 3 budget types (OPEX, CAPEX, TRANSFER).
/// </summary>
public static class BudgetTypeSeedData
{
    public static List<BudgetType> GetBudgetTypes() =>
    [
        new()
        {
            Code = "OPEX",
            Name = "Operational Expenditure",
            ControlMethod = BudgetControlMethod.Warning,
            AllowOverrun = false,
            IsActive = true
        },
        new()
        {
            Code = "CAPEX",
            Name = "Capital Expenditure",
            ControlMethod = BudgetControlMethod.Blocking,
            AllowOverrun = false,
            IsActive = true
        },
        new()
        {
            Code = "TRANSFER",
            Name = "Transfer Budget",
            ControlMethod = BudgetControlMethod.None,
            AllowOverrun = true,
            IsActive = true
        }
    ];
}
