# Tasks: Disbursement of Approved Payment Orders

**Input**: Design documents from `/specs/018-disbursement-payment-orders/`

**Prerequisites**: plan.md, spec.md, research.md, data-model.md, contracts/

**Tests**: TDD is mandatory per Constitution Principle XI. Test tasks are included for every behavioral change.

**Organization**: Tasks grouped by user story. Most code already exists — tasks focus on gaps and corrections.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (US1–US9)
- Include exact file paths in descriptions

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Verify existing code builds and tests pass before any changes

- [ ] T001 Verify backend builds: `dotnet build src/Web/Web.csproj`
- [ ] T002 [P] Verify unit tests pass: `dotnet test tests/Application.UnitTests`
- [ ] T003 [P] Verify functional tests pass: `dotnet test tests/Application.FunctionalTests`
- [ ] T004 [P] Verify frontend builds: `cd src/Web/ClientApp && npm run build`

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core gaps that block multiple user stories

**⚠ CRITICAL**: US2 and US3 cannot be fully tested until this phase completes

- [ ] T005 [P] Create UpdateDisbursementRequestCommand in `src/Application/Payments/Commands/DisbursementRequests/UpdateDisbursementRequest/UpdateDisbursementRequestCommand.cs` — command, handler, validator. Validates: status must be Draft for full edit; if PendingApproval with ≥1 approval, only Notes and Purpose may be updated (amount freeze). Uses RowVersion concurrency check.
- [ ] T006 [P] Create UpdateDisbursementRequestCommandValidator in same file — FluentValidation: RequestedAmount > 0 (when allowed), BeneficiaryName not blank, Purpose not blank, valid CurrencyId, valid FinancialYearId
- [ ] T007 [US3] Add role check to step 2 in `src/Application/Payments/Commands/DisbursementRequests/ApproveDisbursementRequest/ApproveDisbursementRequestCommand.cs` — currently step 1 checks role; step 2 must also check AccountsManager/AuthorizingOfficer role before finalizing

**Checkpoint**: Foundation ready — US2 and US3 can now be fully tested

---

## Phase 3: User Story 1 - Create Disbursement Request (Priority: P1) 🎯 MVP

**Goal**: Accountant creates a disbursement request with beneficiary, amount, currency, purpose, fiscal year. System assigns number, creates Draft, records requester.

**Independent Test**: Create request with valid data → 201, Draft, number assigned. Create with invalid data → 400 with validation errors.

### Tests for User Story 1

- [ ] T008 [P] [US1] Unit test for CreateDisbursementRequest success path in `tests/Application.UnitTests/Payments/CreateDisbursementRequestTests.cs` — verify Draft status, RequestNumber assigned, RequestedById/RequestedByName recorded
- [ ] T009 [P] [US1] Unit test for CreateDisbursementRequest validation failures — zero amount, blank beneficiary, blank purpose, invalid currency, invalid fiscal year

### Implementation for User Story 1

> **Note**: CreateDisbursementRequestCommand and handler already exist. Verify they match spec.

- [ ] T010 [US1] Verify `src/Application/Payments/Commands/DisbursementRequests/CreateDisbursementRequest/CreateDisbursementRequestCommand.cs` matches spec: validates amount > 0, beneficiary not blank, purpose not blank, valid currency, valid fiscal year, assigns DSB-{D6} number, creates Draft, records requester
- [ ] T011 [US1] Verify `src/Web/Endpoints/DisbursementRequests/DisbursementRequests.cs` POST / endpoint matches spec: PermissionCodes.DisbursementRequestsCreate, returns 201 with DTO

**Checkpoint**: US1 fully functional and tested

---

## Phase 4: User Story 2 - Edit Draft Disbursement Request (Priority: P2)

**Goal**: Any user with DisbursementRequestsUpdate permission can edit Draft requests. After first approval, amount is frozen — only notes/purpose editable.

**Independent Test**: Edit Draft → changes persist. Edit after first approval → amount rejected, notes/purpose accepted. Edit non-Draft → rejected. RowVersion mismatch → 409.

### Tests for User Story 2

- [ ] T012 [P] [US2] Unit test for UpdateDisbursementRequest success — Draft status, all fields updated, RowVersion checked
- [ ] T013 [P] [US2] Unit test for amount freeze — PendingApproval with 1 approval, amount edit rejected, notes/purpose edit accepted
- [ ] T014 [P] [US2] Unit test for non-Draft rejection — status != Draft → full edit rejected
- [ ] T015 [P] [US2] Unit test for RowVersion conflict — mismatched RowVersion → concurrency error

### Implementation for User Story 2

- [ ] T016 [US2] Verify UpdateDisbursementRequestCommand handler implements amount-freeze logic: query ApprovalHistory for count; if ≥1 approval and field is RequestedAmount/CurrencyId/BeneficiaryName/FinancialYearId → reject
- [ ] T017 [US2] Add PUT /api/DisbursementRequests/{id} endpoint in `src/Web/Endpoints/DisbursementRequests/DisbursementRequests.cs` — PermissionCodes.DisbursementRequestsUpdate, accepts UpdateDisbursementRequestCommand, returns updated DTO or 409 on concurrency
- [ ] T018 [US2] Update frontend `src/Web/ClientApp/src/features/payments/disbursement-requests/pages/DisbursementRequestDetailPage.tsx` — add edit button for Draft status, inline form for beneficiary/amount/currency/purpose/notes, submit calls PUT endpoint with rowVersion

**Checkpoint**: US2 fully functional — draft editing with amount-freeze protection

---

## Phase 5: User Story 3 - Dual-Signature Approval (Priority: P1)

**Goal**: Two distinct qualified approvers must approve. Both record approved amount, authority name, capacity. Amount frozen after first. Second approval auto-generates PaymentOrder.

**Independent Test**: Submit → PendingApproval. First approve (qualified) → step 1 recorded. Second approve (distinct, same amount) → Approved + order generated. Same user → rejected. Wrong role → rejected. Amount mismatch → rejected.

### Tests for User Story 3

- [ ] T019 [P] [US3] Unit test for dual-signature happy path — two distinct qualified approvers, same amount, order generated
- [ ] T020 [P] [US3] Unit test for same-user rejection — same approver tries twice → rejected
- [ ] T021 [P] [US3] Unit test for role check on both steps — step 1 without role → rejected; step 2 without role → rejected
- [ ] T022 [P] [US3] Unit test for amount mismatch — step 2 amount ≠ step 1 → rejected
- [ ] T023 [P] [US3] Unit test for amount exceeds requested — approved amount > requested → rejected

### Implementation for User Story 3

- [ ] T024 [US3] Verify ApproveDisbursementRequestCommand step 2 role check (from T007) works correctly
- [ ] T025 [US3] Verify order auto-generation in ApproveDisbursementRequestCommand: FundId=0, AppropriationId=null, AmountGross=approvedAmount, BeneficiaryName copied, IssuingAuthorityName/Capacity copied, DisbursementRequestId linked
- [ ] T026 [US3] Verify frontend `src/Web/ClientApp/src/features/payments/disbursement-requests/pages/DisbursementRequestDetailPage.tsx` approval dialog captures: approvedAmount, issuingAuthorityName, issuingAuthorityCapacity, rowVersion

**Checkpoint**: US3 fully functional — dual-signature with authority tracking and order generation

---

## Phase 6: User Story 5 - Order Lifecycle and Budget Check (Priority: P1)

**Goal**: Auto-generated order follows Draft → Submitted → Approved → SentToTreasury → Paid. User prepares order (Fund, Appropriation, deductions) before submit. Budget check at submission.

**Independent Test**: Prepare order → set Fund/Appropriation. Submit → budget check runs. Approve → status change. Send to treasury → treasury details recorded. Failed budget → blocks approval.

### Tests for User Story 5

- [ ] T027 [P] [US5] Unit test for order preparation — UpdatePaymentOrder on Draft order with FundId/AppropriationId/deductions
- [ ] T028 [P] [US5] Unit test for submit with budget check — FundId/AppropriationId required, BudgetCheckStatus set
- [ ] T029 [P] [US5] Unit test for failed budget blocks approval — BudgetCheckStatus=Failed → approve rejected
- [ ] T030 [P] [US5] Unit test for budget override — OverrideFailedBudgetCheck=true with permission → approve succeeds

### Implementation for User Story 5

- [ ] T031 [US5] Verify SubmitPaymentOrderCommand validates FundId > 0 and AppropriationId has value before running budget check
- [ ] T032 [US5] Verify ApprovePaymentOrderCommand checks BudgetCheckStatus before allowing approval; supports OverrideFailedBudgetCheck with PaymentOrdersOverrideBudgetCheck permission
- [ ] T033 [US5] Verify SendToTreasuryCommand records TreasuryReference and TreasurySentAt
- [ ] T034 [US5] Update frontend `src/Web/ClientApp/src/features/payments/payment-orders/pages/PaymentOrderDetailPage.tsx` — show "Prepare" action for Draft orders auto-generated from requests (FundId=0), display budget check status badge

**Checkpoint**: US5 fully functional — order lifecycle with budget control

---

## Phase 7: User Story 6 - Deductions on Payment Orders (Priority: P1)

**Goal**: Line-item deductions on Draft orders. Mandatory protection. Sum validation. Tax authority required for tax deductions.

**Independent Test**: Create order with deductions → sum matches. Update → mandatory ones protected. Tax deduction without authority → rejected.

### Tests for User Story 6

- [ ] T035 [P] [US6] Unit test for deduction sum validation — sum(deductions.amount) must equal DeductionAmount within 0.01
- [ ] T036 [P] [US6] Unit test for mandatory deduction protection — IsMandatory=true deduction removed → rejected
- [ ] T037 [P] [US6] Unit test for tax deduction validation — IsTaxDeduction=true without TaxAuthorityId → rejected

### Implementation for User Story 6

- [ ] T038 [US6] Verify CreatePaymentOrderCommand and UpdatePaymentOrderCommand enforce: sum match, mandatory protection (BR-4), tax authority check, DeductionAmount ≤ AmountGross
- [ ] T039 [US6] Verify GetPaymentOrderTotalsQuery computes: TotalDeductions, NetAmount, PaidAmount, RemainingAmount, IsFullyPaid

**Checkpoint**: US6 fully functional — deduction lifecycle with protections

---

## Phase 8: User Story 7 - Record Payment (Priority: P1)

**Goal**: Record payment against Approved/SentToTreasury order. One payment per order. Triggers ledger posting. Updates order to Paid, request to Disbursed.

**Independent Test**: Record payment → Payment created, order Paid, request Disbursed. Second payment → rejected. Non-approved order → rejected.

### Tests for User Story 7

- [ ] T040 [P] [US7] Unit test for payment recording — Amount = AmountGross - DeductionAmount, order → Paid, request → Disbursed
- [ ] T041 [P] [US7] Unit test for duplicate payment prevention — second payment on same order → rejected
- [ ] T042 [P] [US7] Unit test for payment status guard — only Approved/SentToTreasury orders accepted

### Implementation for User Story 7

- [ ] T043 [US7] Verify RecordPaymentCommand: checks order status, checks for existing payment, computes netTotal, creates Payment, updates order status, updates request status, emits PaymentRecordedEvent
- [ ] T044 [US7] Verify PaymentRecordedEvent handler creates balanced JournalEntry via posting pipeline

**Checkpoint**: US7 fully functional — payment execution with ledger posting

---

## Phase 9: User Story 4 - Cancel Disbursement Request (Priority: P2)

**Goal**: Cancel at Draft/PendingApproval/Approved(unpaid). DisbursementRequestsCancel permission required. Invalidates linked order.

**Independent Test**: Cancel Draft → Cancelled. Cancel PendingApproval → Cancelled + approval history. Cancel Approved with Draft order → both invalidated. Cancel with Paid order → rejected.

### Tests for User Story 9

- [ ] T045 [P] [US4] Unit test for cancel at each status — Draft, PendingApproval, Approved(unpaid)
- [ ] T046 [P] [US4] Unit test for cancel blocked when order paid — Approved request with Paid order → rejected
- [ ] T047 [P] [US4] Unit test for order invalidation — cancel Approved request → linked order → Cancelled

### Implementation for User Story 4

- [ ] T048 [US4] Verify CancelDisbursementRequestCommand: checks DisbursementRequestsCancel permission, validates status, checks if linked order is paid, invalidates order if exists, records in ApprovalHistory

**Checkpoint**: US4 fully functional — cancellation with cascading invalidation

---

## Phase 10: User Story 9 - Void Payment Order (Priority: P2)

**Goal**: Void Approved/SentToTreasury orders with no payment. Terminal state. Invalidates linked request.

**Independent Test**: Void Approved order → Voided, request Invalidated. Void Paid order → rejected.

### Tests for User Story 9

- [ ] T049 [P] [US9] Unit test for void happy path — Approved order, no payment → Voided, request Invalidated
- [ ] T050 [P] [US9] Unit test for void blocked when paid → rejected

### Implementation for User Story 9

- [ ] T051 [US9] Verify VoidPaymentOrderCommand: checks no completed payment, transitions to Voided, invalidates linked DisbursementRequest

**Checkpoint**: US9 fully functional — void with cascading invalidation

---

## Phase 11: User Story 8 - List and Detail Views (Priority: P1)

**Goal**: List and detail views for requests and orders with filtering and complete information display.

**Independent Test**: List with filters → correct results. Detail → all required fields shown.

### Tests for User Story 8

- [ ] T052 [P] [US8] Unit test for GetDisbursementRequestsQuery — filter by status, requestedById
- [ ] T053 [P] [US8] Unit test for GetDisbursementRequestByIdQuery — returns full detail with approvals, order link, payment info

### Implementation for User Story 8

- [ ] T054 [US8] Verify GetDisbursementRequestsQuery returns correct DTOs with filtering
- [ ] T055 [US8] Verify GetDisbursementRequestByIdQuery loads: header, ApprovalHistory steps (with amounts, authority details), linked PaymentOrderNumber, Payment info
- [ ] T056 [US8] Verify GetPaymentOrdersQuery and GetPaymentOrderByIdQuery load: header, deductions, linked DisbursementRequestNumber
- [ ] T057 [US8] Verify frontend list pages: `DisbursementRequestsListPage.tsx` shows number, beneficiary, amount, status, date with status/requester filters
- [ ] T058 [US8] Verify frontend detail pages: `DisbursementRequestDetailPage.tsx` shows all fields, approval history with amounts/authority, order link, payment info
- [ ] T059 [US8] Verify frontend `PaymentOrderDetailPage.tsx` shows: beneficiary details, gross/deductions/net, deduction line items, budget check status, treasury details, request link, approval history, payment info

**Checkpoint**: US8 fully functional — complete visibility into disbursement pipeline

---

## Phase 12: Polish & Cross-Cutting Concerns

**Purpose**: Final validation and edge cases

- [ ] T060 [P] Run quickstart.md validation scenarios V1–V18 end-to-end
- [ ] T061 [P] Verify all permission codes are bound to endpoints (DisbursementRequests: View/Create/Update/Submit/Approve/Reject/Cancel; PaymentOrders: View/Create/Update/Submit/Approve/Reject/Cancel/SendToTreasury/Void/OverrideBudgetCheck; Payments: View/Create)
- [ ] T062 [P] Verify RowVersion concurrency on all mutating endpoints — every PATCH/PUT accepts and validates rowVersion
- [ ] T063 [P] Verify append-only audit: ApprovalHistory and DocumentStatusLog records are INSERT-ONLY
- [ ] T064 Run full test suite: `dotnet test tests/Application.UnitTests && dotnet test tests/Application.FunctionalTests`
- [ ] T065 Run frontend lint and build: `cd src/Web/ClientApp && npm run lint && npm run build`

---

## Dependencies & Execution Order

### Phase Dependencies

- **Phase 1 (Setup)**: No dependencies — start immediately
- **Phase 2 (Foundational)**: Depends on Phase 1 — BLOCKS US2, US3
- **Phases 3–11 (User Stories)**: Depend on Phase 2; can proceed in parallel after Phase 2
- **Phase 12 (Polish)**: Depends on all desired stories complete

### User Story Dependencies

```
Phase 1: Setup
    ↓
Phase 2: Foundational (T005-T007)
    ↓
    ├── Phase 3:  US1  Create Request (P1) ─────── independent
    ├── Phase 4:  US2  Edit Draft (P2) ──────────── depends on T005 (UpdateCommand)
    ├── Phase 5:  US3  Dual-Signature (P1) ─────── depends on T007 (step 2 role check)
    ├── Phase 6:  US5  Order Lifecycle (P1) ─────── depends on US3 (order generated on approval)
    ├── Phase 7:  US6  Deductions (P1) ──────────── depends on US5 (order preparation)
    ├── Phase 8:  US7  Record Payment (P1) ──────── depends on US5 (order in Approved status)
    ├── Phase 9:  US4  Cancel (P2) ──────────────── depends on US3 (order exists to invalidate)
    ├── Phase 10: US9  Void (P2) ────────────────── depends on US5 (order lifecycle)
    └── Phase 11: US8  Views (P1) ───────────────── depends on all above (queries read all entities)
    ↓
Phase 12: Polish
```

### Within Each User Story

1. Tests FIRST (red)
2. Implementation (green)
3. Verify tests pass
4. Commit

### Parallel Opportunities

- **Phase 1**: T002, T003, T004 can run in parallel
- **Phase 2**: T005, T006 can run in parallel; T007 independent
- **Phases 3–10**: Once Phase 2 completes, all user story phases can start in parallel (different files, different entities)
- **Within each story**: Test tasks marked [P] can run in parallel
- **Phase 12**: T060–T063 can run in parallel

---

## Implementation Strategy

### MVP First (US1 + US3 + US5 + US7)

1. Complete Phase 1: Setup — verify build and tests
2. Complete Phase 2: Foundational — UpdateCommand + step 2 role check
3. Complete Phase 3: US1 — Create request
4. Complete Phase 5: US3 — Dual-signature approval + order generation
5. Complete Phase 6: US5 — Order lifecycle + budget check
6. Complete Phase 8: US7 — Record payment + ledger posting
7. **STOP and VALIDATE**: Run quickstart V1, V5, V9, V12
8. Deploy/demo if ready

### Incremental Delivery

1. Setup + Foundational → Foundation ready
2. US1 (Create) → Test → MVP entry point
3. US3 (Approve) → Test → Requests can be approved
4. US5 (Order Lifecycle) → Test → Orders can be prepared and submitted
5. US7 (Payment) → Test → Full disbursement cycle works
6. US2 (Edit) → Test → Draft corrections supported
7. US4 (Cancel) → Test → Cancellation supported
8. US9 (Void) → Test → Void supported
8. US6 (Deductions) → Test → Financial controls complete
9. US8 (Views) → Test → Full visibility
10. Polish → All validation scenarios pass

---

## Notes

- [P] tasks = different files, no dependencies
- [Story] label maps task to specific user story for traceability
- Most code already exists — tasks focus on gaps (T005–T007) and verification
- TDD mandatory: write tests first, observe red, implement, green
- Commit after each task or logical group
- Stop at any checkpoint to validate story independently
