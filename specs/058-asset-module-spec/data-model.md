# Data Model: Asset Management Module Rebuild

**Feature**: 058-asset-module-spec | **Date**: 2026-09-15

All FKs are **Restrict** (no cascades). Every mutable table carries `RowVersion` (optimistic concurrency) and audit columns (`BaseAuditableEntity`). Monetary precision is explicit per class (Constitution VI). Legacy tables dropped per DEP-030; the 13 tables below are created empty.

## 1. AssetGroups (مجموعات الأصول)

| Field | Type | Rules |
|---|---|---|
| Id | int PK | identity |
| Code | nvarchar(50) | UNIQUE (FR-010) |
| Name | nvarchar(200) | required |
| Description? | nvarchar(1000) | |
| ParentAssetGroupId? | int FK→self | hierarchy, no cycles (traversal check) |
| AssetAccountId? | int FK→Accounts | default asset account |
| DepreciationAccountId? | int FK→Accounts | **the single depreciation account** (C5/FR-010) — debit leg source (FR-094a) |
| DisposalAccountId? / RevaluationAccountId? / ImpairmentLossAccountId? | int FK→Accounts | per-operation defaults |
| DepreciationMethod | nvarchar(50) | current constrained values |
| DepreciationRate? | decimal(18,6) | |
| DefaultUsefulLifeYears? / ResidualValuePercentage? decimal(5,2) ⚠️ | int? / decimal(5,2) | conditional per spec |
| IsDepreciable / IsActive | bit | |
| AssetCategory | nvarchar(50) | constrained current values |

Removed per C5 (never re-added automatically): `DepreciationExpenseAccountId`, `AccumulatedDepreciationAccountId`, `ExpenseAccountId`. Group accounts may be incomplete during configuration; postable-ness is validated at posting (FR-014). Account changes never rewrite posted entries (FR-013).

## 2. Assets (الأصول)

| Field | Type | Rules |
|---|---|---|
| Id | int PK | |
| Code | nvarchar(50) | UNIQUE within numbering scope; server-generated via DocumentSequenceService (FR-031) |
| Name / Description? | nvarchar(200) / nvarchar(2000) | |
| AssetGroupId | int FK→AssetGroups | required |
| LocationId? / FundId? / CostCenterId? | int FK | |
| EmployeeId? | int FK→Employees | **custodian (Q1/FR-038)** — replaced user-based CustodianId; current department is DERIVED from `Employees.DepartmentId`, never stored (FR-038a) |
| CurrencyId | int FK→Currencies | maps current currency code (C4) |
| AssetTag? / Barcode? / SerialNumber? | nvarchar(100) | uniqueness per current rule (C6 open — preserved) |
| ImageUrl? | nvarchar(500) | kept until replacement proven |
| OriginalValue ⚠️ / RelinquishmentValue ⚠️ / IsActive? ⚠️ | decimal(23,2) / bit | conditional per spec |
| AcquisitionCost / ResidualValue? | decimal(23,2) | |
| AccumulatedDepreciation | decimal(23,6) | six-decimal class (FR-091) |
| CurrentValue? | decimal(23,2) | current-state summary, NOT a rebuildable invariant (FR-033) |
| PurchaseDate? / ActivationDate? / DepreciationStartDate? / LastDepreciationDate? | date | |
| Status | nvarchar(50) | constrained: Draft, Active, UnderMaintenance, Disposed, WrittenOff |
| AcquisitionType | nvarchar(50) | constrained current values |
| UsefulLifeYears? / IsFullyDepreciated | int? / bit | |
| Notes? | nvarchar(2000) | |

## 3. AssetTransactions (معاملات الأصول — unified header)

| Field | Type | Rules |
|---|---|---|
| Id | int PK | |
| TransactionNumber | nvarchar(50) | per-type sequence (R5); UNIQUE (TransactionType, TransactionNumber) (FR-044) |
| AssetId | int FK→Assets | |
| TransactionType | int | 1 Transfer, 2 Disposal, 3 Revaluation, 4 Impairment |
| TransactionDate | date | effect date per current policy |
| Status | int enum | Draft → Approved → Posting → Posted; PostingFailed (re-drivable); see State Machines |
| CurrencyId | int FK→Currencies | |
| JournalEntryId? | int FK→JournalEntries | written by the posting consumer (R2) |
| IsPosted | bit | false until consumer completes |
| ReferenceType? / ReferenceId? | nvarchar(50) / int? | external reference, no generic FK (FR-040) |
| Notes? | nvarchar(2000) | |

Exactly one detail row (1:1) per transaction, discriminated by type (tables 4–7).

## 4. AssetTransferDetails (تفاصيل النقل)

PK/FK: `AssetTransactionId` (transfer type only). `OccurredAt datetime2` (movement time), `FromLocationId?/ToLocationId?` FK→Locations, `FromEmployeeId?/ToEmployeeId?` FK→Employees (FR-038), `FromDepartmentId?/ToDepartmentId?` FK→Departments — **historical snapshots at execution** (FR-038b), never refreshed. Null destination = intentional final clearing where allowed (FR-051); unchanged fields carry current values (FR-050).

## 5. AssetDisposalDetails (تفاصيل الاستبعاد)

PK/FK: `AssetTransactionId` (disposal only). `DisposalMethod nvarchar(50)` (current values), `BookValueAtDisposal decimal(23,2)`, `AccumulatedDepreciationAtDisposal decimal(23,6)` (historical snapshots, FR-060), `SaleProceeds?/DisposalCost? decimal(23,2)`, `BuyerName? nvarchar(200)`, `BuyerContact? nvarchar(100)`. Stored `NetProceeds?/GainOrLoss? decimal(23,2)` kept consistent with derivation (C2, FR-061). **No disposal account here** — group default at posting, actual accounts in entry lines (FR-062). Second disposal of the same asset rejected (FR-064).

## 6. AssetRevaluationDetails (تفاصيل إعادة التقييم)

PK/FK: `AssetTransactionId` (revaluation only). `RevaluationMethod nvarchar(50)` (current values; required at completion), `OldBookValue/NewBookValue decimal(23,2)` (one currency basis), `AppraiserName? nvarchar(200)`, `ReportNumber? nvarchar(100)`. Stored `RevaluationAmount? decimal(23,2)` + `RevaluationType? nvarchar(50)` (C2, FR-071). Zero difference is not auto-classified as increase (FR-071); details alone never determine account distribution (FR-072).

## 7. AssetImpairmentDetails (تفاصيل انخفاض القيمة)

PK/FK: `AssetTransactionId` (impairment only). `CarryingAmount/RecoverableAmount/LossAmount decimal(23,2)` (sign convention fixed once in the implementation map, FR-080), `Reason nvarchar(500)`, `Description? nvarchar(2000)`, `AssessorName? nvarchar(200)`, `ReportNumber? nvarchar(100)`, `ReversalOfTransactionId?` FK→AssetTransactions (same asset, impairment type only; self/cycle rejection, FR-081), `ReversalReason? nvarchar(500)`.

## 8. DepreciationSchedules (جداول الإهلاك)

| Field | Type | Rules |
|---|---|---|
| Id | int PK | |
| AssetId | int FK→Assets | |
| DepreciationDate | date | |
| FiscalYearId | int FK | period-in-year validated (FR-095) |
| FiscalPeriodId? | int FK | optional per current model |
| Method / Base / Rate | nvarchar(50) / decimal(23,6) / decimal(18,6) | **historical snapshots** (FR-090) |
| PeriodNumber? / TotalPeriods? | int? | conditional, retained per current use |
| Amount / AccumulatedAfterOperation / NetBookValueAfter | decimal(23,6) | six-decimal class; stored == posted (FR-091) |
| Status | int enum | Draft → Posted → Reversed |
| JournalEntryId? | int FK→JournalEntries | per-record link (aggregation per Q3: B at line level) |
| ReversalOfId? | int FK→self | reversal link; IsReversed + ReversalDate? + ReversalReason? stored (C8) |

**No UNIQUE (asset, period)** — duplicate prevention is use-case logic keyed on current-period identity + record nature (FR-094). Posted records never deleted/rewritten (FR-093).

## 9. AssetPhysicalCounts (رأس الجرد)

| Field | Type | Rules |
|---|---|---|
| Id | int PK | |
| CountNumber | nvarchar(50) | UNIQUE (CNT sequence) |
| CountDate | date | |
| LocationId? / DepartmentId? | int FK | scope inputs (FR-111 semantics) |
| ResolvedScopeLabel | nvarchar(200) | stored at creation — e.g. «جميع المواقع — جميع الإدارات» (FR-111b display) |
| CountType | nvarchar(50) | current values |
| Status | int enum | Draft → InProgress → Completed |
| StartedAt? / CompletedAt? | datetime2 | |
| CountedById? / ReviewedById? | int FK→Users | approved references; counter required at the stage that demands one (FR-118) |
| Notes? | nvarchar(2000) | |

## 10. AssetPhysicalCountDetails (تفاصيل الجرد)

| Field | Type | Rules |
|---|---|---|
| Id | int PK | |
| AssetPhysicalCountId | int FK | **UNIQUE (AssetPhysicalCountId, AssetId)** (FR-116) |
| AssetId | int FK→Assets | registered assets only (FR-115) |
| SystemLocationId? / PhysicalLocationId? | int? FK→Locations | System* frozen at generation (FR-112); physical NEVER auto-filled (FR-113) |
| SystemEmployeeId? / PhysicalEmployeeId? | int? FK→Employees | custodian snapshots (FR-038) |
| SystemStatus? / PhysicalStatus? | nvarchar(50) | |
| IsFound | int enum | NotExamined=0, Found=1, NotFound=2 (Q2: C, FR-114); completion requires no NotExamined line |
| IsMatch? | bit | stored per C2 pending decision |
| DiscrepancyNotes? | nvarchar(2000) | |

Re-examination updates the row in place; every change writes an `AuditTrail` record (OldValues/NewValues/FieldChanges, actor, timestamp — Constitution VIII, FR-116). After completion, no edits via the normal path. The count never mutates the card (FR-117).

## 11. AssetAttributeDefinitions (تعريفات الخصائص)

`Id int PK`, `Code nvarchar(50) UNIQUE`, `Name nvarchar(200)` required, `AttributeDataType int enum` (Text/Integer/Decimal/Date/Boolean), `IsActive bit`.

## 12. AssetGroupAttributes (ربط الخصائص بالمجموعات)

Composite PK (`AssetGroupId` FK, `AssetAttributeDefinitionId` FK) — the binding identity (FR-022). `IsRequired bit`, `SortOrder? int` (absent order falls back to the definition's default; never reorders history, FR-022).

## 13. AssetAttributeValues (قيم الخصائص)

`Id int PK`, `AssetId int FK`, `AssetAttributeDefinitionId int FK`, **UNIQUE (AssetId, AssetAttributeDefinitionId)** (FR-024), typed value columns — exactly one populated per the definition's data type (R9): `TextValue? nvarchar(1000)`, `IntegerValue? int`, `DecimalValue? decimal(18,4)`, `DateValue? date`, `BooleanValue? bit`. Required-ness validated from the group binding; values locked per current card lock rules.

## Accounting module — additive changes (R3/R4)

- **JournalEntryTemplateLine** `+ LineRole int?` (enum: `Fixed`, `GroupDepreciationAccount`) — nullable; designates the substitution slot explicitly (never order/name). Idempotent seed designates the depreciation template's roles.
- **JournalEntryLine** `+ DepreciationScheduleId int?` FK Restrict — line-level source tagging for aggregated depreciation entries (Q3: B, FR-094/FR-124); mirrors the existing `PaymentOrderId` pattern.
- Depreciation template located by a trusted configuration key (never `JournalId=6`).

## State Machines (guarded server-side, Principle III)

```text
AssetTransaction: Draft → Approved → Posting → Posted
                                  Posting → PostingFailed → Posting (bounded retry / manual re-drive)
Impairment reversal: new impairment-type transaction (ReversalOfTransactionId) — original never mutated
Asset: Draft → Active → UnderMaintenance → Active | Disposed | WrittenOff (current values)
DepreciationSchedule: Draft → Posted → Reversed (reversal = new linked record, FR-100)
AssetPhysicalCount: Draft → InProgress → Completed (blocked while any line NotExamined, FR-114)
```

## Cross-cutting integrity rules

- Execution persists document state + outbox event + card effect atomically; the consumer idempotently creates the entry and writes back `JournalEntryId` (FR-120/122, R1/R2).
- Posting gates (FR-094a, IV): group account present, template complete, substitution role defined, entry balanced, fiscal period open/unlocked, account postable — validated before event emission.
- Every header AND card update verifies concurrency tokens (FR-121); repeating an execution returns the same result (FR-122).
- Retrospective-dated transactions compute balances as of the effect date, never copy current balances (FR-123).
