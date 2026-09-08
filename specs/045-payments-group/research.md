# Research: Payments Group (PAY-01..04)

**Branch**: `045-payments-group` | **Date**: 2026-09-08

All unknowns resolved by direct code inspection of the as-built Payments module (AGENTS.md: "the code is the truth"). No external research required.

## D1 — Where does the budget check live, and how is it upgraded to spec?

**Decision**: Keep the check inside `SubmitPaymentOrderCommand` but replace the fund-level heuristic (`context.Budgets.FirstOrDefault(FundId...)`) with `IBudgetAvailabilityService.GetAvailableForAppropriationAsync(entity.AppropriationId)` compared against the order's net amount (`AmountGross − DeductionAmount`). `Overridden` is set by `ApprovePaymentOrderCommand` only.

**Rationale**: `BudgetAvailabilityService` (src/Application/Budgeting/Common/BudgetAvailabilityService.cs) is the project's SSOT for appropriation-level availability (net appropriations − open encumbrances); Principle V requires the check before approval and multi-dimension awareness. The current fund-level check is a placeholder (its own comment admits it).

**Alternatives considered**: dedicated BudgetCheck use case (rejected — adds a boundary the spec doesn't require; check happens at submit as the lifecycle guard); check at approve time only (rejected — spec requires the badge state visible before approval).

## D2 — How is the Failed-check override modeled? (clarified Q3)

**Decision**: `ApprovePaymentOrderCommand` gains an optional `OverrideFailedBudgetCheck` flag. When `BudgetCheckStatus == Failed`, approval fails with an explicit message unless the flag is set AND the caller holds the new `PaymentOrders.OverrideBudgetCheck` permission; on override the command sets `BudgetCheckStatus = Overridden` and logs the decision through `IDocumentStatusLogger` + an `ApprovalHistory` row with an evaluation snapshot (actor/decision/time/reason).

**Rationale**: Constitution V ("overrides MUST be explicit, permissioned, and recorded") + VIII (decision history at the moment of decision). Permission-code-per-action matches the as-built RBAC shape (`PermissionCodes` constants + `[Authorize]` on commands + policy registration in `Web/DependencyInjection.cs`).

**Alternatives considered**: separate `/override` endpoint (rejected — the decision is still "approve"; one endpoint keeps the lifecycle table honest); role-based check (rejected by clarification Q3 — permission code, roles bind later in RBAC wiring).

## D3 — Void guard correction (clarified Q1)

**Decision**: `VoidPaymentOrderCommand` guard changes from `Status == Paid` to `Status is Approved or SentToTreasury` AND paid amount (from `/totals` logic: payments recorded against the order) == 0. Partially paid → refused with a verbatim message. An order with a non-terminal disbursement request (Draft/PendingApproval/Approved) → refused, message directs the user to cancel/void the request first (the request itself is auto-invalidated per D4 when the order is already gone). Remove the `TODO: reversing entry` — nothing was posted for an unpaid order.

**Rationale**: clarified 2026-09-08 (Q1: Option A). Constitution IV: reversing entries only exist where a posted entry exists.

**Alternatives considered**: keep Paid-only void as a paid-reversal (rejected — contradicts clarified contract); allow partial void with reversal (rejected — Option A explicitly refuses partial).

## D4 — Automatic request invalidation (clarified Q4)

**Decision**: In `CancelPaymentOrderCommand` and `VoidPaymentOrderCommand`, after the order transitions, any `DisbursementRequest` linked to it in `Draft`/`PendingApproval`/`Approved`-unpaid state is set to `Invalidated`. Each invalidation writes an `ApprovalHistory` row (Action = Invalidate/Cancel snapshot, reason = "linked order cancelled/voided").

**Rationale**: clarified 2026-09-08 (Q4: Option A); enum state already exists (`DisbursementRequestStatus.Invalidated = 6`, never assigned — dead enum weight removed). Keeps BR-4 (release rule) symmetric: `CancelDisbursementRequestCommand` already releases by zeroing `PaymentOrderId`.

**Alternatives considered**: manual invalidate endpoint (rejected — no UI story, adds permission surface); not used in v1 (rejected by clarification).

## D5 — Dual-signature step-2 role check (clarified Q2)

**Decision**: `ApproveDisbursementRequestCommand` step 2 adds the same `RequiredRoles` membership check as step 1 (`AccountsManager`/`AuthorizingOfficer` via `IIdentityService.IsInRoleAsync`) and records `RequiredRole` on the history row (currently hard-coded empty string). Distinct-user check already exists and stays.

**Rationale**: clarified 2026-09-08 (Q2: Option A — both steps qualified). The existing handler already proved the pattern in step 1; the role name is persisted so the Stepper can render it from `approvals[]`.

**Alternatives considered**: split roles per step (AccountsManager then AuthorizingOfficer — rejected; clarification chose per-signer qualification, not role ordering).

## D6 — Bank account default auto-switch (clarified Q5)

**Decision**: In `CreateBankAccountCommand` and `UpdateBankAccountCommand`, when `IsDefault == true`, demote all other accounts (`IsDefault = false`) inside the same `SaveChangesAsync` transaction. Currency-scoping is NOT applied (single default across all accounts, matching the as-built single-flag contract).

**Rationale**: clarified 2026-09-08 (Q5: Option B); Constitution VI — atomic transaction; BR-2 — exactly one default at all times.

**Alternatives considered**: refuse second default (rejected by clarification); per-currency default (rejected — contract has one boolean, inventing scoping would diverge from the OpenAPI shape).

## D7 — Partial payments: ContractSnapshot conflict (DEFERRED — flag for converge)

**Decision**: Implement `PartiallyPaid`/`remainingAmount` in Totals as `netAmount − Σ completed payment amounts`, but RecordPayment v1 keeps paying the full snapshot (`Amount = netTotal`, no API amount field exists — spec FR-003/contract L35672 has **no amount field**). Consequence: with the current contract, every payment is full and `PartiallyPaid` is unreachable in practice; the Totals arithmetic ships ready for it.

**Rationale**: The HTTP contract is SSOT (Constitution IX); adding an amount field is a breaking contract change requiring a decision record — out of scope here. PAY-01 US4 partial scenario is satisfied structurally (Totals math) without inventing a payload field.

**Alternatives considered**: adding `amount?` to `RecordPaymentRequest` (rejected — contract change, needs DEP decision); calculating partials client-side (forbidden — CC-1).

## D8 — exchangeRate conditionality (spec OQ-N3, deferred)

**Decision**: Keep `ExchangeRate` optional in `CreatePaymentOrderCommand` (as-built). Validation stays: required (non-null, > 0) only when `CurrencyId != base currency` — add this refine in the command handler (an `ExchangeRate` entity from FinancialSettings 024 exists for validity lookup, but no rate-row enforcement is added here).

**Rationale**: Constitution IV (foreign-currency lines carry their rate); minimal change; base-currency determination uses the existing single-active-base-currency invariant.

**Alternatives considered**: always-required (rejected — base-currency orders don't need it); server-derived rate lookup (deferred — FinancialSettings owns rate validity; not in this spec's scope).

## D9 — Payment permission codes (spec note stale)

**Decision**: No new codes needed except `PaymentOrders.OverrideBudgetCheck`. Verified in `src/Application/Common/Security/PermissionCodes.cs`: `BankAccounts.View/Create/Update/Activate/Deactivate` ✓, `PaymentOrders.*` (9 codes) ✓, `DisbursementRequests.*` (6 codes) ✓, `Payments.View/Create` ✓. The new code is added to `PermissionCodes`, `[Authorize]` on the approve command path, policy registration in `Web/DependencyInjection.cs`, and the idempotent permission merge in `RolePermissionSeedData.GetAllPermissionCodes()` + `ApplicationDbContextInitialiser` (existing merge logic re-runs safely — no migration needed unless a nullable column addition is required; none is).

**Rationale**: spec marked these GAP-ADD before the code scan; code scan shows they exist (research D9 supersedes the spec's permissions_status notes; spec section untouched per CONTRACT NOTE binding).

**Alternatives considered**: renaming codes to match spec guesses (forbidden — breaking change).

## D10 — PaymentStatus.Failed path

**Decision**: `RecordPaymentCommand` as-built always records `Status = Completed`. The Failed path (spec FR-005) surfaces only when the recording itself fails (Result.Failure — e.g., non-approved request, double payment constraint). A persisted `Failed` payment row requires a server-side recording pipeline that can fail post-write; that is not reachable from the current command shape, so no Failed rows are fabricated in tests either — failure UX renders the verbatim `Result` errors (CC-2) with retry = re-submitting the command.

**Rationale**: no silent fabrication of enum states; spec FR-005's UI obligations (verbatim reason + retry) are fully met through the Result error path.

**Alternatives considered**: synthesizing Failed rows for posting failures (rejected — posting is async event-driven (ACC-05); the payment row reflects recording truth, the event pipeline carries posting truth).

## D11 — Frontend structure

**Decision**: New `src/Web/ClientApp/src/features/payments/` domain with entity folders `payment-orders/`, `disbursement-requests/`, `payments/`, `bank-accounts/` plus cross-entity `hooks/`/`shared/`/`utils/` at the domain root (mirrors `features/budgeting/` exemplar). Feature-scoped components (filters, badges, Stepper, typed grids) live in `src/components/` with a `Payments*` prefix. API access via NSwag client regenerated after backend changes; manual wrappers only with a WHY header. Routes under `/payments/*`.

**Rationale**: AGENTS.md Frontend Architecture (entity-based nesting, no `components/` inside features, dependency-cruiser enforced acyclicity).

**Alternatives considered**: one flat payments folder (rejected — violates entity-based nesting convention).

## D12 — Void with existing disbursement request (spec OQ-N1 remainder)

**Decision**: Covered by D3 + D4 combined rule matrix:

| Order state | Non-terminal request exists? | Order cancel/void outcome |
|---|---|---|
| Draft | any | cancel allowed; request → Invalidated |
| Submitted | any | cancel allowed; request → Invalidated |
| Approved/SentToTreasury | Draft/PendingApproval | void allowed; request → Invalidated |
| Approved/SentToTreasury | Approved (unpaid) | void allowed; request → Invalidated |
| Approved/SentToTreasury | any request already Disbursed | impossible (order would be Paid/PartiallyPaid → void refused as partially paid) |

**Rationale**: single consistent rule; no request can survive its order's death except as Invalidated.

**Alternatives considered**: refusing void whenever a request exists (rejected — traps money behind a dead order; D4 auto-invalidation is the clarified behavior).
