# Tasks: Unified Party Registry & Shared Document Panels

**Input**: Design documents from `/specs/022-unified-party-registry/`

**Prerequisites**: plan.md (required), spec.md (required for user stories), research.md, data-model.md, contracts/

**Tests**: Explicitly requested — tests included for each user story.

**Organization**: Tasks grouped by user story. Backend endpoint tasks in Foundational phase (shared by all stories).

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Create feature folder structure and shared types

- [x] T001 Create `src/Web/ClientApp/src/features/parties/` directory structure: `__tests__/`, `shared/`, `hooks/`, `pages/`
- [x] T002 Create `src/Web/ClientApp/src/features/documents/` directory structure: `__tests__/`, `components/`, `shared/`
- [x] T003 [P] Create party types and enums in `src/Web/ClientApp/src/features/parties/shared/types.ts` (PartyType enum, PARTY_TYPE_LABELS, PartyResponse, CreatePartyCommand, UpdatePartyCommand, PartyFilters, PartyDocumentResponse)
- [x] T004 [P] Create document shared types in `src/Web/ClientApp/src/features/documents/shared/types.ts` (ApprovalRecord, StatusLogRecord, AttachmentRecord, AttachmentRequirement, AttachmentGateCheckResponse, panel props interfaces)
- [x] T005 [P] Create barrel exports in `src/Web/ClientApp/src/features/parties/shared/index.ts` and `src/Web/ClientApp/src/features/documents/shared/index.ts`

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Backend endpoints + frontend API clients that ALL user stories depend on

**CRITICAL**: No user story work can begin until this phase is complete

### Backend Endpoints (3 new)

- [x] T006 Add `GET /api/Documents/{documentType}/{documentId:int}/attachment-gate-check` endpoint in `src/Web/Endpoints/Documents/Documents.cs` — calls `IAttachmentGateService.CheckMandatoryAttachmentsAsync`, returns `string[]` of missing type codes
- [x] T007 Add `GET /api/Documents/attachments/{id:int}/download` endpoint in `src/Web/Endpoints/Documents/Documents.cs` — streams file via `IFileStorageService.OpenReadAsync`
- [x] T008 Add `GET /api/Parties/{id:int}/documents` endpoint in `src/Web/Endpoints/Parties/Parties.cs` — queries documents referencing the party across ReceiptVouchers, PaymentOrders, Encumbrances tables, returns `PartyDocumentResponse[]`

### Frontend API Clients

- [x] T009 Create `src/Web/ClientApp/src/features/parties/shared/client.ts` — partiesClient with `list`, `getById`, `create`, `update`, `toggleActive`, `getDocuments` methods; includes `handleResponse<T>`, `buildQuery`, `partiesKeys` cache key factory; follows `budgetTypesClient` pattern from `src/Web/ClientApp/src/features/budgeting/shared/client.ts`
- [x] T010 Create `src/Web/ClientApp/src/features/documents/shared/client.ts` — documentsClient with `getApprovals`, `getStatusLog`, `getAttachments`, `getRequirements`, `checkGate`, `upload`, `deleteAttachment`, `downloadAttachment` methods; includes `documentsKeys` cache key factory
- [x] T011 [P] Create `src/Web/ClientApp/src/features/parties/hooks/useParties.ts` — react-query hooks: `usePartiesList(filters)`, `useParty(id)`, `useCreateParty()`, `useUpdateParty(id)`, `useTogglePartyActive(id)`, `usePartyDocuments(id)`; wraps partiesClient; follows `useBudgets.ts` pattern
- [x] T012 [P] Create `src/Web/ClientApp/src/features/documents/hooks/useDocuments.ts` — react-query hooks: `useApprovals(type, id)`, `useStatusLog(type, id)`, `useAttachments(type, id)`, `useAttachmentRequirements(type)`, `useAttachmentGateCheck(type, id)`, `useUploadAttachment()`, `useDeleteAttachment()`

### NSwag Regeneration

- [x] T013 Run `npm run generate-api` in `src/Web/ClientApp` to regenerate `web-api-client.ts` with correct types for Documents endpoints

**Checkpoint**: Foundation ready — user story implementation can now begin

---

## Phase 3: User Story 1 — Party Management (Priority: P1) — MVP

**Goal**: Registrar creates, edits, toggles active parties; searches by Arabic name or tax number; duplicate tax warning on save

**Independent Test**: Create parties of each type, edit details, toggle active/inactive, filter by type, search by name and tax number, observe duplicate tax number warnings. No documents or approvals needed.

### Tests for User Story 1

- [x] T014 [P] [US1] Write PartiesListPage test in `src/Web/ClientApp/src/features/parties/__tests__/PartiesListPage.test.tsx` — renders party list, filters by PartyType, searches by NameAr and TaxNumber, toggle active visible only with permission
- [x] T015 [P] [US1] Write PartyDetailPage test in `src/Web/ClientApp/src/features/parties/__tests__/PartyDetailPage.test.tsx` — renders party profile, edit form pre-fills values, edit button hidden without permission
- [x] T016 [P] [US1] Write DuplicateTaxWarning test in `src/Web/ClientApp/src/features/parties/__tests__/DuplicateTaxWarning.test.tsx` — shows warning on duplicate TaxNumber, user can confirm to proceed, warning clears when TaxNumber changes

### Implementation for User Story 1

- [x] T017 [P] [US1] Create PartiesListPage in `src/Web/ClientApp/src/features/parties/pages/PartiesListPage.tsx` — DataGrid with columns (PartyCode, NameAr, PartyType, TaxNumber, IsActive); FilterBar with PartyType dropdown, IsActive toggle, search input; uses `usePartiesList` hook; default export
- [x] T018 [P] [US1] Create PartyDetailPage in `src/Web/ClientApp/src/features/parties/pages/PartyDetailPage.tsx` — loads party by `useParams().id`; profile section with all fields in read mode; edit mode via react-hook-form + zod validation; Toggle Active button; uses `useParty`, `useUpdateParty`, `useTogglePartyActive` hooks; default export
- [x] T019 [US1] Add duplicate tax number check on blur in PartiesListPage — calls `partiesClient.list({ search: taxNumber })` on TaxNumber field blur, filters for exact match, shows ConfirmDialog if duplicate found; follow spec clarification: soft warning with confirmation
- [x] T020 [US1] Add permission-based button visibility in PartiesListPage and PartyDetailPage — use `usePermission` hook from `@/shared/hooks/usePermission`; hide "New Party" button when `PermissionCodes.PartiesCreate` not granted; hide "Edit" button when `PermissionCodes.PartiesUpdate` not granted
- [x] T021 [US1] Add deactivation guard in PartyDetailPage — when toggling active on a party, check `partyDocuments` for open (Draft/Submitted) documents; show blocking warning listing open documents; allow confirmation to proceed

**Checkpoint**: Party management fully functional and independently testable

---

## Phase 4: User Story 2 — Shared Approvals Timeline & Status Log (Priority: P1)

**Goal**: Any document screen shows a shared approvals timeline (who, when, decision, reason) and status history log; panels are collapsible, start expanded

**Independent Test**: View any document's approval timeline and status log entries; verify correct data from ApprovalHistory and DocumentStatusLog; verify empty states; verify collapsible behavior

### Tests for User Story 2

- [x] T022 [P] [US2] Write ApprovalsPanel test in `src/Web/ClientApp/src/features/documents/__tests__/ApprovalsPanel.test.tsx` — renders approval records with ApproverName, DecisionAt, Decision, Reason; shows "No approvals yet" empty state; shows Pending badge for undecided steps; panel collapsible
- [x] T023 [P] [US2] Write StatusLogPanel test in `src/Web/ClientApp/src/features/documents/__tests__/StatusLogPanel.test.tsx` — renders status log with FromStatus, ToStatus, ChangedBy, ChangedAt, Reason; shows "No status changes recorded" empty state; panel collapsible

### Implementation for User Story 2

- [x] T024 [P] [US2] Create ApprovalsPanel in `src/Web/ClientApp/src/features/documents/components/ApprovalsPanel.tsx` — accepts `ApprovalsPanelProps { documentType, documentId }`; uses `useApprovals` hook; renders timeline with Decision badge (Approved=green, Rejected=red, Pending=amber); collapsible via Accordion from `@/components/ui`; ordered newest first
- [x] T025 [P] [US2] Create StatusLogPanel in `src/Web/ClientApp/src/features/documents/components/StatusLogPanel.tsx` — accepts `StatusLogPanelProps { documentType, documentId }`; uses `useStatusLog` hook; renders table with FromStatus→ToStatus arrow; collapsible via Accordion; ordered newest first
- [x] T026 [US2] Export shared panels from `src/Web/ClientApp/src/features/documents/shared/index.ts` — `export { ApprovalsPanel } from '../components/ApprovalsPanel'` and same for StatusLogPanel
- [x] T027 [US2] Integrate ApprovalsPanel and StatusLogPanel into PartyDetailPage — add as collapsible sections below the party profile; pass `documentType="Party"` and `documentId={partyId}`

**Checkpoint**: Approvals and status log panels reusable on any document screen

---

## Phase 5: User Story 3 — Attachments with Mandatory Gate (Priority: P2)

**Goal**: Upload and delete attachments on any document; mandatory attachment gate badge shows whether approval is blocked

**Independent Test**: Upload/delete attachments, check gate badge against known requirements, observe approval blocking when mandatory attachments missing; test oversized upload rejection

### Tests for User Story 3

- [x] T028 [P] [US3] Write AttachmentsPanel test in `src/Web/ClientApp/src/features/documents/__tests__/AttachmentsPanel.test.tsx` — renders attachment list with FileName, UploadedBy, CreatedAt, Size; upload shows type dropdown with requirements + "Other"; delete shows confirmation; gate badge shows "Missing required attachment" when requirements unmet; gate badge shows satisfied when met; oversized upload shows error

### Implementation for User Story 3

- [x] T029 [P] [US3] Create AttachmentsPanel in `src/Web/ClientApp/src/features/documents/components/AttachmentsPanel.tsx` — accepts `AttachmentsPanelProps { documentType, documentId, showGate?, onGateChange? }`; uses `useAttachments`, `useAttachmentRequirements`, `useAttachmentGateCheck`, `useUploadAttachment`, `useDeleteAttachment` hooks
- [x] T030 [US3] Implement upload flow in AttachmentsPanel — "Upload Attachment" button opens file picker; type dropdown populated from `useAttachmentRequirements` + "Other" option with free-text; file size validation (10 MB max) on client side; multipart/form-data POST via `documentsClient.upload`; refresh list on success
- [x] T031 [US3] Implement delete flow in AttachmentsPanel — "Delete" button shows ConfirmDialog; on confirm calls `documentsClient.deleteAttachment(id)`; refresh list on success
- [x] T032 [US3] Implement gate badge in AttachmentsPanel — when `showGate=true`, call `useAttachmentGateCheck` on mount and after upload/delete; if missing codes exist, show amber Badge "Missing required attachment: {code}" and call `onGateChange(false)`; if empty, show green Badge and call `onGateChange(true)`
- [x] T033 [US3] Integrate AttachmentsPanel into PartyDetailPage — add as collapsible section below approvals/status log; pass `documentType="Party"`, `documentId={partyId}`, `showGate={false}` (parties don't have mandatory attachment requirements by default)

**Checkpoint**: Attachments panel reusable on any document screen with gate support

---

## Phase 6: User Story 4 — Party Detail with Related Documents (Priority: P2)

**Goal**: Party detail page lists related documents (vouchers, payment orders, encumbrances) with filter by document type

**Independent Test**: Create documents referencing a party, view party detail page, verify documents appear in related documents list; test empty state; test filter by document type

### Tests for User Story 4

- [x] T034 [P] [US4] Write related documents test in `src/Web/ClientApp/src/features/parties/__tests__/PartyDetailPage.test.tsx` (extend existing) — renders related documents list with DocumentType, DocumentNumber, Status, Date, Amount; shows "No related documents" empty state; filter by document type; click row navigates to document

### Implementation for User Story 4

- [x] T035 [US4] Add Related Documents section to PartyDetailPage — DataGrid below attachments panel; columns: DocumentType, DocumentNumber, Status, Date, Amount; uses `usePartyDocuments` hook; filter dropdown for DocumentType; row click navigates to `/api/Documents/{type}/{id}` route
- [x] T036 [US4] Add pagination to related documents — use `Pagination` component from `@/components/ui`; server-side pagination if >20 items; follow existing DataGrid + Pagination pattern from budgeting list pages

**Checkpoint**: Party detail page shows full party context (profile + related documents + all shared panels)

---

## Phase 7: Polish & Cross-Cutting Concerns

**Purpose**: Route registration, final integration, validation

- [x] T037 Register `/parties` routes in `src/Web/ClientApp/src/app/routes.tsx` — add RouteConfig entries for `/parties` (PartiesListPage) and `/parties/:id` (PartyDetailPage); follow existing route pattern from budgeting
- [x] T038 [P] Verify barrel exports — `src/Web/ClientApp/src/features/parties/shared/index.ts` exports all public types and client; `src/Web/ClientApp/src/features/documents/shared/index.ts` exports ApprovalsPanel, StatusLogPanel, AttachmentsPanel
- [x] T039 Run frontend lint and type check — `cd src/Web/ClientApp && npm run lint && npm run typecheck`
- [x] T040 Run frontend tests — `cd src/Web/ClientApp && npm run test -- --run`
- [x] T41 Run quickstart.md validation — execute V1 through V11 scenarios from `specs/022-unified-party-registry/quickstart.md`

---

## Dependencies & Execution Order

### Phase Dependencies

- **Phase 1 (Setup)**: No dependencies — can start immediately
- **Phase 2 (Foundational)**: Depends on Phase 1 — BLOCKS all user stories
- **Phase 3 (US1)**: Depends on Phase 2 — can start immediately after foundation
- **Phase 4 (US2)**: Depends on Phase 2 — can run in parallel with US1
- **Phase 5 (US3)**: Depends on Phase 2 — can run in parallel with US1 and US2
- **Phase 6 (US4)**: Depends on Phase 3 (needs PartyDetailPage) and Phase 5 (needs AttachmentsPanel integration)
- **Phase 7 (Polish)**: Depends on all desired stories being complete

### User Story Dependencies

- **US1 (P1)**: Can start after Phase 2 — no dependencies on other stories
- **US2 (P1)**: Can start after Phase 2 — no dependencies on other stories; integrates into US1's PartyDetailPage in T027
- **US3 (P2)**: Can start after Phase 2 — no dependencies on other stories; integrates into US1's PartyDetailPage in T033
- **US4 (P2)**: Depends on US1 (PartyDetailPage exists) and US3 (AttachmentsPanel exists)

### Within Each User Story

- Tests MUST be written and FAIL before implementation (TDD per AGENTS.md constitution)
- Types before client before hooks before pages before integration

### Parallel Opportunities

```bash
# Phase 1: All type files in parallel
Task T003 + T004 + T005

# Phase 2: Backend endpoints in parallel, then frontend clients
Task T006 + T007 + T008  (backend)
Task T011 + T012         (hooks, after clients)

# Phase 3: Tests + pages in parallel
Task T014 + T015 + T016  (tests)
Task T017 + T018         (pages)

# Phase 4: Tests + components in parallel
Task T022 + T023         (tests)
Task T024 + T025         (components)

# Cross-story: US1 and US2 implementation in parallel after Phase 2
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup (T001-T005)
2. Complete Phase 2: Foundational (T006-T013)
3. Complete Phase 3: US1 (T014-T021)
4. **STOP and VALIDATE**: Party list + detail pages functional, duplicate tax warning working, permission-based buttons working
5. Deploy/demo if ready

### Incremental Delivery

1. Setup + Foundational → Foundation ready
2. US1 (Party Management) → Test independently → Deploy/Demo (MVP!)
3. US2 (Approvals + Status Log) → Test independently → Deploy/Demo
4. US3 (Attachments + Gate) → Test independently → Deploy/Demo
5. US4 (Related Documents) → Test independently → Deploy/Demo
6. Each story adds value without breaking previous stories

### Parallel Team Strategy

With multiple developers:
1. Team completes Setup + Foundational together
2. Once Foundational is done:
   - Developer A: US1 (Party Management)
   - Developer B: US2 (Approvals + Status Log panels)
   - Developer C: US3 (Attachments panel)
3. US4 waits for US1 + US3 completion
4. Stories integrate into PartyDetailPage

---

## Notes

- [P] tasks = different files, no dependencies
- [Story] label maps task to specific user story for traceability
- Each user story should be independently completable and testable
- Tests written FIRST (TDD) — observe red, implement, observe green
- Commit after each task or logical group
- Stop at any checkpoint to validate story independently
