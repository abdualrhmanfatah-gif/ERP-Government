# Data Model: Deposit Slips (TRE-02)

**Date**: 2026-09-07 | **Spec**: [spec.md](spec.md)

All entities exist (spec 017 baseline). No schema changes required — this document records the
verified shape and the validation rules this feature enforces.

## DepositSlip (`DepositSlips`)

`src/Domain/Revenue/Entities/DepositSlip.cs` — inherits `BaseAuditableEntity` (audit + RowVersion).

| Field | Type | Rules |
|---|---|---|
| `SlipNumber` | string, unique | `DSL-{D6}` from `IDocumentSequenceService` ("DepositSlip"), allocated in the creation transaction (FR-009) |
| `SlipDate` | DateOnly | ≥ latest member voucher date; ≤ today (FR-002). With zero members: only ≤ today (FR-015) |
| `FormType` | enum `FormType` (`Form47=1`, `Form48=2`) | Homogeneity key: Form47 ⇒ all members `PaymentMethod.Cash`; Form48 ⇒ all members `PaymentMethod.Check` (FR-001) |
| `Status` | enum `DepositSlipStatus` (`Draft=1`, `Approved=2`) | Lifecycle Draft→Approved, terminal; no cancel/delete/reopen (FR-011) |
| `ApprovedById` | int? | Set at approval; self-approval allowed (FR-014) |
| `ApprovedAt` | DateTimeOffset? | Set at approval |
| `TotalAmount` | decimal(23,2) | Computed server-side from member voucher lines on every membership change (FR-007); never client-trusted |
| `RowVersion` | byte[] (rowversion) | Verified on every mutation (FR-008); token round-trips from client (FR-012/constitution IX) |

Relationships: 1-many `ReceiptVoucher` (members). Approval decisions also land in `ApprovalHistory` + `DocumentStatusLog` (append-only) — never inline-only columns (FR-010).

## ReceiptVoucher (member) (`ReceiptVouchers`)

`src/Domain/Revenue/Entities/ReceiptVoucher.cs` — TRE-01 owns it; TRE-02 reads/writes membership only.

| Field (relevant) | Rules |
|---|---|
| `Status` | Must be `Approved` to be eligible (Draft/Cancelled never eligible); guard re-checked at approval (cancelled-after-join edge case) |
| `PaymentMethod` | `Cash`/`Check` — determines eligible slip form (FR-001) |
| `DepositSlipId` | int? — the single active membership. Removing from a Draft slip nulls it → voucher returns to the eligible pool and may join another Draft slip (FR-013). Membership via an Approved slip is permanent (no removal path) |
| `Lines` / `Checks` | Totals and Form48 transitions derive from these — never stored on the slip except the recomputed `TotalAmount` |

## Check (`Checks`)

`src/Domain/Revenue/Entities/Check.cs` — TRE-02 reads status; TRE-03 owns clearing.

- `Status`: `UnderCollection=1 / Cleared=2 / Bounced=3`. Form48 approval sets member checks to `UnderCollection` (idempotent; FR-004). Statement `totalCleared`/`totalBounced` read existing statuses.

## ApprovalHistory / DocumentStatusLog (append-only)

- Approval: actor, decision, time, optional reason, **approval-rule evaluation snapshot** (R8 — `IApprovalRuleEvaluationService` on the `ApprovePaymentOrder` exemplar).
- Status log: Draft→Approved transitions and Draft creation, via `IDocumentStatusLogger` where the exemplar uses it.

## Validation Rules (enforced server-side)

1. Homogeneity on Create and AddVoucher, keyed on `PaymentMethod` (R2).
2. `slipDate` bounds (FR-002); future date rejected.
3. Membership changes only while Draft; removal requires non-empty reason (R3).
4. At most one active membership per voucher; membership on Approved slip immutable (FR-013).
5. Approval: members ≥ 1 (FR-005), all members still Approved (R9), rule evaluation passed (R8), RowVersion matches (FR-008).
6. Slip `TotalAmount` recomputed from member lines at create/add/remove (FR-007).

## State Transitions

```text
DepositSlip:  (created, may be empty) → Draft → Approved   [terminal]
ReceiptVoucher (member): Approved ──join slip──▶ member of Draft slip
                         ──remove (reason)──▶ unassigned (eligible again, FR-013)
                         ──slip approved──▶ permanently bound
Check: UnderCollection ──(Form48 slip approval)──▶ UnderCollection (idempotent)
       Cleared / Bounced ── out of scope (TRE-03)
```

## MonthlyStatement (read-only projection, not a table)

Per (year, month, fundId): `summary` six figures + `vouchers[]` (ALL Approved vouchers of the month — full activity, FR-016) + `clearings[]` (checks Cleared in the month). No fund dimension on vouchers yet — single-fund v1 reading, `fundName` resolved from `Funds` (R5). Empty month ⇒ explicit zeros.
