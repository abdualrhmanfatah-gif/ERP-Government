# Research: Checks & Monthly Statement (TRE-03)

**Branch**: `033-checks-statements` | **Date**: 2026-09-07

All unknowns resolved against the codebase (verified file:line). No open NEEDS CLARIFICATION remains.

## R1 — Checks list source and shape (FR-001)

- **Decision**: Build the list by querying `ReceiptVouchers` (Status = Approved, PaymentMethod = Check, VoucherDate within the user-picked from/to range, defaulting to the current month) with their `Checks` collection; project each check to `CheckDto`. Optional `status` filter param serves the "under-collection only" scenario (US1.2).
- **Rationale**: The spec forbids a standalone check registry (FR-001 — "مؤكد"). The voucher is the aggregate root holding check data; a date-range filter keeps queries bounded. Exemplars: `GetReceiptVouchersByPeriodQuery`, `GetDepositSlipsQuery`.
- **Alternatives considered**: A dedicated checks table scan ignoring vouchers — rejected (violates FR-001 and loses the voucher linkage); client-side filtering of all vouchers — rejected (unbounded query).

## R2 — Single check detail (OQ1)

- **Decision**: Add `GET /api/Revenue/Checks/{id}` returning `CheckDetailDto` (new DTO: `checkId, bankName, checkNumber, checkDate, amount, status, clearedAt?`). The FR-001 "no standalone endpoint" constraint applies to the list only.
- **Rationale**: Documented in spec Assumptions; mirrors `GetReceiptVoucherByIdQuery` pattern.
- **Alternatives considered**: Reuse voucher detail and let the client extract the check — rejected (contract names a dedicated DTO/endpoint per OQ1 resolution).

## R3 — Bounce semantics: lock, don't create (D1, D2)

- **Decision**: Rebuild `BounceCheckCommand`: transition check → Bounced, set `BouncedAt`, append DocumentStatusLog with reason. No voucher creation, no `DepositSlipId` mutation. The "pending-replacement lock" is derived state: `Check.Status == Bounced && ReplacementVoucherId == null` ⇒ source voucher locked for replacement only.
- **Rationale**: Matches clarified contract (Clarifications Q2, FR-003). Derived lock avoids a new schema column (constitution VI — no unnecessary schema change) and cannot go stale because both states live on the same row guarded by RowVersion.
- **Alternatives considered**: Explicit `IsLockedForReplacement` flag on ReceiptVoucher — rejected: redundant state that can desync from the check; a new voucher status value — rejected: voucher stays Approved per clarification.
- **Integration note (out of this feature's tasks)**: TRE-02's slip voucher picker should exclude vouchers of bounced-pending-replacement checks; the exclusion predicate is publishable from the Revenue query layer. Recorded as cross-feature follow-up in tasks.

## R4 — Replace semantics: create and link (D4, D11)

- **Decision**: Rebuild `ReplaceCheckCommand` per contract: require `Status == Bounced`; REJECT when `ReplacementVoucherId != null` (FR-005 double replace). Create the replacement `ReceiptVoucher` (number via `IDocumentSequenceService` — same document type as TRE-01 voucher creation; VoucherDate from request; amount = original check amount; PartyId/ReceivedFrom copied from source voucher; Status = Draft per TRE-01 lifecycle), set `check.ReplacementVoucherId`, and when `paymentMethod == Check` also create the new `Check` row (`UnderCollection`) from `CreateCheckDto` on the replacement voucher.
- **Rationale**: FR-004 + Clarifications Q3 (user supplies date; amount fixed; check details required for Check method). Sequence-service numbering is a binding AGENTS.md convention; the hand-rolled `RCV-` parser in the current code (D3) is deleted with the rebuild.
- **Alternatives considered**: Reusing `CreateReceiptVoucherCommand` via MediatR — rejected: it would enforce its own DTO/validation (e.g., non-fixed amount) and split the transaction; keeping the current "mutate auto-created voucher" flow — rejected: violates the binding contract (D4/D11).

## R5 — Clearing posting effect (D9, FR-009)

- **Decision (SUPERSEDED, DEP-027)**: Clearing's posting effect is now produced by the native `CreateAccrualEntry`/treasury posting handler during voucher collection; the `CheckCleared` event stays as a domain signal but the rule-engine path (rule seed, `EventTypeMapper`, `PostingPipelineHandler`) was removed with the PostingRules engine (DEP-027). No rule seed is added. Verify in functional tests that a balanced `JournalEntry` is produced via the outbox path linked to the source event.
- **Rationale**: SC-003 requires a visible, traceable posting effect per clearing; the pipeline is generic — only rule matching is missing. Rule-based posting keeps account configuration data-driven (constitution IV).
- **Alternatives considered**: Direct JournalEntry write in the handler — prohibited (constitution IV/II); a dedicated event consumer — unnecessary, generic pipeline already handles it.

## R6 — Date validation (D6, FR-002/FR-003)

- **Decision**: `ClearedAt`/`BouncedAt` validated server-side in both FluentValidation and the handler (defense in depth, constitution III): `date.Date >= check.CheckDate` and `date <= DateTimeOffset.UtcNow` (local-day tolerance for "today" via server-local conversion at handler level). Note `CheckDate` is `DateOnly` while transition stamps are `DateTimeOffset`.
- **Rationale**: Prevents back-dating before the check existed and future-dating that corrupts the monthly statement's month attribution.
- **Alternatives considered**: Validator-only — rejected: validators lack the loaded check; handler-only — rejected: cheap 400s belong in the validator pipeline.

## R7 — Concurrency pattern (D10, FR-008)

- **Decision**: Follow the established pattern: on tracked entities set the property entry's original RowVersion value to the incoming token so `SaveChangesAsync` raises `DbUpdateConcurrencyException`, caught and mapped to a distinct concurrency failure message. Never assign `RowVersion = request.RowVersion` directly.
- **Rationale**: Constitution VI (every update verifies the token) + IX (conflicts surfaced distinctly). The current manual assignment would overwrite the token and silently skip conflict detection — a latent financial-integrity bug the rebuild removes.
- **Alternatives considered**: Manual token comparison before save — rejected: racy without the DB-level check.

## R8 — Permissions (D5)

- **Decision**: Add `public const string ChecksReplace = "Checks.Replace";` to PermissionCodes.cs (alongside existing `Checks.View/Clear/Bounce` — verified present) and register the policy in src/Web/DependencyInjection.cs following the repo's current placeholder pattern (`RequireAssertion(_ => true)`) per registered exception #1. Endpoint `replace` switches to `ChecksReplace`. Add seed row for the new permission in RolePermissionSeedData only if the Revenue seed block already lists `Checks.*` codes (keep consistent; verification task).
- **Rationale**: Constitution VII (every endpoint declares a named permission); contract is literal.

## R9 — Monthly statement clearings (US4)

- **Decision**: No change to `GetMonthlyStatementQuery` — `Clearings[]` already projects cleared checks (`CheckClearingDto` verified at GetMonthlyStatementQuery.cs:83-89). Month attribution uses `ClearedAt`; empty months naturally yield an empty list (explicit-empty UI rule is frontend).
- **Rationale**: Existing code satisfies FR-012; duplicating logic would drift.
- **Alternatives considered**: Restructure to query the new list endpoint — rejected: statement is a read-model composition, keep server-side.

## R10 — Frontend scope

- **Decision**: New `features/treasury/checks/` folder: list page (date-range default current month, status filter, table: رقم/بنك/تاريخ/مبلغ/سند/الحالة) + clear/bounce/replace dialogs (Zod schemas entity-scoped, RowVersion round-trip, server errors via `result-to-ui.ts`). Feature-scoped components in `src/components/` domain-prefixed. Statement clearings tab already rendered by TRE-02 — verify only. No frontend tests (governance override); lint + build must pass.
- **Rationale**: Frontend Architecture layer map in AGENTS.md; NSwag client regenerated via `npm run generate-api` after endpoint changes.

## R11 — Test strategy (constitution XI)

- **Decision**: TDD per cycle. Functional tests (Application.FunctionalTests/Revenue) cover: lifecycle transitions + guards (clear-bounced, bounce-cleared, double-replace, cancelled-voucher, date rules), concurrency conflict, DocumentStatusLog append, replacement voucher creation (numbering, amount fixed, linkage), posting pairing (AccountingEvent + JournalEntry after clear). Unit tests for validators/date logic. Acceptance tests for the two P1 journeys (list→clear; bounce→replace).
- **Rationale**: Constitution XI non-negotiables; exemplars: AppropriationEdgeCaseTests.cs, EncumbranceLifecycleTests.cs.
- **Alternatives considered**: None — TDD is mandatory.
