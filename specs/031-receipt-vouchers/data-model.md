# Data Model: Receipt Vouchers (TRE-01)

**Date**: 2026-09-07 | **Spec**: [spec.md](./spec.md)

All three entities exist and are migrated (spec 017). No schema changes planned — every gap is code-level. Documented as-built for plan/tasks reference.

## Entity: ReceiptVoucher

**Table**: `ReceiptVouchers` | **Inherits**: `BaseAuditableEntity`

| Field | Type | Nullable | Constraints | Notes |
|-------|------|----------|-------------|-------|
| Id | int | No | PK, identity | From BaseEntity |
| VoucherNumber | string(20) | No | Unique index | `DSL-{D6}` via DocumentSequenceService (gap fix: prefix RCV → DSL) |
| VoucherDate | DateOnly | No | Required | |
| PartyId | int | No | FK → Party (Restrict) | Party must be active at create/submit |
| PaymentMethod | PaymentMethod | No | Enum (int) | Cash=0, Check=1 |
| ReceivedFrom | string(200) | No (semantically optional) | — | Optional free text; empty = payer = party (gap fix: drop NotEmpty rule) |
| Notes | string(500) | Yes | — | |
| DepositSlipId | int? | Yes | FK → DepositSlip (Restrict) | Set by TRE-02; FR-007 display link |
| Status | ReceiptVoucherStatus | No | Enum (int), default Draft | Draft=0, PendingReview=1, Approved=2, Cancelled=3 |
| SubmittedById | int? | Yes | — | Set on submit |
| SubmittedAt | DateTimeOffset? | Yes | — | |
| ReviewedById | int? | Yes | — | Set on approve; SC-002 display via users lookup |
| ReviewedAt | DateTimeOffset? | Yes | — | |
| CancellationReason | string(500) | Yes | Required when Cancelled | FR-006 |
| RowVersion | byte[] | No | Optimistic concurrency | Round-tripped on every mutation (constitution IX) |

Audit columns from BaseAuditableEntity: Created, CreatedBy, LastModified, LastModifiedBy.

### State transitions (server-guarded)

| From | Action | To | Guards |
|------|--------|----|--------|
| — | create | Draft | ≥1 line, party active, method-conditional checks, Σ(checks) ≤ Σ(lines) |
| Draft | submit | PendingReview | ≥1 lines, Σ(checks) ≤ Σ(lines), rowVersion |
| PendingReview | approve | Approved | rowVersion; reason optional; NO submitter≠approver restriction (clarified); history + status log |
| Draft \| PendingReview | cancel | Cancelled | reason required; rowVersion; **Approved is terminal — reject**; deposit-linked cancel rejected |
| Approved, Cancelled | any | — | Terminal; all transitions rejected |

## Entity: ReceiptVoucherLine

**Table**: `ReceiptVoucherLines` | **Inherits**: `BaseAuditableEntity`

| Field | Type | Nullable | Constraints | Notes |
|-------|------|----------|-------------|-------|
| Id | int | No | PK | |
| ReceiptVoucherId | int | No | FK → ReceiptVoucher (Restrict) | |
| RevenueAccountId | int | No | FK → Account (Restrict) | Must exist and be active |
| Amount | decimal(23,2) | No | > 0 | |
| Description | string(200) | Yes | | |
| RowVersion | byte[] | No | | |

## Entity: Check

**Table**: `Checks` | **Inherits**: `BaseAuditableEntity`

| Field | Type | Nullable | Constraints | Notes |
|-------|------|----------|-------------|-------|
| Id | int | No | PK | |
| ReceiptVoucherId | int | No | FK → ReceiptVoucher (Restrict) | |
| BankName | string(100) | No | Required when method=Check | |
| CheckNumber | string(50) | No | Required when method=Check | |
| CheckDate | DateOnly | No | Required when method=Check | |
| Amount | decimal(23,2) | No | > 0 | Σ(checks) ≤ voucher total (FR-008) |
| Status | CheckStatus | No | Default UnderCollection | Managed in TRE-03 |
| ClearedAt / BouncedAt / ReplacementVoucherId | — | Yes | — | TRE-03 domain |
| RowVersion | byte[] | No | | |

## Validation rules (enforced server-side)

1. ≥1 line at create; lines referenced again at submit (zero-line submit rejected).
2. Party exists and is active at create (selector also excludes disabled parties).
3. method=Check ⇒ ≥1 check with all four fields; method=Cash ⇒ checks not allowed.
4. Σ(checks.Amount) ≤ Σ(lines.Amount) at create and submit (FR-008; equality not required).
5. Total computed server-side from Σ(lines) — never client-supplied (FR-004).
6. Cancel requires reason; lifecycle guard per transitions table; FromStatus captured before mutation.
7. Every mutation verifies RowVersion.

## Sequence

- `DocumentSequences` row keyed `DocumentType = "ReceiptVoucher"`, prefix now `DSL`, format `DSL-{D6}`. Allocated within the create use case; gaps accepted and auditable.

## Domain events (existing, out of scope for this feature's tasks)

- `ReceiptVoucherCollected` (deposit-slip approval path, TRE-02) — unchanged.
