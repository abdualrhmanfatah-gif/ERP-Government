# Tasks: Unified Party + Document Infrastructure

**Input**: Design documents from `/specs/016-unified-party-document/`

**Prerequisites**: plan.md, spec.md, research.md, data-model.md, contracts/

**Tests**: Included per Constitution Principle XI (TDD NON-NEGOTIABLE for new features).

**Organization**: Tasks grouped by user story for independent implementation and testing.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (US1–US7)

---

## Phase 1: Setup

**Purpose**: Decision record and project scaffolding

- [x] T001 Create decision record DR-001 for Suppliers module removal in docs/decision-records/DR-001-remove-suppliers-module.md per Constitution Principle XII

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: All entities, enums, configurations, interfaces that user stories depend on

**⚠️ CRITICAL**: No user story work can begin until this phase is complete

- [x] T002 [P] Create PartyType enum in src/Domain/Parties/Enums/PartyType.cs (Supplier=0, Customer=1, GovEntity=2, TaxAuthority=3, Other=4)
- [x] T003 [P] Create Party entity in src/Domain/Parties/Entities/Party.cs per data-model.md (BaseAuditableEntity, PartyCode, PartyType, NameAr, NameEn, TaxNumber, NationalId, Phone, Email, Address, Notes, IsActive, RowVersion)
- [x] T004 [P] Create ApprovalAction enum in src/Domain/Security/Enums/ApprovalAction.cs (Submit=0, Approve=1, Reject=2, Return=3, Cancel=4)
- [x] T005 [P] Modify ApprovalHistory entity in src/Domain/Security/Entities/ApprovalHistory.cs — add ApprovalStep (int, default 1) and Action (ApprovalAction) fields
- [x] T006 [P] Create DocumentStatusLog entity in src/Domain/Security/Entities/DocumentStatusLog.cs (BaseEntity, EntityName, DocumentId, FromStatus, ToStatus, ChangedById, ChangedAt, Reason)
- [x] T007 [P] Create DocumentAttachmentRequirement entity in src/Domain/Security/Entities/DocumentAttachmentRequirement.cs (BaseAuditableEntity, DocumentType, AttachmentTypeCode, TitleAr, IsMandatory, IsActive, RowVersion) with UNIQUE constraint on (DocumentType, AttachmentTypeCode)
- [x] T008 [P] Modify Attachment entity in src/Domain/Security/Entities/Attachment.cs — add DocumentType (string, required), IsRequired (bool, default false), AttachmentTypeCode (string, required)
- [x] T009 Add Party entity configuration in src/Infrastructure/Data/Configurations/PartyConfiguration.cs (indexes: UNIQUE PartyCode, composite PartyType+IsActive, TaxNumber, NameAr)
- [x] T010 [P] Add DocumentStatusLog entity configuration in src/Infrastructure/Data/Configurations/DocumentStatusLogConfiguration.cs (indexes: composite EntityName+DocumentId, ChangedAt)
- [x] T011 [P] Add DocumentAttachmentRequirement entity configuration in src/Infrastructure/Data/Configurations/DocumentAttachmentRequirementConfiguration.cs (UNIQUE composite on DocumentType+AttachmentTypeCode)
- [x] T012 Add DbSet<Party> Parties to IApplicationDbContext in src/Application/Common/Interfaces/IApplicationDbContext.cs
- [x] T013 [P] Add DbSet<DocumentStatusLog> DocumentStatusLogs to IApplicationDbContext in src/Application/Common/Interfaces/IApplicationDbContext.cs
- [x] T014 [P] Add DbSet<DocumentAttachmentRequirement> DocumentAttachmentRequirements to IApplicationDbContext in src/Application/Common/Interfaces/IApplicationDbContext.cs
- [x] T015 Extend DocumentSequenceService PrefixMap with Party→PTY, ReceiptVoucher→RCV, DepositSlip→DSL, DisbursementRequest→DSB, Payment→PAY in src/Application/FinancialSettings/Common/Services/DocumentSequenceService.cs
- [x] T016 Create IDocumentStatusLogger interface in src/Application/Parties/Common/IDocumentStatusLogger.cs with LogAsync(entityName, documentId, fromStatus, toStatus, changedById, reason, ct)
- [x] T017 Create DocumentStatusLogger implementation in src/Application/Security/Common/DocumentStatusLogger.cs injecting IApplicationDbContext

**Checkpoint**: Foundation ready — user story implementation can now begin

---

## Phase 3: User Story 1 — Migrate Suppliers to Unified Party (Priority: P1) 🎯 MVP

**Goal**: All existing Supplier records consolidated into Party entity with PTY codes, all FK references migrated, Supplier entity deleted.

**Independent Test**: Row count comparison, zero "Supplier" references in src/, FK references point to Parties.

### Tests for User Story 1

- [ ] T018 [P] [US1] Write Supplier-to-Party migration dedup test in tests/Infrastructure.IntegrationTests/Migrations/SupplierMigrationTests.cs — verify TaxNumber dedup produces one Party per unique TaxNumber
- [ ] T019 [P] [US1] Write Supplier-to-Party field mapping test in tests/Infrastructure.IntegrationTests/Migrations/SupplierMigrationTests.cs — verify NameAr, Email, Phone, Address mapped correctly
- [ ] T020 [P] [US1] Write FK replacement test in tests/Infrastructure.IntegrationTests/Migrations/SupplierMigrationTests.cs — verify PaymentOrder.VendorPartyId, Encumbrance.VendorPartyId, PurchaseOrder.SupplierPartyId, Quotation.PartyId, RFQSupplier.PartyId reference Parties

### Implementation for User Story 1

- [x] T021 [US1] Create data migration script for Supplier→Party in src/Infrastructure/Data/Migrations/ — insert Parties from Suppliers with first-wins dedup (TaxNumber match, then NameAr+PartyType), generate PTY codes per row
- [x] T022 [US1] Add FK columns VendorPartyId (int) to PaymentOrder in src/Domain/Payments/Entities/PaymentOrder.cs and EF configuration in src/Infrastructure/Data/Configurations/
- [x] T023 [US1] [P] Add FK column VendorPartyId (int?) to Encumbrance in src/Domain/Budgeting/Entities/Encumbrance.cs and EF configuration
- [x] T024 [US1] [P] Add FK column SupplierPartyId (int) to PurchaseOrder in src/Domain/Procurement/Entities/PurchaseOrder.cs and EF configuration — remove Supplier? navigation property
- [x] T025 [US1] [P] Add FK column PartyId (int) to Quotation in src/Domain/Procurement/Entities/Quotation.cs and EF configuration
- [x] T026 [US1] [P] Add FK column PartyId (int) to RFQSupplier in src/Domain/Procurement/Entities/RFQSupplier.cs and EF configuration
- [x] T027 [US1] Update CreatePaymentOrderCommand validation in src/Application/Payments/Commands/PaymentOrders/CreatePaymentOrder/ — replace context.Suppliers.FindAsync with context.Parties.FindAsync + PartyType check
- [x] T028 [US1] Migrate data: copy VendorId values to VendorPartyId on PaymentOrder and Encumbrance in migration script
- [x] T029 [US1] Migrate data: copy SupplierId values to SupplierPartyId/PartyId on PurchaseOrder, Quotation, RFQSupplier in migration script
- [x] T030 [US1] Drop Suppliers table in migration script
- [x] T031 [US1] Remove Suppliers DbSet from IApplicationDbContext in src/Application/Common/Interfaces/IApplicationDbContext.cs
- [x] T032 [US1] Delete src/Domain/Suppliers/ directory (Supplier.cs, SupplierConfiguration.cs, any other files)
- [x] T033 [US1] Grep src/ for remaining "Supplier" references and remove/replace all — verification task

**Checkpoint**: Supplier fully absorbed into Party; zero legacy references remain

---

## Phase 4: User Story 2 — ApprovalHistory as Sole Approval Source (Priority: P1)

**Goal**: ApprovalHistory carries Action enum + ApprovalStep; PaymentOrder approval refactored to ApprovalHistory-only with single SaveChanges.

**Independent Test**: All approval records have Action set; PaymentOrder approval writes to ApprovalHistory and DocumentStatusLog in single SaveChanges.

### Tests for User Story 2

- [ ] T034 [P] [US2] Write ApprovalHistory backfill test in tests/Infrastructure.IntegrationTests/Migrations/ApprovalHistoryBackfillTests.cs — verify "Approved" → Action=Approve, "Draft -> Submitted" → Action=Submit, "* -> Cancelled" → Action=Cancel
- [ ] T035 [P] [US2] Write ApprovalHistory backfill Reason preservation test — verify original Decision string preserved in Reason when Reason was empty

### Implementation for User Story 2

- [x] T036 [US2] Add data migration for ApprovalHistory backfill — add ApprovalStep (default 1) and Action columns, backfill Action from Decision string per rules in data-model.md
- [x] T037 [US2] Refactor ApprovePaymentOrderCommand handler in src/Application/Payments/Commands/PaymentOrders/ApprovePaymentOrder/ — inject IDocumentStatusLogger, write ApprovalHistory with real RequiredRole + Action=Approve + ApprovalStep=1, write DocumentStatusLog row, single SaveChanges
- [x] T038 [US2] Verify PaymentOrder entity has no inline approval columns (FR-012 no-op defensive check) — add comment noting no-op in migration

**Checkpoint**: ApprovalHistory is sole approval source; Action enum authoritative

---

## Phase 5: User Story 3 — Append-Only Document Status Log (Priority: P1)

**Goal**: All 22 writer sites inject IDocumentStatusLogger and produce DocumentStatusLog rows on every state transition.

**Independent Test**: DocumentStatusLog rows created for every handler execution; no update/delete API exists.

### Tests for User Story 3

- [ ] T039 [P] [US3] Write DocumentStatusLog append-only test in tests/Application.FunctionalTests/Documents/DocumentStatusLogTests.cs — verify no PUT/PATCH/DELETE endpoints exist for status log
- [ ] T040 [P] [US3] Write DocumentStatusLog creation test — verify LogAsync creates row with correct EntityName, DocumentId, FromStatus, ToStatus, ChangedById, ChangedAt

### Implementation for User Story 3

- [x] T041 [US3] Inject IDocumentStatusLogger into CreateEncumbranceCommand in src/Application/Budgeting/Commands/Encumbrances/CreateEncumbranceCommand.cs — call LogAsync on state change
- [x] T042 [US3] [P] Inject IDocumentStatusLogger into SubmitEncumbranceCommand in src/Application/Budgeting/Commands/Encumbrances/SubmitEncumbranceCommand.cs
- [x] T043 [US3] [P] Inject IDocumentStatusLogger into ApproveEncumbranceCommand in src/Application/Budgeting/Commands/Encumbrances/ApproveEncumbranceCommand.cs
- [x] T044 [US3] [P] Inject IDocumentStatusLogger into ActivateEncumbranceCommand in src/Application/Budgeting/Commands/Encumbrances/ActivateEncumbranceCommand.cs
- [x] T045 [US3] [P] Inject IDocumentStatusLogger into SuspendEncumbranceCommand in src/Application/Budgeting/Commands/Encumbrances/SuspendEncumbranceCommand.cs
- [x] T046 [US3] [P] Inject IDocumentStatusLogger into ReverseEncumbranceCommand in src/Application/Budgeting/Commands/Encumbrances/ReverseEncumbranceCommand.cs
- [x] T047 [US3] [P] Inject IDocumentStatusLogger into CloseEncumbranceCommand in src/Application/Budgeting/Commands/Encumbrances/CloseEncumbranceCommand.cs
- [x] T048 [US3] [P] Inject IDocumentStatusLogger into CancelEncumbranceCommand in src/Application/Budgeting/Commands/Encumbrances/CancelEncumbranceCommand.cs
- [x] T049 [US3] Inject IDocumentStatusLogger into SubmitBudgetCommand in src/Application/Budgeting/Commands/Budgets/SubmitBudgetCommand.cs
- [x] T050 [US3] [P] Inject IDocumentStatusLogger into ApproveBudgetCommand in src/Application/Budgeting/Commands/Budgets/ApproveBudgetCommand.cs
- [x] T051 [US3] [P] Inject IDocumentStatusLogger into ActivateBudgetCommand in src/Application/Budgeting/Commands/Budgets/ActivateBudgetCommand.cs
- [x] T052 [US3] [P] Inject IDocumentStatusLogger into SuspendBudgetCommand in src/Application/Budgeting/Commands/Budgets/SuspendBudgetCommand.cs
- [x] T053 [US3] [P] Inject IDocumentStatusLogger into CloseBudgetCommand in src/Application/Budgeting/Commands/Budgets/CloseBudgetCommand.cs
- [x] T054 [US3] [P] Inject IDocumentStatusLogger into CancelBudgetCommand in src/Application/Budgeting/Commands/Budgets/CancelBudgetCommand.cs
- [x] T055 [US3] Inject IDocumentStatusLogger into SubmitAppropriationCommand in src/Application/Budgeting/Commands/Appropriations/SubmitAppropriationCommand.cs
- [x] T056 [US3] [P] Inject IDocumentStatusLogger into ApproveAppropriationCommand in src/Application/Budgeting/Commands/Appropriations/ApproveAppropriationCommand.cs
- [x] T057 [US3] [P] Inject IDocumentStatusLogger into ActivateAppropriationCommand in src/Application/Budgeting/Commands/Appropriations/ActivateAppropriationCommand.cs
- [x] T058 [US3] [P] Inject IDocumentStatusLogger into SuspendAppropriationCommand in src/Application/Budgeting/Commands/Appropriations/SuspendAppropriationCommand.cs
- [x] T059 [US3] [P] Inject IDocumentStatusLogger into ReverseAppropriationCommand in src/Application/Budgeting/Commands/Appropriations/ReverseAppropriationCommand.cs
- [x] T060 [US3] [P] Inject IDocumentStatusLogger into CloseAppropriationCommand in src/Application/Budgeting/Commands/Appropriations/CloseAppropriationCommand.cs
- [x] T061 [US3] [P] Inject IDocumentStatusLogger into CancelAppropriationCommand in src/Application/Budgeting/Commands/Appropriations/CancelAppropriationCommand.cs
- [x] T062 [US3] Inject IDocumentStatusLogger into ApprovalService in src/Application/Security/Common/ApprovalService.cs — log after approval record created

**Checkpoint**: All 22 writer sites produce DocumentStatusLog rows

---

## Phase 6: User Story 4 — Formalized Attachments with Mandatory Requirements (Priority: P2)

**Goal**: DocumentAttachmentRequirement CRUD works; attachment gate blocks Submitted→Approved when mandatory attachments missing.

**Independent Test**: Create requirement, attempt approval without attachment → blocked; upload attachment → approval proceeds.

### Tests for User Story 4

- [ ] T063 [P] [US4] Write attachment gate blocking test in tests/Application.FunctionalTests/Documents/AttachmentGateTests.cs — verify approval blocked when mandatory attachment missing
- [ ] T064 [P] [US4] Write attachment gate passing test — verify approval proceeds when mandatory attachment present
- [ ] T065 [P] [US4] Write attachment gate backward compatibility test — verify no check when no requirements exist for document type

### Implementation for User Story 4

- [x] T066 [US4] Create DocumentAttachmentRequirement CRUD commands in src/Application/DocumentAttachmentRequirements/Commands/ — Create, Update, Delete (soft-delete)
- [x] T067 [US4] [P] Create DocumentAttachmentRequirement queries in src/Application/DocumentAttachmentRequirements/Queries/ — GetById, GetByDocumentType
- [x] T068 [US4] Create IAttachmentGateService interface and implementation in src/Application/Security/Common/ — CheckMandatoryAttachmentsAsync(documentType, documentId, ct) returns missing codes
- [x] T069 [US4] Wire attachment gate into ApproveBudgetCommand — check mandatory attachments before allowing Submitted→Approved transition
- [x] T070 [US4] [P] Wire attachment gate into ApproveAppropriationCommand
- [x] T071 [US4] [P] Wire attachment gate into ApproveEncumbranceCommand
- [x] T072 [US4] [P] Wire attachment gate into ApprovePaymentOrderCommand
- [x] T073 [US4] Seed DocumentAttachmentRequirements for Budget and Appropriation in migration script (e.g., BOQ, CONTRACT as mandatory)

**Checkpoint**: Attachment gate functional for Budget, Appropriation, Encumbrance, PaymentOrder

---

## Phase 7: User Story 5 — Party CRUD and Management (Priority: P2)

**Goal**: Full CRUD API for Party with filtering, search, and toggle-active.

**Independent Test**: POST creates party with PTY code; GET filters by type/active; PATCH toggles isActive; search by name/tax works.

### Tests for User Story 5

- [ ] T074 [P] [US5] Write Party CRUD test in tests/Application.FunctionalTests/Parties/PartyTests.cs — verify create returns PTY code, get by id, update, toggle-active
- [ ] T075 [P] [US5] Write Party filtering test — verify GET with PartyType and IsActive filters returns correct subset
- [ ] T076 [P] [US5] Write Party search test — verify search by NameAr and TaxNumber returns matching parties

### Implementation for User Story 5

- [x] T077 [P] [US5] Create CreatePartyCommand + handler + FluentValidation in src/Application/Parties/Commands/CreateParty/
- [x] T078 [P] [US5] Create UpdatePartyCommand + handler + FluentValidation in src/Application/Parties/Commands/UpdateParty/
- [x] T079 [P] [US5] Create TogglePartyActiveCommand + handler in src/Application/Parties/Commands/TogglePartyActive/
- [x] T080 [P] [US5] Create GetPartiesQuery + handler with PartyType/IsActive/search filters in src/Application/Parties/Queries/GetParties/
- [x] T081 [P] [US5] Create GetPartyByIdQuery + handler in src/Application/Parties/Queries/GetPartyById/
- [x] T082 [US5] Create Party endpoint group in src/Web/Endpoints/Parties/Parties.cs implementing IEndpointGroup — GET /, GET /{id}, POST /, PUT /{id}, PATCH /{id}/toggle-active per contracts/parties.md
- [x] T083 [US5] Register Party endpoints in Web endpoint routing (check if auto-discovered or needs manual registration)

**Checkpoint**: Party CRUD fully functional with filtering, search, and toggle-active

---

## Phase 8: User Story 6 — Generic Document Endpoints (Priority: P2)

**Goal**: Unified API for approvals, status-log, attachments (with real file upload), and attachment requirements.

**Independent Test**: GET approvals/status-log/attachments by entity+id; POST attachment with file upload; DELETE attachment removes file and row.

### Tests for User Story 6

- [ ] T084 [P] [US6] Write document approvals endpoint test in tests/Application.FunctionalTests/Documents/DocumentEndpointTests.cs — verify GET returns ApprovalHistory for entity+id
- [ ] T085 [P] [US6] Write document status-log endpoint test — verify GET returns DocumentStatusLog ordered by ChangedAt
- [ ] T086 [P] [US6] Write attachment upload endpoint test — verify POST with file creates Attachment row and stores file
- [ ] T087 [P] [US6] Write attachment delete endpoint test — verify DELETE removes row and file

### Implementation for User Story 6

- [x] T088 [P] [US6] Create GetDocumentApprovalsQuery + handler in src/Application/Documents/Queries/GetDocumentApprovals/
- [x] T089 [P] [US6] Create GetDocumentStatusLogQuery + handler in src/Application/Documents/Queries/GetDocumentStatusLog/
- [x] T090 [P] [US6] Create GetDocumentAttachmentsQuery + handler in src/Application/Documents/Queries/GetDocumentAttachments/
- [x] T091 [P] [US6] Create GetAttachmentRequirementsQuery + handler in src/Application/Documents/Queries/GetAttachmentRequirements/
- [x] T092 [US6] Create IFileStorageService interface in src/Application/Documents/Common/IFileStorageService.cs with SaveStreamAsync, DeleteAsync
- [x] T093 [US6] Create LocalFileStorageService implementation in src/Infrastructure/Services/LocalFileStorageService.cs — saves to configured base directory
- [x] T094 [US6] Create UploadAttachmentCommand + handler in src/Application/Documents/Commands/UploadAttachment/ — save file via IFileStorageService, create Attachment row
- [x] T095 [US6] Create DeleteAttachmentCommand + handler in src/Application/Documents/Commands/DeleteAttachment/ — delete file via IFileStorageService, delete Attachment row
- [x] T096 [US6] Create Document endpoint group in src/Web/Endpoints/Documents/Documents.cs implementing IEndpointGroup — all routes per contracts/documents.md (approvals, status-log, attachments CRUD, requirements)
- [x] T097 [US6] Register Document endpoints in Web endpoint routing

**Checkpoint**: Generic document endpoints fully functional with file upload

---

## Phase 9: User Story 7 — Document Sequence Prefixes (Priority: P3)

**Goal**: All 5 new prefixes generate unique, gap-free codes within active fiscal year.

**Independent Test**: Create entities for each prefix; verify unique sequential numbers; verify concurrency safety.

### Tests for User Story 7

- [x] T098 [P] [US7] Write sequence prefix generation test in tests/Application.UnitTests/DocumentSequenceServiceTests.cs — verify PTY, RCV, DSL, DSB, PAY generate correct format
- [x] T099 [P] [US7] Write sequence concurrency test — verify concurrent requests produce unique numbers

### Implementation for User Story 7

- [x] T100 [US7] Verify DocumentSequenceService PrefixMap already extended (T015) — integration check
- [x] T101 [US7] Seed DocumentSequence entries for PTY, RCV, DSL, DSB, PAY prefixes for active fiscal year 2026 in migration script
- [x] T102 [US7] Update DocumentSequenceService to handle Party document type specifically (generate PTY codes on Party creation) — wire into CreatePartyCommand

**Checkpoint**: All 5 prefixes generate unique sequential codes

---

## Phase 10: Polish & Cross-Cutting Concerns

**Purpose**: Validation, documentation, final verification

- [ ] T103 Run quickstart.md validation scenarios V1–V8 and record results
- [x] T104 [P] Update docs/database-schema.md with new/modified/deleted tables
- [x] T105 [P] Run full test suite — dotnet test on all test projects
- [x] T106 Verify zero "Supplier" references remain in src/ via grep
- [x] T107 Verify all new endpoints have declared permissions per Constitution Principle VII
- [x] T108 Verify OpenAPI contract matches implementation for all new endpoints

---

## Dependencies & Execution Order

### Phase Dependencies

- **Phase 1 (Setup)**: No dependencies — can start immediately
- **Phase 2 (Foundational)**: Depends on Phase 1 — BLOCKS all user stories
- **Phases 3–9 (User Stories)**: All depend on Phase 2 completion
  - Phase 3 (US1) and Phase 4+5 (US2+US3) can run in parallel
  - Phase 6 (US4) depends on Phase 4 (US2) for ApprovalHistory refactoring
  - Phase 7 (US5) is independent after Phase 2
  - Phase 8 (US6) depends on Phase 5 (US3) for DocumentStatusLog and Phase 4 (US2) for ApprovalHistory
  - Phase 9 (US7) depends on Phase 7 (US5) for Party entity
- **Phase 10 (Polish)**: Depends on all desired user stories being complete

### User Story Dependencies

- **US1 (P1)**: Foundational only — no dependency on other stories
- **US2 (P1)**: Foundational only — modifies ApprovalHistory
- **US3 (P1)**: Foundational + US2 (IDocumentStatusLogger used in same handlers)
- **US4 (P2)**: Foundational + US2 (attachment gate wired into approval commands)
- **US5 (P2)**: Foundational only — Party CRUD is independent
- **US6 (P2)**: Foundational + US2 + US3 (generic document endpoints query ApprovalHistory and DocumentStatusLog)
- **US7 (P3)**: Foundational + US5 (Party entity must exist for PTY sequence)

### Parallel Opportunities

**Phase 2 (all [P] tasks can run in parallel)**:
```bash
Task T002 + T003 + T004 + T005 + T006 + T007 + T008 (all entity/enum creation)
Task T009 + T010 + T011 (all EF configurations)
Task T012 + T013 + T014 (all DbSet additions)
```

**US3 (parallel handler injection)**:
```bash
Task T042 + T043 + T044 + T045 + T046 + T047 + T048 (all Encumbrance handlers)
Task T050 + T051 + T052 + T053 + T054 (all Budget handlers except Submit)
Task T056 + T057 + T058 + T059 + T060 + T061 (all Appropriation handlers except Submit)
```

**US5 (parallel command/query creation)**:
```bash
Task T077 + T078 + T079 + T080 + T081 (all Party commands/queries)
```

**US6 (parallel query creation)**:
```bash
Task T088 + T089 + T090 + T091 (all document queries)
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup (DR-001)
2. Complete Phase 2: Foundational (entities, enums, configurations, interfaces)
3. Complete Phase 3: User Story 1 (Supplier→Party migration)
4. **STOP and VALIDATE**: Run migration tests, verify zero Supplier references
5. Deploy/demo if ready

### Incremental Delivery

1. Setup + Foundational → Foundation ready
2. Add US1 → Supplier absorbed → Deploy/Demo (MVP!)
3. Add US2+US3 → ApprovalHistory refactored + status logging → Deploy/Demo
4. Add US4 → Attachment gate → Deploy/Demo
5. Add US5 → Party CRUD → Deploy/Demo
6. Add US6 → Document endpoints → Deploy/Demo
7. Add US7 → Sequence prefixes → Deploy/Demo (Complete!)

### Recommended Parallel Team Strategy

With multiple developers:
1. Team completes Setup + Foundational together
2. Once Foundational is done:
   - Developer A: US1 (Supplier migration)
   - Developer B: US2+US3 (ApprovalHistory + status log)
   - Developer C: US5 (Party CRUD)
3. After US2+US3 complete:
   - Developer B: US4 (Attachment gate)
   - Developer C: US6 (Document endpoints)
4. After US5:
   - Developer C: US7 (Sequence prefixes)

---

## Notes

- [P] tasks = different files, no dependencies
- [Story] label maps task to specific user story for traceability
- Each user story should be independently completable and testable
- TDD per Constitution Principle XI: tests written FIRST, observed failing, then implementation
- Commit after each task or logical group
- Stop at any checkpoint to validate story independently
- 22 writer sites for status logging (not 14 as originally spec'd — see research.md R1)
- PaymentOrder inline approval columns do not exist — FR-012 is no-op with defensive check (see research.md R2)
