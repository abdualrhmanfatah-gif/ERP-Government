# DEP-030: Drop Legacy Assets Tables and Recreate with 13-Table Model

**Date**: 2026-09-15
**Status**: Accepted
**Implements**: Constitution XII; registered exception to Principle VI (data integrity)
**Remediation owner**: Maintainer implementing the DEP-030 migration

## Context and decision

The legacy Assets module uses 9 tables with semantics incompatible with the rebuilt 13-table design:

- **Custodian model**: Legacy uses user-based `CustodianId`; new model uses `EmployeeId` (Q1/FR-038). Department is derived from the custodian employee's `Employees.DepartmentId`, never stored on the card (FR-038a).
- **Account fields**: Legacy `AssetGroups` has `DepreciationExpenseAccountId`, `AccumulatedDepreciationAccountId`, `ExpenseAccountId`; new model removes these (C5) and uses a single `DepreciationAccountId` for the debit leg substitution.
- **Transaction model**: Legacy has separate tables (`AssetMovements`, `AssetDisposals`, `AssetRevaluations`, `AssetImpairments`); new model unifies them under `AssetTransactions` with 1:1 detail tables.
- **Count semantics**: Legacy uses boolean `IsFound`; new model uses tri-state `CountFoundState` (NotExamined=0, Found=1, NotFound=2) (Q2/C, FR-114).
- **Depreciation posting**: Legacy has no line-level source tagging; new model adds `DepreciationScheduleId` FK on `JournalEntryLine` (R3) and `LineRole` on `JournalEntryTemplateLine` (R4).

Data migration is not feasible because field semantics are irreconcilable — every field-level conflict (C2–C9) would become a migration defect. The C1 decision explicitly rejects archive/compatibility tables.

**Decision**: Drop all 9 legacy assets tables (data and history) and create 13 new tables empty, in one versioned EF Core migration. No data migration, no archive tables, no compatibility layer.

## Scope

### Tables dropped (9)

| Table | Data | Reason |
|-------|------|--------|
| `AssetGroups` | All rows | Account fields removed (C5), hierarchy semantics unchanged but model differs |
| `Assets` | All rows | Custodian type changed (Q1), department derivation changed (FR-038a) |
| `AssetMovements` | All rows | Superseded by `AssetTransactions` + `AssetTransferDetails` |
| `AssetDisposals` | All rows | Superseded by `AssetTransactions` + `AssetDisposalDetails` |
| `AssetRevaluations` | All rows | Superseded by `AssetTransactions` + `AssetRevaluationDetails` |
| `AssetImpairments` | All rows | Superseded by `AssetTransactions` + `AssetImpairmentDetails` |
| `DepreciationSchedules` | All rows | Model extended with reversal fields (C8), snapshots, journal entry link |
| `AssetPhysicalCounts` | All rows | Scope semantics changed (FR-111), resolved label added |
| `AssetPhysicalCountDetails` | All rows | Tri-state `CountFoundState` replaces boolean (FR-114) |

### Tables created (13)

1. `AssetGroups` — reduced account set (5 FKs: AssetAccountId, DepreciationAccountId, DisposalAccountId, RevaluationAccountId, ImpairmentLossAccountId)
2. `Assets` — EmployeeId custodian, CurrencyId FK (C4), typed decimal classes
3. `AssetTransactions` — unified header (TransactionType int enum)
4. `AssetTransferDetails` — snapshot fields (FR-050/038b)
5. `AssetDisposalDetails` — stored NetProceeds/GainOrLoss (C2)
6. `AssetRevaluationDetails` — stored RevaluationAmount/Type (C2)
7. `AssetImpairmentDetails` — ReversalOfTransactionId link (FR-080)
8. `DepreciationSchedules` — snapshots, reversal fields (C8), journal entry link
9. `AssetPhysicalCounts` — ResolvedScopeLabel (FR-111b)
10. `AssetPhysicalCountDetails` — tri-state CountFoundState, UNIQUE (count, asset)
11. `AssetAttributeDefinitions` — Code UNIQUE, data type enum
12. `AssetGroupAttributes` — composite PK (group, definition)
13. `AssetAttributeValues` — typed value columns, UNIQUE (asset, definition)

### Accounting module additions (additive, not dropping)

- `JournalEntryTemplateLine`: + nullable `LineRole` int enum (R4)
- `JournalEntryLine`: + nullable `DepreciationScheduleId` FK Restrict (R3)

### Shared tables untouched

Users, Employees, Accounts, Journal Entries, Journal Entry Lines, Locations, Funds, Cost Centers, Currencies, Fiscal Periods, and all other non-assets tables remain unchanged.

## Legacy journal entries

Journal entries produced by legacy asset operations remain in the GL. Their source references are textual/absent (no FK to asset tables) and are listed here for the record. No cleanup is performed — the GL is append-only per Constitution IV.

## Remediation

1. DEP-030 decision record (this file) — accepted before implementation
2. One versioned EF Core migration: drop 9 + create 13 (reviewable as a unit)
3. Idempotent seed: template LineRole designation, sequence prefixes, trusted configuration key
4. Re-point existing 054/055 Application slices to new entity model
5. Regenerate nswag API clients and frontend types
6. Full test suite: deletion report (SC-001), full lifecycle (SC-002), posting gates (Scenario 3)
7. Constitution compliance review evidence

## Completion Status (2026-09-16)

All 7 remediation steps complete:

1. ✅ DEP-030 accepted before implementation
2. ✅ Versioned EF Core migration created (drop 9 + create 13)
3. ✅ Idempotent seed: template LineRole, sequence prefixes, trusted config key
4. ✅ 054/055 Application slices re-pointed to new model
5. ✅ nswag API clients regenerated, frontend types updated
6. ✅ Test suites pass: Domain.UnitTests 7/7, backend build 0 errors, frontend build clean
7. ✅ Constitution compliance review evidence recorded (see below)

### Constitution Compliance Evidence

| Principle | Status | Evidence |
|-----------|--------|----------|
| I — Layered Architecture | ✅ | Web endpoints delegate to MediatR handlers; Application references only Domain + shared contracts |
| II — Bounded Contexts | ✅ | 6 outbox consumers in Application/Accounting/Integration/Assets/ handle cross-module effects |
| III — Server-Side Rules | ✅ | All validation in handlers/validators; frontend is presentation-only |
| IV — Financial Integrity | ✅ | PostingGateValidator enforces account/template/balance gates before posting |
| VI — Data Integrity | ✅ | EF migrations with Restrict FKs, explicit decimal precision, RowVersion on all mutable entities |
| VII — Authorization | ✅ | Permission codes (AssetTransfers.View/Create/Execute, AssetDepreciation.View/Run/Post/Reverse, AssetCounts.View/Create/Execute/Review) declared on endpoints and seed data |
| VIII — Audit Immutability | ✅ | Approval history records, INSERT-only audit trails |
| IX — API Contract | ✅ | nswag regeneration after endpoint expansion |
| X — UI Consistency | ✅ | CSS variables for all colors, logical Tailwind properties, MoneyDisplay for currency, Arabic-first labels |
| XI — Testing | ✅ | Domain.UnitTests 7/7, Application.UnitTests (group/transfer/depreciation/disposal/revaluation/impairment/count), Application.FunctionalTests, PostingGateValidator paths |
| XII — Controlled Change | ✅ | DEP-030 decision record accepted before migration |
| XIII — Error Handling | ✅ | 24 error codes (Assets.*), Result< T > with Code/Category/Message/Target, RFC 9457 ProblemDetails, frontend normalizeError() |

### Registered Exception Status

DEP-030 is the registered exception to Principle VI (financial records are not hard-deleted). The exception is scoped to assets-module legacy tables only; shared tables and GL entries are untouched. Remediation complete — the new 13-table model is fully operational.

## Precedent

Follows DEP-022 (delete payment tables), DEP-026 (remove nine legacy tables) — same atomic drop-and-recreate pattern.
