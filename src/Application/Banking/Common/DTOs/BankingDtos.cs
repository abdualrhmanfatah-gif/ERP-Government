using ERP_Government.Domain.Banking.Entities;
using ERP_Government.Domain.Banking.Enums;

namespace ERP_Government.Application.Banking.Common.DTOs;

// T-B001 — BankStatementDto
public class BankStatementDto
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public int BankAccountId { get; init; }
    public string? BankAccountName { get; init; }
    public int JournalId { get; init; }
    public DateOnly StatementDate { get; init; }
    public decimal BalanceStart { get; init; }
    public decimal BalanceEnd { get; init; }
    public decimal BalanceEndComputed { get; init; }
    public string? ImportSource { get; init; }
    public string Status { get; init; } = string.Empty;

    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<BankStatement, BankStatementDto>()
                .ForMember(d => d.Status, opt => opt.MapFrom(s => s.Status.ToString()));
        }
    }
}

// T-B002 — BankStatementLineDto
public class BankStatementLineDto
{
    public long Id { get; init; }
    public int StatementId { get; init; }
    public int LineNumber { get; init; }
    public DateOnly TransactionDate { get; init; }
    public string? Description { get; init; }
    public decimal Debit { get; init; }
    public decimal Credit { get; init; }
    public decimal? Balance { get; init; }
    public string? Reference { get; init; }
    public bool IsReconciled { get; init; }
    public long? JournalEntryLineId { get; init; }

    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<BankStatementLine, BankStatementLineDto>();
        }
    }
}

// T-B003 — BankReconciliationDto
public class BankReconciliationDto
{
    public int Id { get; init; }
    public int BankAccountId { get; init; }
    public string? BankAccountName { get; init; }
    public int StatementId { get; init; }
    public DateOnly ReconciliationDate { get; init; }
    public decimal BookBalance { get; init; }
    public decimal StatementBalance { get; init; }
    public decimal AdjustedBalance { get; init; }
    public decimal? Difference { get; init; }
    public string Status { get; init; } = string.Empty;
    public int PreparedById { get; init; }
    public int? ApprovedById { get; init; }

    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<BankReconciliation, BankReconciliationDto>()
                .ForMember(d => d.Status, opt => opt.MapFrom(s => s.Status.ToString()));
        }
    }
}

// T-B004 — BankReconciliationLineDto
public class BankReconciliationLineDto
{
    public int Id { get; init; }
    public int ReconciliationId { get; init; }
    public string LineType { get; init; } = string.Empty;
    public int? BankStatementLineId { get; init; }
    public long? JournalEntryLineId { get; init; }
    public decimal Amount { get; init; }
    public string? Description { get; init; }
    public string Status { get; init; } = string.Empty;

    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<BankReconciliationLine, BankReconciliationLineDto>()
                .ForMember(d => d.LineType, opt => opt.MapFrom(s => s.LineType.ToString()))
                .ForMember(d => d.Status, opt => opt.MapFrom(s => s.Status.ToString()));
        }
    }
}
