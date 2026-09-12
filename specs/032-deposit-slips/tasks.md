# Tasks: Deposit Slips (TRE-02)

**Input**: Design documents from `/specs/032-deposit-slips/`

**Prerequisites**: plan.md, spec.md, research.md, data-model.md, contracts/api.md, quickstart.md

**Tests**: REQUIRED — constitution XI mandates TDD (test first, observe red, implement, green). Test tasks are not optional.

**Organization**: Grouped by user story. Baseline note: spec-017 code already exists — every backend task is an alignment (red test first against the *clarified* rule, then fix the handler).

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: User story (US1–US4)
- Exact file paths in every description

---

## Phase 1: Setup

**Purpose**: Baseline verification — no scaffolding needed (project exists)

- [x] T001 Verify clean baseline: `dotnet build src/Web/Web.csproj` passes and `dotnet test tests/Application.FunctionalTests --filter "FullyQualifiedName~Revenue"` is green before any change (document result in tasks notes)
- [x] T002 [P] Verify legacy DepositSlips surface matches plan gap list in specs/032-deposit-slips/plan.md (create/add/remove/approve + statement) — read-only confirmation, no edits

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Contract alignment that every user story depends on

**⚠ CRITICAL**: No user story work before this phase is complete

- [x] T003 Rename permission code `DepositSlipsManage`→`DepositSlipsUpdate` ("DepositSlips.Update") in src/Application/Common/Security/PermissionCodes.cs and update both usages (add/remove endpoints) in src/Web/Endpoints/Revenue/DepositSlips.cs (research R6)
- [x] T004 [P] Register/rename policy `DepositSlips.Update` alongside `DepositSlips.View/Create/Approve` in src/Web/DependencyInjection.cs (keep open-placeholder pattern per registered exception 1)
- [x] T005 Add `RoutePrefix => "/api/Revenue/DepositSlips"` to the `DepositSlips` endpoint group in src/Web/Endpoints/Revenue/DepositSlips.cs (research R7; contract route is binding)
- [x] T006 Verify `DepositSlipDto`/`MonthlyStatementDto` field-for-field against specs/032-deposit-slips/contracts/api.md in src/Application/Revenue/Common/DTOs — fix any drift; regenerate the NSwag client via `cd src/Web/ClientApp && npm run generate-api` after T003–T005 (constitution IX)
- [x] T007 Add red unit tests for the contract-alignment gates: policy name resolution and route prefix exposed in OpenAPI (tests/Web.AcceptanceTests or Application.UnitTests depending on existing harness location — observe red, then confirm green after T003–T005)

**Checkpoint**: Contract-aligned surface — user stories can begin.

---

## Phase 3: User Story 1 — Create Deposit Slip with Members (P1) — MVP

**Goal**: Create a homogeneous slip (empty or with a batch of eligible vouchers), Draft + DSL number + server total.

**Independent Test**: Empty create → 201 Draft; batch create with 2 cash vouchers → Draft + DSL number + server total; wrong-method voucher → 400 homogeneity error (quickstart Journey 1).

### Tests for User Story 1 (write FIRST, observe RED)

- [x] T008 [P] [US1] Unit tests: create decision table (empty `voucherIds` allowed; future slipDate rejected; slipDate < latest voucher date rejected; non-Approved voucher rejected; wrong-`PaymentMethod` voucher rejected with homogeneity message; DSL number allocated; total from member lines) in tests/Application.UnitTests/Revenue/CreateDepositSlipTests.cs
- [ ] T009 [P] [US1] Functional tests: create lifecycle against real DB (empty Draft slip persisted; cash batch creates Draft with members and total; homogeneity rejection; sequence number uniqueness) in tests/Application.FunctionalTests/Revenue/DepositSlipLifecycleTests.cs

### Implementation for User Story 1

- [x] T010 [US1] Allow empty-member creation (FR-015): drop the ≥1-voucher handler guard and the `NotEmpty` rule in src/Application/Revenue/Commands/DepositSlips/CreateDepositSlipCommand.cs; with zero members validate slipDate ≤ today only
- [x] T011 [US1] Base homogeneity on `voucher.PaymentMethod` only (FR-001, research R2): remove the `Checks`-existence heuristic in src/Application/Revenue/Commands/DepositSlips/CreateDepositSlipCommand.cs
- [x] T012 [US1] Confirm green for T008–T009 (`dotnet test tests/Application.UnitTests --filter CreateDepositSlip` + functional filter); do not weaken assertions

### Frontend for User Story 1

- [x] T013 [P] [US1] Create slip form page + hook: formType toggle (47/48), slipDate picker, member batch from voucher picker; empty-start allowed with FR-005 warning in src/Web/ClientApp/src/features/treasury/deposit-slips/pages/CreateDepositSlipPage.tsx (+ hooks/)
- [x] T014 [P] [US1] Eligible-voucher picker hook: pre-filter Approved + unassigned + method-matching (FR-001 pre-filter), NSwag client only, in src/Web/ClientApp/src/features/treasury/deposit-slips/hooks/useEligibleVouchers.ts

**Checkpoint**: US1 fully functional and independently testable.

---

## Phase 4: User Story 2 — Manage Slip Members (P1)

**Goal**: Add/remove members while Draft only; removal requires reason; removed voucher returns to eligible pool; totals recomputed server-side.

**Independent Test**: Remove with reason → 204 + recomputed total; empty reason → 400; remove then add same voucher to another Draft slip → 204; any membership change on Approved slip → 400 (quickstart Journey 2).

### Tests for User Story 2 (write FIRST, observe RED)

- [x] T016 [P] [US2] Unit tests: add/remove decision table (Draft-only guard; removal empty/whitespace reason rejected (R3); non-member remove rejected; re-add after removal allowed (FR-013); total recomputed from member lines not arithmetic drift (FR-007); concurrency failure path) in tests/Application.UnitTests/Revenue/SlipMembershipTests.cs
- [ ] T017 [P] [US2] Functional tests: membership + concurrency against real DB (remove→re-join other slip; rowVersion conflict surfaced; approved-slip mutation rejected) in tests/Application.FunctionalTests/Revenue/DepositSlipLifecycleTests.cs (append)

### Implementation for User Story 2

- [x] T018 [US2] Enforce non-empty removal reason at handler + validator (contract `reason?` shape preserved, business rule enforced) in src/Application/Revenue/Commands/DepositSlips/RemoveVoucherFromSlipCommand.cs
- [x] T019 [US2] Recompute `TotalAmount` from member voucher lines after add/remove (replace `+=`/`-=`) in src/Application/Revenue/Commands/DepositSlips/AddVoucherToSlipCommand.cs and RemoveVoucherFromSlipCommand.cs
- [x] T020 [US2] Base add-voucher homogeneity on `PaymentMethod` only (FR-001) in src/Application/Revenue/Commands/DepositSlips/AddVoucherToSlipCommand.cs
- [x] T021 [US2] Confirm green for T016–T017 (filtered runs)

### Frontend for User Story 2

- [x] T022 [P] [US2] Member list + add/remove controls on slip detail page: reason dialog for removal, hidden when Approved, rowVersion round-trip, in src/Web/ClientApp/src/features/treasury/deposit-slips/pages/DepositSlipDetailPage.tsx (+ components/)

**Checkpoint**: US1 + US2 both work independently.

---

## Phase 5: User Story 3 — Approval and Ledger Effect (P1)

**Goal**: One-way Draft→Approved with guards and rule evaluation; Form47 emits posting events; Form48 sets member checks UnderCollection.

**Independent Test**: Approve Form47 → Approved + outbox `ReceiptVoucherCollected` per member; approve Form48 → checks UnderCollection; empty slip → 400; cancelled member → 400; stale rowVersion → concurrency failure (quickstart Journey 3).

### Tests for User Story 3 (write FIRST, observe RED)

- [x] T024 [P] [US3] Unit tests: approve decision table (empty-members rejected (FR-005); member-cancelled-after-join rejected with voucher name (R9); non-Draft rejected; rule-evaluation failure path (R8); self-approval allowed (FR-014)) in tests/Application.UnitTests/Revenue/ApproveDepositSlipTests.cs
- [ ] T025 [P] [US3] Functional tests: Form47 approval emits `ReceiptVoucherCollected` per member (outbox rows) + ApprovalHistory with evaluation snapshot + status log Draft→Approved; Form48 sets member checks UnderCollection; concurrency conflict distinct failure in tests/Application.FunctionalTests/Revenue/DepositSlipLifecycleTests.cs (append)

### Implementation for User Story 3

- [x] T026 [US3] Add approve guards: ≥1 member (FR-005) and all members still Approved with offending voucher named (R9) in src/Application/Revenue/Commands/DepositSlips/ApproveDepositSlipCommand.cs
- [x] T027 [US3] Migrate approve to `IApprovalRuleEvaluationService.EvaluateAsync("DepositSlip", total, …)` + `IIdentityService` role match + `IDocumentStatusLogger`, recording the evaluation snapshot in ApprovalHistory, on the ApprovePaymentOrder exemplar (src/Application/Payments/Commands/PaymentOrders/ApprovePaymentOrder/ApprovePaymentOrderCommand.cs); documented fallback to TreasuryManager role when no rules configured (R8)
- [x] T028 [US3] Make the Form48 branch an explicit idempotent transition loop setting member checks `UnderCollection` (FR-004, research R4) in src/Application/Revenue/Commands/DepositSlips/ApproveDepositSlipCommand.cs
- [x] T029 [US3] Confirm green for T024–T025 (filtered runs)

### Frontend for User Story 3

- [x] T030 [P] [US3] Approve confirmation dialog (optional reason + confirm) with rowVersion round-trip; read-only Approved state on detail page; nav entry carries `DepositSlips.Approve` permission id, in src/Web/ClientApp/src/features/treasury/deposit-slips/pages/DepositSlipDetailPage.tsx (+ components/)

**Checkpoint**: US3 independently functional — full P1 chain works.

---

## Phase 6: User Story 4 — Monthly Statement (P2)

**Goal**: Per-fund monthly statement: six-figure summary over full activity (all Approved vouchers), cleared-check table, explicit zeros.

**Independent Test**: After US3 journeys, statement figures reconcile (deposited = approved slip totals; vouchers[] = all Approved vouchers incl. un-deposited); empty month → explicit zeros (quickstart Journey 4).

### Tests for User Story 4 (write FIRST, observe RED)

- [x] T032 [P] [US4] Functional tests: statement over real DB (Approved-only vouchers[] incl. un-deposited (FR-016); summaries per definition; `fundName` resolved from Funds; empty month → explicit zeros; unknown fundId → failure) in tests/Application.FunctionalTests/Revenue/MonthlyStatementTests.cs

### Implementation for User Story 4

- [x] T033 [US4] Fix statement query: restrict `vouchers[]` and cash/check/under-collection/bounced figures to `Status == Approved` vouchers; drop non-approved rows; `TotalDeposited` from Approved slips only in src/Application/Revenue/Queries/Statements/GetMonthlyStatementQuery.cs
- [x] T034 [US4] Resolve `FundName` from the Funds table via shared persistence read; validate `fundId` exists (explicit failure otherwise); keep single-fund v1 semantics per research R5 in src/Application/Revenue/Queries/Statements/GetMonthlyStatementQuery.cs
- [x] T035 [P] [US4] Statement page (month + fund selectors, six-figure summary, vouchers + clearings tables, explicit-zero empty state, money-display + RTL) in src/Web/ClientApp/src/features/treasury/deposit-slips/pages/MonthlyStatementPage.tsx (+ components/)

**Checkpoint**: All user stories independently functional.

---

## Phase 7: Polish & Cross-Cutting Concerns

- [x] T037 [P] Replace/retire legacy always-pass assertions touching deposit slips if any exist in tests/Application.FunctionalTests/Revenue/ReceiptVoucherTests.cs (replacement as explicit task per constitution XI — none deleted without replacement)
- [ ] T038 Add Web.AcceptanceTests browser journey: create→manage→approve→statement happy path + one guard rejection, in tests/Web.AcceptanceTests (mirrors quickstart.md)
- [x] T039 [P] Update docs/feature-architecture-map-v1.0.md entry for Revenue deposit slips if the map lists it (read-only check first; update only if stale)
- [x] T040 Run full 5-project suite gate (Domain.UnitTests, Application.UnitTests, Application.FunctionalTests, Infrastructure.IntegrationTests, Web.AcceptanceTests) + `npm run lint && npm run test && npm run build` in src/Web/ClientApp — all green before converge

---

## Dependencies & Execution Order

### Phase Dependencies

- Setup (Phase 1) → Foundational (Phase 2) → US1 (Phase 3) → US2 (Phase 4) → US3 (Phase 5) → US4 (Phase 6) → Polish (Phase 7)
- US2 depends on US1 (a slip must exist to manage members); US3 depends on US1 (members to approve); US4 is data-independent (works on TRE-01 vouchers alone) but sequenced last (P2)
- Backend tasks within a story precede that story's frontend tasks (contract fixed first)

### Parallel Opportunities

- [P] test tasks (T008/T009, T016/T017, T024/T025, T032) run in parallel within their phase — but must be observed RED before the implementation task they gate
- T013/T014 frontend pieces are parallelizable after the picker contract is settled
- US4 backend (T033/T034) is independent of US2/US3 frontend work — different files

### TDD discipline (constitution XI)

- Every implementation task (T010–T011, T018–T020, T026–T028, T033–T034) is gated by its red test task; single filtered test first, touched project's suite only; full suite only at T040 (converge gate). No assertion weakening, skipping, or deletion.

## Implementation Strategy

- **MVP**: Phases 1–3 (US1) — slip creation alone closes the daily batch problem (SC-001)
- **Incremental**: +US2 (correction) → +US3 (financial recognition — the control point) → +US4 (reconciliation)
- **Coordination risks** (plan.md): TRE-01 rebuild pending on shared files — sequence against current code, re-run T040 after 031 merges; permission rename (T003) is cheapest before RBAC wiring lands
