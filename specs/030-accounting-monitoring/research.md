# Research: ACC-05 — مراقبة المحاسبة (Accounting Monitoring)

**Date**: 2026-09-07

## R1: AccountBalance Entity — Already Exists

**Decision**: Reuse existing `AccountBalance` entity at `src/Domain/Accounting/Entities/AccountBalance.cs`

**Rationale**: Entity already has all required fields: AccountId, FiscalYearId, FiscalPeriodId, CurrencyId, OpeningDebit/Credit, Debit/Credit, ClosingDebit/Credit, IsFinalized, FinalizedAt. No schema changes needed.

**Alternatives considered**: Creating a new entity — rejected as redundant.

## R2: AccountingEvent Status Model — Extend Existing

**Decision**: Extend `EventStatus` enum to add `Processing` and `Failed` states.

**Current**: `Pending=0, Posted=1, Reversed=2`

**Proposed**: `Pending=0, Processing=1, Posted=2, Reversed=3, Failed=4`

**Rationale**: Spec requires `pending/processing/processed/failed`. Existing Posted maps to "processed". Need Processing (in-progress) and Failed (terminal with error) states. Renaming Posted→Posted keeps backward compat.

**Alternatives considered**: Add as separate int values without reordering — rejected because EventStatus is already used in queries/filters. Adding Processing before Posted is cleaner.

## R3: AccountSource / AmountSource Enums — Define as Server-Side

**Decision**: Define as enums stored as int in DB.

**AccountSource**:
- `FixedAccount = 0` — uses `FixedAccountId` from PostingRuleLine
- `FromEventDimension = 1` — derives from event source entity

**AmountSource**:
- `FixedAmount = 2` — uses fixed amount from event/line
- `EventAmount = 3` — uses transaction amount from event source

**Rationale**: These control how posting rules map accounts and amounts. Server-side enforcement prevents invalid combinations (e.g., FixedAccount without FixedAccountId).

## R4: Period Close Guard — Reject if Pending Events

**Decision**: FinalizePeriod command checks for AccountingEvents with Status=Pending in the target period. If any exist, reject with error message.

**Rationale**: Clarification Q2 confirmed rejection. Prevents closing period while events are still queued — ensures completeness of posted data.

## R5: Posting Rule Priority — Execution Order

**Decision**: `Priority` field determines execution order (highest priority first). PostingRuleMatcher already orders by Priority ascending in existing code.

**Rationale**: Clarification Q3 confirmed execution order. Higher priority = processed first. Changing priority is a safe operation (rules are evaluated per-event, not retroactively).

## R6: Reconciliation — Compare Materialized vs Calculated

**Decision**: `ReconcileAccountBalancesQuery` computes balances from JournalEntryLines for a period, compares against materialized AccountBalance rows. Returns discrepancies per currency.

**Rationale**: Constitution Principle IV states "Derived balances are materializations that MUST be rebuildable from posted data, and a rebuild MUST reconcile against the ledger." Reconciliation is the verification; Rebuild is the correction.

## R7: Frontend — NSwag Client for New Endpoints

**Decision**: Use NSwag-generated clients for new Accounting endpoints (AccountingBalances, AccountingEvents, PostingRules). Follow accounting feature pattern.

**Rationale**: Existing accounting feature uses NSwag clients. New endpoints auto-generate typed clients on `npm run generate-api`.

## R8: Permissions — Already Defined

**Decision**: Use existing PermissionCodes without changes.

- `Accounting.Balances.Read` / `.Rebuild` / `.Finalize` / `.Unfinalize`
- `Accounting.AccountingEvents.Read`
- `Accounting.PostingRules.Read` / `.Create` / `.Edit` / `.Delete`

**Rationale**: All required codes already exist in PermissionCodes.cs. No new codes needed.
