namespace ERP_Government.Domain.Budgeting.Enums;

public enum BudgetTransactionType
{
    InitialAppropriation = 0,
    Supplement = 1,
    Reduction = 2,

    CarryForward = 4,
    Adjustment = 5,
    Lapse = 6,
    Reversal = 7
}
