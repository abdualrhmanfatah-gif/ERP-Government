using ERP_Government.Domain.Accounting.Entities;

namespace ERP_Government.Application.Accounting.Common;

// AccountBalanceDto — FR-016
public class AccountBalanceDto
{
    public int Id { get; init; }
    public int AccountId { get; init; }
    public string AccountCode { get; init; } = string.Empty;
    public string AccountName { get; init; } = string.Empty;
    public int FiscalYearId { get; init; }
    public string FiscalYearName { get; init; } = string.Empty;
    public int FiscalPeriodId { get; init; }
    public string PeriodName { get; init; } = string.Empty;
    public DateOnly PeriodStartDate { get; init; }
    public DateOnly PeriodEndDate { get; init; }
    public int CurrencyId { get; init; }
    public string CurrencyCode { get; init; } = string.Empty;
    public decimal OpeningDebit { get; init; }
    public decimal OpeningCredit { get; init; }
    public decimal Debit { get; init; }
    public decimal Credit { get; init; }
    public decimal ClosingDebit { get; init; }
    public decimal ClosingCredit { get; init; }
    public string BalanceDirection { get; init; } = "Zero"; // "Debit" | "Credit" | "Zero"
    public bool IsFinalized { get; init; }
    public DateTimeOffset? FinalizedAt { get; init; }

    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<AccountBalance, AccountBalanceDto>()
                .ForMember(d => d.AccountCode, opt => opt.MapFrom(s => s.Account.Code))
                .ForMember(d => d.AccountName, opt => opt.MapFrom(s => s.Account.Name))
                .ForMember(d => d.FiscalYearName, opt => opt.MapFrom(s => s.FiscalYear.Name))
                .ForMember(d => d.PeriodName, opt => opt.MapFrom(s => s.FiscalPeriod.Name))
                .ForMember(d => d.PeriodStartDate, opt => opt.MapFrom(s => s.FiscalPeriod.StartDate))
                .ForMember(d => d.PeriodEndDate, opt => opt.MapFrom(s => s.FiscalPeriod.EndDate))
                .ForMember(d => d.CurrencyCode, opt => opt.MapFrom(s => s.Currency.Code))
                .ForMember(d => d.BalanceDirection, opt => opt.MapFrom(s =>
                    s.ClosingDebit > s.ClosingCredit ? "Debit" :
                    s.ClosingCredit > s.ClosingDebit ? "Credit" : "Zero"));
        }
    }
}

// ReconciliationResultDto — FR-013, FR-014
public class ReconciliationResultDto
{
    public int FiscalYearId { get; init; }
    public int FiscalPeriodId { get; init; }
    public List<ReconciliationDiscrepancyDto> Discrepancies { get; init; } = [];
    public bool IsBalanced { get; init; }
    public int TotalAccountsChecked { get; init; }
    public int DiscrepancyCount { get; init; }
}

public class ReconciliationDiscrepancyDto
{
    public int AccountId { get; init; }
    public string AccountCode { get; init; } = string.Empty;
    public string AccountName { get; init; } = string.Empty;
    public int CurrencyId { get; init; }
    public string CurrencyCode { get; init; } = string.Empty;
    public decimal MaterializedDebit { get; init; }
    public decimal CalculatedDebit { get; init; }
    public decimal MaterializedCredit { get; init; }
    public decimal CalculatedCredit { get; init; }
    public decimal DebitDifference { get; init; }
    public decimal CreditDifference { get; init; }
    public string DiscrepancyType { get; init; } = string.Empty; // "MissingRecord" | "AmountMismatch" | "OrphanRecord"
}
