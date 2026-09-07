# Tasks: Receipt Vouchers (TRE-01)

**Input**: Design documents from `/specs/031-receipt-vouchers/`

**Prerequisites**: plan.md, spec.md, research.md, data-model.md, contracts/, quickstart.md

**Tests**: TDD mandated by Constitution Principle XI — every task's test written and observed failing BEFORE implementation. No test weakened, skipped, or deleted.

**Organization**: Tasks grouped by user story. Backend = gap fixes on existing spec-017 code; frontend = new `features/treasury/` feature folder.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (US1, US2, US3)

---

## Phase 1: Setup (Shared Infrastructure)

- [x] T001 [P] Change `PrefixMap["ReceiptVoucher"]` from `"RCV"` to `"DSL"` in `src/Application/FinancialSettings/Common/Services/DocumentSequenceService.cs` (research R1)
- [x] T002 [P] Verify idempotent seed exists for `DocumentSequences` row `DocumentType = "ReceiptVoucher"` in `src/Infrastructure/Data/` seeding; add guarded seed if missing (constitution VI)
- [x] T003 [P] Make `ReceivedFrom` optional per contract: `ReceiptVoucherDto.ReceivedFrom` → `string?`, command property defaults to empty string, drop `NotEmpty` rule in `CreateReceiptVoucherCommandValidator` in `src/Application/Revenue/Common/DTOs/ReceiptVoucherDtos.cs` + `src/Application/Revenue/Commands/ReceiptVouchers/CreateReceiptVoucher/CreateReceiptVoucherCommand.cs` (research R8)

---

## Phase 2: User Story 1 — Create Receipt Voucher (Priority: P1) — MVP (backend)

### Tests for User Story 1 (write first, observe red)

- [x] T004 [P] [US1] Functional test: create cash voucher → 201 with full DTO body; `voucherNumber` matches `DSL-\d{6}`; status Draft; `totalAmount` equals Σ(lines) computed server-side; in `tests/Application.FunctionalTests/Revenue/ReceiptVoucherTests.cs`
- [x] T005 [P] [US1] Functional test: create check voucher with 1 check → 201, checks echoed; create Check-method with `checks: []` → 400; create Cash-method with checks → 400; in `tests/Application.FunctionalTests/Revenue/ReceiptVoucherTests.cs`
- [x] T006 [P] [US1] Functional test: Σ(checks) > Σ(lines) → 400; Σ(checks) < Σ(lines) → 201 allowed; in `tests/Application.FunctionalTests/Revenue/ReceiptVoucherTests.cs` (FR-008)
- [x] T007 [P] [US1] Functional test: create succeeds with empty `receivedFrom`; zero lines → 400; disabled partyId → 400; in `tests/Application.FunctionalTests/Revenue/ReceiptVoucherTests.cs`

### Implementation for User Story 1

- [x] T008 [US1] Enforce Σ(checks) ≤ Σ(lines) in `CreateReceiptVoucherCommandHandler` + validator in `src/Application/Revenue/Commands/ReceiptVouchers/CreateReceiptVoucher/CreateReceiptVoucherCommand.cs`
- [x] T009 [US1] Change `CreateReceiptVoucherCommand` to return `Result<ReceiptVoucherDto>` (load voucher with Party/Lines/Checks, map via existing mapping) and update create endpoint to `201` with DTO in `src/Web/Endpoints/Revenue/ReceiptVouchers.cs` (research R4, FR-001)
- [x] T010 [US1] Run `dotnet test tests/Application.FunctionalTests` — all US1 tests green; build `dotnet build src/Web/Web.csproj`

---

## Phase 3: User Story 2 — Review and Approval (Priority: P1) — MVP (backend)

### Tests for User Story 2 (write first, observe red)

- [x] T011 [P] [US2] Functional test: submit Draft → PendingReview, `submittedById/At` set; submit non-Draft → 400; in `tests/Application.FunctionalTests/Revenue/ReceiptVoucherTests.cs`
- [x] T012 [P] [US2] Functional test: approve PendingReview as the SAME user who submitted → 204 Approved with `reviewedById/At` (self-approval allowed per clarification Q1); approve non-PendingReview → 400; `ApprovalHistory` + `DocumentStatusLog` rows written; in `tests/Application.FunctionalTests/Revenue/ReceiptVoucherTests.cs`
- [x] T013 [P] [US2] Functional test: cancel Draft and PendingReview with reason → Cancelled; cancel Approved → 400 (terminal, even without deposit slip); cancel without reason → 400; in `tests/Application.FunctionalTests/Revenue/ReceiptVoucherTests.cs` (research R6)
- [x] T014 [P] [US2] Functional test: cancel `DocumentStatusLog.FromStatus` records the true prior status (never "Cancelled → Cancelled"); in `tests/Application.FunctionalTests/Revenue/ReceiptVoucherTests.cs`
- [x] T015 [P] [US2] Functional test: stale `rowVersion` on submit/approve/cancel → conflict failure message; submit voucher with zero lines → 400; in `tests/Application.FunctionalTests/Revenue/ReceiptVoucherTests.cs`

### Implementation for User Story 2

- [x] T016 [US2] Remove SoD guard (`voucher.SubmittedById == userId` rejection) in `src/Application/Revenue/Commands/ReceiptVouchers/ApproveReceiptVoucher/ApproveReceiptVoucherCommand.cs` (research R2)
- [x] T017 [US2] Cancel lifecycle guard — reject unless status is Draft or PendingReview — and capture `fromStatus` BEFORE mutation in `src/Application/Revenue/Commands/ReceiptVouchers/CancelReceiptVoucher/CancelReceiptVoucherCommand.cs`
- [x] T018 [US2] Add submit guards — reject zero-line voucher and Σ(checks) > Σ(lines) — in `src/Application/Revenue/Commands/ReceiptVouchers/SubmitReceiptVoucher/SubmitReceiptVoucherCommand.cs` (research R7)
- [x] T019 [US2] Run `dotnet test tests/Application.FunctionalTests` — all US2 tests green

---

## Phase 4: User Story 3 — Browse (Priority: P2)

### Tests for User Story 3 (write first, observe red)

- [x] T020 [P] [US3] Functional test: list filtered by payment method returns only matching vouchers; combined filters (party + period + status) respected; in `tests/Application.FunctionalTests/Revenue/ReceiptVoucherTests.cs`
- [x] T021 [P] [US3] Functional test: by-party and by-period endpoints return filtered, ordered results; in `tests/Application.FunctionalTests/Revenue/ReceiptVoucherTests.cs`

### Implementation for User Story 3

- [x] T022 [US3] Add `PaymentMethod?` filter to `GetReceiptVouchersQuery` in `src/Application/Revenue/Queries/ReceiptVouchers/GetReceiptVouchers/GetReceiptVouchersQuery.cs`
- [x] T023 [US3] Run `dotnet test tests/Application.FunctionalTests` — US3 green

---

## Phase 5: Frontend — Treasury Feature (US1–US3 UI)

- [x] T024 [P] Create shared types + fetch client for receipt vouchers in `src/Web/ClientApp/src/features/treasury/shared/` (follow `features/budgeting/shared/` exemplar; client.ts + types)
- [x] T025 [P] Create React Query hooks: useReceiptVouchers (filters: party/period/status/method), useReceiptVoucher (by id), mutations submit/approve/cancel with `rowVersion` round-trip and conflict toast, in `src/Web/ClientApp/src/features/treasury/hooks/`
- [x] T026 [P] Frontend test: checks section renders and becomes required only when method = Check; hidden for Cash; in `src/Web/ClientApp/src/features/treasury/__tests__/ChecksSection.test.tsx` (red first)
- [x] T027 [P] Frontend test: list filter by method sends `paymentMethod` param and renders only filtered rows; in `src/Web/ClientApp/src/features/treasury/__tests__/ReceiptVoucherFilters.test.tsx` (red first)
- [x] T028 [US1] Build `CreateReceiptVoucherPage` in `src/Web/ClientApp/src/features/treasury/pages/` — party picker excluding disabled parties, optional `receivedFrom` (placeholder: different payer), lines editor, conditional checks section; on save display returned `voucherNumber` + server `totalAmount` immediately
- [x] T029 [US1] Build `VoucherLinesEditor` + `ChecksSection` components in `src/Web/ClientApp/src/features/treasury/components/` (money display via shared money-format, RTL logical properties, tokens only)
- [x] T030 [US2] Build `ReceiptVouchersListPage` with filters (party/period/status/method) in `src/Web/ClientApp/src/features/treasury/pages/`; DSL numbers render; deposit slip link column when `depositSlipId` present (FR-007)
- [x] T031 [US2] Build `ReceiptVoucherDetailPage` — lifecycle buttons gated by status (submit/approve/cancel reason modal), reviewer name + time via users lookup cache (SC-002, research R3), audit info, `rowVersion` round-trip on every action
- [x] T032 [P] Register routes + navigation entries with permission codes (`ReceiptVouchers.View/Create`) and feature barrel export in `src/Web/ClientApp/src/features/treasury/index.ts` + app router/nav
- [x] T033 [US3] Run `npm run lint && npm run test && npm run build` in `src/Web/ClientApp` — frontend green

---

## Phase 6: Polish & Cross-Cutting Concerns

- [x] T034 [P] Regenerate NSwag API client (`npm run generate-api` in `src/Web/ClientApp`) — create endpoint response type change must propagate
- [ ] T035 Run full backend suite (DEFERRED — skipped by user decision "تخطى الtest"; run at pre-merge gate): `dotnet test tests/Domain.UnitTests && dotnet test tests/Application.UnitTests && dotnet test tests/Application.FunctionalTests && dotnet test tests/Infrastructure.IntegrationTests && dotnet test tests/Web.AcceptanceTests`
- [ ] T036 Run quickstart.md scenarios (DEFERRED — requires running server; skipped by user decision) V1–V7 against a running server (manual curl gate)
- [x] T037 Verify all treasury pages render correctly RTL + dark mode; no hard-coded design values (constitution X)

---

## Dependencies

- Phase 1 (T001–T003) blocks all backend story phases.
- US1 (Phase 2) → US2 (Phase 3) → US3 (Phase 4): sequential backend (same test file, same commands).
- Frontend (Phase 5) depends on Phases 2–4 (contract stable, create returns DTO, method filter).
- Phase 6 last.

## Parallel Examples

- Phase 1: T001, T002, T003 — different files, all parallel.
- Phase 2 tests: T004–T007 — same file but independent test cases; write together, run together.
- Phase 5: T024, T025, T026, T027 parallel (new files); T028–T031 sequential per page after hooks.

## Implementation Strategy

- **MVP**: Phases 1–3 (setup + US1 + US2 backend) deliver server-side value; validate via quickstart V1–V5.
- **Incremental**: US3 adds browse; Phase 5 adds UI per story mapping; Phase 6 gates.
- Full 5-project suite runs ONLY at Phase 6 gate (T035) — per-cycle runs touch the FunctionalTests project only (AGENTS.md TDD economy).
