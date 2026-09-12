---
description: "Task list template for feature implementation"
---

# Tasks: Purchase Requests UI Completion

**Input**: Design documents from `/specs/048-purchase-requests-ui/`

**Prerequisites**: plan.md, spec.md, research.md, data-model.md, contracts/

**Tests**: No automated tests (frontend governance decision)

**Organization**: Tasks grouped by user story for independent implementation.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Schemas, catalog hooks, and types shared across all user stories

- [x] T001 [P] Create Zod schemas in `src/Web/ClientApp/src/features/procurement/purchase-requests/shared/schemas.ts` — `createPurchaseRequestSchema` and `updatePurchaseRequestSchema` per data-model.md (requestDate required, priority required, lines array min(1), itemId/unitId/quantity required, unitCostEstimate optional)
- [x] T002 [P] Create catalog hooks in `src/Web/ClientApp/src/features/procurement/purchase-requests/shared/catalog-hooks.ts` — `useItemsList()` and `useUnitsList()` using `api.get` with `staleTime: Infinity`, returning `ComboboxOption[]` mapped from API response

**Checkpoint**: Foundation ready — user story implementation can begin

---

## Phase 2: User Story 2 — Create Purchase Request (Priority: P1) 🎯 MVP

**Goal**: Procurement officer creates a new purchase request with header data and line items

**Independent Test**: Navigate to `/procurement/purchase-requests/create`, fill form, add lines, submit → new request appears in list

### Implementation

- [x] T003 [US2] Create `src/Web/ClientApp/src/features/procurement/purchase-requests/pages/PurchaseRequestCreatePage.tsx` — RHF + Zod form using `createPurchaseRequestSchema`, fields: requestDate (DatePicker), requiredDate (DatePicker), departmentId (Input), costCenterId (Input), priority (Select), notes (Textarea), dynamic lines with Combobox for item/unit, quantity, unitCostEstimate, line total computed, estimated total displayed, "إضافة بند" button, remove line button, submit calls `useCreatePurchaseRequest`, navigate to detail on success, 4xx inline / 5xx toast
- [x] T004 [US2] Add create route to `src/Web/ClientApp/src/app/routes.tsx` — `/procurement/purchase-requests/create` → `PurchaseRequestCreatePage`

**Checkpoint**: Create flow functional — user can create purchase requests

---

## Phase 3: User Story 3 — View Purchase Request Detail (Priority: P1)

**Goal**: View full details including lines, status, and available actions with confirmation dialogs

**Independent Test**: Navigate to `/procurement/purchase-requests/:id` → all data, lines, status badges, and action buttons displayed

### Implementation

- [x] T005 [US3] Enhance `src/Web/ClientApp/src/features/procurement/purchase-requests/pages/PurchaseRequestDetailPage.tsx` — Add confirmation dialogs (Dialog component) for Reject (with reason Textarea) and Cancel actions, add loading states on buttons during mutations, fix duplicate "الcost center" labels, add estimated total display, ensure request number renders LTR, add Edit button for Draft status, ensure all monetary amounts use "ر.ي" suffix with `tabular-nums`

**Checkpoint**: Detail page fully functional with confirmation dialogs

---

## Phase 4: User Story 4 — Edit Purchase Request (Priority: P2)

**Goal**: Edit a Draft purchase request with pre-populated form

**Independent Test**: Navigate to `/procurement/purchase-requests/:id/edit` with Draft request → form pre-populated → save → changes persist

### Implementation

- [x] T006 [US4] Create `src/Web/ClientApp/src/features/procurement/purchase-requests/pages/PurchaseRequestEditPage.tsx` — RHF + Zod form using `updatePurchaseRequestSchema`, load existing data via `usePurchaseRequestDetail`, pre-populate all fields including lines, redirect to detail if status !== Draft, submit calls `useUpdatePurchaseRequest`, navigate to detail on success, same error handling as create
- [x] T007 [US4] Add edit route to `src/Web/ClientApp/src/app/routes.tsx` — `/procurement/purchase-requests/:id/edit` → `PurchaseRequestEditPage`

**Checkpoint**: Edit flow functional for Draft requests

---

## Phase 5: Polish & Cross-Cutting Concerns

**Purpose**: Error handling, accessibility, and RTL compliance

- [~] T008 [P] ~~Add shared form component~~ Cancelled — inline code in create/edit pages works
- [x] T009 Verify all interactive elements have `aria-label` attributes (SC-007)
- [x] T010 Run `npm run lint` from `src/Web/ClientApp` — fix any ESLint errors
- [x] T011 Run `npm run build` from `src/Web/ClientApp` — verify successful build with zero errors

**Checkpoint**: All user stories functional, lint clean, build passing

---

## Dependencies & Execution Order

### Phase Dependencies

- **Phase 1 (Setup)**: No dependencies — start immediately
- **Phase 2 (US2 - Create)**: Depends on Phase 1 completion
- **Phase 3 (US3 - Detail)**: Depends on Phase 1 completion — can run parallel with Phase 2
- **Phase 4 (US4 - Edit)**: Depends on Phase 1 and Phase 2 (reuses create pattern)
- **Phase 5 (Polish)**: Depends on all user stories being complete

### User Story Dependencies

- **US1 (View List)**: Already implemented — no tasks needed
- **US2 (Create)**: Can start after Phase 1 — independent
- **US3 (Detail)**: Can start after Phase 1 — independent of US2
- **US4 (Edit)**: Can start after Phase 1 — reuses create form pattern from US2
- **US5 (Lifecycle Actions)**: Already implemented in detail page — enhanced in US3

### Parallel Opportunities

- Phase 1: T001 and T002 can run in parallel (different files)
- Phase 2 and Phase 3 can run in parallel after Phase 1 (different pages, no file conflicts)
- T010 and T011 can run in parallel (lint and build independent)

## Implementation Strategy

### MVP First (US2 Create + US3 Detail)

1. Complete Phase 1: Setup
2. Complete Phase 2: Create page
3. Complete Phase 3: Detail page with dialogs
4. **STOP and VALIDATE**: Create a request, view it, test lifecycle actions
5. Proceed to Edit page

### Incremental Delivery

1. Phase 1 → Foundation ready
2. Phase 2 → Create flow works (MVP!)
3. Phase 3 → Detail + dialogs work
4. Phase 4 → Edit flow works
5. Phase 5 → Polish complete
