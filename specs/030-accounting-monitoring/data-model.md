# Data Model: ACC-05 — مراقبة المحاسبة (Accounting Monitoring)

## Entities

### AccountBalance (existing — no schema change)

**Table**: `AccountBalances`
**Inherits**: `BaseAuditableEntity` (int PK + audit + RowVersion)

| Field | Type | Notes |
|-------|------|-------|
| AccountId | int FK → Accounts | Required |
| FiscalYearId | int FK → FiscalYears | Required |
| FiscalPeriodId | int FK → FiscalPeriods | Required |
| CurrencyId | int FK → Currencies | Required |
| OpeningDebit | decimal(23,2) | Materialized at period open |
| OpeningCredit | decimal(23,2) | Materialized at period open |
| Debit | decimal(23,2) | Running total of debit postings |
| Credit | decimal(23,2) | Running total of credit postings |
| ClosingDebit | decimal(23,2) | Computed: Opening + Debit |
| ClosingCredit | decimal(23,2) | Computed: Opening + Credit |
| IsFinalized | bool | Period close flag |
| FinalizedAt | DateTimeOffset? | Timestamp of last close |

**Unique constraint**: `(AccountId, FiscalYearId, FiscalPeriodId, CurrencyId)` — one balance per account/period/currency

**State transitions**:
```
Open (IsFinalized=false) → Finalized (IsFinalized=true, FinalizedAt set)
Finalized → Open (IsFinalized=false, FinalizedAt cleared)
Guard: Cannot finalize if AccountingEvents with Status=Pending exist in period
```

### AccountingEvent (existing — extend EventStatus)

**Table**: `AccountingEvents`
**Inherits**: `BaseAuditableEntity`

| Field | Type | Notes |
|-------|------|-------|
| EventType | string | Maps to EventType enum name |
| SourceDocumentType | string | Entity name (e.g., "PaymentOrder") |
| SourceDocumentId | int | FK to source entity |
| Status | EventStatus enum | Pending=0, Processing=1, Posted=2, Reversed=3, Failed=4 |
| JournalEntryId | int? FK → JournalEntries | Set after successful posting |
| EventCategory | EventCategory enum | Revenue/Expenditure/Transfer/Adjustment/Other |
| ErrorMessage | string? | Populated on failure |
| ProcessedAt | DateTimeOffset? | Timestamp of completion |
| RetryCount | int | Incremented on retry |

**State transitions**:
```
Pending → Processing → Posted (success)
Pending → Processing → Failed (error, RetryCount++)
Failed → Processing → Posted (retry success)
Failed → Processing → Failed (retry exhausted)
Posted → Reversed (reversal)
```

### PostingRule (existing — no schema change)

**Table**: `PostingRules`
**Inherits**: `BaseAuditableEntity`

| Field | Type | Notes |
|-------|------|-------|
| Name | string | Required, unique display name |
| EventType | string | Required, maps to EventType enum |
| JournalId | int FK → Journals | Required, target journal |
| Priority | int | Execution order (highest first) |
| IsActive | bool | Soft-disable flag |

**Relationships**: 1:Many → PostingRuleLines

### PostingRuleLine (existing — no schema change)

**Table**: `PostingRuleLines`
**Inherits**: `BaseAuditableEntity`

| Field | Type | Notes |
|-------|------|-------|
| PostingRuleId | int FK → PostingRules | Required |
| Sequence | int | Line order within rule |
| AccountSource | AccountSource enum | FixedAccount=0, FromEventDimension=1 |
| FixedAccountId | int? FK → Accounts | Required when AccountSource=FixedAccount |
| DebitOrCredit | DebitOrCredit enum | Debit=0, Credit=1 |
| AmountSource | AmountSource enum | FixedAmount=2, EventAmount=3 |
| FundDimensionRequired | bool | Fund dimension required from event |
| CostCenterDimensionRequired | bool | Cost center dimension required |
| ProjectDimensionRequired | bool | Project dimension required |

**Validation**: If AccountSource=FixedAccount then FixedAccountId must not be null

## Enums (new/extended)

### EventStatus (extended)

| Value | Name | Notes |
|-------|------|-------|
| 0 | Pending | Event queued, not yet processed |
| 1 | Processing | Event being processed (NEW) |
| 2 | Posted | Event successfully posted (was 1) |
| 3 | Reversed | Event reversed (was 2) |
| 4 | Failed | Event failed permanently (NEW) |

### AccountSource (defined)

| Value | Name | Description |
|-------|------|-------------|
| 0 | FixedAccount | Use FixedAccountId from rule line |
| 1 | FromEventDimension | Derive from event source entity |

### AmountSource (defined)

| Value | Name | Description |
|-------|------|-------------|
| 2 | FixedAmount | Use fixed amount |
| 3 | EventAmount | Use transaction amount from source |

## New DTOs

### AccountBalanceDto

Returned by GET /api/Accounting/AccountingBalances. Six financial columns + metadata. Server-computed only.

### ReconciliationResultDto

Returned by GET /api/Accounting/AccountingBalances/reconcile. Contains isBalanced, totalAccountsChecked, discrepancyCount, and list of ReconciliationDiscrepancyDto.

### PostingRuleDto / PostingRuleLineDto

Returned by CRUD endpoints. Includes nested lines in detail view.

## Relationships

```
AccountBalance ─── Account (FK)
AccountBalance ─── FiscalYear (FK)
AccountBalance ─── FiscalPeriod (FK)
AccountBalance ─── Currency (FK)

AccountingEvent ─── JournalEntry (FK, nullable)
AccountingEvent ←→ Source entity (by SourceDocumentType + SourceDocumentId)

PostingRule ─── Journal (FK)
PostingRule ─── 1:N PostingRuleLine
PostingRuleLine ─── Account (FK, nullable for FixedAccount)
```
