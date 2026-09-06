# Research: Financial Settings Administration UI

**Feature**: 024-financial-settings-admin
**Date**: 2026-09-06

## Research Questions

### R1: Frontend API client pattern

**Decision**: Use hand-written fetch client (`shared/client.ts`) with typed DTOs and cache key factory, following the budgeting/funds pattern.

**Rationale**: The budgeting feature uses two systems — NSwag-generated clients (Budgets, Appropriations, Encumbrances) and hand-written fetch clients (Classifications, BudgetTypes, Funds). The hand-written pattern provides better type safety, explicit API shape knowledge, and aligns with the contract-conformant requirement (Principle IX). For a UI-only feature over existing backend APIs, hand-written clients ensure we mirror the published OpenAPI contract exactly.

**Alternatives considered**:
- NSwag-generated clients: Used by some budgeting hooks. Requires `npm run generate-api` regeneration. Less control over API shape. Rejected for clarity and contract conformance.
- Direct fetch in components: Violates separation of concerns. Rejected.

### R2: Hook pattern for financial settings

**Decision**: TanStack Query `useQuery`/`useMutation` with cache key factory, matching `useBudgets.ts` pattern.

**Rationale**: Established pattern in the codebase. Cache invalidation via `qc.invalidateQueries({ queryKey: [...] })`. Lifecycle transitions (open, close, lock, unlock, activate, deactivate) use the same `useMutation` pattern.

**Alternatives considered**:
- Custom hooks with raw fetch: Reinvents TanStack Query. Rejected.
- Context-based state: Not needed for server-state management. Rejected.

### R3: Page layout pattern

**Decision**: Follow budgeting list/detail/create page pattern — header row with title + create button, table with status badges, detail page with info card + lifecycle actions.

**Rationale**: Consistent UX across the admin panel. The budgeting list pages (BudgetsListPage, AppropriationsListPage) provide the exact template for tables, filters, loading states, empty states, and lifecycle action buttons.

**Alternatives considered**:
- Sheet/drawer-based detail: Could work for quick views but fiscal year detail needs full page for periods table. Rejected for v1.
- Tab-based navigation within financial settings: Possible but adds complexity. Keep flat navigation per sub-section.

### R4: Arabic label maps for financial settings enums

**Decision**: Create Arabic label maps in `shared/types.ts` for FiscalYearStatus, FiscalPeriodLockStatus, ExchangeRateType, ClosingEntryStatus, ResetPolicy — following the exact pattern of `budgetStatusLabels`, `appropriationStatusLabels`, etc.

**Rationale**: Arabic-first UI (Principle X). Every status/type must have an Arabic label. The `getLabel()` helper provides fallback for unknown values.

**Alternatives considered**:
- Inline Arabic strings in components: Violates single-source-of-truth. Rejected.
- Backend-provided labels: Adds unnecessary API dependency for static data. Rejected.

### R5: Exchange rate conflict deactivation UX

**Decision**: When activating an exchange rate, the UI sends the activate request. Backend handles conflict deactivation (deactivates overlapping rates). UI refreshes the list to show the new active state.

**Rationale**: Business rule "others in conflict deactivate" is server-side (Principle III). Frontend simply reflects the result. No client-side conflict detection needed.

**Alternatives considered**:
- Client-side conflict preview: Adds complexity for a backend-handled concern. Rejected.
- Confirmation dialog before activation: Could add UX value but not required by spec. Deferred.

### R6: Closing entry proposal review UX

**Decision**: ClosingEntryDetailPage shows a table of closing lines (account, debit, credit, description) with approve/reverse action buttons. Lines are fetched from the backend-generated proposal.

**Rationale**: Spec requires "reviewable proposal with balanced lines before posting." The backend `GenerateYearEndClosingCommand` produces the proposal; the UI displays it for review before approval.

**Alternatives considered**:
- Side-by-side comparison view: Overkill for v1. Lines table is sufficient.
- Editable proposal: Backend generates the proposal; frontend reviews. No editing needed.
