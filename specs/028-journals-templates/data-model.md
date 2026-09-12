# Data Model: ACC-03 — Journals & Templates

**Source**: Domain entities from `src/Domain/Accounting/Entities/` + Data contract from `web-api-client.ts`

## Entities

### Journal

**Domain class**: `src/Domain/Accounting/Entities/Journal.cs`
**Inherits**: `BaseAuditableEntity` (int PK, audit fields, RowVersion)

| Field | C# Type | DB | Contract (Dto) | Notes |
|-------|---------|-----|-----------------|-------|
| Id | int | PK | id | Auto |
| Code | string | required, unique | code | Max 20 chars |
| Name | string | required | name | Max 200 chars |
| Type | JournalType (enum) | required | type | General=0..Closing=6 |
| AccountId | int? | FK → Account | accountId | Default account |
| SuspenseAccountId | int? | FK → Account | suspenseAccountId | Suspense account |
| AllowForeignCurrency | bool | required | allowForeignCurrency | Default false |
| SequenceId | int? | FK → DocumentSequence | sequenceId | Deferred to Module 1 |
| RequireApprovalBeforePosting | bool | required | requireApprovalBeforePosting | Default false |
| IsActive | bool | required | isActive | Default true |
| RowVersion | byte[] | optimistic concurrency | — | Not in Dto |

**Relationships**:
- Account? → Account (default account)
- Account? → Account (suspense account)
- JournalEntry[] → Journal (1:N, entries classified by this journal)
- JournalEntryTemplate[] → Journal (1:N, templates linked to this journal)

### JournalEntryTemplate

**Domain class**: `src/Domain/Accounting/Entities/JournalEntryTemplate.cs`
**Inherits**: `BaseAuditableEntity` (int PK, audit fields, RowVersion)

| Field | C# Type | DB | Contract (Dto) | Notes |
|-------|---------|-----|-----------------|-------|
| Id | int | PK | id | Auto |
| TemplateName | string | required | templateName | Max 200 chars |
| Description | string? | nullable | description | Optional |
| JournalId | int | FK → Journal, required | journalId | |
| TemplateType | JournalEntryTemplateType (enum) | required | templateType | Standard=0, Recurring=1, Adjustment=2 |
| IsSystemTemplate | bool | required | isSystemTemplate | Server-only, not in UI create form |
| IsActive | bool | required | isActive | Default true |
| RowVersion | byte[] | optimistic concurrency | — | Not in Dto |

**Read-only from Dto**: `journalName` (resolved from JournalId join), `lines` (ordered by Sequence), `totalDebit/totalCredit/balanced` (computed, never stored)

**Relationships**:
- Journal → Journal (N:1, required)
- JournalEntryTemplateLine[] → JournalEntryTemplate (1:N, required — in scope US4)

### JournalEntryTemplateLine

**Domain class**: `src/Domain/Accounting/Entities/JournalEntryTemplateLine.cs`
**Inherits**: `BaseAuditableEntity` (int PK, audit fields, RowVersion)
**Table**: `JournalEntryTemplateLines` — strict mirror of `JournalEntryLine` except FK

| Field | C# Type | DB | Contract (Dto) | Notes |
|-------|---------|-----|-----------------|-------|
| Id | int | PK | id | Auto |
| TemplateId | int | FK → JournalEntryTemplate, required, indexed | templateId | Parent, Restrict delete |
| Sequence | int | required | sequence | Auto max+1 per template |
| AccountId | int | FK → Account, required, indexed | accountId | Must be postable + active |
| AccountCode/AccountName | — | join | accountCode/accountName | Read-only |
| Description | string? | nullable, max 200 | description | Line narration |
| CurrencyId | int | FK → Currency, required, indexed | currencyId | |
| ExchangeRate | decimal | decimal(18,6), default 1 | exchangeRate | > 0 |
| Debit | decimal | decimal(23,2) | debit | XOR with credit, >= 0 |
| Credit | decimal | decimal(23,2) | credit | XOR with debit, >= 0 |
| CostCenterId | int? | FK → CostCenter, nullable, indexed | costCenterId | Optional |
| RowVersion | byte[] | optimistic concurrency | — | Not in Dto |

**Relationships**:
- Template → JournalEntryTemplate (N:1)
- Account → Account (N:1, postable only)
- CostCenter? → CostCenter (N:1, nullable)

**Invariants** (enforced handler + validator, mirrored from `CreateJournalEntryLineCommand`):
- XOR: exactly one of debit / credit > 0; both > 0 reject; both == 0 reject; negative reject.
- Account must exist + IsPostable + IsActive.
- CurrencyId > 0, ExchangeRate > 0.
- Sequence server-assigned, never client-supplied.
- Totals computed in queries/handlers — NO stored computed columns.

## Enums

### JournalType

```csharp
public enum JournalType
{
    General = 0,
    Purchase = 1,
    Sale = 2,
    Cash = 3,
    Bank = 4,
    Adjustment = 5,
    Closing = 6
}
```

### JournalEntryTemplateType

```csharp
public enum JournalEntryTemplateType
{
    Standard = 0,
    Recurring = 1,
    Adjustment = 2
}
```

## Validation Rules (from existing validators)

### CreateJournalCommand
- Code: required, max 20 chars
- Name: required, max 200 chars
- Type: valid enum value
- AccountId: if set, must reference existing Account
- SuspenseAccountId: if set, must reference existing Account

### UpdateJournalCommand
- Id: > 0
- Name: required, max 200 chars
- Type: valid enum value
- RowVersion: required (concurrency)
- AccountId/SuspenseAccountId: if set, must reference existing Account
- **Used-journal lock**: Code, Name, Type, AccountId, SuspenseAccountId, AllowForeignCurrency, SequenceId blocked when journal has entries. IsActive and RequireApprovalBeforePosting remain editable.

### CreateTemplateCommand
- TemplateName: required, max 200 chars
- JournalId: > 0, must reference existing Journal
- TemplateType: valid enum value

### UpdateTemplateCommand
- Id: > 0
- TemplateName: required, max 200 chars
- JournalId: > 0
- TemplateType: valid enum value
- RowVersion: required

### CreateTemplateLineCommand (new, mirror CreateJournalEntryLineCommand)
- TemplateId: > 0, must reference existing template
- AccountId: > 0, postable + active only
- CurrencyId: > 0
- ExchangeRate: > 0
- Debit/Credit: >= 0 + XOR rule in handler

### UpdateTemplateLineCommand (new)
- Id: > 0 + RowVersion required
- Same field rules as create; sequence immutable server-side
