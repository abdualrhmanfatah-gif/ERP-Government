# Data Model: Budget Execution Report (RPT-01)

**Date**: 2026-09-08 | Read-only feature — NO new entities, NO migrations. This document maps the report contract to existing entities consumed via `IApplicationDbContext`.

## Source Entities (read-only)

### FiscalYear (src/Domain/FinancialSettings/Entities/FiscalYear.cs)
| Field | Type | Role in report |
|---|---|---|
| Id | int | `fiscalYearId` in contract |
| YearNumber | int | rendered as `fiscalYearName` (handler uses YearNumber.ToString()) |
| Status | FiscalYearStatus enum | auto-select Open year (frontend, research D5); lapsed/closed years still reportable |

### Budget / BudgetItem (src/Domain/Budgeting/Entities/)
| Field | Type | Role |
|---|---|---|
| Budget.FiscalYearId | int | year scoping |
| Budget.FundId | int | fund filter + line dimension |
| BudgetItem.ItemCode / ItemName | string | line identity columns |
| BudgetItem.BudgetClassificationId | int? | → program/project resolution via classification ancestry (research D1) |

### BudgetClassification (src/Domain/Budgeting/Entities/BudgetClassification.cs)
| Field | Type | Role |
|---|---|---|
| Id / Code / Name | int / string / string | program/project dimension codes on lines |
| ParentId | int? | ancestry walk: node → program level / project level; subtree filter (ProgramId/ProjectId = node or descendant) |

### Appropriation (src/Domain/Budgeting/Entities/Appropriation.cs)
| Field | Type | Role |
|---|---|---|
| BudgetItemId | int | grouping key per line |
| Amount | decimal(23,2) | `appropriatedAmount` (Σ per item) |
| Status | AppropriationStatus | exclude Cancelled |

### Encumbrance (src/Domain/Budgeting/Entities/Encumbrance.cs)
| Field | Type | Role |
|---|---|---|
| AppropriationId → Appropriation.BudgetItemId | int | line linkage |
| Amount | decimal(23,2) | `encumberedAmount` (Σ outstanding) |
| Status | EncumbranceStatus | include Active, PartiallyReleased, PartiallyLiquidated (research D3) |

### PaymentOrder (src/Domain/Payments/Entities/PaymentOrder.cs)
| Field | Type | Role |
|---|---|---|
| AppropriationId | int | line linkage |
| AmountGross | decimal(23,2) | `paidAmount` (Σ) |
| Status | PaymentOrderStatus | include Approved, SentToTreasury, Paid, PartiallyPaid (research D3) |

## Report Contract (binding — mirrors src/Application/Reporting/BudgetExecution DTOs, verified 2026-09-08)

```text
BudgetExecutionReportDto
├── fiscalYearId: int
├── fiscalYearName: string
├── lines: BudgetExecutionLineDto[]
│   ├── budgetItemId: int · itemCode: string · itemName: string
│   ├── fundId: int · fundNumber: string · fundName: string
│   ├── programId?: int · programCode?: string        ← to be POPULATED (gap)
│   ├── projectId?: int · projectCode?: string        ← to be POPULATED (gap)
│   └── appropriatedAmount · encumberedAmount · paidAmount · availableAmount : decimal(23,2)
└── totals: BudgetExecutionTotalDto { appropriatedAmount, encumberedAmount, paidAmount, availableAmount }

BudgetExecutionDetailDto (GET /{budgetItemId}/detail)
├── budgetItemId · itemCode · itemName · fundId · fundNumber
├── encumbrances: EncumbranceDetailDto[]
└── payments: PaymentDetailDto[]
```

## Derived Values (display layer ONLY — never server fields)

| Value | Formula | Edge rule |
|---|---|---|
| Usage ratio (عمود "عرض") | paidAmount / appropriatedAmount | appropriated = 0 → "—"; render as % |
| Available | appropriated − encumbered − paid (server-computed) | negative → red money representation |

## Validation Rules & Invariants

- SC-002: `totals.X == lines.Sum(X)` for all four amounts (existing comparison test extended to filtered sets).
- SC-004: per line `appropriated = encumbered + paid + available` (existing reconciliation test).
- Totals always computed over ALL filtered rows — never page-scoped (clarified 2026-09-08).
- No divide-by-zero in usage ratio (FR-003 edge case).

## State Transitions

None — read-only report. Statuses of source documents are observed, never mutated.
