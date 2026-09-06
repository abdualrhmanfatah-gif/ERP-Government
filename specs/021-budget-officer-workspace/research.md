# Research: Budget Officer Workspace

**Feature**: 021-budget-officer-workspace
**Date**: 2026-09-06

## R1: Budget CRUD Pages — Pattern from Existing Reference Data Pages

**Decision**: Follow the same pattern as `FundsListPage` / `FundDetailPage` for list and detail pages.

**Rationale**: The existing fund pages demonstrate the established conventions: `useQuery` hooks, React Router `useParams`, Arabic labels, design-token-driven styling, `Badge` for status, `Button` for navigation. Reusing this pattern ensures consistency.

**Alternatives considered**:
- Dedicated table component with server-side pagination — rejected because budgets are low-volume (typically <50 per fiscal year) and client-side filtering suffices.
- Modal-based create — rejected because budget creation includes a multi-field form that benefits from a full page.

## R2: BudgetItem Tree — Reuse Existing Component

**Decision**: Reuse the existing `BudgetItemTree` component from `components/BudgetItemTree.tsx` as-is. Add a wrapper page that fetches the tree data and passes it to the component.

**Rationale**: The tree component already handles expand/collapse, keyboard navigation, empty states, and RTL layout. It was built for exactly this use case (per spec 013 tests). No modifications needed.

**Alternatives considered**:
- Build a new drag-and-drop tree — rejected because the existing component already supports the required interactions and the spec says drag is "Draft only" (can be added incrementally).

## R3: Availability Indicator — Live Fetch Strategy

**Decision**: Use the existing `AvailabilityIndicator` component in "fetch" mode. Pass `appropriationId` or `budgetItemId` as props. The component already handles loading states and error degradation.

**Rationale**: The component at `components/AvailabilityIndicator.tsx` already implements the exact pattern needed: it fetches from the appropriate availability endpoint, displays green/amber/red tones based on `BudgetControlMethod`, and handles loading/error states. The spec's "live fetch" requirement is met by React Query's automatic refetching when query keys change.

**Alternatives considered**:
- Custom debounced fetch — rejected because the existing component uses React Query which handles caching and deduplication efficiently. No need for custom debounce.

## R4: Monthly Plan — LocalStorage Persistence

**Decision**: Create a `useMonthlyPlan(budgetItemId)` hook that reads/writes to `localStorage` with key pattern `monthly-plan-{budgetItemId}`. The hook returns `{ plan, setMonth, save, total, variance }`.

**Rationale**: The spec clarifies that monthly plans are client-side only. localStorage is the simplest persistence mechanism that survives page reloads. The hook abstracts the storage concern away from the UI component.

**Alternatives considered**:
- In-memory state (useState) — rejected because the spec requires persistence across page reloads.
- IndexedDB — rejected as overkill for 12 number fields per item.

## R5: Transfer Target Selector — Filtering Logic

**Decision**: When `AppropriationType` is Transfer, fetch the budget's item tree via `budgetsClient.getItemsTree(budgetId)`, flatten it, and filter out: (a) the source item, (b) items with status Closed or Cancelled. Display as a searchable dropdown using the existing `useBudgetItems` hook pattern.

**Rationale**: The transfer target must be from the same budget. The item tree endpoint already returns all items. Client-side filtering is acceptable because budgets have <100 items typically.

**Alternatives considered**:
- Server-side filter endpoint — rejected because no backend changes are in scope.

## R6: Execution Drill-Down — Expandable Panel Pattern

**Decision**: Implement as a set of nested expandable sections within `BudgetDetailPage`. Click an item row → expand to show its appropriations. Click an appropriation → expand to show its encumbrances. Use CSS transitions for expand/collapse animation.

**Rationale**: The spec clarifies this should be expandable panels within the budget detail page (not a separate page). This matches the existing pattern of inline expansion used in tree views.

**Alternatives considered**:
- Dedicated drill-down page with breadcrumbs — rejected per spec clarification.
- Accordion component — rejected because nested accordions have accessibility issues; simple expand/collapse with `aria-expanded` is more appropriate.

## R7: Lifecycle Actions — Reuse Existing Component

**Decision**: Use the existing `LifecycleActions` component for all lifecycle transitions (budget, appropriation, encumbrance). Define action arrays per status per entity type.

**Rationale**: The component already handles permission gating via `can(permission)`, confirmation dialogs, and pending state. It's the established pattern for lifecycle buttons.

**Alternatives considered**:
- Individual buttons per action — rejected because `LifecycleActions` centralizes the pattern and reduces duplication.

## R8: Form Patterns — React Hook Form vs Plain Forms

**Decision**: Use plain controlled forms with `useState` for form fields, matching the existing pattern in `FundDetailPage` and the budgeting hooks. No form library is currently in use in the budgeting feature.

**Rationale**: The existing codebase doesn't use React Hook Form or similar. Introducing a form library for this feature would break consistency. The forms are simple enough (5-8 fields) that controlled inputs suffice.

**Alternatives considered**:
- React Hook Form — rejected because it's not established in the codebase and the forms don't need complex validation beyond required fields.
