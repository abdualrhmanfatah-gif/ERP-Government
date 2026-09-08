# Tasks: Payments Group (PAY-01..04)

**Input**: Design documents from `/specs/045-payments-group/`

**Prerequisites**: plan.md, spec.md, research.md, data-model.md, contracts/payments-api.md, quickstart.md

**Tests**: No automated tests. Backend quality gate: `dotnet build src/Web/Web.csproj`. Frontend gate: `npm run lint` + `npm run build`. Validation via quickstart scenarios (S1–S6).

## Story Label Mapping

Spec user stories are grouped by sub-module; task labels are sequential:

| Label | Spec story | Title |
|---|---|---|
| US1 | PAY-01 US1 | Create a complete order (CRUD + lines + deductions) |
| US2 | PAY-01 US2 | Budget check (Pending/Passed/Failed/Overridden) |
| US3 | PAY-01 US3 | Lifecycle and treasury |
| US4 | PAY-01 US4 | Totals and partial payment |
| US5 | PAY-01 US5 | Voiding (+ order-side auto-invalidation, D4) |
| US6 | PAY-02 US1 | Create request on approved order (1:1) |
| US7 | PAY-02 US2 | Dual signature |
| US8 | PAY-02 US3 | Rejection and cancellation (release) |
| US9 | PAY-02 US4 | Availability gate |
| US10 | PAY-03 US1 | Record payment (triad closure) |
| US11 | PAY-03 US2 | Visible failure |
| US12 | PAY-03 US3 | Payments list |
| US13 | PAY-04 US1 | Accounts registry |
| US14 | PAY-04 US2 | Create/edit with full details (+ default auto-switch) |
| US15 | PAY-04 US3 | Activate/deactivate |

**Chain note**: US6–US12 operate on PAY-01 output; their backend exists — story independence is preserved because each story ships its own UI increment against the shared built contract.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: different files, no dependencies on incomplete tasks
- **[Story]**: label from the mapping above

---

## Phase 1: Setup (Shared Infrastructure)

- [x] T001 Add `PaymentOrders.OverrideBudgetCheck` permission: constant in `src/Application/Common/Security/PermissionCodes.cs`, policy registration in `src/Web/DependencyInjection.cs`, entry in `RolePermissionSeedData.GetAllPermissionCodes()` (`src/Infrastructure/Data/Seeds/RolePermissionSeedData.cs`) — idempotent merge, no migration
- [x] T002 Baseline gate: run `dotnet build src/Web/Web.csproj` and verify build succeeds before any change

**Checkpoint**: permission ready; build green.

---

## Phase 2: Foundational (Blocking Prerequisites)

- [x] T003 Scaffold frontend domain: `src/Web/ClientApp/src/features/payments/` with `payment-orders/`, `disbursement-requests/`, `payments/`, `bank-accounts/` folders (+ cross-entity `hooks/`, `shared/`, `utils/` per plan structure decision) and register `/payments/*` routes in `src/Web/ClientApp/src/routes/` with navigation entries carrying their permission identifiers
- [x] T004 Shared UI primitives in `src/Web/ClientApp/src/components/`: `PaymentsStatusBadge.tsx` (9 order states, 7 request states, 2 payment states, 4 budget-check states — token-driven colors), `PaymentsApprovalStepper.tsx` (renders approvals[] two-step history); RTL logical properties only; Arabic hardcoded strings

**Checkpoint**: Frontend skeleton ready; backend stories (US1–US5) can proceed in parallel with frontend wiring.

---

## Phase 3: User Story US1 — Create a complete order (P1) — MVP

**Goal**: Accountant creates a full order (header + lines + deductions + beneficiary) with server-issued number and server-computed amounts.

**Independent Test**: Create order with 5+ lines (all 5 line types) and 3+ deductions (all flags) → Draft + `PO-{D6}`; mandatory deduction deletion blocked; submit with 0 lines refused (quickstart S1.1, S1.6).

### Implementation for US1

- [x] T005 [US1] Close any red gaps in `src/Application/Payments/Commands/PaymentOrders/CreatePaymentOrder/CreatePaymentOrderCommand.cs` + `UpdatePaymentOrder/UpdatePaymentOrderCommand.cs` (mandatory-deduction guard, zero-line submit guard lives in Submit — verify ordering)
- [x] T006 [P] [US1] Frontend list page: `src/Web/ClientApp/src/features/payments/payment-orders/pages/PaymentOrdersListPage.tsx` + `hooks/usePaymentOrders.ts` + `shared/client.ts` (NSwag client only) + `shared/schemas.ts` — table (number/vendor/date/net/status/budget-check badge) + filters, CC-2 states
- [x] T007 [US1] Frontend create page: `src/Web/ClientApp/src/features/payments/payment-orders/pages/PaymentOrderCreatePage.tsx` + `shared/schemas.ts` (Zod: lines net = gross − deductions ±0.01, deduction > gross blocked) — sections: header (vendor/fund/fiscal year/appropriation/dimensions/currency+exchangeRate/due date), lines typed grid, deductions typed grid (mandatory/tax/tax authority), beneficiary, server-fed totals preview
- [x] T008 [US1] Frontend detail page base: `src/Web/ClientApp/src/features/payments/payment-orders/pages/PaymentOrderDetailPage.tsx` — header + lines + deductions + Totals card (6 server numbers) + badges

**Checkpoint**: MVP — orders CRUD + UI complete; validate via quickstart S1.1/S1.6.

---

## Phase 4: User Story US2 — Budget check (P1)

**Goal**: Appropriation-level availability check at submit; Failed blocks approval; explicit permissioned override documented.

**Independent Test**: Submit against drained appropriation → Failed persisted + verbatim refusal; approve Failed without flag → 400; with flag without permission → 403; with permission → Approved + Overridden + history rows (quickstart S1.2–S1.5).

### Implementation for US2

- [x] T009 [US2] Implement appropriation-level check in `src/Application/Payments/Commands/PaymentOrders/SubmitPaymentOrder/SubmitPaymentOrderCommand.cs` (research D1: `GetAvailableForAppropriationAsync(entity.AppropriationId)` vs net amount; Failed persists)
- [x] T010 [US2] Implement override in `src/Application/Payments/Commands/PaymentOrders/ApprovePaymentOrder/ApprovePaymentOrderCommand.cs` (research D2: optional `overrideFailedBudgetCheck`, `PaymentOrders.OverrideBudgetCheck` permission, `BudgetCheckStatus=Overridden`, ApprovalHistory snapshot + `IDocumentStatusLogger`) + extend command shape per `contracts/payments-api.md`
- [ ] T011 [US2] Frontend override affordance: budget-check badge wiring + permission-gated override confirm dialog in `src/Web/ClientApp/src/features/payments/payment-orders/pages/PaymentOrderDetailPage.tsx` + `src/Web/ClientApp/src/components/PaymentOrdersBudgetCheckOverrideDialog.tsx`

**Checkpoint**: US1+US2 functional.

---

## Phase 5: User Story US3 — Lifecycle and treasury (P1)

**Goal**: Full 9-state lifecycle with conditional actions; treasury trace + journal link displayed.

**Independent Test**: Drive Draft→Submitted→Approved→SentToTreasury with guards (≥1 line, reason on reject, treasury reference required); Rejected reason visible in 022 panels (quickstart S1.7).

### Implementation for US3

- [x] T012 [US3] Close lifecycle gaps in `src/Application/Payments/Commands/PaymentOrders/` (guard messages verbatim; status logging on every transition via `IDocumentStatusLogger`)
- [ ] T013 [US3] Frontend lifecycle actions: conditional submit/approve/reject/cancel/send-to-treasury/void buttons + reason/reference dialogs + treasury trace + `journalEntryId` link on `src/Web/ClientApp/src/features/payments/payment-orders/pages/PaymentOrderDetailPage.tsx` + dialog components in `src/Web/ClientApp/src/components/Payments*Dialog.tsx`

**Checkpoint**: order lifecycle complete end-to-end.

---

## Phase 6: User Story US4 — Totals (P1)

**Goal**: Six server-issued totals + isFullyPaid; partial-payment-ready arithmetic (research D7).

**Independent Test**: For any order `net = gross − deductions`, `remaining = net − paid`, `isFullyPaid = remaining == 0` — all from `GET /{id}/totals` (quickstart S6).

### Implementation for US4

- [x] T014 [US4] Fix `src/Application/Payments/Queries/PaymentOrders/GetPaymentOrderTotals/` to compute `paidAmount` from completed payments (D7: Σ Completed amounts; remaining = net − paid)
- [ ] T015 [P] [US4] Frontend Totals card server-fed audit: ensure `PaymentOrderDetailPage.tsx` renders totals exclusively from the totals endpoint (no client computation)

**Checkpoint**: totals verifiable.

---

## Phase 7: User Story US5 — Voiding + auto-invalidation (P2)

**Goal**: Void unpaid Approved/SentToTreasury orders; partial refused; linked non-terminal requests auto-invalidated (D3/D4).

**Independent Test**: Void unpaid → Voided final; void partially paid → refused verbatim; void/cancel order with Draft request → request Invalidated (quickstart S2.1–S2.3).

### Implementation for US5

- [x] T016 [US5] Implement D3+D4: `src/Application/Payments/Commands/PaymentOrders/VoidPaymentOrder/VoidPaymentOrderCommand.cs` (guard Approved/SentToTreasury + unpaid; partial refused; remove TODO reversing-entry comment) and `CancelPaymentOrder/CancelPaymentOrderCommand.cs` (auto-invalidate linked Draft/PendingApproval/Approved-unpaid requests + history rows)
- [ ] T017 [P] [US5] Frontend void/cancel dialogs (confirm + reason) in `src/Web/ClientApp/src/components/PaymentsOrderVoidDialog.tsx` / `PaymentsOrderCancelDialog.tsx`, wired on detail page

---

## Phase 8: User Story US6 — Create request 1:1 (P1)

**Goal**: Draft request on an Approved order without one; second request refused verbatim; warning badge.

**Independent Test**: Create → Draft + `DSB-{D6}` + snapshot amount; duplicate → verbatim server message (quickstart S3.1–S3.2).

### Implementation for US6

- [x] T018 [US6] Close gaps in `src/Application/Payments/Commands/DisbursementRequests/CreateDisbursementRequest/CreateDisbursementRequestCommand.cs` (verify 1:1 message + warning flag)
- [ ] T019 [US6] Frontend: `src/Web/ClientApp/src/features/payments/disbursement-requests/pages/DisbursementRequestCreatePage.tsx` (picker of Approved orders without request + notes) + list page `DisbursementRequestsListPage.tsx` (number/order/amount/status/warning badge + filters)

---

## Phase 9: User Story US7 — Dual signature (P1)

**Goal**: Two qualified distinct signers; step 2 role-checked (D5); Stepper from approvals[].

**Independent Test**: quickstart S3.4–S3.8 (unqualified step-1 refused; same-user step-2 refused; unqualified step-2 refused; two qualified → Approved + approvalDate).

### Implementation for US7

- [x] T020 [US7] Implement D5 in `src/Application/Payments/Commands/DisbursementRequests/ApproveDisbursementRequest/ApproveDisbursementRequestCommand.cs` (step-2 `RequiredRoles` membership + record role)
- [ ] T021 [US7] Frontend Stepper wiring + approve dialog (reason) + conditional PATCH buttons on `src/Web/ClientApp/src/features/payments/disbursement-requests/pages/DisbursementRequestDetailPage.tsx` using `PaymentsApprovalStepper.tsx`

---

## Phase 10: User Story US8 — Reject and cancel (P1)

**Goal**: Rejection with visible reason; cancellation releases the order for a new request.

**Independent Test**: quickstart S4.2 + reject-reason visibility; Disbursed cancel refused.

### Implementation for US8

- [x] T022 [US8] Close gaps in `src/Application/Payments/Commands/DisbursementRequests/RejectDisbursementRequest/` and `CancelDisbursementRequest/`
- [ ] T023 [P] [US8] Frontend reject/cancel dialogs (reason) on `DisbursementRequestDetailPage.tsx`; released-order visibility in create picker

---

## Phase 11: User Story US9 — Availability gate (P1)

**Goal**: Submit blocked on Blocking availability with verbatim detail; Warning passes with badge.

**Independent Test**: quickstart S3.3 — Blocking ⇒ refusal with detail; Warning ⇒ HasWarning badge on list/detail.

### Implementation for US9

- [x] T024 [US9] Close gaps in `src/Application/Payments/Commands/DisbursementRequests/SubmitDisbursementRequest/SubmitDisbursementRequestCommand.cs` (availability at transaction time; refusal detail verbatim)
- [ ] T025 [P] [US9] Frontend availability-refusal panel (verbatim detail) + HasWarning badge on `DisbursementRequestsListPage.tsx` / `DisbursementRequestDetailPage.tsx`

---

## Phase 12: User Story US10 — Record payment (P1)

**Goal**: Single immutable payment on Approved request; triad closes; amount is server snapshot.

**Independent Test**: quickstart S4.1, S4.4, S4.5 — Approved-only; Completed + `PAY-{D6}`; request Disbursed + order Paid; double record refused.

### Implementation for US10

- [x] T026 [US10] Close gaps in `src/Application/Payments/Commands/Payments/RecordPayment/RecordPaymentCommand.cs` (verify sequence allocation in same transaction + event raise)
- [ ] T027 [US10] Frontend record dialog (readonly amount snapshot + method select + referenceNumber + notes + confirm) on `DisbursementRequestDetailPage.tsx` + success screen (triad numbers) in `src/Web/ClientApp/src/features/payments/payments/pages/PaymentSuccessPage.tsx` (or inline success state)

---

## Phase 13: User Story US11 — Visible failure (P1)

**Goal**: No silent failure — verbatim reason + retry.

**Independent Test**: Force a refusal (non-approved request) → verbatim server message rendered + retry affordance (quickstart S4.4 UI side).

### Implementation for US11

- [ ] T028 [US11] Frontend failure state on `PaymentSuccessPage.tsx`/record dialog: render `Result.Errors` verbatim via `shared/api/result-to-ui.ts`, retry re-opens the record dialog (research D10 — no synthetic Failed rows)

---

## Phase 14: User Story US12 — Payments list (P2)

**Goal**: Payments list with filters (period/method/status).

**Independent Test**: Filter by method/status/period returns matching rows; no edit/delete affordances (immutable).

### Implementation for US12

- [ ] T029 [US12] Frontend list page `src/Web/ClientApp/src/features/payments/payments/pages/PaymentsListPage.tsx` + `hooks/usePayments.ts` — verify list query filters; no PUT/DELETE affordances

---

## Phase 15: User Story US13 — Accounts registry (P1 for PAY-04)

**Goal**: Registry columns + deactivated accounts absent from pickers.

**Independent Test**: Registry shows all 17-field columns; deactivated account absent from PAY-01 picker (quickstart S5.3).

### Implementation for US13

- [ ] T030 [US13] Frontend registry `src/Web/ClientApp/src/features/payments/bank-accounts/pages/BankAccountsListPage.tsx` (columns incl. isDefault/limits/lastReconciliationDate + filters) + detail `BankAccountDetailPage.tsx` (all fields display-only where contract says so)

---

## Phase 16: User Story US14 — Create/edit + default auto-switch (P1 for PAY-04)

**Goal**: Full 17-field CRUD; duplicate accountNumber refused; IsDefault auto-switch (D6).

**Independent Test**: quickstart S5.1–S5.2 — B default demotes A; duplicate refused.

### Implementation for US14

- [x] T031 [US14] Implement D6 in `src/Application/Payments/Commands/BankAccounts/CreateBankAccount/CreateBankAccountCommand.cs` and `UpdateBankAccount/UpdateBankAccountCommand.cs` (demote others atomically when IsDefault=true)
- [ ] T032 [US14] Frontend create/edit dialog `src/Web/ClientApp/src/components/PaymentsBankAccountEditDialog.tsx` — 4 sections (identity/branch/financial/controls) + Zod schema in `src/Web/ClientApp/src/features/payments/bank-accounts/shared/schemas.ts`

---

## Phase 17: User Story US15 — Activate/deactivate (P2)

**Goal**: isActive toggle with `{id, rowVersion}` + confirmation; history preserved.

**Independent Test**: quickstart S5.3–S5.4 — deactivate (confirm) → excluded from pickers; stale RowVersion → conflict surfaced.

### Implementation for US15

- [x] T033 [US15] Close gaps in `src/Application/Payments/Commands/BankAccounts/DeactivateBankAccount/DeactivateBankAccountCommand.cs` and `ActivateBankAccount/ActivateBankAccountCommand.cs`
- [ ] T034 [P] [US15] Frontend activate/deactivate buttons (confirmation) on `BankAccountDetailPage.tsx`

---

## Phase 18: Polish & Cross-Cutting

- [ ] T035 Regenerate NSwag client: `cd src/Web/ClientApp && npm run generate-api` (approve request body gained `overrideFailedBudgetCheck`), then `npm run lint` and `npm run build`
- [ ] T036 Update `docs/database-schema.md` only if any migration landed (expected: none — permission seed only); verify AGENTS.md exemplar table still points at budgeting (no repoint needed unless payments folder supersedes — do NOT repoint speculatively)
- [ ] T037 Full-suite gate: run `dotnet build src/Web/Web.csproj` — green required before converge
- [ ] T038 Execute `specs/045-payments-group/quickstart.md` scenarios S1–S6 against a running AppHost; record evidence

---

## Dependencies & Execution Order

### Phase Dependencies

- Phase 1 (Setup) → Phase 2 (Foundational) → backend stories in order US1→US2→US3→US4→US5 (same module, sequential recommended) while frontend stories can interleave after T003/T004
- US6–US9 (PAY-02) depend on US1–US3 backend surface (orders exist) — can start after Phase 5
- US10–US12 (PAY-03) depend on US6–US8 (request lifecycle) — after Phase 10
- US13–US15 (PAY-04) independent of the chain — can start after Phase 2
- Phase 18 depends on all

### Story Independence

- US13–US15 (bank accounts) have zero coupling to US1–US12 — safe parallel lane
- US2 (budget check) and US5 (void) touch different commands — parallelizable after US1

### Parallel Opportunities

- Frontend tasks T006/T007/T008 run parallel to backend US2–US5 once T003/T004 land
- Bank-accounts lane (US13–US15) fully parallel to the order→request→payment chain

---

## Implementation Strategy

### MVP First (US1 only)

1. Phase 1 + Phase 2
2. Phase 3 (US1) → STOP and validate (list/create/detail + build green)
3. Demo-able increment

### Incremental Delivery

US1 → US2 → US3 → US4 → US5 (order complete) → US6 → US7 → US8 → US9 (requests complete) → US10 → US11 → US12 (payments complete) → US13 → US14 → US15 (bank accounts) → Phase 18 gates.

### Quality Discipline (No TDD)

- Every task: implement → build green → validate via quickstart scenario
- No assertion weakening/skipping; full build gate only at Phase 18 (T037) and converge
- Frontend increments gate on lint + build

## Notes

- Commit after each task or logical group
- All lifecycle refusals must surface verbatim server messages (CC-2)
- Do not invent contract fields (D7 partial payments, D10 Failed rows, Transfer/InKind methods remain out of scope)
