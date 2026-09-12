# Feature Specification: Budget Ledger Frontend Alignment

**Feature Branch**: `046-budget-ledger-redesign`

**Created**: 2026-09-10

**Status**: Draft

**Parent Spec**: `046-budget-ledger-redesign/spec.md`

**Input**: Frontend remaining work after backend BudgetTransactions + EncumbranceLines redesign. Appropriation entity removed from backend; frontend types/hooks/pages must align.

## Problem Statement

Backend redesign replaced Appropriations with BudgetTransactions + Lines, removed `Budget.TotalAmount`, `BudgetItem.FundId`, `Fund.FiscalYearId`, and `PaymentOrder.AppropriationId`. Frontend still references these removed fields and entities. BudgetTransactions pages are built but shared types, hooks, and other pages are stale.

**Evidence**: Backend build succeeds; frontend `npm run lint` and `npm run build` will fail on stale type references.

**Frequency**: Every frontend build and dev session.

**Impact**: Frontend cannot compile or display data correctly until aligned.

---

## Non-Goals

| Non-Goal | Rationale |
|----------|-----------|
| New feature development | This is alignment/cleanup, not new features |
| Backend API changes | Backend is already rebuilt and verified |
| Test suite creation | Frontend has no test suite by governance decision |
| Data migration | DB is experimental, can be wiped |

---

## Current State Analysis

### Backend (DONE)

| Change | Status |
|--------|--------|
| `BudgetTransaction` entity + enums | DONE |
| `BudgetTransactionLine` entity | DONE |
| `EncumbranceLine` entity | DONE |
| `Appropriation` entity deleted | DONE |
| `AppropriationType/Status` enums deleted | DONE |
| `Budget.TotalAmount` removed | DONE |
| `BudgetItem.FundId` removed | DONE |
| `Fund.FiscalYearId` removed | DONE |
| `PaymentOrder.AppropriationId` removed | DONE |
| `BudgetAvailabilityService` rewritten | DONE |
| Reporting queries rewritten | DONE |
| `DocumentSequenceService` APR prefix removed | DONE |
| `PaymentOrderConfiguration` updated | DONE |
| Unit tests updated | DONE |

### Frontend (PARTIAL)

| Component | Status | Files |
|-----------|--------|-------|
| `budget-transactions/` pages (3) | DONE | `pages/` folder |
| `budget-transactions/shared/types.ts` | DONE | types defined |
| `budget-transactions/shared/client.ts` | DONE | API client |
| `hooks/useBudgetTransactions.ts` | DONE | hook exists |
| Routes registered | DONE | `routes.tsx` |
| Navigation updated | DONE | `navigation.ts` |
| Permissions added | DONE | `permissions.ts` |
| **`shared/types.ts`** | **STALE** | Old enums/DTOs remain |
| **`shared/client.ts`** | **STALE** | `encumbrancesClient.getAvailability` remains |
| **`hooks/useEncumbrances.ts`** | **STALE** | `appropriationId` filter remains |
| **`hooks/useBudgets.ts`** | **STALE** | `totalAmount` in create/update |
| **`hooks/useBudgetItems.ts`** | **STALE** | `fundId` in create/update |
| **`hooks/useMonthlyPlan.ts`** | **STALE** | `month` array instead of `fiscalPeriodId` |
| **`encumbrances/pages/`** | **STALE** | No lines support |
| **`budgets/pages/`** | **STALE** | `totalAmount` references |
| **`funds/pages/`** | **STALE** | `fiscalYearId` references |
| **`budgets-items/pages/`** | **STALE** | `fundId` references |

---

## Requirements

### FR-F001: Remove stale enums from shared/types.ts

**Type**: Cleanup

- Remove `AppropriationType` enum (if present)
- Remove `AppropriationStatus` enum (if present)
- Remove `AppropriationDto` interface (if present)
- Remove `EncumbranceAvailabilityDto` interface
- Remove `ItemAvailabilityDto` interface
- Remove `fiscalYearId` from `FundDto`
- Remove `totalAmount` from `BudgetDto`
- Remove `fundId` from `BudgetItemDto`
- Add `BudgetTransactionType` enum (InitialAppropriation, Supplement, Reduction, Transfer, CarryForward, Adjustment, Lapse, Reversal)
- Add `BudgetTransactionStatus` enum (Draft, Submitted, Approved, Posted, Reversed, Rejected)
- Add `TransactionDirection` enum (Increase, Decrease)
- Add Arabic label maps for new enums
- Add `BudgetTransactionDto` interface
- Add `BudgetTransactionLineDto` interface
- Add `EncumbranceLineDto` interface (already exists, verify alignment)
- Update `EncumbranceDto` to use `lines: EncumbranceLineDto[]` (verify)
- Add `BudgetAvailabilitySummaryDto` interface with `revisedBudget`, `actualExpenditure`, `outstandingEncumbrance`, `available`
- Remove `netAppropriated`, `totalSupplement`, `totalReduction`, `totalAdjustment` from availability DTOs

### FR-F002: Update shared/client.ts

**Type**: Cleanup

- Remove `encumbrancesClient.getAvailability` method
- Remove `encumbrances.availability` cache key
- Add `budgetTransactionsClient` with: list, getById, create, update, delete, submit, approve, post, cancel, reverse
- Add `budgetTransactions.*` cache keys

### FR-F003: Update hooks/useEncumbrances.ts

**Type**: Cleanup

- Remove `appropriationId` from filter parameters
- Remove `useEncumbranceAvailability` hook (or replace with budget availability)
- Update to use `EncumbranceLineDto[]` in encumbrance data

### FR-F004: Update hooks/useBudgets.ts

**Type**: Cleanup

- Remove `totalAmount` from create/update command payloads
- Verify all field names match backend DTOs

### FR-F005: Update hooks/useBudgetItems.ts

**Type**: Cleanup

- Remove `fundId` from create/update command payloads
- Verify all field names match backend DTOs

### FR-F006: Update hooks/useMonthlyPlan.ts

**Type**: Cleanup

- Change `month` (1-12 number) to `fiscalPeriodId` (integer reference)
- Update monthly plan DTO to use `fiscalPeriodId`

### FR-F007: Update encumbrances pages for lines support

**Type**: Feature alignment

- `EncumbrancesListPage.tsx`: Show total from lines, not single amount
- `CreateEncumbrancePage.tsx`: Replace single `budgetItemId` + `amount` with multi-line editor (add/remove lines, each with budgetItemId + amount + description)
- `EncumbranceDetailPage.tsx`: Show lines table with budget item codes, amounts, liquidated amounts, outstanding amounts
- Remove `AppropriationId` from all encumbrance forms

### FR-F008: Update budgets pages

**Type**: Cleanup

- Remove `totalAmount` display and form fields from `BudgetsListPage.tsx` and create/edit forms
- Budget total is now computed from transactions (display only, not editable)

### FR-F009: Update funds pages

**Type**: Cleanup

- Remove `fiscalYearId` from `FundsListPage.tsx` and create/edit forms
- Fund is now standalone; fiscal year is on Budget

### FR-F010: Update budget items pages

**Type**: Cleanup

- Remove `fundId` from `BudgetItemsListPage.tsx` and create/edit forms
- Fund comes from Budget transitively

### FR-F011: Verify budget-transactions pages alignment

**Type**: Verification

- Ensure `BudgetTransactionsListPage.tsx` columns match new DTOs
- Ensure `CreateBudgetTransactionPage.tsx` form uses correct field names
- Ensure `BudgetTransactionDetailPage.tsx` displays lines correctly
- Verify all lifecycle action buttons work (submit, approve, post, cancel, reverse)

---

## Edge Cases

- What if NSwag regeneration produces different field names than expected? → Verify against actual generated client after `npm run generate-api`.
- What if encumbrance lines page needs to show real-time availability per line? → Use `BudgetAvailabilitySummaryDto` per item, not per encumbrance.
- What if monthly plan page shows 12 month columns but backend uses fiscal period IDs? → Map fiscal period IDs to display names (e.g., "يناير", "فبراير", etc.).

---

## Success Criteria

- **SC-F001**: `npm run lint` passes with zero errors
- **SC-F002**: `npm run build` succeeds with zero errors
- **SC-F003**: No references to `Appropriation`, `AppropriationType`, `AppropriationStatus`, `AppropriationDto` in frontend code
- **SC-F004**: No references to `totalAmount` in BudgetDto or budget forms
- **SC-F005**: No references to `fundId` in BudgetItemDto or item forms
- **SC-F006**: No references to `fiscalYearId` in FundDto or fund forms
- **SC-F007**: No references to `appropriationId` in encumbrance hooks or pages
- **SC-F008**: BudgetTransactions pages display correctly with lines
- **SC-F009**: Encumbrances pages support multi-line creation and display
- **SC-F010**: All Arabic labels render correctly in RTL

---

## Assumptions

- Backend API endpoints are stable and will not change during frontend alignment
- NSwag regeneration will produce correct TypeScript types matching backend DTOs
- No new backend endpoints are needed — all required endpoints already exist
- Frontend remains Arabic-only with RTL layout
- No test files will be created (governance decision)

---

## Dependencies

1. Backend build must succeed (verified: DONE)
2. `npm run generate-api` must run after backend is ready to regenerate NSwag client
3. `npm run lint` must pass after all changes
4. `npm run build` must succeed after all changes

---

## Execution Order

1. Run `npm run generate-api` to regenerate NSwag client from backend
2. Update `shared/types.ts` — remove old enums/DTOs, add new ones
3. Update `shared/client.ts` — remove stale client methods, add BudgetTransactions client
4. Update `hooks/useEncumbrances.ts` — remove appropriationId
5. Update `hooks/useBudgets.ts` — remove totalAmount
6. Update `hooks/useBudgetItems.ts` — remove fundId
7. Update `hooks/useMonthlyPlan.ts` — change month to fiscalPeriodId
8. Update `encumbrances/pages/` — add lines support
9. Update `budgets/pages/` — remove totalAmount
10. Update `funds/pages/` — remove fiscalYearId
11. Update `budgets-items/pages/` — remove fundId
12. Verify `budget-transactions/pages/` alignment
13. Run `npm run lint` — fix any errors
14. Run `npm run build` — verify success
