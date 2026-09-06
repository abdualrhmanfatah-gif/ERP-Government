# Tasks: Disbursement of Approved Payment Orders

**Input**: Design documents from `/specs/018-disbursement-payment-orders/`

**Prerequisites**: plan.md (required), spec.md (required for user stories), research.md, data-model.md, contracts/

**Tests**: TDD mandatory per Constitution Principle XI. Test tasks are included and must be executed first (red → green → refactor).

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Entity definitions, enums, DbContext registration, document sequences

- [x] T001 [P] Create DisbursementRequestStatus enum in src/Domain/Payments/Enums/DisbursementRequestStatus.cs
- [x] T002 [P] Create PaymentStatus enum in src/Domain/Payments/Enums/PaymentStatus.cs
- [x] T003 [P] Create DisbursementRequest entity in src/Domain/Payments/Entities/DisbursementRequest.cs
- [x] T004 [P] Create Payment entity in src/Domain/Payments/Entities/Payment.cs
- [x] T005 Register DbSets in src/Application/Common/Interfaces/IApplicationDbContext.cs (DisbursementRequests, Payments)
- [x] T006 Register DbSets in src/Infrastructure/Data/ApplicationDbContext.cs
- [x] T007 [P] Create DisbursementRequest EF configuration in src/Infrastructure/Data/Configurations/Payments/DisbursementRequestConfiguration.cs (unique indexes on PaymentOrderId, RequestNumber)
- [x] T008 [P] Create Payment EF configuration in src/Infrastructure/Data/Configurations/Payments/PaymentConfiguration.cs (unique indexes on DisbursementRequestId, PaymentNumber)
- [x] T009 Add EF migration for DisbursementRequest and Payment entities
- [x] T010 Register DSB and PAY document sequences in DocumentSequenceService seed data
- [x] T011 Add PermissionCodes for Disbursements and Payments in src/Application/Common/Security/PermissionCodes.cs
- [x] T012 Register authorization policies in src/Web/DependencyInjection.cs for new permission codes

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Shared DTOs and query infrastructure that all user stories depend on

**⚠️ CRITICAL**: No user story work can begin until this phase is complete

- [x] T013 [P] Create DisbursementRequestDto in src/Application/Payments/Common/DTOs/DisbursementRequestDto.cs
- [x] T014 [P] Create PaymentDto in src/Application/Payments/Common/DTOs/PaymentDto.cs
- [x] T015 [P] Create AvailabilityBreakdownDto in src/Application/Payments/Common/DTOs/AvailabilityBreakdownDto.cs
- [x] T016 [P] Create GetDisbursementRequestsQuery in src/Application/Payments/Queries/DisbursementRequests/GetDisbursementRequests/GetDisbursementRequestsQuery.cs
- [x] T017 [P] Create GetDisbursementRequestByIdQuery in src/Application/Payments/Queries/DisbursementRequests/GetDisbursementRequestById/GetDisbursementRequestByIdQuery.cs
- [x] T018 [P] Create GetPaymentsQuery in src/Application/Payments/Queries/Payments/GetPayments/GetPaymentsQuery.cs

**Checkpoint**: Foundation ready — user story implementation can now begin

---

## Phase 3: User Story 1 — Create Disbursement Request (Priority: P1) 🎯 MVP

**Goal**: Accountant creates a disbursement request against an approved payment order; budget availability is checked; creation rejected with breakdown when Blocking + insufficient funds.

**Independent Test**: Create request against approved PO with sufficient funds (Draft status). Attempt creation against PO with insufficient funds on Blocking item (rejection with breakdown). Attempt duplicate request (rejected).

### Tests for User Story 1 (TDD) ⚠️

> **Write these tests FIRST, ensure they FAIL before implementation**

- [x] T019 [P] [US1] Unit test: CreateDisbursementRequest succeeds with sufficient budget — in tests/Application.UnitTests/Payments/CreateDisbursementRequestTests.cs
- [x] T020 [P] [US1] Unit test: CreateDisbursementRequest rejected when Blocking + insufficient funds — in tests/Application.UnitTests/Payments/CreateDisbursementRequestTests.cs
- [x] T021 [P] [US1] Unit test: CreateDisbursementRequest allowed when Warning + insufficient funds (hasWarning=true) — in tests/Application.UnitTests/Payments/CreateDisbursementRequestTests.cs
- [x] T022 [P] [US1] Unit test: CreateDisbursementRequest allowed when control=None (no check) — in tests/Application.UnitTests/Payments/CreateDisbursementRequestTests.cs
- [x] T023 [P] [US1] Unit test: CreateDisbursementRequest rejected when PO not Approved — in tests/Application.UnitTests/Payments/CreateDisbursementRequestTests.cs
- [x] T024 [P] [US1] Unit test: CreateDisbursementRequest rejected when PO net total is zero — in tests/Application.UnitTests/Payments/CreateDisbursementRequestTests.cs
- [x] T025 [P] [US1] Unit test: CreateDisbursementRequest rejected when PO already has a request (1:1 enforced) — in tests/Application.UnitTests/Payments/CreateDisbursementRequestTests.cs

### Implementation for User Story 1

- [x] T026 [US1] Implement CreateDisbursementRequestCommand + Handler + Validator in src/Application/Payments/Commands/DisbursementRequests/CreateDisbursementRequest/CreateDisbursementRequestCommand.cs (depends on T001-T012, T013-T015)
- [x] T027 [US1] Implement DisbursementRequests endpoint group (create, list, get by id) in src/Web/Endpoints/DisbursementRequests/DisbursementRequests.cs (depends on T026, T016-T017)

**Checkpoint**: Create disbursement request fully functional — budget gate working, 1:1 enforced

---

## Phase 4: User Story 2 — Dual-Signature Approval (Priority: P1)

**Goal**: Disbursement request requires dual signature: ≥2 distinct approvers, ≥1 AccountsManager or AuthorizingOfficer. All decisions recorded in ApprovalHistory.

**Independent Test**: Submit Draft → PendingApproval. First approval (AccountsManager) → still PendingApproval. Second approval (distinct user, AuthorizingOfficer) → Approved. Same user re-approval → rejected. Wrong role on first approval → rejected.

### Tests for User Story 2 (TDD) ⚠️

> **Write these tests FIRST, ensure they FAIL before implementation**

- [x] T028 [P] [US2] Unit test: Approve with first approver (AccountsManager) records step 1, stays PendingApproval — in tests/Application.UnitTests/Payments/DisbursementLifecycleTests.cs
- [x] T029 [P] [US2] Unit test: Approve with second distinct approver transitions to Approved — in tests/Application.UnitTests/Payments/DisbursementLifecycleTests.cs
- [x] T030 [P] [US2] Unit test: Same user double-approval rejected — in tests/Application.UnitTests/Payments/DisbursementLifecycleTests.cs
- [x] T031 [P] [US2] Unit test: First approver without AccountsManager/AuthorizingOfficer role rejected — in tests/Application.UnitTests/Payments/DisbursementLifecycleTests.cs
- [x] T032 [P] [US2] Unit test: Reject records in ApprovalHistory, status → Rejected — in tests/Application.UnitTests/Payments/DisbursementLifecycleTests.cs
- [x] T033 [P] [US2] Unit test: Cancel (approved, unpaid) → Cancelled, clears PO link — in tests/Application.UnitTests/Payments/DisbursementLifecycleTests.cs
- [x] T034 [P] [US2] Unit test: Submit Draft → PendingApproval — in tests/Application.UnitTests/Payments/DisbursementLifecycleTests.cs

### Implementation for User Story 2

- [x] T035 [US2] Implement SubmitDisbursementRequestCommand in src/Application/Payments/Commands/DisbursementRequests/SubmitDisbursementRequest/SubmitDisbursementRequestCommand.cs (depends on T026)
- [x] T036 [US2] Implement ApproveDisbursementRequestCommand + Handler + Validator in src/Application/Payments/Commands/DisbursementRequests/ApproveDisbursementRequest/ApproveDisbursementRequestCommand.cs (depends on T026)
- [x] T037 [US2] Implement RejectDisbursementRequestCommand in src/Application/Payments/Commands/DisbursementRequests/RejectDisbursementRequest/RejectDisbursementRequestCommand.cs (depends on T026)
- [x] T038 [US2] Implement CancelDisbursementRequestCommand + Handler + Validator in src/Application/Payments/Commands/DisbursementRequests/CancelDisbursementRequest/CancelDisbursementRequestCommand.cs (depends on T026)
- [x] T039 [US2] Add submit/approve/reject/cancel routes to DisbursementRequests endpoint group in src/Web/Endpoints/DisbursementRequests/DisbursementRequests.cs (depends on T035-T038)

**Checkpoint**: Dual-signature approval fully functional — approval history enforced

---

## Phase 5: User Story 3 — Execute Payment and Post to Ledger (Priority: P2)

**Goal**: On approval, payment is executed (method, reference, amount = PO net total). Payment posts to ledger via domain event pipeline.

**Independent Test**: Approve request (dual sig) → execute payment → Payment record created, amount matches PO net total, disbursement request → Disbursed, payment order → Paid. Ledger entry posted with balanced debits/credits.

### Tests for User Story 3 (TDD) ⚠️

> **Write these tests FIRST, ensure they FAIL before implementation**

- [x] T040 [P] [US3] Unit test: RecordPayment creates Payment with correct amount snapshot — in tests/Application.UnitTests/Payments/RecordPaymentTests.cs
- [x] T041 [P] [US3] Unit test: RecordPayment rejected when request not Approved — in tests/Application.UnitTests/Payments/RecordPaymentTests.cs
- [x] T042 [P] [US3] Unit test: RecordPayment transitions request to Disbursed, PO to Paid — in tests/Application.UnitTests/Payments/RecordPaymentTests.cs
- [x] T043 [P] [US3] Unit test: RecordPayment raises AccountingEvent domain event — in tests/Application.UnitTests/Payments/RecordPaymentTests.cs

### Implementation for User Story 3

- [x] T044 [US3] Implement RecordPaymentCommand + Handler + Validator in src/Application/Payments/Commands/Payments/RecordPayment/RecordPaymentCommand.cs (depends on T026, T036)
- [x] T045 [US3] Add AccountingEvent domain event handler for Payment in src/Application/Payments/Commands/Payments/RecordPayment/ (depends on T044)
- [x] T046 [US3] Add record/list routes to Payments endpoint group in src/Web/Endpoints/Payments/Payments.cs (depends on T044, T018)

**Checkpoint**: Payment execution fully functional — ledger posting working

---

## Phase 6: User Story 4 — Disbursement Register Report (Priority: P3)

**Goal**: Report lists requests and payments by period, fund, status with totals.

**Independent Test**: Generate report filtered by period/fund/status. Verify totals and correct row details.

### Tests for User Story 4 (TDD) ⚠️

> **Write these tests FIRST, ensure they FAIL before implementation**

- [x] T047 [P] [US4] Unit test: GetDisbursementRequests filters by status, fund, period — in tests/Application.UnitTests/Payments/GetDisbursementRequestsTests.cs
- [x] T048 [P] [US4] Unit test: GetDisbursementRequests includes payment details when linked — in tests/Application.UnitTests/Payments/GetDisbursementRequestsTests.cs
- [x] T049 [P] [US4] Unit test: GetDisbursementRequests computes totals correctly — in tests/Application.UnitTests/Payments/GetDisbursementRequestsTests.cs

### Implementation for User Story 4

- [x] T050 [US4] Update GetDisbursementRequestsQuery with full filter/total logic in src/Application/Payments/Queries/DisbursementRequests/GetDisbursementRequests/GetDisbursementRequestsQuery.cs (depends on T016)
- [x] T051 [US4] Update GetPaymentsQuery with filter logic in src/Application/Payments/Queries/Payments/GetPayments/GetPaymentsQuery.cs (depends on T018)

**Checkpoint**: Register report fully functional — all filters and totals working

---

## Phase 7: Functional Tests & Integration

**Purpose**: End-to-end functional tests covering the full disbursement lifecycle

- [x] T052 Functional test: Full lifecycle — create request → dual approval → execute payment → verify PO Paid — in tests/Application.FunctionalTests/Payments/DisbursementLifecycleTests.cs
- [x] T053 Functional test: Budget Blocking rejection with breakdown — in tests/Application.FunctionalTests/Payments/DisbursementLifecycleTests.cs
- [x] T054 Functional test: Dual-signature combinations (role, distinct user, same-user rejection) — in tests/Application.FunctionalTests/Payments/DisbursementLifecycleTests.cs
- [x] T055 Functional test: Cancellation clears PO link, allows new request — in tests/Application.FunctionalTests/Payments/DisbursementLifecycleTests.cs
- [x] T056 Functional test: Payment posting produces balanced journal entry — in tests/Application.FunctionalTests/Payments/DisbursementLifecycleTests.cs

---

## Phase 8: Polish & Cross-Cutting Concerns

**Purpose**: Validation, cleanup, and documentation

- [x] T057 Run quickstart.md validation scenarios end-to-end
- [x] T058 Run full test suite: `dotnet test tests/Application.UnitTests` + `dotnet test tests/Application.FunctionalTests`
- [x] T059 Run backend build: `dotnet build src/Web/Web.csproj` — zero warnings
- [x] T060 Update docs/database-schema.md with new tables
- [x] T061 Run NSwag regeneration: `npm run generate-api` in src/Web/ClientApp

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies — can start immediately
- **Foundational (Phase 2)**: Depends on Phase 1 completion — BLOCKS all user stories
- **US1 (Phase 3)**: Depends on Phase 2
- **US2 (Phase 4)**: Depends on Phase 2 + T026 (US1 CreateDisbursementRequestCommand — needed for approval to reference)
- **US3 (Phase 5)**: Depends on Phase 2 + T026 + T036 (US1 + US2 approval — payment requires approved request)
- **US4 (Phase 6)**: Depends on Phase 2 + T016/T018 (queries exist)
- **Functional Tests (Phase 7)**: Depends on Phases 3-6
- **Polish (Phase 8)**: Depends on all phases

### User Story Dependencies

- **US1 (Create Request)**: Independent after Foundational. Foundation for all other stories.
- **US2 (Dual Approval)**: Depends on US1 command (T026) existing — but can be developed in parallel once T026 is done.
- **US3 (Payment Execution)**: Depends on US1 + US2 — cannot execute payment without an approved request.
- **US4 (Register Report)**: Depends on queries (Phase 2) — can start once queries are built.

### Parallel Opportunities

- T001-T004: All enum/entity creation in parallel
- T007-T008: EF configurations in parallel
- T013-T015: All DTOs in parallel
- T016-T018: All queries in parallel
- T019-T025: All US1 tests in parallel
- T028-T034: All US2 tests in parallel
- T040-T043: All US3 tests in parallel
- T047-T049: All US4 tests in parallel
- US1 and US2 can overlap after T026 is complete

---

## Parallel Example: User Story 1

```
# Write all US1 tests first (parallel):
Task T019: Unit test — sufficient budget success
Task T020: Unit test — Blocking + insufficient rejection
Task T021: Unit test — Warning + insufficient allowed
Task T022: Unit test — None control no check
Task T023: Unit test — PO not Approved rejection
Task T024: Unit test — PO zero amount rejection
Task T025: Unit test — duplicate request rejection

# Then implement (sequential — single file):
Task T026: CreateDisbursementRequestCommand + Handler + Validator
Task T027: DisbursementRequests endpoint group
```

---

## Implementation Strategy

### MVP First (US1 + US2 — both P1)

1. Complete Phase 1: Setup (entities, enums, DbContext, sequences)
2. Complete Phase 2: Foundational (DTOs, queries)
3. Complete Phase 3: US1 — Create Disbursement Request
4. Complete Phase 4: US2 — Dual-Signature Approval
5. **STOP and VALIDATE**: Full request-approval cycle working
6. Deploy/demo if ready

### Incremental Delivery

1. Setup + Foundational → Foundation ready
2. Add US1 → Test independently → Deploy/Demo
3. Add US2 → Test independently → Deploy/Demo (MVP!)
4. Add US3 → Test independently → Deploy/Demo
5. Add US4 → Test independently → Deploy/Demo
6. Each story adds value without breaking previous stories

---

## Notes

- [P] tasks = different files, no dependencies
- [Story] label maps task to specific user story for traceability
- TDD mandatory: tests must be written and observed failing before implementation (Constitution Principle XI)
- Commit after each task or logical group
- Stop at any checkpoint to validate story independently
- Entity status values: DisbursementRequest uses Draft/PendingApproval/Approved/Rejected/Cancelled/Disbursed/Invalidated; Payment uses Completed/Failed
