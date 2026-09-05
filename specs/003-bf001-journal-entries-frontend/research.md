# Research: BF-001 Journal Entries Frontend

**Date**: 2026-09-02
**Feature**: BF-001 Journal Entries Frontend
**Spec**: [spec.md](./spec.md)

## Research Tasks

### R1: DataGrid Client-Side Pagination Pattern

**Decision**: Use DataGrid component with client-side pagination via `@tanstack/react-table`.

**Rationale**: The API (`/api/Moves`) returns a flat list without skip/take parameters. Client-side pagination works with current API. Server-side can be added later when API is extended.

**Alternatives Considered**:
- Server-side pagination: Requires API changes (out of scope)
- Infinite scroll: Poor UX for government users who need to jump to specific pages

**Evidence**: DataGrid component at `src/components/ui/DataGrid.tsx` supports `pagination`, `totalItems`, `onPageChange`, `onPageSizeChange` props. Already used by other list pages.

### R2: URL Search Params for Filter State

**Decision**: Use `URLSearchParams` + `useSearchParams` from react-router-dom for filter state persistence.

**Rationale**: Enables bookmarkable filtered views, correct back-button behavior, and shareable links. Matches existing pattern in fiscal-years list page.

**Alternatives Considered**:
- Component state only: Loses filters on refresh, no back-button support
- Redux/Context: Overkill for URL-synced filter state

**Evidence**: `react-router-dom` 7.6.1 installed. `useSearchParams` available. Fiscal years list page already uses URL params pattern.

### R3: React Hook Form for Header Form

**Decision**: Use RHF + Zod for MoveCreatePage header form (Document Date, Journal, Entry Type, Reference, Narration). Manual `useState` for lines editor.

**Rationale**: Hybrid approach matches Decision Q3. Header form benefits from RHF validation, error display, and form reset. Lines editor needs flexibility for inline editing and dynamic add/remove.

**Alternatives Considered**:
- Full RHF: Lines editor with RHF is complex (dynamic array, nested fields)
- Full manual: Inconsistent with codebase conventions

**Evidence**: MoveForm.tsx exists with RHF/Zod but is not used by MoveCreatePage. All other forms in codebase use RHF/Zod.

### R4: Fiscal Year Auto-Detection Pattern

**Decision**: Create `useFiscalYearByDate` hook using React Query to call `/api/FiscalYears/by-date?date=`.

**Rationale**: Currently uses raw `fetch` in MoveCreatePage. Migrating to React Query provides caching, error handling, and loading states consistent with the rest of the codebase.

**Alternatives Considered**:
- Keep raw fetch: Inconsistent, no caching
- Include in header form state: Couples FY detection to form state

**Evidence**: `/api/FiscalYears/by-date` endpoint verified at `src/Web/Endpoints/FinancialSettings/FiscalYears.cs`. Returns `GetFiscalYearPeriodByDateResult` with `FiscalYearId`, `FiscalPeriodId`.

### R5: Exchange Rate Auto-Fetch Pattern

**Decision**: Create `useExchangeRateLookup` hook using React Query to call `/api/ExchangeRates/lookup`.

**Rationale**: Currently uses raw `fetch` in MoveCreatePage and MoveLinesEditor. Migrating to React Query provides caching and consistency.

**Alternatives Considered**:
- Keep raw fetch: Inconsistent, no caching
- Include in currency selection: Couples rate fetch to selection

**Evidence**: `/api/ExchangeRates/lookup` endpoint verified. Returns `ExchangeRateLookupDto` with `Rate`, `RateDate`, `RateType`.

### R6: Confirmation Dialog Pattern

**Decision**: Use existing `ConfirmDialog` component for Cancel and Post actions. Existing `ReverseDialog` for Reverse.

**Rationale**: ConfirmDialog already exists in shared UI library. ReverseDialog already exists in accounting components. No new dialog components needed.

**Alternatives Considered**:
- Native `window.confirm`: Inconsistent with design system
- New custom dialog: Unnecessary when ConfirmDialog exists

**Evidence**: `src/components/ui/ConfirmDialog.tsx` exists with `open`, `onClose`, `onConfirm`, `message`, `title`, `confirmLabel`, `destructive`, `loading` props.

### R7: Balance Indicator Placement

**Decision**: Place BalanceIndicator below the lines table in MoveCreatePage and MoveDetailPage (Draft mode).

**Rationale**: Visual proximity to the lines table. Users see the balance immediately after reviewing lines. Standard accounting software pattern.

**Alternatives Considered**:
- Above the table: Less intuitive — balance depends on lines
- In the submit button: Hides the actual numbers

**Evidence**: No existing BalanceIndicator component. Must be created. Current MoveCreatePage shows totals informally in the submit button text.

### R8: Testing Strategy

**Decision**: Unit tests with Vitest + React Testing Library for all new/modified components.

**Rationale**: Constitution Principle XI requires frontend tests. Vitest + RTL is the configured testing framework.

**Alternatives Considered**:
- Cypress/Playwright: Browser-level tests — deferred to future (Constitution says "critical user journeys")
- No tests: Violates Constitution

**Evidence**: `vitest` 4.1.11 configured in `package.json`. `@testing-library/react` available. `jsdom` 29.1.1 available.

## Summary

| Decision | Choice | Risk |
|----------|--------|------|
| Pagination | Client-side | Low — may be slow with 1000+ entries |
| Filter state | URL search params | Low — well-established pattern |
| Header form | RHF + Zod | Low — consistent with codebase |
| Lines editor | Manual useState | Low — flexible for inline editing |
| FY auto-detect | React Query hook | Low — replaces raw fetch |
| Exchange rate | React Query hook | Low — replaces raw fetch |
| Confirmation dialogs | ConfirmDialog + ReverseDialog | Low — existing components |
| Balance indicator | New component below lines table | Low — standard pattern |
| Testing | Vitest + RTL | Low — configured framework |
