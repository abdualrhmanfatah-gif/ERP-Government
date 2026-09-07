# Research: Receipt Vouchers (TRE-01)

**Branch**: `031-receipt-vouchers` | **Date**: 2026-09-07

## R1: Voucher number prefix — contract mandates `DSL-{D6}`

**Decision**: Change `PrefixMap["ReceiptVoucher"]` from `"RCV"` to `"DSL"` in `DocumentSequenceService`.

**Rationale**: The TRE-01 binding contract is literal: FR-001 and the DTO contract state `voucherNumber` is `DSL-{D6}`. The sequence table is keyed by `DocumentType`, so `ReceiptVoucher` and `DepositSlip` keep independent counters even though both display the `DSL` prefix. Gaps-in-sequence remain auditable per document type.

**Alternatives considered**:
- Keep `RCV` and amend the spec — rejected: the spec declares the contract binding and the clarification session did not touch it.
- Reuse the `DepositSlip` sequence for vouchers — rejected: merges two independent counters; gaps would span both documents and break per-document auditability.

**Risk note**: Both DepositSlip and ReceiptVoucher numbers will visually share the `DSL` prefix. If operations later requires distinguishable prefixes, that is a contract change requiring a spec amendment — not a silent divergence. Verify a seeded `DocumentSequences` row exists for `DocumentType == "ReceiptVoucher"`; add/repair seed if missing (idempotent seeding per constitution VI).

## R2: SoD guard removal (self-approval allowed)

**Decision**: Delete the `voucher.SubmittedById == userId` rejection in `ApproveReceiptVoucherCommandHandler`.

**Rationale**: Clarification session 2026-09-07, Q1 — the user explicitly chose Option A: the same user may create, submit, and approve. The constitution's SoD principle evaluates against the configured conflict matrix; no conflict is configured for receipt vouchers, and the spec records no submitter/approver separation requirement.

**Alternatives considered**: Keep guard configurable (Option C) — rejected: user chose unconditional A; adding config is scope creep.

## R3: Reviewer display (SC-002) without breaking the literal DTO

**Decision**: Frontend resolves `reviewedById` → reviewer display name through the existing users lookup/cache (user-management feature already loads users); backend DTO stays exactly as contracted.

**Rationale**: The binding contract enumerates DTO fields literally — `reviewedById`, `reviewedAt` — with no `reviewedByName`. SC-002 requires reviewer "name/time" display. Client-side lookup satisfies SC-002 while preserving the literal contract.

**Alternatives considered**: Add `reviewedByName` to the DTO — rejected: contract is declared literal/harfî; reshaping the payload is a breaking change requiring a decision record.

**Fallback**: If a reviewer was deleted/deactivated, display the numeric id with a muted style. (Users are never hard-deleted per constitution VI, so this is defensive only.)

## R4: Create response shape

**Decision**: `CreateReceiptVoucherCommand` returns `Result<ReceiptVoucherDto>`; the create endpoint responds `201` with the DTO body.

**Rationale**: FR-001 requires the number displayed immediately at Draft creation; returning only `int` forces a second round-trip and shows nothing server-authoritative. The mapped DTO already exists (used by all queries); reusing it keeps the response inside the binding contract.

**Alternatives considered**: Keep `int` + immediate GET by id — rejected: two calls, non-atomic display; client-generated number preview — rejected: numbers MUST come from the server sequence (constitution III).

## R5: Numbering transactionality and gaps

**Decision**: Keep the current allocation pattern (sequence incremented and saved inside the create use case; visible gaps accepted).

**Rationale**: The spec lists "number gaps visible in order" as accepted behavior; gaps from failed creates are acceptable. `DocumentSequenceService` relies on RowVersion optimistic concurrency, surfacing conflicts distinctly (constitution IX).

**Alternatives considered**: Transactional numbering with rollback reclaim — rejected: contradicts accepted gap behavior and adds lock contention for no spec value.

## R6: Cancel lifecycle guard

**Decision**: Cancel allowed only from Draft or PendingReview. Cancel from Approved → rejected server-side, with or without a deposit slip. FromStatus captured before mutation.

**Rationale**: Spec lifecycle: `Draft → Cancelled`, `PendingReview → Cancelled`; Approved is terminal in scope. The existing guard (reject Approved-only-when-on-slip) is more permissive than the contract. FromStatus bug: current code reads `voucher.Status` after assignment, so the log always records "Cancelled → Cancelled" — an audit-accuracy defect (constitution VIII).

**Alternatives considered**: Allow cancel of Approved pre-deposit (current behavior) — rejected: contradicts clarified lifecycle; approved revenue recognition is final pending TRE-02/TRE-03 flows.

## R7: Σ(checks) vs total rule

**Decision**: Enforce at create AND submit: `sum(checks.Amount) > sum(lines.Amount)` → rejection with explicit error. Σ(checks) < total allowed (partial coverage).

**Rationale**: FR-008 + clarification Q2 (Option B). Enforcing at submit too protects vouchers created before the rule or via any other path.

**Alternatives considered**: Exact equality (Option C) — rejected by user choice; create-only enforcement — rejected: leaves pre-rule drafts unvalidated.

## R8: `receivedFrom` optionality

**Decision**: Remove the `NotEmpty` rule; command/DTO field becomes nullable/optional; entity keeps non-null string storage (empty = not provided). UI: optional free text, placeholder "افتراضي: اسم الجهة" semantics per clarification Q3.

**Rationale**: Binding contract lists `receivedFrom?`. Clarified usage: only when actual payer differs from registered party.
