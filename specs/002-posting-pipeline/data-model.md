# Data Model: Posting Pipeline

**Date**: 2026-09-02
**Spec**: SPEC-002

## Entities

### AccountingEvent (Existing)

| Property | Type | Nullable | Constraints |
|----------|------|----------|------------|
| Id | int | No | PK |
| EventType | string(50) | No | Required, INDEX |
| SourceTable | string(50) | No | Required |
| SourceId | int | No | Required |
| Status | enum | No | Pending/Processing/Completed/Failed |
| MoveId | int? | Yes | FK → Move (Restrict) |
| ErrorMessage | string(1000)? | Yes | — |
| ProcessedAt | DateTimeOffset? | Yes | — |
| RetryCount | int | No | Default 0 |
| RowVersion | byte[] | No | ROW VERSION |
| Created | DateTimeOffset | No | Audit |
| CreatedBy | string? | Yes | Audit |
| LastModified | DateTimeOffset? | Yes | Audit |
| LastModifiedBy | string? | Yes | Audit |

**Unique Index**: `(SourceTable, SourceId, EventType)` — one event per source entity per event type

**Relationships**:
- AccountingEvent (N) → (1) Move [Restrict delete]

### PostingRule (Existing)

| Property | Type | Nullable | Constraints |
|----------|------|----------|------------|
| Id | int | No | PK |
| Name | string(100) | No | Required |
| EventType | string(50) | No | Required, INDEX |
| JournalId | int | No | FK → Journal (Restrict) |
| Priority | int | No | Default 100 |
| IsActive | bool | No | Default true |
| RowVersion | byte[] | No | ROW VERSION |
| Created | DateTimeOffset | No | Audit |
| CreatedBy | string? | Yes | Audit |
| LastModified | DateTimeOffset? | Yes | Audit |
| LastModifiedBy | string? | Yes | Audit |

**Relationships**:
- PostingRule (1) → (N) PostingRuleLine [Restrict delete]
- PostingRule (1) → (0..1) Journal [Restrict delete]

### PostingRuleLine (Existing)

| Property | Type | Nullable | Constraints |
|----------|------|----------|------------|
| Id | int | No | PK |
| PostingRuleId | int | No | FK → PostingRule (Restrict) |
| Sequence | int | No | — |
| AccountSource | string(20) | No | NEEDS_BUSINESS_CONFIRMATION |
| FixedAccountId | int? | Yes | FK → Account (Restrict) |
| DebitOrCredit | enum | No | Debit/Credit |
| AmountSource | string(30) | No | NEEDS_BUSINESS_CONFIRMATION |
| FundDimensionRequired | bool | No | Default false |
| CostCenterDimensionRequired | bool | No | Default false |
| ProjectDimensionRequired | bool | No | Default false |
| IsActive | bool | No | Default true |
| RowVersion | byte[] | No | ROW VERSION |

**Relationships**:
- PostingRuleLine (1) → (1) PostingRule [Restrict delete]
- PostingRuleLine (1) → (0..1) Account [Restrict delete]

### OutboxMessage (Existing)

| Property | Type | Nullable | Constraints |
|----------|------|----------|------------|
| Id | int | No | PK |
| TypeName | string(500) | No | Required |
| Payload | string | No | JSON |
| AggregateId | string(100)? | Yes | — |
| CorrelationId | Guid | No | Unique per message |
| Status | enum | No | Pending/Processing/Processed/Failed |
| CreatedAt | DateTimeOffset | No | — |
| ProcessedAt | DateTimeOffset? | Yes | — |
| RetryCount | int | No | Default 0 |
| NextRetryAt | DateTimeOffset? | Yes | — |
| ErrorMessage | string? | Yes | — |

**Relationships**: None (standalone)

## New Commands

### DeletePostingRuleCommand

**Input**:
- Id (int, required)
- RowVersion (byte[], required)

**Validation**:
- Id > 0
- PostingRule exists
- No Pending/Processing AccountingEvents reference this rule's EventType

**State Transition**: Hard delete (Constitution Principle VI: Restrict FKs)

**Side Effects**: None

**Authorization**: PostingRulesDelete

## State Transitions

### AccountingEvent

```
┌─────────┐   DomainEventHandler   ┌───────────┐
│ (new)    │──────────────────────▶│  Pending   │
└─────────┘                        └───────────┘
                                      │
                      OutboxProcessorService polls
                                      │
                                      ▼
                                ┌───────────┐
                                │ Processing │
                                └───────────┘
                                      │
                    ┌─────────────────┼─────────────────┐
                    │                 │                 │
                    ▼                 ▼                 ▼
              ┌───────────┐    ┌───────────┐    ┌───────────┐
              │ Completed  │    │  Failed    │    │  Failed    │
              └───────────┘    │ (retry<3)  │    │ (retry≥3)  │
                               └───────────┘    └───────────┘
                                      │                 │
                          Auto retry   │    Manual retry │
                                      │                 │
                                      ▼                 ▼
                                ┌───────────┐    ┌───────────┐
                                │  Pending   │    │  Pending   │
                                └───────────┘    └───────────┘
```

### OutboxMessage

```
┌─────────┐   DispatchDomainEvents   ┌───────────┐
│ (new)    │────────────────────────▶│  Pending   │
└─────────┘                          └───────────┘
                                        │
                        OutboxProcessorService polls
                                        │
                                        ▼
                                  ┌───────────┐
                                  │ Processing │
                                  └───────────┘
                                        │
                          ┌─────────────┼─────────────┐
                          │             │             │
                          ▼             ▼             ▼
                    ┌───────────┐ ┌───────────┐ ┌───────────┐
                    │ Processed │ │  Failed    │ │  Failed    │
                    └───────────┘ │ (retry<5)  │ │ (retry≥5)  │
                                  └───────────┘ └───────────┘
                                        │             │
                            Auto retry   │    Terminal │
                                        │             │
                                        ▼             ▼
                                  ┌───────────┐ ┌───────────┐
                                  │  Pending   │ │  Failed    │
                                  └───────────┘ └───────────┘
```

## Validation Rules

| Entity | Rule | Source |
|--------|------|--------|
| AccountingEvent | EventType required, max 50 chars | FR-001 |
| AccountingEvent | SourceTable required, max 50 chars | FR-001 |
| AccountingEvent | SourceId > 0 | FR-001 |
| AccountingEvent | Status in enum range | FR-006, FR-007 |
| PostingRule | EventType required, max 50 chars | FR-002 |
| PostingRule | JournalId > 0 | FR-003 |
| PostingRule | Priority >= 0 | FR-002 |
| DeletePostingRule | No Pending/Processing events | FR-019 |
