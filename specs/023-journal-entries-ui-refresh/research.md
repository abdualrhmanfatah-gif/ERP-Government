# Research: Journal Entries UI Refresh

**Feature**: 023-journal-entries-ui-refresh
**Date**: 2026-09-06

## Decision Log

### 1. API Client Strategy

**Decision**: Use NSwag-generated `JournalEntriesClient` from `web-api-client.ts` instead of the hand-written `movesClient`/`moveLinesClient`.

**Rationale**: The backend already exposes `/api/JournalEntries` with full CRUD + lifecycle endpoints. The NSwag client is auto-generated from OpenAPI and stays in sync with the backend contract. The old hand-written client targets `/api/Moves` which is the broken legacy endpoint.

**Alternatives considered**:
- Keep hand-written client and retarget to `/api/JournalEntries` — rejected because NSwag already provides typed client with all operations
- Create a new manual fetch wrapper — rejected per constitution principle IX (frontend MUST conform to generated contract)

**Action**: Delete `features/accounting/client.ts` (movesClient/moveLinesClient). Import `JournalEntriesClient` from `web-api-client.ts` in new hooks.

### 2. Shared Component Reuse

**Decision**: Import and reuse `LifecycleActions`, `ApprovalsPanel`, `StatusLogPanel` from existing features.

**Rationale**: These components are already built and tested in budgeting/documents features. Reusing them avoids duplication (constitution principle X) and ensures consistent UX across all document types.

**Alternatives considered**:
- Build new lifecycle panel in accounting — rejected per principle X (per-feature duplicates of shared primitives prohibited)
- Inline lifecycle actions as current MoveDetail does — rejected because it bypasses the tested shared component

**Action**: Import from:
- `features/budgeting/components/LifecycleActions.tsx`
- `features/documents/components/ApprovalsPanel.tsx`
- `features/documents/components/StatusLogPanel.tsx`

### 3. Dimension Picker Pattern

**Decision**: Create a single `DimensionPickers` component that renders inline `<Select>` elements populated from existing hooks.

**Rationale**: The codebase already uses this pattern (inline Select + hook data) for accounts, currencies, cost centers in MoveCreatePage. No standalone picker components exist. Creating one reusable component for all five dimension fields avoids duplication.

**Alternatives considered**:
- Five separate picker components — rejected as over-engineering for simple Select wrappers
- Autocomplete/search pickers — deferred to future enhancement; current pattern uses dropdown Select

**Action**: Create `DimensionPickers.tsx` with props for which dimensions to show, using hooks:
- `useFundsList()` from `features/budgeting/hooks/useFunds.ts`
- `useProjects()` from `features/organization/hooks/useProjects.ts`
- `useBudgetItemsTree()` from `features/budgeting/hooks/useBudgetItems.ts`
- Encumbrance and PaymentOrder pickers need hooks — research if they exist

### 4. Encumbrance and PaymentOrder Dimension Hooks

**Decision**: Check if hooks exist; if not, create minimal hooks following the existing pattern.

**Rationale**: The journal entry lines have `EncumbranceId` and `PaymentOrderId` FKs. The UI must provide pickers for these dimensions.

**Alternatives considered**:
- Skip these pickers — rejected per spec FR-007 (system MUST support five analytic dimension pickers)
- Use generic reference data hook — need to verify if EncumbranceClient/PaymentOrdersClient exist in NSwag

**Action**: Verify in web-api-client.ts for `EncumbrancesClient` and `PaymentOrdersClient` classes. Create hooks if missing.

### 5. Route Structure

**Decision**: Keep existing route paths (`/accounting/journal-entries/*`) and swap component imports in `routes.tsx`.

**Rationale**: The user explicitly stated "paths already /accounting/journal-entries — keep paths, swap components." This avoids breaking any existing bookmarks or navigation state.

**Alternatives considered**:
- Create new routes — rejected per user instruction
- Redirect old routes — unnecessary since paths already correct

**Action**: In `routes.tsx`, change:
- `MovesListPage` → `JournalEntriesListPage`
- `MoveCreatePage` → `JournalEntryCreatePage`
- `MoveDetailPage` → `JournalEntryDetailPage`

### 6. Status Badge Mapping

**Decision**: Create a `StatusBadge` component that maps `EntryStatus` enum values to distinct visual treatments (color + label).

**Rationale**: The spec requires distinct visual treatment for each status (FR-002). A dedicated component ensures consistency and easy maintenance.

**Alternatives considered**:
- Inline badge logic in grid — rejected because it would be duplicated across list and detail views
- Use shared badge from design system — check if one exists; if not, create in accounting feature

**Action**: Create `StatusBadge.tsx` with color mapping:
- Draft → gray
- Submitted → blue
- Approved → yellow/amber
- Posted → green
- Reversed → orange
- Cancelled → red

### 7. Balance Guard Implementation

**Decision**: Client-side balance validation as UX feedback; server enforces at post time.

**Rationale**: The constitution (principle III) requires server-side enforcement. The frontend balance guard is UX only — it prevents submitting unbalanced entries but the server re-validates. This matches the existing `BalanceIndicator.tsx` pattern.

**Alternatives considered**:
- Client-only validation — rejected because server must enforce (principle IV)
- No client validation — rejected because it degrades UX (user discovers imbalance only after server round-trip)

**Action**: In `JournalEntryCreatePage`, compute running totals from lines array. Show `BalanceIndicator` with debit/credit totals. Disable submit button when totals don't match. Server re-validates on POST.

### 8. Reversal Link Display

**Decision**: Show reversal link on detail view when `ReversalOfId` is set (original → reversal link) or when entry has reversals (reversal → original link).

**Rationale**: The spec requires bidirectional links (FR-012). The `JournalEntryDto` already includes `ReversalOfId` and `ReversalReason`. The detail query should also return linked reversal entries.

**Alternatives considered**:
- Separate API call for linked entries — adds latency; prefer embedding in detail query response
- No link on list view — acceptable; links only on detail view per spec

**Action**: Verify if `GetJournalEntryByIdQuery` returns linked entry summary. If not, extend DTO or make a secondary fetch.

## Open Questions (Resolved)

| Question | Resolution |
|---|---|
| Does JournalEntriesClient exist in NSwag? | Yes — `JournalEntriesClient` class at line 18107 of web-api-client.ts |
| Do EncumbranceClient/PaymentOrdersClient exist? | Need to verify — will check during implementation |
| Is there a shared StatusBadge component? | No — create new one in accounting feature |
| Does detail query return linked reversal entry? | Need to verify — will check during implementation |
