# Tasks: Receipt Voucher & Deposit Slip Workflow

**Input**: Design documents from `/specs/017-receipt-voucher-deposit/`

**Prerequisites**: plan.md (required), spec.md (required for user stories), research.md, data-model.md, contracts/

**Tests**: TDD required per Constitution Principle XI. Test tasks included for each user story.

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Project initialization and basic structure

- [x] T001 Add PermissionCodes for ReceiptVouchers, DepositSlips, Checks in src/Application/Common/Security/PermissionCodes.cs
- [x] T002 Add ReceiptVoucherCollected and CheckCleared to EventType enum in src/Domain/Accounting/Enums/EventType.cs
- [x] T003 [P] Create ReceiptVoucherStatus enum in src/Domain/Revenue/Enums/ReceiptVoucherStatus.cs
- [x] T004 [P] Create PaymentMethod enum in src/Domain/Revenue/Enums/PaymentMethod.cs
- [x] T005 [P] Create CheckStatus enum in src/Domain/Revenue/Enums/CheckStatus.cs
- [x] T006 [P] Create DepositSlipStatus enum in src/Domain/Revenue/Enums/DepositSlipStatus.cs
- [x] T007 [P] Create FormType enum in src/Domain/Revenue/Enums/FormType.cs

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core infrastructure that MUST be complete before ANY user story can be implemented

**⚠️ CRITICAL**: No user story work can begin until this phase is complete

- [x] T008 Create ReceiptVoucher entity in src/Domain/Revenue/Entities/ReceiptVoucher.cs
- [x] T009 Create ReceiptVoucherLine entity in src/Domain/Revenue/Entities/ReceiptVoucherLine.cs
- [x] T010 Create Check entity in src/Domain/Revenue/Entities/Check.cs
- [x] T011 Create DepositSlip entity in src/Domain/Revenue/Entities/DepositSlip.cs
- [x] T012 Create ReceiptVoucherCollected domain event in src/Domain/Events/Revenue/ReceiptVoucherCollected.cs
- [x] T013 Create CheckCleared domain event in src/Domain/Events/Revenue/CheckCleared.cs
- [x] T014 [P] Create ReceiptVoucherConfiguration in src/Infrastructure/Data/Configurations/ReceiptVoucherConfiguration.cs
- [x] T015 [P] Create ReceiptVoucherLineConfiguration in src/Infrastructure/Data/Configurations/ReceiptVoucherLineConfiguration.cs
- [x] T016 [P] Create CheckConfiguration in src/Infrastructure/Data/Configurations/CheckConfiguration.cs
- [x] T017 [P] Create DepositSlipConfiguration in src/Infrastructure/Data/Configurations/DepositSlipConfiguration.cs
- [ ] T018 Generate EF Core migration in src/Infrastructure/Data/Migrations/
- [x] T019 Create DTOs for ReceiptVoucher in src/Application/Revenue/Common/DTOs/ReceiptVoucherDtos.cs

**Checkpoint**: Foundation ready - user story implementation can now begin in parallel

---

## Phase 3: User Story 1 - Record Revenue Collection (Priority: P1) ⭐ MVP

**Goal**: Cashier creates receipt voucher with line items and check details

**Independent Test**: Create voucher with multiple line items (cash + check), verify number assignment, Draft status

### Tests for User Story 1

> **NOTE: Write these tests FIRST, ensure they FAIL before implementation**

- [x] T020 [P] [US1] Unit test for CreateReceiptVoucherCommand in tests/Domain.UnitTests/Revenue/ReceiptVoucherTests.cs
- [x] T021 [P] [US1] Functional test for voucher creation in tests/Application.FunctionalTests/Revenue/ReceiptVoucherTests.cs

### Implementation for User Story 1

- [x] T022 [US1] Implement CreateReceiptVoucherCommand in src/Application/Revenue/Commands/ReceiptVouchers/CreateReceiptVoucher/CreateReceiptVoucherCommand.cs
- [x] T023 [US1] Implement GetReceiptVouchersQuery in src/Application/Revenue/Queries/ReceiptVouchers/GetReceiptVouchers/GetReceiptVouchersQuery.cs
- [x] T024 [US1] Implement GetReceiptVoucherByIdQuery in src/Application/Revenue/Queries/ReceiptVouchers/GetReceiptVoucherById/GetReceiptVoucherByIdQuery.cs
- [x] T025 [US1] Implement GetReceiptVouchersByPartyQuery in src/Application/Revenue/Queries/ReceiptVouchers/GetReceiptVouchersByParty/GetReceiptVouchersByPartyQuery.cs
- [x] T026 [US1] Implement GetReceiptVouchersByPeriodQuery in src/Application/Revenue/Queries/ReceiptVouchers/GetReceiptVouchersByPeriod/GetReceiptVouchersByPeriodQuery.cs
- [x] T027 [US1] Create ReceiptVouchers endpoint group in src/Web/Endpoints/Revenue/ReceiptVouchers.cs

**Checkpoint**: Cashier can create and view receipt vouchers with line items

---

## Phase 4: User Story 2 - Review and Approve Voucher (Priority: P1)

**Goal**: Accounts reviewer gate enforced, vouchers transition Draft → PendingReview → Approved

**Independent Test**: Submit voucher for review, approve with different user, verify status and ApprovalHistory

### Tests for User Story 2

- [x] T028 [P] [US2] Unit test for reviewer gate enforcement in tests/Domain.UnitTests/Revenue/ReceiptVoucherTests.cs
- [x] T029 [P] [US2] Functional test for submit/approve flow in tests/Application.FunctionalTests/Revenue/ReceiptVoucherTests.cs

### Implementation for User Story 2

- [x] T030 [US2] Implement SubmitReceiptVoucherCommand in src/Application/Revenue/Commands/ReceiptVouchers/SubmitReceiptVoucher/SubmitReceiptVoucherCommand.cs
- [x] T031 [US2] Implement ApproveReceiptVoucherCommand in src/Application/Revenue/Commands/ReceiptVouchers/ApproveReceiptVoucher/ApproveReceiptVoucherCommand.cs
- [x] T032 [US2] Implement CancelReceiptVoucherCommand in src/Application/Revenue/Commands/ReceiptVouchers/CancelReceiptVoucher/CancelReceiptVoucherCommand.cs
- [x] T033 [US2] Add submit/approve/cancel routes to ReceiptVouchers endpoint in src/Web/Endpoints/Revenue/ReceiptVouchers.cs

**Checkpoint**: Voucher lifecycle complete (Draft → PendingReview → Approved/Cancelled)

---

## Phase 5: User Story 3 - Create Deposit Slip (Priority: P2)

**Goal**: Cashier batches approved vouchers into Form 47 (cash) or Form 48 (checks) slips

**Independent Test**: Create Form 47/48 slips, verify homogeneity enforcement and date validation

### Tests for User Story 3

- [x] T034 [P] [US3] Unit test for form type homogeneity in tests/Domain.UnitTests/Revenue/DepositSlipTests.cs
- [x] T035 [P] [US3] Functional test for slip creation in tests/Application.FunctionalTests/Revenue/DepositSlipTests.cs

### Implementation for User Story 3

- [x] T036 [US3] Implement CreateDepositSlipCommand in src/Application/Revenue/Commands/DepositSlips/CreateDepositSlip/CreateDepositSlipCommand.cs
- [x] T037 [US3] Implement AddVoucherToSlipCommand in src/Application/Revenue/Commands/DepositSlips/AddVoucherToSlip/AddVoucherToSlipCommand.cs
- [x] T038 [US3] Implement RemoveVoucherFromSlipCommand in src/Application/Revenue/Commands/DepositSlips/RemoveVoucherFromSlip/RemoveVoucherFromSlipCommand.cs
- [x] T039 [US3] Implement GetDepositSlipsQuery in src/Application/Revenue/Queries/DepositSlips/GetDepositSlips/GetDepositSlipsQuery.cs
- [x] T040 [US3] Implement GetDepositSlipByIdQuery in src/Application/Revenue/Queries/DepositSlips/GetDepositSlipById/GetDepositSlipByIdQuery.cs
- [x] T041 [US3] Create DepositSlips endpoint group in src/Web/Endpoints/Revenue/DepositSlips.cs

**Checkpoint**: Cashier can create and manage deposit slips with type enforcement

---

## Phase 6: User Story 4 - Check Clearing and Bounced Check Handling (Priority: P2)

**Goal**: Two-stage revenue recognition for Form 48, bounced check voucher reopening

**Independent Test**: Clear check (revenue recognized), bounce check (new voucher created)

### Tests for User Story 4

- [x] T042 [P] [US4] Unit test for check clearing posting in tests/Domain.UnitTests/Revenue/CheckTests.cs
- [x] T043 [P] [US4] Functional test for bounce handling in tests/Application.FunctionalTests/Revenue/CheckTests.cs

### Implementation for User Story 4

- [x] T044 [US4] Implement ClearCheckCommand in src/Application/Revenue/Commands/Checks/ClearCheck/ClearCheckCommand.cs
- [x] T045 [US4] Implement BounceCheckCommand in src/Application/Revenue/Commands/Checks/BounceCheck/BounceCheckCommand.cs
- [x] T046 [US4] Implement ReplaceCheckCommand in src/Application/Revenue/Commands/Checks/ReplaceCheck/ReplaceCheckCommand.cs
- [x] T047 [US4] Implement ReceiptVoucherCollectedHandler for Form 47 posting in src/Application/Revenue/EventHandlers/ReceiptVoucherCollectedHandler.cs
- [x] T048 [US4] Implement CheckClearedHandler for Form 48 posting in src/Application/Revenue/EventHandlers/CheckClearedHandler.cs
- [x] T049 [US4] Create Checks endpoint group in src/Web/Endpoints/Revenue/Checks.cs

**Checkpoint**: Two-stage revenue recognition complete, bounced checks handled

---

## Phase 7: User Story 5 - Monthly Collections Statement (Priority: P3)

**Goal**: Generate monthly statement per financial law for any month and fund

**Independent Test**: Generate statement for specific month/fund, verify all collections included

### Tests for User Story 5

- [x] T050 [P] [US5] Functional test for statement generation in tests/Application.FunctionalTests/Revenue/StatementTests.cs

### Implementation for User Story 5

- [x] T051 [US5] Implement ApproveDepositSlipCommand in src/Application/Revenue/Commands/DepositSlips/ApproveDepositSlip/ApproveDepositSlipCommand.cs
- [x] T052 [US5] Implement GetMonthlyStatementQuery in src/Application/Revenue/Queries/Statements/GetMonthlyStatement/GetMonthlyStatementQuery.cs
- [x] T053 [US5] Add approve route to DepositSlips endpoint in src/Web/Endpoints/Revenue/DepositSlips.cs
- [x] T054 [US5] Add monthly statement route to DepositSlips endpoint in src/Web/Endpoints/Revenue/DepositSlips.cs

**Checkpoint**: All user stories complete, monthly statement generation functional

---

## Phase 8: Migration & Data Migration

**Purpose**: Migrate from RevenueReceipt to ReceiptVoucher system

- [ ] T055 Implement data migration script in src/Infrastructure/Data/Migrations/
- [x] T056 Update PostingRules EventType from "RevenueReceiptPosted" to "ReceiptVoucherCollected"
- [x] T057 Register new event types in AccountingEvent configuration
- [ ] T058 Drop RevenueReceipt and RevenueReceiptLine tables after verification

---

## Phase 9: Polish & Cross-Cutting Concerns

**Purpose**: Improvements that affect multiple user stories

- [x] T059 [P] Add status history logging using IDocumentStatusLogger across all commands
- [x] T060 [P] Add ApprovalHistory recording for voucher and slip approvals
- [ ] T061 [P] Run quickstart.md validation scenarios
- [x] T062 [P] Update OpenAPI documentation for new endpoints
- [x] T063 Code cleanup and remove old RevenueReceipt code
- [ ] T064 Verify all tests pass and mark any stub tests as debt

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies - can start immediately
- **Foundational (Phase 2)**: Depends on Setup completion - BLOCKS all user stories
- **User Stories (Phase 3-7)**: All depend on Foundational phase completion
  - US1 and US2 can proceed in parallel (both P1)
  - US3 depends on US1 (vouchers must exist before slip creation)
  - US4 depends on US3 (slip approval triggers check status changes)
  - US5 depends on US3 (statement queries slip data)
- **Migration (Phase 8)**: Depends on all user stories being complete
- **Polish (Phase 9)**: Depends on Migration completion

### User Story Dependencies

- **User Story 1 (P1)**: Can start after Foundational (Phase 2) - No dependencies on other stories
- **User Story 2 (P1)**: Can start after Foundational (Phase 2) - Depends on US1 (vouchers must exist)
- **User Story 3 (P2)**: Can start after US1 and US2 complete - Requires approved vouchers
- **User Story 4 (P2)**: Can start after US3 complete - Requires deposit slips with checks
- **User Story 5 (P3)**: Can start after US3 complete - Queries slip and check data

### Within Each User Story

- Tests MUST be written and FAIL before implementation
- Enums before entities
- Entities before commands/queries
- Commands/queries before endpoints
- Core implementation before integration

### Parallel Opportunities

- All Setup tasks marked [P] can run in parallel
- All Foundational tasks marked [P] can run in parallel (within Phase 2)
- US1 and US2 can be worked on in parallel after Foundational phase
- All tests for a user story marked [P] can run in parallel
- Configurations marked [P] can run in parallel

---

## Parallel Example: User Story 1

```bash
# Launch all tests for User Story 1 together:
Task: "Unit test for CreateReceiptVoucherCommand in tests/Domain.UnitTests/Revenue/ReceiptVoucherTests.cs"
Task: "Functional test for voucher creation in tests/Application.FunctionalTests/Revenue/ReceiptVoucherTests.cs"

# Launch all queries for User Story 1 together:
Task: "Implement GetReceiptVouchersQuery"
Task: "Implement GetReceiptVoucherByIdQuery"
Task: "Implement GetReceiptVouchersByPartyQuery"
Task: "Implement GetReceiptVouchersByPeriodQuery"
```

---

## Implementation Strategy

### MVP First (User Story 1 + 2)

1. Complete Phase 1: Setup
2. Complete Phase 2: Foundational (CRITICAL - blocks all stories)
3. Complete Phase 3: User Story 1 (Record Revenue Collection)
4. Complete Phase 4: User Story 2 (Review and Approve Voucher)
5. **STOP and VALIDATE**: Test voucher creation and approval flow
6. Deploy/demo if ready

### Incremental Delivery

1. Complete Setup + Foundational → Foundation ready
2. Add US1 + US2 → Test independently → Deploy/Demo (MVP!)
3. Add US3 → Test deposit slips → Deploy/Demo
4. Add US4 → Test check clearing → Deploy/Demo
5. Add US5 → Test monthly statement → Deploy/Demo
6. Each story adds value without breaking previous stories

### Parallel Team Strategy

With multiple developers:

1. Team completes Setup + Foundational together
2. Once Foundational is done:
   - Developer A: User Story 1 (Create Voucher)
   - Developer B: User Story 2 (Review/Approve)
3. After US1 + US2:
   - Developer A: User Story 3 (Deposit Slip)
   - Developer B: User Story 4 (Check Clearing)
4. After US3 + US4:
   - Developer A: User Story 5 (Monthly Statement)
   - Developer B: Migration & Polish

---

## Notes

- [P] tasks = different files, no dependencies
- [Story] label maps task to specific user story for traceability
- Each user story should be independently completable and testable
- Verify tests fail before implementing
- Commit after each task or logical group
- Stop at any checkpoint to validate story independently
- Avoid: vague tasks, same file conflicts, cross-story dependencies that break independence
- Constitution Principle XI requires TDD: every behavior change driven by a test that failed first
