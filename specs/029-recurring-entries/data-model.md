# Data Model: Recurring Entries (ACC-04)

**Date**: 2026-09-07 | **Spec**: [spec.md](./spec.md)

## Entity: RecurringEntry

**Table**: `RecurringEntries` | **Schema**: `Accounting` | **Inherits**: `BaseAuditableEntity`

| Field | Type | Nullable | Constraints | Notes |
|-------|------|----------|-------------|-------|
| Id | int | No | PK, identity | From BaseEntity |
| EntryNumber | string(50) | No | Unique index | DocumentSequenceService: `REC-{D6}` |
| TemplateId | int? | Yes | FK → JournalEntryTemplate (Restrict) | Optional line template source |
| JournalId | int | No | FK → Journal (Restrict) | Target journal definition |
| Name | string(200) | No | Required | Human-readable schedule name |
| Frequency | RecurringFrequency | No | Enum (int) | Weekly=0, Monthly=1, Quarterly=2, Yearly=3 |
| StartDate | DateOnly | No | Required | First execution date |
| EndDate | DateOnly? | Yes | ≥ StartDate (validation) | Null = no end; auto-complete when NextExecution > EndDate |
| NextExecutionDate | DateOnly | No | Computed from StartDate + Frequency | Updated after each execution |
| LastExecutedAt | DateTime? | Yes | UTC timestamp | Set on successful generation |
| Amount | decimal(23,2) | Yes | Required if TemplateId null | Schedule-level amount override |
| CurrencyId | int? | Yes | FK → Currency (Restrict) | Deferred FK to Module 1 |
| FundId | int? | Yes | FK → Fund (Restrict) | Deferred FK to Module 5 |
| CostCenterId | int? | Yes | FK → CostCenter (Restrict) | Cross-module FK |
| ProjectId | int? | Yes | FK → Project (Restrict) | Cross-module FK |
| DescriptionTemplate | string(500) | Yes | | Narration template for generated entries |
| Status | RecurringEntryStatus | No | Enum (int), default Active | Active=0, Paused=1, Completed=2, Cancelled=3 |
| GeneratedJournalEntryId | int? | Yes | FK → JournalEntry (Restrict) | Last generated entry |
| IsActive | bool | No | Default true | Soft delete flag |
| RowVersion | byte[] | No | Optimistic concurrency | Auto-generated |

**Indexes**:
- Unique: `EntryNumber`
- Non-unique: `TemplateId`, `JournalId`, `CurrencyId`, `FundId`, `CostCenterId`, `ProjectId`, `GeneratedJournalEntryId`

**Relationships**:
- `Template` → JournalEntryTemplate (optional, Restrict)
- `Journal` → Journal (required, Restrict)
- `GeneratedJournalEntry` → JournalEntry (optional, Restrict)
- `CostCenter` → CostCenter (optional, Restrict)
- `Project` → Project (optional, Restrict)
- `ExecutionLogs` → RecurringEntryExecutionLog (one-to-many, Restrict)

## Entity: RecurringEntryExecutionLog

**Table**: `RecurringEntryExecutionLogs` | **Schema**: `Accounting` | **Inherits**: `BaseAuditableEntity`

| Field | Type | Nullable | Constraints | Notes |
|-------|------|----------|-------------|-------|
| Id | int | No | PK, identity | From BaseEntity |
| RecurringEntryId | int | No | FK → RecurringEntry (Restrict) | Parent schedule |
| ExecutionDate | DateOnly | No | | Date this execution was for |
| GeneratedJournalEntryId | int? | Yes | FK → JournalEntry (Restrict) | Created entry (null on failure) |
| Status | RecurringEntryExecutionStatus | No | Enum (int) | Created=0, Success=1, Failed=2 |
| StartedAt | DateTimeOffset | No | | Execution start |
| CompletedAt | DateTimeOffset? | Yes | | Execution end |
| ErrorMessage | string? | Yes | | Failure details |
| TriggeredBy | string | No | Default "Scheduler" | "Scheduler" or "Manual" |
| RowVersion | byte[] | No | Optimistic concurrency | |

## Enum: RecurringEntryStatus

| Value | Name | Meaning |
|-------|------|---------|
| 0 | Active | Schedule is running, will generate entries |
| 1 | Paused | Schedule suspended, no generation |
| 2 | Completed | Schedule reached EndDate naturally |
| 3 | Cancelled | Schedule permanently stopped by user |

**State Machine**:
```
Active → Paused (via Pause)
Active → Cancelled (via Cancel)
Active → Completed (auto on EndDate)
Paused → Active (via Resume)
Paused → Cancelled (via Cancel)
Completed → (terminal)
Cancelled → (terminal)
```

## Enum: RecurringFrequency

| Value | Name | NextExecution Calculation |
|-------|------|--------------------------|
| 0 | Weekly | Current + 7 days |
| 1 | Monthly | Current + 1 month |
| 2 | Quarterly | Current + 3 months |
| 3 | Yearly | Current + 1 year |

## Enum: RecurringEntryExecutionStatus

| Value | Name |
|-------|------|
| 0 | Created |
| 1 | Success |
| 2 | Failed |

## Validation Rules (from spec)

| Rule | Field | Constraint | Spec Ref |
|------|-------|------------|----------|
| VR-001 | JournalId | Required, > 0 | FR-011 |
| VR-002 | Name | Required, max 200 chars | FR-001 |
| VR-003 | Frequency | Valid enum value | FR-006 |
| VR-004 | StartDate | Required | FR-006 |
| VR-005 | EndDate | ≥ StartDate when present | FR-009 |
| VR-006 | Amount | Required when TemplateId is null | FR-017 |
| VR-007 | RowVersion | Required (for lifecycle ops) | Concurrency |
| VR-008 | Pause status | Only Active | FR-013 |
| VR-009 | Resume status | Only Paused | FR-013 |
| VR-010 | Cancel status | Active or Paused (not Completed, not Cancelled) | FR-013 |
