# Data Model — Budgeting Backend Completion (015)

All entities: int PKs, `BaseAuditableEntity` (Created, CreatedBy?, LastModified, LastModifiedBy?) + RowVersion concurrency token; enums stored as int; money = decimal(23,2). FK actions: Restrict only.

## Changed entities

### Appropriation
| Member | Change | Notes |
|--------|--------|-------|
| `TargetBudgetItemId` | ADD `int?` | FK → BudgetItems (Restrict), indexed. Marks transfer-pair rows (see Transfer Pair rules). |
| `Effect` (AppropriationType) | unchanged | Original=0, Supplement=1, Reduction=2, Transfer=3, Adjustment=4 |
| `Status` | unchanged enum | lifecycle commands extended for joint transfer transitions |
| Number | behavior change | `AppropriationNumber` = APR-###### assigned at create via DocumentSequenceService; immutable |

**Transfer Pair rules** (two rows, one command, one SaveChanges):
- Source row: `BudgetItemId = source`, `Effect = Transfer`, negative-effect amount.
- Target row: `BudgetItemId = target`, `Effect = Transfer`, positive amount, `TargetBudgetItemId = source item id`; source row carries `TargetBudgetItemId = target item id` (mutual reference).
- Pair match: partner of X = other row with `Effect = Transfer`, same `BudgetId`, `BudgetItemId == X.TargetBudgetItemId`, `TargetBudgetItemId == X.BudgetItemId`, `Amount == -X.Amount`.
- Joint lifecycle: any transition command applied to one row transitions both in the same unit of work.
- Validation at create (FR-15): same Budget, source ≠ target, source item net active appropriation ≥ amount.
- Availability: Transfer rows net 0 at item level (service unchanged).

### Budget
| Member | Change | Notes |
|--------|--------|-------|
| `IsActive` | REMOVE | property + config + column dropped; Status is sole lifecycle state |
| `TotalAmount` | wiring | accepted at create/update (entered plan), persisted (bug: always 0 today) |
| `AllowOverrun bool?` | wiring | accepted at create/update |
| `EffectiveFrom / EffectiveTo` | wiring | create/update; EffectiveTo stays nullable |
| `Description` | wiring | accepted at create/update; column length aligned to config |
| `BudgetNumber` | behavior change | BGT-###### system-assigned at create; user-supplied number removed from command + validator; immutable |

### BudgetItem
| Member | Change | Notes |
|--------|--------|-------|
| `OriginalAmount` | REMOVE | property + config + column dropped (zero-stored-aggregates; item net summary computed from Appropriations) |
| `Remarks` | KEEP (name stays `Remarks`) | column ADDED by migration (missing in DB today); aligned entity↔config↔DTO |

### Encumbrance
| Member | Change | Notes |
|--------|--------|-------|
| `EncumbranceNumber` | behavior change | ENC-###### system-assigned at create; immutable; unique index exists |
| `Status` enum | ADD `Suspended = 10` | append-only; int storage, no DB change |
| `VendorId` | FK dropped | FK `FK_Encumbrances_Suppliers_VendorId` removed (config has no relation; snapshot/DB do); plain VendorId index kept; proper Party FK lands in party spec |
| `ReversalOfId int?` | unchanged | reversal target; non-reversal rows have null |

## New entity

### BudgetItemMonthlyPlan (table `BudgetItemMonthlyPlans`)
| Field | Type | Rules |
|-------|------|-------|
| Id | int PK | |
| BudgetItemId | int | required FK → BudgetItems, Restrict |
| Month | int | 1–12 |
| PlannedAmount | decimal(23,2) | ≥ 0, ENTERED (never computed) |
| audit + RowVersion | | BaseAuditableEntity + rowversion |
| UNIQUE | | (BudgetItemId, Month) |
| Semantics | | informational/statistical only — no status, no enforcement, no workflow reads it |

## Encumbrance lifecycle FSM

```text
            submit        approve       activate
  Draft ──────────▶ PendingApproval ──────▶ Approved ──────▶ Active
    │                  │                                     │  │
    │ delete/update ok │ (no mutations)                      │  └─── suspend ──▶ Suspended ─┐
    ▼                  ▼                                     │                              │
  Cancelled ◀──────────┘                                     ├── close ────▶ Closed ◀───────┘
                                                             ├── suspend ──▶ Suspended
                                                             └── reverse ──▶ Reversed  (new row ReversalOfId = original)

  Suspended: ── close ──▶ Closed   ── reverse ──▶ Reversed   (reactivation back to Active NOT offered by this spec)
  Cancelled reachable from: Draft, PendingApproval, Approved, Active, Suspended
  Terminal: Closed, Cancelled, Reversed
  Reverse allowed ONLY from Active | Suspended (clarified Q1)
  Update / Delete: Draft only
  PartiallyReleased / PartiallyLiquidated / FullyLiquidated: producers are payment-side (later spec)
```

ApprovalHistory row on EVERY transition: `DocumentType="Encumbrance"`, `Decision="{from} -> {to}"`, optional `Reason` (Warning-override message on gated creates). Current shape frozen (no Action enum).

Availability "open" encumbrance set (service, unchanged): `{Active, PartiallyReleased, PartiallyLiquidated}` with `ReversalOfId == null`. Suspended/FullyLiquidated/Closed/Cancelled/Reversed/Draft/PendingApproval/Approved are NOT encumbering.

## Budget lifecycle (unchanged FSM, corrected writers)

Draft → Submitted → Approved → Active → (Suspended | Closed) | Cancelled. Approve/Activate set **Status only** — no IsActive writes/reads anywhere.

## Availability computation (single source)

`BudgetAvailabilityService` summary for a BudgetItem:
- `netAppropriated` = Σ Active appropriations: Original/Supplement/Adjustment `+`, Reduction `−`, Transfer `0`
- `encumbered` = Σ open encumbrances (set above)
- `available` = netAppropriated − encumbered
- `effectiveAllowOverrun` = `EvaluateAllowOverrun(item.AllowOverrun ??, budget.AllowOverrun ??, budgetType.AllowOverrun)` (item > budget > type)

## Schema change list (ONE migration)

1. `BudgetItems`: DROP `OriginalAmount`; ADD `Remarks` (config length).
2. `Budgets`: DROP `IsActive`; ALTER `Description` length (config wins).
3. `Funds`: ALTER `FundNumber`/`LegalAuthority`/`Description` lengths (config wins).
4. `Encumbrances`: ALTER `Description` length; DROP FK `FK_Encumbrances_Suppliers_VendorId` (keep index).
5. `Appropriations`: ADD `TargetBudgetItemId` int? + FK Restrict + index.
6. CREATE `BudgetItemMonthlyPlans` (+ UNIQUE (BudgetItemId, Month)).
7. `EncumbranceStatus`: + `Suspended = 10` (enum only, no DB change).

## Sequence service (behavioral, no schema change)

Atomic allocation (single UPDATE...OUTPUT), typed exceptions (`DocumentSequenceNotFoundException`, `DocumentSequenceInactiveException`) → handlers translate to Result failures. Format `{prefix}-{D6}` unchanged; prefixes Budget/Appropriation/Encumbrance already seeded.
