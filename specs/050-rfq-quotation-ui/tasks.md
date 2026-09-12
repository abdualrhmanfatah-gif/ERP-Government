# Tasks: RFQ & Quotation Management

**Input**: Design documents from `/specs/050-rfq-quotation-ui/`

**Prerequisites**: plan.md, spec.md, research.md, data-model.md, contracts/

**Tests**: Tests are included per constitution (TDD mandatory).

**Organization**: Tasks grouped by user story for independent implementation.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Validators and new commands needed by multiple user stories

- [X] T001 [P] Create CreateRFQCommandValidator in src/Application/Procurement/Commands/RequestForQuotations/CreateRFQ/CreateRFQCommandValidator.cs
- [X] T002 [P] Create CreateQuotationCommandValidator in src/Application/Procurement/Commands/Quotations/CreateQuotation/CreateQuotationCommandValidator.cs
- [X] T003 [P] Create UpdateRFQCommand + handler in src/Application/Procurement/Commands/RequestForQuotations/UpdateRFQ/
- [X] T004 [P] Create UpdateQuotationCommand + handler in src/Application/Procurement/Commands/Quotations/UpdateQuotation/
- [X] T005 [P] Create UpdateRFQCommandValidator in src/Application/Procurement/Commands/RequestForQuotations/UpdateRFQ/UpdateRFQCommandValidator.cs
- [X] T006 [P] Create UpdateQuotationCommandValidator in src/Application/Procurement/Commands/Quotations/UpdateQuotation/UpdateQuotationCommandValidator.cs
- [X] T007 Run dotnet build src/Web/Web.csproj to verify compilation
- [X] T008 Run dotnet test tests/Application.UnitTests to verify no regressions

---

## Phase 2: Foundational (Endpoint Wiring)

**Purpose**: Fix all backend endpoint gaps before frontend work

**CRITICAL**: No frontend work can begin until backend endpoints are functional

### RFQ Endpoint Fixes

- [X] T009 Wire PUT /api/RequestForQuotations/{id} route in src/Web/Endpoints/Procurement/RequestForQuotations.cs using UpdateRFQCommand
- [X] T010 Wire PATCH /api/RequestForQuotations/suppliers/{rfqSupplierId}/response route in src/Web/Endpoints/Procurement/RequestForQuotations.cs using RecordRFQResponseCommand
- [X] T011 Add RecordRFQResponseRequest record (ResponseDate, Notes) in src/Web/Endpoints/Procurement/RequestForQuotations.cs

### Quotation Endpoint Fixes

- [X] T012 Wire GET /api/Quotations route in src/Web/Endpoints/Procurement/Quotations.cs using GetQuotationsQuery
- [X] T013 Fix GET /api/Quotations/{id} in src/Web/Endpoints/Procurement/Quotations.cs to dispatch GetQuotationByIdQuery (replace stub)
- [X] T014 Wire PUT /api/Quotations/{id} route in src/Web/Endpoints/Procurement/Quotations.cs using UpdateQuotationCommand
- [X] T015 Wire PATCH /api/Quotations/{id}/submit route in src/Web/Endpoints/Procurement/Quotations.cs using SubmitQuotationCommand
- [X] T016 Wire PATCH /api/Quotations/{id}/reject route in src/Web/Endpoints/Procurement/Quotations.cs using RejectQuotationCommand
- [X] T017 Add RejectQuotationRequest record (RejectionReason) in src/Web/Endpoints/Procurement/Quotations.cs

### Backend Tests

- [X] T018 [P] Write unit test for CreateRFQCommandValidator in tests/Application.UnitTests/
- [X] T019 [P] Write unit test for CreateQuotationCommandValidator in tests/Application.UnitTests/
- [X] T020 [P] Write unit test for UpdateRFQCommand in tests/Application.UnitTests/
- [X] T021 [P] Write unit test for UpdateQuotationCommand in tests/Application.UnitTests/
- [X] T022 Write functional test for GET /api/Quotations in tests/Application.FunctionalTests/
- [X] T023 Write functional test for GET /api/Quotations/{id} in tests/Application.FunctionalTests/
- [X] T024 Write functional test for PATCH /api/Quotations/{id}/submit in tests/Application.FunctionalTests/
- [X] T025 Write functional test for PATCH /api/Quotations/{id}/reject in tests/Application.FunctionalTests/
- [X] T026 Write functional test for PATCH /api/RequestForQuotations/suppliers/{id}/response in tests/Application.FunctionalTests/
- [X] T027 Run dotnet test tests/Application.UnitTests
- [X] T028 Run dotnet test tests/Application.FunctionalTests
- [X] T029 Run dotnet build src/Web/Web.csproj

**Checkpoint**: All backend endpoints functional — frontend can now be built

---

## Phase 3: User Story 1 - Create and Manage RFQs (Priority: P1) — MVP

**Goal**: Procurement officer can create RFQs from approved PRs, invite suppliers, publish, close, and cancel

**Independent Test**: Create an RFQ, invite suppliers, publish, close, cancel — verify all status transitions

### Frontend: RFQ Shared

- [X] T030 [P] [US1] Create RFQ Zod schemas in src/Web/ClientApp/src/features/procurement/request-for-quotations/shared/schemas.ts
- [X] T031 [P] [US1] Verify/update RFQ types in src/Web/ClientApp/src/features/procurement/request-for-quotations/shared/types.ts

### Frontend: RFQ Pages

- [X] T032 [US1] Create RFQ Create Page in src/Web/ClientApp/src/features/procurement/request-for-quotations/pages/RFQCreatePage.tsx
- [X] T033 [US1] Create RFQ Edit Page in src/Web/ClientApp/src/features/procurement/request-for-quotations/pages/RFQEditPage.tsx

### Frontend: RFQ Hooks

- [X] T034 [US1] Update useRFQs hook in src/Web/ClientApp/src/features/procurement/request-for-quotations/hooks/useRFQs.ts — add create, update, publish, close, cancel mutations

### Frontend: Routes

- [X] T035 [US1] Add RFQ routes in src/Web/ClientApp/src/routes/ — /procurement/rfqs, /procurement/rfqs/create, /procurement/rfqs/:id, /procurement/rfqs/:id/edit

**Checkpoint**: RFQ create, edit, list, detail pages fully functional

---

## Phase 4: User Story 2 - View RFQ Details and Supplier Responses (Priority: P1)

**Goal**: Officer views RFQ detail with invited suppliers, response status, and can record responses

**Independent Test**: Navigate to RFQ detail, verify supplier list, record a supplier response

### Frontend: RFQ Detail

- [X] T036 [US2] Create RFQ Detail Page in src/Web/ClientApp/src/features/procurement/request-for-quotations/pages/RFQDetailPage.tsx
- [X] T037 [US2] Create ProcurementRFQSupplierResponseDialog component in src/Web/ClientApp/src/components/ProcurementRFQSupplierResponseDialog.tsx
- [X] T038 [US2] Add RecordRFQResponse mutation to useRFQs hook in src/Web/ClientApp/src/features/procurement/request-for-quotations/hooks/useRFQs.ts

**Checkpoint**: RFQ detail page shows suppliers, response recording works

---

## Phase 5: User Story 3 - Create and Manage Quotations (Priority: P1)

**Goal**: Officer creates quotations with line items, submits, evaluates, selects, awards

**Independent Test**: Create quotation, add lines, submit, evaluate, select, award — verify all transitions

### Frontend: Quotation Shared

- [X] T039 [P] [US3] Create Quotation types in src/Web/ClientApp/src/features/procurement/quotations/shared/types.ts
- [X] T040 [P] [US3] Create Quotation Zod schemas in src/Web/ClientApp/src/features/procurement/quotations/shared/schemas.ts
- [X] T041 [P] [US3] Create Quotation API client wrapper in src/Web/ClientApp/src/features/procurement/quotations/shared/client.ts

### Frontend: Quotation Hooks

- [X] T042 [US3] Create useQuotations hook in src/Web/ClientApp/src/features/procurement/quotations/hooks/useQuotations.ts — list, detail, create, update, submit, startEvaluation, completeEvaluation, select, award, reject mutations

### Frontend: Quotation Pages

- [X] T043 [US3] Create Quotations List Page in src/Web/ClientApp/src/features/procurement/quotations/pages/QuotationsListPage.tsx
- [X] T044 [US3] Create Quotation Create Page in src/Web/ClientApp/src/features/procurement/quotations/pages/QuotationCreatePage.tsx
- [X] T045 [US3] Create Quotation Edit Page in src/Web/ClientApp/src/features/procurement/quotations/pages/QuotationEditPage.tsx

### Frontend: Routes

- [X] T046 [US3] Add Quotation routes in src/Web/ClientApp/src/routes/ — /procurement/quotations, /procurement/quotations/create, /procurement/quotations/:id, /procurement/quotations/:id/edit

### Frontend: Shared Dialogs

- [X] T047 [US3] Create ProcurementQuotationEvaluationDialog component in src/Web/ClientApp/src/components/ProcurementQuotationEvaluationDialog.tsx (for CompleteEvaluation with scores)
- [X] T048 [US3] Create ProcurementQuotationSelectDialog component in src/Web/ClientApp/src/components/ProcurementQuotationSelectDialog.tsx (for Select with SelectionReason)
- [X] T049 [US3] Create ProcurementQuotationRejectDialog component in src/Web/ClientApp/src/components/ProcurementQuotationRejectDialog.tsx (for Reject with RejectionReason)

**Checkpoint**: Quotation list, create, edit pages functional with full lifecycle

---

## Phase 6: User Story 4 - View Quotation Details (Priority: P1)

**Goal**: Officer views full quotation detail with commercial data, line items, evaluation scores

**Independent Test**: Navigate to quotation detail, verify all fields, verify action buttons per status

### Frontend: Quotation Detail

- [X] T050 [US4] Create Quotation Detail Page in src/Web/ClientApp/src/features/procurement/quotations/pages/QuotationDetailPage.tsx
- [X] T051 [US4] Integrate evaluation/select/reject dialogs with detail page action buttons

**Checkpoint**: Quotation detail page shows all data with correct actions per status

---

## Phase 7: User Story 5 - Edit Draft Quotations (Priority: P2)

**Goal**: Officer can edit draft quotations with pre-populated form

**Independent Test**: Create quotation (Draft), edit line items and terms, save, verify changes

(Already covered by T045 — QuotationEditPage. Verify edit form loads pre-populated values and saves correctly.)

- [X] T052 [US5] Verify Quotation Edit Page loads pre-populated data and saves via UpdateQuotation endpoint

**Checkpoint**: Draft quotation editing works end-to-end

---

## Phase 8: Polish & Cross-Cutting Concerns

**Purpose**: Final quality pass across all user stories

- [X] T053 [P] Verify all RFQ pages show skeleton loading states during data fetch
- [X] T054 [P] Verify all Quotation pages show skeleton loading states during data fetch
- [X] T055 [P] Verify all list pages show empty state when no results match filters
- [X] T056 [P] Verify all action buttons disabled during mutations
- [X] T057 [P] Verify 4xx errors display inline, 5xx errors display as toast
- [X] T058 [P] Verify RFQNumber and QuotationNumber render with dir="ltr"
- [X] T059 [P] Verify all monetary amounts use tabular-nums
- [X] T060 [P] Verify no CSS physical properties (ml/mr/pl/pr) in RFQ/quotation code — use ms/me/ps/pe
- [X] T061 [P] Verify all interactive elements have aria-label attributes
- [X] T062 [P] Verify Arabic-only text throughout, no English strings in UI
- [X] T063 Run npm run lint from src/Web/ClientApp
- [X] T064 Run npm run build from src/Web/ClientApp
- [X] T065 Run dotnet build src/Web/Web.csproj
- [X] T066 Run dotnet test tests/Application.UnitTests
- [X] T067 Run dotnet test tests/Application.FunctionalTests

---

## Dependencies & Execution Order

### Phase Dependencies

- **Phase 1 (Setup)**: No dependencies — can start immediately
- **Phase 2 (Foundational)**: Depends on Phase 1 (validators + new commands must exist before endpoint wiring)
- **Phase 3 (US1 - RFQ Create/Manage)**: Depends on Phase 2 (backend endpoints ready)
- **Phase 4 (US2 - RFQ Detail)**: Depends on Phase 3 (RFQ pages and hooks exist)
- **Phase 5 (US3 - Quotation Create/Manage)**: Depends on Phase 2 (backend endpoints ready), can run parallel with Phase 3/4
- **Phase 6 (US4 - Quotation Detail)**: Depends on Phase 5 (quotation pages and hooks exist)
- **Phase 7 (US5 - Quotation Edit)**: Depends on Phase 5 (edit page created in T045)
- **Phase 8 (Polish)**: Depends on all user stories complete

### User Story Dependencies

- **US1 (RFQ Create/Manage)**: Depends on Phase 2 only — independent of other stories
- **US2 (RFQ Detail)**: Depends on US1 (RFQ hooks and pages must exist)
- **US3 (Quotation Create/Manage)**: Depends on Phase 2 only — independent of US1/US2
- **US4 (Quotation Detail)**: Depends on US3 (quotation hooks and pages must exist)
- **US5 (Quotation Edit)**: Depends on US3 (edit page created in T045)

### Parallel Opportunities

- Phase 1: T001-T006 can all run in parallel (different files)
- Phase 2: T009-T011 (RFQ endpoints) can run parallel with T012-T017 (Quotation endpoints)
- Phase 2: T018-T021 (unit tests) can run in parallel
- Phase 2: T022-T026 (functional tests) can run in parallel
- Phase 3+5: US1 and US3 can be worked on in parallel after Phase 2
- Phase 4+6: US2 and US4 can be worked on in parallel after US1/US3

### Parallel Example: Phase 1

```bash
# Launch all validators and commands in parallel:
Task: "Create CreateRFQCommandValidator" (T001)
Task: "Create CreateQuotationCommandValidator" (T002)
Task: "Create UpdateRFQCommand + handler" (T003)
Task: "Create UpdateQuotationCommand + handler" (T004)
Task: "Create UpdateRFQCommandValidator" (T005)
Task: "Create UpdateQuotationCommandValidator" (T006)
```

### Parallel Example: Phase 2

```bash
# Launch RFQ and Quotation endpoint fixes in parallel:
Task: "Wire PUT RFQ endpoint" (T009)
Task: "Wire RecordResponse endpoint" (T010)
Task: "Wire GET /api/Quotations" (T012)
Task: "Fix GET /api/Quotations/{id}" (T013)
Task: "Wire PUT Quotation endpoint" (T014)
Task: "Wire Submit endpoint" (T015)
Task: "Wire Reject endpoint" (T016)
```

---

## Implementation Strategy

### MVP First (US1 + US2 — RFQ Management)

1. Complete Phase 1: Setup (validators + new commands)
2. Complete Phase 2: Foundational (endpoint wiring)
3. Complete Phase 3: US1 (RFQ create, edit, list pages)
4. Complete Phase 4: US2 (RFQ detail page)
5. **STOP and VALIDATE**: Test RFQ flow end-to-end via quickstart.md V1-V3

### Incremental Delivery

1. Setup + Foundational → Backend endpoints ready
2. US1 + US2 → RFQ management complete → Test V1-V3
3. US3 + US4 → Quotation management complete → Test V4-V6
4. US5 → Quotation editing complete → Test V5
5. Polish → Quality gates pass → Test V7-V9

### Parallel Team Strategy

With multiple developers:
1. Team completes Phase 1 + Phase 2 together
2. Once Phase 2 done:
   - Developer A: US1 + US2 (RFQ frontend)
   - Developer B: US3 + US4 + US5 (Quotation frontend)
3. Both converge at Phase 8 (Polish)

---

## Notes

- [P] tasks = different files, no dependencies
- [Story] label maps task to specific user story for traceability
- Each user story is independently completable and testable
- TDD: tests written before implementation per constitution XI
- No frontend tests per AGENTS.md governance decision (frontend uses lint + build only)
- Commit after each task or logical group
- Stop at any checkpoint to validate story independently
