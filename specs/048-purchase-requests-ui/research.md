# Research: Purchase Requests UI Completion

**Date**: 2026-09-11

## R1: Combobox for Item/Unit selection

**Decision**: Use existing `Combobox` component from `src/components/ui/Combobox.tsx`

**Rationale**: Component already exists, supports search, keyboard navigation, loading state, empty state. Takes `ComboboxOption[]` (value/label pairs).

**Implementation**: Fetch item/unit catalogs via dedicated hooks (`useItemsList`, `useUnitsList`) using React Query with `staleTime: Infinity` (catalogs rarely change). Map API response to `ComboboxOption[]`.

**Alternatives considered**:
- shadadcn Combobox (not available) — skipped
- Custom select with search — already built as `Combobox`

## R2: Form validation pattern (RHF + Zod vs manual)

**Decision**: Use React Hook Form + Zod (as specified in constitution and AGENTS.md)

**Rationale**: Constitution requires RHF + Zod for all forms. Existing schemas pattern in `shared/schemas.ts` per feature.

**Implementation**: Create `schemas.ts` with `createPurchaseRequestSchema` and `updatePurchaseRequestSchema`. Use `zodResolver` with RHF `useForm`.

**Alternatives considered**:
- Manual validation (as in EncumbranceCreatePage) — inconsistent with architecture, rejected

## R3: Confirmation dialogs for lifecycle actions

**Decision**: Use existing `Dialog` component from `src/components/ui/Dialog.tsx`

**Rationale**: Already available in shadcn. Supports title, description, confirm/cancel buttons.

**Implementation**: Add `useState` for dialog state per action (reject, cancel). Show dialog with reason input (Textarea) before mutation.

## R4: Error handling pattern

**Decision**: 4xx → inline field errors via Zod `refine` + form `setError`; 5xx → toast via `sonner`

**Rationale**: Per constitution and AGENTS.md: "Server errors: 4xx → inline field error (via Zod refine), 5xx → toast. Never swallow."

**Implementation**: Catch API errors in `onError` callback of `useMutation`. Check `problem.status` — if >= 500, call `toast.error()`. If < 500, set field errors via `setError`.

## R5: Catalog data flow (items + units)

**Decision**: Fetch once on form mount, cache in React Query, lookup by ID for display

**Rationale**: Clarified in session. Avoids repeated API calls.

**Implementation**: New hooks in `shared/catalog-hooks.ts`:
- `useItemsList()` → `api.get('/api/Items')` with `staleTime: Infinity`
- `useUnitsList()` → `api.get('/api/Units')` with `staleTime: Infinity`

## R6: Known bug — self-import in types.ts

**Decision**: Leave as-is (pre-existing, build passes)

**Rationale**: All procurement types files have `import type { X } from './types'` self-imports. TypeScript resolves this. Fixing is out of scope for this feature.

**Alternatives considered**:
- Fix self-imports — would touch multiple files, unrelated to PR UI, deferred
