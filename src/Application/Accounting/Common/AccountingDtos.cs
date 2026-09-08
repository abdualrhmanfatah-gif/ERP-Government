using ERP_Government.Domain.Accounting.Entities;
using ERP_Government.Domain.Accounting.Enums;

namespace ERP_Government.Application.Accounting.Common;

// T001 — AccountDto
public class AccountDto
{
    public int Id { get; init; }
    public string Code { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public int AccountGroupId { get; init; }
    public string AccountGroupName { get; init; } = string.Empty;
    public int? ParentId { get; init; }
    public byte Level { get; init; }
    public string NormalBalance { get; init; } = string.Empty;
    public bool IsPostable { get; init; }
    public bool IsReconcilable { get; init; }
    public int? CurrencyId { get; init; }
    public bool IsActive { get; init; }
    public byte[] RowVersion { get; init; } = [];

    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<Account, AccountDto>()
                .ForMember(d => d.NormalBalance, opt => opt.MapFrom(s => s.NormalBalance.ToString()))
                .ForMember(d => d.AccountGroupName, opt => opt.MapFrom(s => s.AccountGroup.Name));
        }
    }
}

// T002 — AccountGroupDto (expanded for 005-account-groups: Level, ParentId, RowVersion, AncestorPath)
public class AccountGroupDto
{
    public int Id { get; init; }
    public string Code { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string Type { get; init; } = string.Empty;
    public string NormalBalance { get; init; } = string.Empty;
    public string? Description { get; init; }
    public int? ParentId { get; init; }
    public byte Level { get; init; }
    public bool IsActive { get; init; }
    public byte[] RowVersion { get; init; } = [];
    public List<AncestorRefDto>? AncestorPath { get; init; }

    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<AccountGroup, AccountGroupDto>()
                .ForMember(d => d.Type, opt => opt.MapFrom(s => s.Type.ToString()))
                .ForMember(d => d.NormalBalance, opt => opt.MapFrom(s => s.NormalBalance.ToString()));
        }
    }
}

public class AncestorRefDto
{
    public int Id { get; init; }
    public string Code { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
}

public class AccountGroupTreeItemDto : AccountGroupDto
{
    public List<AccountGroupTreeItemDto> Children { get; init; } = [];
}

public class PaginatedAccountGroupsResponse
{
    public string Mode { get; init; } = string.Empty; // tree | flat
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int TotalCount { get; init; }
    public int TotalPages { get; init; }
    public List<AccountGroupDto> Items { get; init; } = [];
}

public class LinkedAccountRefDto
{
    public int Id { get; init; }
    public string Code { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public bool IsActive { get; init; }
    public bool IsPostable { get; init; }
}

public class AccountGroupAuditEntryDto
{
    public long Id { get; init; }
    public string Action { get; init; } = string.Empty;
    public int? UserId { get; init; }
    public string? UserName { get; init; }
    public DateTimeOffset Timestamp { get; init; }
    public string? FieldChanges { get; init; }
    public string? ChangeSummary { get; init; }
    public string? IpAddress { get; init; }
}

public class AccountGroupDetailResponse
{
    public AccountGroupDto Group { get; init; } = null!;
    public List<AccountGroupDto> Children { get; init; } = [];
    public List<LinkedAccountRefDto> Accounts { get; init; } = [];
    public List<AccountGroupAuditEntryDto> Audit { get; init; } = [];
}

// T003 — JournalDto
public class JournalDto
{
    public int Id { get; init; }
    public string Code { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string Type { get; init; } = string.Empty;
    public int? AccountId { get; init; }
    public int? SuspenseAccountId { get; init; }
    public bool AllowForeignCurrency { get; init; }
    public int? SequenceId { get; init; }
    public bool RequireApprovalBeforePosting { get; init; }
    public bool IsActive { get; init; }

    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<Journal, JournalDto>()
                .ForMember(d => d.Type, opt => opt.MapFrom(s => s.Type.ToString()));
        }
    }
}

// T004 — JournalEntryDto
public class JournalEntryDto
{
    public int Id { get; init; }
    public string EntryNumber { get; init; } = string.Empty;
    public string? Ref { get; init; }
    public DateOnly DocumentDate { get; init; }
    public DateOnly? PostingDate { get; init; }
    public MoveEntryType? EntryType { get; init; }
    public string EntryStatus { get; init; } = string.Empty;
    public int? JournalId { get; init; }
    public string? JournalName { get; init; }
    public int PeriodId { get; init; }
    public int FiscalYearId { get; init; }
    public string? Narration { get; init; }
    public int? ReversalOfId { get; init; }
    public string? ReversalReason { get; init; }
    public int? PostedById { get; init; }
    public string? PostedByName { get; init; }
    public DateTimeOffset? PostedAt { get; init; }
    public int? CancelledById { get; init; }
    public string? CancelledByName { get; init; }
    public DateTimeOffset? CancelledAt { get; init; }
    public bool IsSystemGenerated { get; init; }
    public byte[] RowVersion { get; init; } = [];
    // Ephemeral base-currency conversion surface (AR-001, DEC-002) — NOT persisted columns
    public int? BaseCurrencyId { get; init; }
    public decimal? TotalBaseDebit { get; init; }
    public decimal? TotalBaseCredit { get; init; }
    public List<JournalEntryLineDto> Lines { get; set; } = [];

    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<JournalEntry, JournalEntryDto>()
                .ForMember(d => d.EntryType, opt => opt.MapFrom(s => s.EntryType))
                .ForMember(d => d.EntryStatus, opt => opt.MapFrom(s => s.EntryStatus.ToString()))
                .ForMember(d => d.JournalName, opt => opt.MapFrom(s => s.Journal != null ? s.Journal.Name : null))
                .ForMember(d => d.PostedByName, opt => opt.MapFrom(s => s.PostedBy != null ? s.PostedBy.Login : null))
                .ForMember(d => d.CancelledByName, opt => opt.MapFrom(s => s.CancelledBy != null ? s.CancelledBy.Login : null));
        }
    }
}

// T005 — JournalEntryLineDto
public class JournalEntryLineDto
{
    public long Id { get; init; }
    public int JournalEntryId { get; init; }
    public int Sequence { get; init; }
    public int AccountId { get; init; }
    public string AccountCode { get; init; } = string.Empty;
    public string AccountName { get; init; } = string.Empty;
    public string? Description { get; init; }
    public int CurrencyId { get; init; }
    public decimal ExchangeRate { get; init; }
    public decimal Debit { get; init; }
    public decimal Credit { get; init; }
    public int? CostCenterId { get; init; }
    public string? CostCenterName { get; init; }
    public byte[] RowVersion { get; init; } = [];
    // Ephemeral base-currency conversion surface (AR-001, DEC-002) — NOT persisted columns
    public decimal? BaseDebit { get; init; }
    public decimal? BaseCredit { get; init; }
    public decimal? ResolvedRate { get; init; }
    public DateOnly? ResolvedRateDate { get; init; }

    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<JournalEntryLine, JournalEntryLineDto>()
                .ForMember(d => d.AccountCode, opt => opt.MapFrom(s => s.Account.Code))
                .ForMember(d => d.AccountName, opt => opt.MapFrom(s => s.Account.Name))
                .ForMember(d => d.CostCenterName, opt => opt.MapFrom(s => s.CostCenter != null ? s.CostCenter.Name : null));
        }
    }
}

// T007 — PostingRuleDto
public class PostingRuleDto
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string EventType { get; init; } = string.Empty;
    public int JournalId { get; init; }
    public string JournalName { get; init; } = string.Empty;
    public int Priority { get; init; }
    public bool IsActive { get; init; }

    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<PostingRule, PostingRuleDto>()
                .ForMember(d => d.JournalName, opt => opt.MapFrom(s => s.Journal.Name));
        }
    }
}

// T076 — PostingRuleLineDto
public class PostingRuleLineDto
{
    public int Id { get; init; }
    public int PostingRuleId { get; init; }
    public int Sequence { get; init; }
    public string AccountSource { get; init; } = string.Empty;
    public int? FixedAccountId { get; init; }
    public string? FixedAccountCode { get; init; }
    public string DebitOrCredit { get; init; } = string.Empty;
    public string AmountSource { get; init; } = string.Empty;
    public bool FundDimensionRequired { get; init; }
    public bool CostCenterDimensionRequired { get; init; }
    public bool ProjectDimensionRequired { get; init; }
    public bool IsActive { get; init; }

    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<PostingRuleLine, PostingRuleLineDto>()
                .ForMember(d => d.FixedAccountCode, opt => opt.MapFrom(s => s.FixedAccount != null ? s.FixedAccount.Code : null))
                .ForMember(d => d.DebitOrCredit, opt => opt.MapFrom(s => s.DebitOrCredit.ToString()))
                .ForMember(d => d.AccountSource, opt => opt.MapFrom(s => s.AccountSource.ToString()))
                .ForMember(d => d.AmountSource, opt => opt.MapFrom(s => s.AmountSource.ToString()));
        }
    }
}

// T008 — JournalEntryTemplateDto (US4: lines + totals, computed never stored)
public class JournalEntryTemplateDto
{
    public int Id { get; init; }
    public string TemplateName { get; init; } = string.Empty;
    public string? Description { get; init; }
    public int JournalId { get; init; }
    public string JournalName { get; init; } = string.Empty;
    public string TemplateType { get; init; } = string.Empty;
    public bool IsSystemTemplate { get; init; }
    public bool IsActive { get; init; }
    public List<JournalEntryTemplateLineDto> Lines { get; set; } = [];
    public decimal TotalDebit { get; set; }
    public decimal TotalCredit { get; set; }
    public bool IsBalanced => TotalDebit == TotalCredit && Lines.Count > 0;

    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<JournalEntryTemplate, JournalEntryTemplateDto>()
                .ForMember(d => d.JournalName, opt => opt.MapFrom(s => s.Journal.Name))
                .ForMember(d => d.TemplateType, opt => opt.MapFrom(s => s.TemplateType.ToString()))
                .ForMember(d => d.Lines, opt => opt.Ignore())
                .ForMember(d => d.TotalDebit, opt => opt.Ignore())
                .ForMember(d => d.TotalCredit, opt => opt.Ignore());
        }
    }
}

// T077 — JournalEntryTemplateLineDto (mirror JournalEntryLineDto except FK)
public class JournalEntryTemplateLineDto
{
    public int Id { get; init; }
    public int TemplateId { get; init; }
    public int Sequence { get; init; }
    public int AccountId { get; init; }
    public string AccountCode { get; init; } = string.Empty;
    public string AccountName { get; init; } = string.Empty;
    public string? Description { get; init; }
    public int CurrencyId { get; init; }
    public decimal ExchangeRate { get; init; }
    public decimal Debit { get; init; }
    public decimal Credit { get; init; }
    public int? CostCenterId { get; init; }
    public string? CostCenterName { get; init; }
    public byte[] RowVersion { get; init; } = [];

    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<JournalEntryTemplateLine, JournalEntryTemplateLineDto>()
                .ForMember(d => d.AccountCode, opt => opt.MapFrom(s => s.Account.Code))
                .ForMember(d => d.AccountName, opt => opt.MapFrom(s => s.Account.Name))
                .ForMember(d => d.CostCenterName, opt => opt.MapFrom(s => s.CostCenter != null ? s.CostCenter.Name : null));
        }
    }
}

// T009 — RecurringEntryDto
public class RecurringEntryDto
{
    public int Id { get; init; }
    public string EntryNumber { get; init; } = string.Empty;
    public int? TemplateId { get; init; }
    public string? TemplateName { get; init; }
    public int JournalId { get; init; }
    public string JournalName { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string Frequency { get; init; } = string.Empty;
    public DateOnly StartDate { get; init; }
    public DateOnly? EndDate { get; init; }
    public DateOnly NextExecutionDate { get; init; }
    public DateTime? LastExecutedAt { get; init; }
    public decimal? Amount { get; init; }
    public int? CurrencyId { get; init; }
    public int? FundId { get; init; }
    public int? CostCenterId { get; init; }
    public int? ProjectId { get; init; }
    public string? DescriptionTemplate { get; init; }
    public string Status { get; init; } = string.Empty;
    public int? GeneratedJournalEntryId { get; init; }
    public bool IsActive { get; init; }

    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<RecurringEntry, RecurringEntryDto>()
                .ForMember(d => d.TemplateName, opt => opt.MapFrom(s => s.Template != null ? s.Template.TemplateName : null))
                .ForMember(d => d.JournalName, opt => opt.MapFrom(s => s.Journal.Name))
                .ForMember(d => d.Frequency, opt => opt.MapFrom(s => s.Frequency.ToString()))
                .ForMember(d => d.Status, opt => opt.MapFrom(s => s.Status.ToString()));
        }
    }
}
