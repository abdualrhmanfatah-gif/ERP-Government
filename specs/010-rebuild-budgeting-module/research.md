# Research: Rebuild Budgeting Module from Scratch

**Feature**: `010-rebuild-budgeting-module` | **Date**: 2026-09-04

All unknowns below were resolved by reading the repository (no NEEDS CLARIFICATION markers remain in the spec).

## R1 — Enum storage: keep `int`, no string conversion

- **Decision**: All 8 budgeting enums are stored as `int` columns. New configurations use plain enum properties (no `.HasConversion<string>()`); the vestigial `.HasMaxLength(20)` on enum mappings is dropped.
- **Rationale**: The model snapshot proves current enums persist as `HasColumnType("int")`. Only `OutboxMessage`/BackgroundJobs/RecurringEntryExecutionLog use explicit conversions. Int storage keeps indexes small and avoids a data-type migration for every status column.
- **Alternatives considered**: String-valued enums (rejected: full-column type churn, larger unique/index footprint, diverges from every other module).

## R2 — Document numbers: entity-name keys, BGT/APR/ENC prefixes

- **Decision**: `DocumentSequenceService` keeps entity-name `DocumentType` keys. PrefixMap becomes `Budget → BGT` (new), `Appropriation → APR` (changed from `APP`), `Encumbrance → ENC` (kept). `CreateBudget` switches from client-supplied `BudgetNumber` to server-generated numbers via `GenerateNextNumberAsync("Budget")`. Migration/seeds ensure `DocumentSequence` rows exist for `Budget`, `Appropriation`, `Encumbrance` (idempotent insert-if-missing; the current seed has none of the three).
- **Rationale**: Matches the service contract (`GenerateNextNumberAsync(documentType)` looks up rows by entity-name `DocumentType`) and the spec's BGT/APR/ENC prefixes. The APP→APR change is safe because all rows are wiped and numbering restarts.
- **Alternatives considered**: Using BGT/APR/ENC as `DocumentType` keys (rejected: contradicts the service lookup and every existing caller, e.g. `"Appropriation"`, `"Encumbrance"`).

## R3 — Fiscal-closure gate: FY status + period lock at transaction time

- **Decision**: `Create/Approve` appropriation and encumbrance commands reject when the Budget's FiscalYear has `IsClosed == true` OR `Status != Open` (Draft/SoftClosed/HardClosed all reject). Additionally the FiscalPeriod containing the transaction date (`EncumbranceDate` for encumbrances, `UtcNow` date for appropriations) must exist, be active, and have `IsLockedForPosting == false`.
- **Rationale**: `FiscalYear` exposes both `IsClosed` and `FiscalYearStatus` (Draft/Open/SoftClosed/HardClosed); `FiscalPeriod` exposes `IsLockedForPosting` with date ranges. Checking both satisfies the spec edge case and Principle III (time-windowed rules evaluated at transaction time).
- **Alternatives considered**: FY-only gate (rejected: leaves the spec's "or period locked" edge case unenforced).

## R4 — Level computation: application-side traversal, cycle-guarded

- **Decision**: `Level` is computed in query handlers by loading `(Id, ParentId)` pairs, building an in-memory lookup, and walking parents iteratively with a visited-set cycle guard (cycle → explicit failure, never infinite loop). Tree queries reuse the existing `GetBudgetClassificationsTreeQuery` lookup pattern, replacing `OrderBy(Level)` with code-first ordering.
- **Rationale**: Provider-portable (no recursive CTE dialect dependency), unit-testable without a database, and safe at 5+ depths. Stored `Level` is forbidden by FR-003/FR-005.
- **Alternatives considered**: Recursive SQL CTE (rejected: SQL Server-specific, harder to unit test); storing Level with trigger maintenance (rejected: forbidden derived column).

## R5 — IsReversed: `EXISTS` projection, never stored

- **Decision**: `EncumbranceDto.IsReversed` is projected as `context.Encumbrances.Any(r => r.ReversalOfId == e.Id)`. Reversal rows themselves carry `ReversalOfId`/`ReversalReason` and are excluded from availability sums by the `ReversalOfId == null` filter (they also carry non-counted statuses).
- **Rationale**: Direct implementation of FR-008; no schema column; single correlated subquery per row.
- **Alternatives considered**: Stored `IsReversed` flag maintained by the reverse command (rejected: forbidden derived column, dual-write inconsistency risk).

## R6 — Availability engine: `IBudgetAvailabilityService` with aggregate queries

- **Decision**: New Application service `IBudgetAvailabilityService` (Budgeting/Common) with `GetAvailabilityForItemAsync(budgetItemId)` and `GetAvailabilityForEncumbranceAsync(appropriationId)` returning `{ BudgetItemId, NetAppropriated, TotalEncumbered, Available, ControlMethod, EffectiveAllowOverrun, Warning }`. Implementation uses EF `SumAsync` aggregates over `Status == Active` + type sets from FR-009 (Adjustment included with sign); encumbrance path resolves `BudgetItemId` from the supplied `AppropriationId`, aggregates appropriations by `BudgetItemId`, and aggregates encumbrances joined through their appropriations to the same `BudgetItemId` with statuses Active/PartiallyReleased/PartiallyLiquidated and `ReversalOfId == null`. Money math stays in `decimal(23,2)`; two-decimal rounding at DTO presentation. Handlers apply None (skip) / Warning (log + allow + surface warning) / Blocking (explicit `Result.Failure` including the computed available amount).
- **Rationale**: One testable seam for both commands and the `GET availability` endpoint; aggregates execute in SQL; matches clarification answers (signed Adjustment, whole-BudgetItem net).
- **Alternatives considered**: Inline LINQ per handler (rejected: duplicated logic, drift risk); database views/stored procedures (rejected: migrations-only schema + C# testability preferred; views would also complicate the wipe migration).

## R7 — ApprovalHistory writes and latest-decision projection

- **Decision**: Every approval-gated transition inserts an `ApprovalHistory` row (`DocumentType`, `DocumentId`, `ApproverUserId`, `Decision`, `DecisionAt = UtcNow`, `Reason`, `EvaluationSnapshot` JSON capturing from/to status, amounts, availability result, ControlMethod, effective AllowOverrun, FY check). Latest-decision projection queries `Where(DocumentType, DocumentId).OrderByDescending(DecisionAt)`. `RequiredRole` captures the evaluated permission/policy.
- **Rationale**: `ApprovalHistory` already has exactly these columns; satisfies Principle VIII and the approval-panel requirement with no new tables.
- **Alternatives considered**: New per-document approval tables (rejected: duplicates existing audit storage).

## R8 — Permissions: extend the existing scheme with missing lifecycle members

- **Decision**: Keep the six existing families and add the missing lifecycle members each new command requires (e.g. `Appropriations.Submit/.Activate/.Suspend/.Close/.Cancel/.Update/.Delete`, `Encumbrances.Submit/.Activate/.Close/.Cancel`, `BudgetItems.Delete`, `BudgetClassifications` toggles), following the `Budgets.*` full-lifecycle precedent. Seed them idempotently; wire `RequireAuthorization` + `[Authorize(Policy)]` per endpoint/use case.
- **Rationale**: Principle VII requires a *named* permission per use case and SC-008 requires distinguishable 403s; coarse-mapping every transition to `*.Approve` would weaken least-privilege and per-status button gating. "Reuse existing codes" (FR-012) is read as reusing the scheme/families with no legacy codes.
- **Alternatives considered**: Strictly no new codes, mapping all transitions to existing `*.Approve` (rejected: see rationale; would also make `navigation.ts` permission identifiers meaningless for lifecycle actions).

## R9 — Migration: single wipe-and-recreate is safe from inbound FKs

- **Decision**: One migration drops all 7 Budgeting tables (order: Encumbrance → Appropriation → BudgetItem → Budget → BudgetClassification → BudgetType/Fund, respecting self-FKs) and recreates them per FR-001–FR-007 with unique + FK indexes. Verified: no non-Budgeting configuration declares a real FK relationship to these tables (Payments only carries plain index columns such as `PaymentOrders.FundId/AppropriationId` with no `HasOne/HasForeignKey`; the two Accounting `FundId` references are commented out). Down migration restores prior schema; data loss accepted per spec.
- **Rationale**: Satisfies FR-015 with minimum ordering risk; historical migrations untouched; `DocumentSequences` table itself is never dropped (only BGT/APR/ENC rows ensured).
- **Alternatives considered**: Per-table migrations (rejected: intermediate states would break the model snapshot; spec mandates a single migration).
