# Research: Deposit Slips (TRE-02)

**Date**: 2026-09-07 | **Spec**: [spec.md](spec.md) | **Branch**: `tre02-deposit-slips`

Baseline discovery: spec 017 (commit d7ffd0f) already implemented the deposit-slip stack.
Each item below states the Decision, the gap it closes against the clarified spec, and the alternatives rejected.

## R1: Empty slip creation (FR-015)

- **Decision**: `CreateDepositSlipCommand` accepts an empty `voucherIds[]`; the slip is created Draft with members added later. `ApproveDepositSlip` gains the missing empty-members guard (FR-005).
- **Gap**: Create currently fails with "At least one voucher is required" (handler check + validator `NotEmpty`); approve has no member-count guard.
- **Rationale**: US1/US2 work as start-then-fill; the Draft-only edit window plus the approval guard is the real control. Ties into the clarified date rule: with zero members, `slipDate` is validated only against "not in the future".
- **Alternatives rejected**: requiring ≥1 voucher at creation — blocks the legitimate start-then-fill flow and adds no protection beyond FR-005.

## R2: Homogeneity basis (FR-001)

- **Decision**: Homogeneity checks use `voucher.PaymentMethod` exclusively (Cash ↔ Form47, Check ↔ Form48) in Create and AddVoucher.
- **Gap**: Existing code derives `isCheckVoucher` from `hasCheck || PaymentMethod == Check` — a heuristic that would accept a Cash-method voucher carrying orphaned check rows.
- **Rationale**: Spec FR-001 binds homogeneity to the voucher's payment method; the picker pre-filters on the same basis so UI and server agree. Server check stays authoritative either way.
- **Alternatives rejected**: keeping the has-checks heuristic — disagrees with the picker filter and can admit a mixed record.

## R3: Removal requires a reason (FR-003 vs contract `reason?`)

- **Decision**: The contract keeps `reason?` (literal DTO shape untouched), but the `RemoveVoucherFromSlip` handler rejects an empty/whitespace reason with an explicit failure; validator enforces non-empty when present.
- **Rationale**: FR-003 is a binding functional requirement ("الإزالة بسبب"); the `?` in the contract is the wire-shape, not a waiver of the business rule. Server-side enforcement is the constitution III default.
- **Alternatives rejected**: making reason truly optional — contradicts FR-003; changing the DTO to `reason` (non-optional) — violates the binding contract note.

## R4: Form48 approval semantics (FR-004)

- **Decision**: On Form48 approval the handler iterates member checks and sets `Status = UnderCollection` (idempotent write, no state-machine claim), then `TotalUnderCollection` in the statement reads that status. TRE-01 currently creates checks as `UnderCollection` at voucher creation, so the transition is a no-op today — the handler keeps the explicit loop so the spec's transition holds regardless of TRE-01's eventual initial state.
- **Gap**: Current Form48 branch is `if (c.Status == UnderCollection) c.Status = UnderCollection` — a literal no-op with no audit trail.
- **Rationale**: `CheckStatus` has no pre-slip state in the current enum; inventing one (`AwaitingDeposit`) is a TRE-01 contract change, out of TRE-02 scope. The idempotent loop plus statement reading keeps the observable behavior correct (US3 scenario 2) without touching the shared enum.
- **Alternatives rejected**: adding a new CheckStatus value — breaking change to TRE-01 scope requiring a decision record; deleting the loop — loses the explicit FR-004 translation step if TRE-01 changes the initial state.

## R5: Monthly statement fund dimension (FR-006, FR-016, OQ2)

- **Decision**: v1 treats all revenue vouchers as belonging to the queried fund's cash box: `FundId` is validated against the `Funds` table (shared persistence read into Budgeting data), `fundName` resolved from it, and the voucher/check sets are computed over ALL **Approved** vouchers of the month (cancelled/draft excluded), with explicit zeros when empty. Slip totals use approved slips only.
- **Gap**: Current implementation (a) includes non-approved vouchers in `vouchers[]` and cash/check summary figures, (b) hardcodes `FundName = "General Fund"`, (c) ignores status in summaries.
- **Rationale**: `ReceiptVoucher` carries no fund dimension — adding one is TRE-01 scope (out of scope here). The clarified spec (Q5) mandates full activity for the matched fund; until vouchers carry `FundId`, single-fund is the honest reading of OQ2's answer ("fundId في الكشف فقط"). Edge case "كشف بلا صندوق مطابق" → explicit zeros.
- **Alternatives rejected**: adding `FundId` to `ReceiptVoucher` now — touches TRE-01 scope and schema without its spec; leaving FundName hardcoded — fake data violates statement integrity.
- **Follow-up**: TRE-01 amendment should add the voucher-level fund dimension; this feature's statement query then filters by it (single query change).

## R6: Permission code naming (contract: members under Update)

- **Decision**: Rename `PermissionCodes.DepositSlipsManage` ("DepositSlips.Manage") to `DepositSlipsUpdate` ("DepositSlips.Update") and update the add/remove endpoint policies. `DepositSlips.View/Create/Approve` already match the contract.
- **Gap**: Binding contract says member add/remove sits under `Update`; existing code registered `Manage`.
- **Rationale**: The Data Contract is literal; a naming rename now (before RBAC wiring lands and roles reference the code) is the cheapest point. Grep confirms no seed/role references `DepositSlips.Manage` yet and the frontend has no deposit-slip client usage outside the generated client.
- **Alternatives rejected**: keeping `Manage` and documenting divergence — unregistered contract deviation (constitution XII defect).

## R7: Routes and client regeneration (constitution IX)

- **Decision**: Add `RoutePrefix => "/api/Revenue/DepositSlips"` to the `DepositSlips` endpoint group (same decision TRE-01 made for `/api/Revenue/ReceiptVouchers`), then regenerate the NSwag client (`npm run generate-api`) — the generated client currently targets `/api/DepositSlips`.
- **Rationale**: Contract is literal. Route rename is done before any frontend work so the generated client is born correct.
- **Alternatives rejected**: keeping `/api/DepositSlips` — contract deviation; hand-written client — mirrors-contract-only rule prefers generated client.

## R8: Approval-rule evaluation (constitution VII/VIII)

- **Decision**: Rewrite `ApproveDepositSlipCommandHandler` on the `ApprovePaymentOrderCommand` exemplar: `IApprovalRuleEvaluationService.EvaluateAsync("DepositSlip", totalAmount, …)` → role match via `IIdentityService` → `ApprovalHistory` with the evaluation snapshot + `IDocumentStatusLogger` for the status log. Fallback: if no rules configured for "DepositSlip", require the configured TreasuryManager role (documented fallback, not silent pass).
- **Gap**: Current handler hardcodes `RequiredRole = "TreasuryManager"` with no evaluation, no snapshot.
- **Rationale**: Sensitive document transitions MUST pass server-side approval-rule evaluation and record an evaluation snapshot. Self-approval (FR-014) stays allowed: only permission + role rules gate the decision.
- **Alternatives rejected**: keeping the hardcoded role — constitution VIII violation; skipping evaluation because policies are placeholders — use-case-layer authorization is the effective control until the registered exception remediates.

## R9: Cancelled-member guard (edge case)

- **Decision**: Approval rejects when ANY member voucher is no longer `Approved` (cancelled after joining), with an error naming the offending voucher.
- **Gap**: No such guard exists.
- **Rationale**: "عضو سندُه أُلغي بعد الانضمام (قاعدة خادمية)" is an explicit edge case; an Approved slip must never contain a cancelled voucher (SC-002-adjacent invariant).

## R10: Test strategy

- **Decision**: Red-first tests, per constitution XI: `Application.UnitTests/Revenue` for handler decision tables (empty create, homogeneity, reason enforcement, guards, concurrency failure paths); `Application.FunctionalTests/Revenue/DepositSlipLifecycleTests.cs` + `MonthlyStatementTests.cs` against the real DB (lifecycle, approval history snapshot, event emission for Form47, statement zeros); frontend vitest for the picker filter and read-only Approved state; Web.AcceptanceTests for the full journey.
- **Rationale**: Every FR gap maps to at least one failing-first test; full 5-project suite gates at converge only (per-cycle economy per AGENTS.md).
- **Alternatives rejected**: asserting only through endpoints (slower, hides which rule failed).
