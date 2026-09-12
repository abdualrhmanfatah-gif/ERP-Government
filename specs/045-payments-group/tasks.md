# Tasks: Payments Group (PAY-01..04)

**Input**: Design documents from `/specs/045-payments-group/`

**Prerequisites**: plan.md (required), spec.md (required for user stories), research.md, data-model.md, contracts/, frontend-requirements.md, frontend-disbursement-requests.md

**Tests**: NO automated tests (Constitution XI exception DEP-027). Backend gate: `dotnet build src/Web/Web.csproj`. Frontend gate: `npm run lint` + `npm run build`. Validation via quickstart.md scenarios (S1–S6).

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

## Phase 1: Setup — ADR-001 + ADR-002 Schema Migration

**Purpose**: Apply ADR-001 breaking schema changes, ADR-002 BeneficiaryPartyId re-introduction, and register new permission code. All subsequent phases depend on this.

- [ ] T001 Create EF Core migration for ADR-001 schema changes: DROP BeneficiaryPartyId from PaymentOrder, DROP BeneficiaryId/HasWarning/PaymentOrderId/PaymentOrderNumber/ApprovedAmount/IssuingAuthorityName/IssuingAuthorityCapacity/ApprovalDate from DisbursementRequest, DROP PaymentOrderLine table, ADD AccountId to PaymentOrder, ADD UNIQUE index on PaymentOrder.DisbursementRequestId, ADD UNIQUE index on Payment.PaymentOrderId in src/Infrastructure/Data/Migrations/
- [ ] T002 Create EF Core migration for ADR-002: ADD BeneficiaryPartyId (int?) to PaymentOrder and DisbursementRequest with Restrict FK to Party, ADD index on BeneficiaryPartyId in src/Infrastructure/Data/Migrations/
- [ ] T003 [P] Add PaymentOrders.OverrideBudgetCheck permission code to src/Application/Common/Security/PermissionCodes.cs
- [ ] T004 [P] Register PaymentOrders.OverrideBudgetCheck policy in src/Web/DependencyInjection.cs
- [ ] T005 [P] Add PaymentOrders.OverrideBudgetCheck to RolePermissionSeedData.GetAllPermissionCodes() for idempotent merge in src/Infrastructure/Data/

**Checkpoint**: Schema migrations applied. Permission code registered. Backend builds: `dotnet build src/Web/Web.csproj`

---

## Phase 2: PAY-01 — Payment Order Lifecycle (P1) 🎯 MVP

**Goal**: Complete payment order CRUD with 8-state lifecycle, budget check, treasury trace, and server-issued totals

**Independent Test**: Create order → submit (budget check) → approve → send to treasury. Verify totals from /{id}/totals. Verify budget check badges. Verify conditional action buttons per lifecycle.

### Implementation for PAY-01

#### Entity & Enum Adjustments

- [ ] T006 [P] Remove PaymentOrderLine entity and its DbSet from src/Domain/Payments/Entities/PaymentOrder.cs and src/Infrastructure/Data/ApplicationDbContext.cs
- [ ] T007 [P] Add AccountId (int?) property to PaymentOrder entity in src/Domain/Payments/Entities/PaymentOrder.cs
- [ ] T008 [P] Add BeneficiaryPartyId (int?) to PaymentOrder entity with Restrict FK to Party in src/Domain/Payments/Entities/PaymentOrder.cs (ADR-002)
- [ ] T009 [P] Verify PaymentOrderStatus enum has 8 states (Draft/Submitted/Approved/SentToTreasury/Paid/Cancelled/Rejected/Voided) — remove PartiallyPaid if present in src/Domain/Payments/Enums/PaymentOrderStatus.cs
- [ ] T010 [P] Verify BudgetCheckStatus enum has 4 states (Pending/Passed/Failed/Overridden) in src/Domain/Payments/Enums/BudgetCheckStatus.cs

#### Commands

- [ ] T011 [P] [US1] Create CreatePaymentOrderCommand + Handler + Validator in src/Application/Payments/Commands/PaymentOrders/CreatePaymentOrder/ — order-first path (beneficiaryName, appropriationId required, deductions, accountId optional, beneficiaryPartyId optional with server validation per ADR-002)
- [ ] T012 [P] [US1] Create UpdatePaymentOrderCommand + Handler + Validator in src/Application/Payments/Commands/PaymentOrders/UpdatePaymentOrder/ — Draft-only editing
- [ ] T013 [US2] Create SubmitPaymentOrderCommand + Handler + Validator in src/Application/Payments/Commands/PaymentOrders/SubmitPaymentOrder/ — fundId + appropriationId + accountId required at submit; budget check via BudgetAvailabilityService; sets BudgetCheckStatus
- [ ] T014 [US2] Create ApprovePaymentOrderCommand + Handler + Validator in src/Application/Payments/Commands/PaymentOrders/ApprovePaymentOrder/ — budgetCheckStatus != Failed guard; optional overrideFailedBudgetCheck flag gated by PaymentOrders.OverrideBudgetCheck permission; sets Overridden when flag+permission
- [ ] T015 [US3] Create RejectPaymentOrderCommand + Handler + Validator in src/Application/Payments/Commands/PaymentOrders/RejectPaymentOrder/ — reason required
- [ ] T016 [US3] Create CancelPaymentOrderCommand + Handler + Validator in src/Application/Payments/Commands/PaymentOrders/CancelPaymentOrder/ — auto-invalidates linked Draft/PendingApproval requests
- [ ] T017 [US3] Create SendToTreasuryPaymentOrderCommand + Handler + Validator in src/Application/Payments/Commands/PaymentOrders/SendToTreasuryPaymentOrder/ — treasuryReference required; sets treasury trace fields
- [ ] T018 [US5] Create VoidPaymentOrderCommand + Handler + Validator in src/Application/Payments/Commands/PaymentOrders/VoidPaymentOrder/ — guard: Approved/SentToTreasury + unpaid only; auto-invalidates linked requests; partially paid refused

#### Queries

- [ ] T019 [P] [US1] Create GetPaymentOrdersQuery + Handler in src/Application/Payments/Queries/PaymentOrders/GetPaymentOrders/ — list with filters (status/budgetCheckStatus/period)
- [ ] T020 [P] [US4] Create GetPaymentOrderByIdQuery + Handler in src/Application/Payments/Queries/PaymentOrders/GetPaymentOrderById/
- [ ] T021 [P] [US4] Create GetPaymentOrderTotalsQuery + Handler in src/Application/Payments/Queries/PaymentOrders/GetPaymentOrderTotals/ — server-issued: amountGross, totalDeductions, netAmount, paidAmount, remainingAmount, isFullyPaid

#### Endpoints

- [ ] T022 [US1] Create PaymentOrders endpoint group with CRUD + lifecycle routes in src/Web/Endpoints/Payments/PaymentOrders.cs — GET /, POST /, GET /{id}, GET /{id}/totals, POST /{id}/submit, POST /{id}/approve, POST /{id}/reject, POST /{id}/cancel, POST /{id}/send-to-treasury, POST /{id}/void, PUT /{id}; authorize each with PaymentOrders.* permission codes

**Checkpoint**: Payment order lifecycle complete. `dotnet build src/Web/Web.csproj` passes. Quickstart S1 and S2 scenarios manually verifiable.

---

## Phase 3: PAY-02 — Disbursement Requests — Request-First Path (P1)

**Goal**: Independent request creation, dual-signature approval, atomic order generation, cancellation/invalidation, chain tracing

**Independent Test**: Create request → submit → two qualified approvals → verify Draft order auto-generated → verify order follows full lifecycle. Verify rejection with reason. Verify cancellation releases order. Verify invalidation propagation.

### Implementation for PAY-02

#### Entity Adjustments

- [ ] T023 [P] Add BeneficiaryPartyId (int?) to DisbursementRequest entity with Restrict FK to Party in src/Domain/Payments/Entities/DisbursementRequest.cs (ADR-002)
- [ ] T024 [P] Remove HasWarning from DisbursementRequest in src/Domain/Payments/Entities/DisbursementRequest.cs
- [ ] T025 [P] Remove ApprovedAmount, IssuingAuthorityName, IssuingAuthorityCapacity, ApprovalDate from DisbursementRequest (approval data in ApprovalHistory only) in src/Domain/Payments/Entities/DisbursementRequest.cs
- [ ] T026 [P] Verify DisbursementRequestStatus enum has 7 states (Draft/PendingApproval/Approved/Rejected/Cancelled/Disbursed/Invalidated) in src/Domain/Payments/Enums/DisbursementRequestStatus.cs

#### Commands

- [ ] T027 [P] [US1] Create CreateDisbursementRequestCommand + Handler + Validator in src/Application/Payments/Commands/DisbursementRequests/CreateDisbursementRequest/ — independent creation (no order required); server-issues requestNumber DSB-{D6}; positive requestedAmount; BeneficiaryName required + BeneficiaryPartyId optional with server validation (ADR-002)
- [ ] T028 [P] [US1] Create UpdateDisbursementRequestCommand + Handler + Validator in src/Application/Payments/Commands/DisbursementRequests/UpdateDisbursementRequest/ — Draft-only editing
- [ ] T029 [P] [US4] Create SubmitDisbursementRequestCommand + Handler + Validator in src/Application/Payments/Commands/DisbursementRequests/SubmitDisbursementRequest/ — pure status transition (no availability check per ADR-001 D-3)
- [ ] T030 [US2] Create ApproveDisbursementRequestCommand + Handler + Validator in src/Application/Payments/Commands/DisbursementRequests/ApproveDisbursementRequest/ — step 1 & 2: qualified role (AccountsManager/AuthorizingOfficer) + distinct users + same amount + approvedAmount ≤ requestedAmount; issuing authority recorded in ApprovalHistory; step 2 atomic: generates linked PaymentOrder (DisbursementRequestId UNIQUE) on final approval; copies beneficiaryName + beneficiaryPartyId from request to order (ADR-002)
- [ ] T031 [US3] Create RejectDisbursementRequestCommand + Handler + Validator in src/Application/Payments/Commands/DisbursementRequests/RejectDisbursementRequest/ — nonblank reason required; no order generated
- [ ] T032 [US5] Create CancelDisbursementRequestCommand + Handler + Validator in src/Application/Payments/Commands/DisbursementRequests/CancelDisbursementRequest/ — reason required; releases order for unpaid Approved requests

#### Queries

- [ ] T033 [P] [US6] Create GetDisbursementRequestsQuery + Handler in src/Application/Payments/Queries/DisbursementRequests/GetDisbursementRequests/ — list with filters (status/requester)
- [ ] T034 [P] [US6] Create GetDisbursementRequestByIdQuery + Handler in src/Application/Payments/Queries/DisbursementRequests/GetDisbursementRequestById/ — includes approvals[] from ApprovalHistory (step/actor/role/decision/time/approvedAmount/issuingAuthority)

#### Endpoints

- [ ] T035 [US1] Create DisbursementRequests endpoint group in src/Web/Endpoints/Payments/DisbursementRequests.cs — GET /, POST /, GET /{id}, PATCH /{id}/submit, PATCH /{id}/approve, PATCH /{id}/reject, PATCH /{id}/cancel; authorize each with DisbursementRequests.* permission codes

**Checkpoint**: Disbursement request workflow complete. `dotnet build src/Web/Web.csproj` passes. Quickstart S3 and S4 scenarios manually verifiable.

---

## Phase 4: PAY-03 — Payment Execution (P1)

**Goal**: Single immutable payment recording, triad closure, concurrent-payment guard

**Independent Test**: Record payment on Approved order → Completed, request Disbursed, order Paid. Attempt double payment → refused. Attempt payment on non-approved order → refused.

### Implementation for PAY-03

#### Entity Adjustments

- [ ] T036 [P] Verify Payment.PaymentOrderId has UNIQUE constraint in src/Domain/Payments/Entities/Payment.cs
- [ ] T037 [P] Verify Payment entity fields: PaymentNumber, PaymentOrderId, DisbursementRequestId, PaymentMethod, Amount (snapshot), PaidById, PaidAt, ReferenceNumber, Notes, Status in src/Domain/Payments/Entities/Payment.cs
- [ ] T038 [P] Verify PaymentMethod enum: Cash/Check only in src/Domain/Payments/Enums/PaymentMethod.cs
- [ ] T039 [P] Verify PaymentStatus enum: Completed/Failed in src/Domain/Payments/Enums/PaymentStatus.cs

#### Commands

- [ ] T040 [US1] Create RecordPaymentCommand + Handler + Validator in src/Application/Payments/Commands/Payments/RecordPayment/ — precondition: order Approved; amount = server snapshot of order netAmount; PaymentOrderId UNIQUE guard; raises PaymentRecordedEvent for posting pipeline
- [ ] T041 [US1] Add PaymentRecordedEvent domain event in src/Domain/Events/Payments/ — inherits BaseEvent, implements IHasSourceEntity; carries payment details for AccountingEvents/ACC-05 posting

#### Queries

- [ ] T042 [P] [US3] Create GetPaymentsQuery + Handler in src/Application/Payments/Queries/Payments/GetPayments/ — list with filters (period/method/status)
- [ ] T043 [P] [US3] Create GetPaymentByIdQuery + Handler in src/Application/Payments/Queries/Payments/GetPaymentById/

#### Endpoints

- [ ] T044 [US1] Create Payments endpoint group in src/Web/Endpoints/Payments/Payments.cs — GET /, POST /, GET /{id}; authorize with Payments.View and Payments.Create

**Checkpoint**: Payment execution complete. `dotnet build src/Web/Web.csproj` passes. Quickstart S4 triad closure and double-payment guard manually verifiable.

---

## Phase 5: PAY-04 — Bank Accounts (P1)

**Goal**: Bank account registry, CRUD with 17 fields, activate/deactivate, default auto-switch

**Independent Test**: Create account A default → create account B default → verify B is default and A demoted. Duplicate accountNumber → refused. Deactivate default → excluded from PAY-01 picker. Activate/deactivate round-trips with RowVersion.

### Implementation for PAY-04

#### Entity Verification

- [ ] T045 [P] Verify BankAccount entity has all 17 contract fields (Name, BankName, AccountNumber, Iban, SwiftCode, BranchName, BranchCode, CurrencyId, FundId, GlAccountId, IsDefault, MaxDailyLimit, MaxTransactionLimit, RequiresDualApproval, LastReconciliationDate, OpeningBalance, CurrentBalance, IsActive) in src/Domain/Payments/Entities/BankAccount.cs

#### Commands

- [ ] T046 [P] [US2] Create CreateBankAccountCommand + Handler + Validator in src/Application/Payments/Commands/BankAccounts/CreateBankAccount/ — all 17 fields except id/lastReconciliationDate/currentBalance/isActive; accountNumber unique; IsDefault=true auto-demotes others in same transaction
- [ ] T047 [P] [US2] Create UpdateBankAccountCommand + Handler + Validator in src/Application/Payments/Commands/BankAccounts/UpdateBankAccount/ — same auto-switch for IsDefault; accountNumber uniqueness check
- [ ] T048 [P] [US3] Create ActivateBankAccountCommand + Handler in src/Application/Payments/Commands/BankAccounts/ActivateBankAccount/ — {id, rowVersion}
- [ ] T049 [P] [US3] Create DeactivateBankAccountCommand + Handler in src/Application/Payments/Commands/BankAccounts/DeactivateBankAccount/ — {id, rowVersion}; deactivated excluded from PAY-01 bank picker

#### Queries

- [ ] T050 [P] [US1] Create GetBankAccountsQuery + Handler in src/Application/Payments/Queries/BankAccounts/GetBankAccounts/ — list with filters (active/currency/fund); excludes inactive from default picker
- [ ] T051 [P] [US1] Create GetBankAccountByIdQuery + Handler in src/Application/Payments/Queries/BankAccounts/GetBankAccountById/

#### Endpoints

- [ ] T052 [US1] Create BankAccounts endpoint group in src/Web/Endpoints/Payments/BankAccounts.cs — GET /, POST /, GET /{id}, PUT /{id}, POST /{id}/activate, POST /{id}/deactivate; authorize with BankAccounts.* permission codes

**Checkpoint**: Bank accounts complete. `dotnet build src/Web/Web.csproj` passes. Quickstart S5 scenarios manually verifiable.

---

## Phase 6: Frontend — features/payments/ — PAY-01, PAY-03, PAY-04

**Goal**: Arabic RTL UI for payment orders, payments, and bank accounts (PAY-02 frontend in Phase 7)

**Independent Test**: Navigate each entity's list, create, detail pages. Verify Arabic strings, RTL layout, color-coded badges, conditional action buttons, server-issued totals. Run `npm run lint && npm run build`.

### Frontend Structure

- [ ] T053 [P] Create frontend domain folder structure: src/Web/ClientApp/src/features/payments/ with subfolders: payment-orders/, disbursement-requests/, payments/, bank-accounts/, hooks/, shared/, utils/
- [ ] T054 [P] Create route definitions for /payments/* routes in src/Web/ClientApp/src/routes/

### Payment Orders Frontend

- [ ] T055 [P] Create PaymentOrdersListPage with table (number/beneficiary/date/net/status/check badge) + filters (status/budgetCheckStatus/period) in src/Web/ClientApp/src/features/payments/payment-orders/pages/PaymentOrdersListPage.tsx
- [ ] T056 [P] Create PaymentOrderCreatePage with sections: header (fund/fiscal year/appropriation/currency/rate/dueDate), deductions (typed grid + mandatory/tax + taxAuthority), beneficiary (name/party/iban/account/bank), totals preview in src/Web/ClientApp/src/features/payments/payment-orders/pages/PaymentOrderCreatePage.tsx
- [ ] T057 [P] Create PaymentOrderDetailPage with header + deductions + Totals card (4 numbers) + budgetCheckStatus badge + 022 panels + treasury trace + journal link + issuing authority badge + conditional lifecycle action dialogs in src/Web/ClientApp/src/features/payments/payment-orders/pages/PaymentOrderDetailPage.tsx
- [ ] T058 [P] Create shared types/schemas/client for payment-orders in src/Web/ClientApp/src/features/payments/payment-orders/shared/
- [ ] T059 [P] Create payment-order-scoped hooks in src/Web/ClientApp/src/features/payments/payment-orders/hooks/

### Payments Frontend

- [ ] T060 [P] Create PaymentsListPage with table (number/request/order/method/amount/status/payer/date) + filters (period/method/status) in src/Web/ClientApp/src/features/payments/payments/pages/PaymentsListPage.tsx
- [ ] T061 [P] Create RecordPaymentDialog with readonly amount snapshot + method select (Cash/Check) + referenceNumber + notes + confirmation in src/Web/ClientApp/src/features/payments/payments/pages/RecordPaymentDialog.tsx
- [ ] T062 [P] Create shared types/schemas/client for payments in src/Web/ClientApp/src/features/payments/payments/shared/
- [ ] T063 [P] Create payment-scoped hooks in src/Web/ClientApp/src/features/payments/payments/hooks/

### Bank Accounts Frontend

- [ ] T064 [P] Create BankAccountsListPage with table (name/bankName/accountNumber/currency/isDefault/isActive) + filters (active/currency/fund) in src/Web/ClientApp/src/features/payments/bank-accounts/pages/BankAccountsListPage.tsx
- [ ] T065 [P] Create BankAccountCreatePage with sections: identity (name/bank/account), branch (iban/swift/branch), financial (currency/fund/GL/opening), controls (limits/dual/default) in src/Web/ClientApp/src/features/payments/bank-accounts/pages/BankAccountCreatePage.tsx
- [ ] T066 [P] Create BankAccountDetailPage with all details + lastReconciliationDate + activate/deactivate buttons (confirmation) in src/Web/ClientApp/src/features/payments/bank-accounts/pages/BankAccountDetailPage.tsx
- [ ] T067 [P] Create shared types/schemas/client for bank-accounts in src/Web/ClientApp/src/features/payments/bank-accounts/shared/
- [ ] T068 [P] Create bank-account-scoped hooks in src/Web/ClientApp/src/features/payments/bank-accounts/hooks/

### Cross-Entity Frontend

- [ ] T069 [P] Create cross-entity hooks in src/Web/ClientApp/src/features/payments/hooks/
- [ ] T070 [P] Create cross-entity shared types in src/Web/ClientApp/src/features/payments/shared/
- [ ] T071 [P] Create Payments-prefixed feature-scoped components (filters, badges, typed grids) in src/Web/ClientApp/src/components/

**Checkpoint**: PAY-01/03/04 frontend complete. `cd src/Web/ClientApp && npm run lint && npm run build` passes. All Arabic RTL pages render correctly.

---

## Phase 7: Frontend — PAY-02 Disbursement Requests (Arabic spec)

**Goal**: Arabic RTL UI for disbursement request lifecycle per frontend-disbursement-requests.md and frontend-requirements.md

**Independent Test**: Create request → submit → approve (two signatures) → verify order generated → verify detail shows full chain. Verify rejection with reason. Verify cancellation. Run `npm run lint && npm run build`.

**Reference documents**: `frontend-disbursement-requests.md` (Arabic), `frontend-requirements.md` (PAY-02 sections)

### Disbursement Requests Frontend

- [ ] T072 [P] Create DisbursementRequestsListPage with table (number/requester/beneficiary/amount/approvedAmount/status/signatureProgress/linkedOrder) + filters (status/requester) per frontend-disbursement-requests.md S1 in src/Web/ClientApp/src/features/payments/disbursement-requests/pages/DisbursementRequestsListPage.tsx
- [ ] T073 [P] Create DisbursementRequestCreatePage with form: beneficiaryName (required), beneficiaryPartyId (optional Party picker per ADR-002), requestedAmount (positive), currency, purpose, financialYear, notes per S2 in src/Web/ClientApp/src/features/payments/disbursement-requests/pages/DisbursementRequestCreatePage.tsx
- [ ] T074 [P] Create DisbursementRequestDetailPage with sections: document header, original request, approval (dual-signature Stepper), negative decision, linked order, execution follow-up, status log per S3 in src/Web/ClientApp/src/features/payments/disbursement-requests/pages/DisbursementRequestDetailPage.tsx
- [ ] T075 [US3] Create ApprovalDialog (signature dialog) with: requestNumber/beneficiary/requestedAmount/currency for display, approvedAmount (required, positive, ≤ requestedAmount), issuingAuthorityName + issuingAuthorityCapacity (dropdown: General Manager / Finance Director), notes (optional) per S4 in src/Web/ClientApp/src/features/payments/disbursement-requests/pages/ApprovalDialog.tsx
- [ ] T076 [US4] Create RejectDialog with: requestNumber/beneficiary, reason (required, nonblank) per S5 in src/Web/ClientApp/src/features/payments/disbursement-requests/pages/RejectDialog.tsx
- [ ] T077 [US4] Create CancelDialog with: requestNumber/beneficiary, reason (required, nonblank) per S5 in src/Web/ClientApp/src/features/payments/disbursement-requests/pages/CancelDialog.tsx
- [ ] T078 [P] Create shared types/schemas/client for disbursement-requests in src/Web/ClientApp/src/features/payments/disbursement-requests/shared/
- [ ] T079 [P] Create disbursement-request-scoped hooks in src/Web/ClientApp/src/features/payments/disbursement-requests/hooks/

**Checkpoint**: PAY-02 frontend complete. `cd src/Web/ClientApp && npm run lint && npm run build` passes. All Arabic RTL pages render correctly per frontend-disbursement-requests.md.

---

## Phase 8: Polish & Cross-Cutting Concerns

**Purpose**: API client regeneration, documentation, final validation

- [ ] T080 [P] Regenerate NSwag API client: `cd src/Web/ClientApp && npm run generate-api`
- [ ] T081 [P] Update docs/database-schema.md with ADR-001 + ADR-002 schema changes (DROP/ADD columns, UNIQUE indexes, BeneficiaryPartyId re-introduction)
- [ ] T082 Update docs/feature-architecture-map-v1.0.md with payments feature entry
- [ ] T083 Run full backend build verification: `dotnet build src/Web/Web.csproj`
- [ ] T084 Run full frontend verification: `cd src/Web/ClientApp && npm run lint && npm run build`
- [ ] T085 Execute quickstart.md validation scenarios S1–S6 and record evidence
- [ ] T086 Verify all permission codes are registered and policies wired (PaymentOrders.*, DisbursementRequests.*, BankAccounts.*, Payments.*, PaymentOrders.OverrideBudgetCheck)
- [ ] T087 Verify all domain events (PaymentRecordedEvent) route through posting pipeline

---

## Dependencies & Execution Order

### Phase Dependencies

- **Phase 1 (ADR-001 + ADR-002)**: No dependencies — start immediately. BLOCKS all subsequent phases.
- **Phase 2 (PAY-01)**: Depends on Phase 1 completion
- **Phase 3 (PAY-02)**: Depends on Phase 1 completion. T030 (order generation) depends on Phase 2 entity structure (T006–T010)
- **Phase 4 (PAY-03)**: Depends on Phase 1 + Phase 2 entity structure (order Approved state exists)
- **Phase 5 (PAY-04)**: Depends on Phase 1 completion only — independent of PAY-01/02/03
- **Phase 6 (Frontend PAY-01/03/04)**: Depends on backend endpoints being available (Phases 2–5)
- **Phase 7 (Frontend PAY-02)**: Depends on Phase 3 backend completion
- **Phase 8 (Polish)**: Depends on all prior phases

### User Story Dependencies

- **PAY-01 US1–US4 (P1)**: Can start after Phase 1 — no dependencies on other sub-modules
- **PAY-01 US5 (P2)**: Depends on PAY-01 US1–US4 + PAY-02 entity structure (for request invalidation)
- **PAY-02 US1–US6 (P1)**: Can start after Phase 1. US2 (order generation) depends on PAY-01 entity being available
- **PAY-03 US1–US3 (P1)**: Can start after Phase 1 + Phase 2 entity (order Approved state)
- **PAY-04 US1–US3 (P1)**: Can start after Phase 1 — fully independent

### Parallel Opportunities

- Phase 1: T003, T004, T005 can run in parallel (different files)
- Phase 2: T006–T010 (entity adjustments) all parallel; T011+T012 parallel; T019+T020+T021 parallel
- Phase 3: T023–T026 parallel; T027+T028+T029 parallel; T033+T034 parallel
- Phase 4: T036–T039 parallel; T042+T043 parallel
- Phase 5: T045–T049 all parallel; T050+T051 parallel
- Phase 6: All [P] frontend tasks parallel (different files)
- Phase 7: T072+T073+T078+T079 parallel; T075+T076+T077 parallel
- Phase 2–5 can partially overlap: PAY-04 (Phase 5) can run in parallel with Phase 2–4

---

## Implementation Strategy

### MVP First (PAY-01 Lifecycle Only)

1. Complete Phase 1: ADR-001 + ADR-002 schema migration
2. Complete Phase 2: PAY-01 lifecycle (order create → submit → approve → treasury)
3. **STOP and VALIDATE**: Quickstart S1 scenario manually
4. Deploy/demo if ready

### Incremental Delivery

1. Phase 1 → Schema ready
2. Phase 2 → Payment order lifecycle (MVP!)
3. Phase 3 → Request-first workflow (PAY-02 → PAY-01 chain)
4. Phase 4 → Payment execution (triad closure)
5. Phase 5 → Bank accounts (independent, can parallel with 2–4)
6. Phase 6 → Frontend for PAY-01/03/04
7. Phase 7 → Frontend for PAY-02 (request-first UI)
8. Phase 8 → Polish, validation, documentation

### Parallel Team Strategy

With multiple developers:
1. Team completes Phase 1 together
2. Once Phase 1 done:
   - Developer A: Phase 2 (PAY-01) + Phase 4 (PAY-03)
   - Developer B: Phase 3 (PAY-02) — depends on PAY-01 entity
   - Developer C: Phase 5 (PAY-04) — fully independent
3. Phase 6 (Frontend PAY-01/03/04) can start as soon as endpoints are available
4. Phase 7 (Frontend PAY-02) starts after Phase 3 backend
5. Phase 8 (Polish) after all phases complete

---

## Notes

- [P] tasks = different files, no dependencies
- [Story] label maps task to specific sub-module story for traceability
- No automated tests (DEP-027 exception) — validation via quickstart scenarios
- Commit after each task or logical group
- Stop at any checkpoint to validate story independently
- ADR-001 breaking changes documented in contracts/payments-api.md — frontend must regenerate API client after backend changes
- ADR-002 (BeneficiaryPartyId optional) supersedes ADR-001 D-1 — both migrations applied in Phase 1
- Frontend follows features/budgeting/ exemplar — entity-based nesting, no components/ inside features/
- All Arabic strings hardcoded in JSX — no t() calls, no locale files
- RTL: logical CSS properties only (ms-/me-, ps-/pe-, start/end) — no physical properties
- PAY-02 frontend spec: `frontend-disbursement-requests.md` (Arabic) + `frontend-requirements.md` (PAY-02 sections)
- BeneficiaryPartyLink: server validates selected Party as active, stores ID + snapshots name; invalid ID refused; name-only accepted when no Party selected (ADR-002)
- Unresolved decisions (OQ-N5..N9) do not block implementation but must be resolved before planning future features
