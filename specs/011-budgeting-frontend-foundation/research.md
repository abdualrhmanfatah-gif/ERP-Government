# Research: Budgeting Frontend Foundation #1 of 5

**Feature**: `011-budgeting-frontend-foundation` | **Date**: 2026-09-04

## Research Tasks

No NEEDS CLARIFICATION items remain — all technical context is resolved. Research documents design decisions and best practices for key integration points.

---

### R1: Shared Types Architecture

**Decision**: Single `types.ts` file under `features/budgeting/shared/` containing all DTOs, enums, and Arabic label maps for all 7 budgeting entities.

**Rationale**: The shared types layer is consumed by all 5 frontend specs. A single file prevents import path proliferation, ensures one source of truth, and makes field-by-field comparison against the backend contract straightforward. The file will be ~200-300 lines (8 DTOs + 8 enums + 8 label maps), which is manageable in a single module.

**Alternatives considered**:
- Per-entity type files (e.g., `types/budget-type.ts`): Rejected — adds import path complexity for 5 specs that all need overlapping types.
- Re-export from NSwag auto-generated `web-api-client.ts`: Rejected — the spec explicitly calls for hand-written types matching the backend contract exactly; NSwag types may drift or include unwanted patterns.

---

### R2: API Client Pattern

**Decision**: Hand-written fetch-based client (Pattern B from codebase exploration) with typed functions per endpoint group and a cache key factory.

**Rationale**: Pattern B is used by organization and notifications features. It provides full control over error surfacing (ProblemDetails), request/response typing, and cache key management. The existing `auth-fetch.ts` wrapper handles Bearer token injection globally via `patch-fetch.ts`, so the client functions can use plain `fetch`.

**Alternatives considered**:
- NSwag auto-generated client (Pattern A): Rejected — the spec requires hand-written types as the single source of truth; NSwag generates from OpenAPI at build time and may not match the exact DTO shapes specified.
- React Query's `queryFn` inline: Rejected — the client module should be testable independently of React; hooks wrap the client functions.

---

### R3: Cache Key Factory Design

**Decision**: A `budgetingKeys` object with a `scope` prefix (`['budgeting']`) and per-entity generators (e.g., `budgetingKeys.budgetTypes.all()`, `budgetingKeys.budgetTypes.detail(id)`).

**Rationale**: TanStack Query v5 uses array-based keys for hierarchical cache invalidation. A scoped prefix allows invalidating all budgeting queries with `invalidateQueries({ queryKey: ['budgeting'] })`. Per-entity generators ensure stable, deterministic keys for individual entity caches.

**Pattern**:
```ts
export const budgetingKeys = {
  all: ['budgeting'] as const,
  budgetTypes: {
    all: [...budgetingKeys.all, 'budget-types'] as const,
    list: (filters?: BudgetTypeFilters) => [...budgetingKeys.budgetTypes.all, 'list', filters] as const,
    detail: (id: number) => [...budgetingKeys.budgetTypes.all, 'detail', id] as const,
  },
  funds: { /* same pattern */ },
  // ... per entity
};
```

---

### R4: Client-Side Filtering Strategy

**Decision**: Client-side filtering using TanStack Table's built-in filter functions, applied on the full dataset loaded from the server.

**Rationale**: Budget types and funds are bounded reference data (typically <500 records). Client-side filtering provides instant feedback with zero additional server requests. TanStack Table v9 (already a dependency) provides column-level filtering, global search, and faceted filter support out of the box.

**Filter architecture**:
- **Search**: Global filter on the table instance, matching across Name/Code/Number fields
- **Enum filters** (ControlMethod, FundType, FundCategory): Column-level faceted filters with `FilterSelect` component
- **IsActive**: Column-level filter with `FilterToggle` or `FilterSelect`
- Server still returns full dataset (GET `/api/BudgetTypes`, GET `/api/Funds` with optional `?isActive`)

---

### R5: Dialog-Based Edit Pattern

**Decision**: Create and edit forms use the same `Dialog` component, toggled between create/edit mode via a prop. The dialog opens as an overlay on the list page.

**Rationale**: Clarified in spec session — dialog overlay keeps the user on the list, is faster for reference data, and avoids route proliferation. The existing `Dialog` component (native `<dialog>` modal) supports this pattern.

**State management**:
- `dialogOpen: boolean` — controls dialog visibility
- `editingItem: Dto | null` — null = create mode, populated = edit mode
- `handleSubmit` — calls create or update mutation based on `editingItem`
- `handleClose` — resets form state and closes dialog

---

### R6: Funds Detail View Route

**Decision**: Separate route at `/budgeting/funds/:id` with a dedicated `FundDetailPage` component.

**Rationale**: Clarified in spec session — funds have 10+ fields that don't fit in an expandable row. A dedicated route supports direct linking, bookmarking, and browser history.

**Route registration**: Added to `AppRoutes` array alongside the list route. Uses `useParams()` for ID extraction.

---

### R7: AvailabilityIndicator State Machine

**Decision**: Four visual states determined by `(available, controlMethod)`:

| available >= 0 | Any controlMethod | → Green |
| available < 0 | ControlMethod None | → Green |
| available < 0 | ControlMethod Warning | → Amber |
| available < 0 | ControlMethod Blocking | → Red |
| Empty response | Any | → Zero-state |

**Rationale**: Clarified in spec session — None means no control enforced, so green is correct. The component fetches from `GET /api/Encumbrances/availability?appropriationId={id}` and derives the visual state from the response.

---

### R8: Permission Gating Strategy

**Decision**: Use existing `usePermission()` hook (currently stub, always-grants) to gate UI actions. Sidebar items use the `permission` field on `NavItem` for future gating.

**Rationale**: The stub implementation is acceptable per spec assumption and will be replaced by real RBAC wiring in a future initiative. The pattern is already established across the codebase. The `BUDGET_PERMISSIONS` constants already define the exact permission strings needed.

**Permission strings used**:
- `BudgetTypes.View`, `BudgetTypes.Create`, `BudgetTypes.Update`
- `Funds.View`, `Funds.Create`, `Funds.Update`, `Funds.Activate`, `Funds.Deactivate`

---

### R9: RTL and Dark Mode Compliance

**Decision**: Use logical CSS properties (start/end) and Tailwind's dark mode utilities. All colors from design tokens via CSS variables.

**Rationale**: Constitution Principle X mandates RTL Arabic-first and design token compliance. The existing `tailwind.config.js` imports all tokens from `tokens.tailwind.json`. Dark mode uses class-based toggling (`darkMode: ['class']`).

**Key patterns**:
- `ps-3` (padding-inline-start) not `pl-3`
- `text-start` not `text-left`
- `bg-[var(--color-surface)]` for token-driven colors
- `dark:` prefix for dark mode overrides (handled by CSS variable swapping)

---

### R10: RowVersion Conflict Handling

**Decision**: Every mutating command sends `rowVersion` in the request body. On 409 conflict response, show a sonner toast notification and refetch the list query.

**Rationale**: Constitution Principle VI requires optimistic concurrency. The backend returns 409-style ProblemDetails on RowVersion mismatch. The frontend cannot resolve the conflict — it must refetch and let the user see current data.

**Error handling pattern**:
```ts
onError: (error) => {
  if (isProblemDetails(error) && error.status === 409) {
    showToast({ title: 'تعارض البيانات', description: 'تم تعديل السجل من مستخدم آخر', variant: 'warning' });
    queryClient.invalidateQueries({ queryKey: [...] });
  }
}
```
