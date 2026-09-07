# Data Model: Checks & Monthly Statement (TRE-03)

**Branch**: `033-checks-statements` | **Date**: 2026-09-07

No schema changes required — all entities exist (verified against `InitialCreate` migration and current configurations). This document pins the as-built shape the feature operates on and the state machine it enforces.

## Entities

### Check — `src/Domain/Revenue/Entities/Check.cs` (exists, no change)

| Field | Type | Notes |
|---|---|---|
| Id | int PK | |
| ReceiptVoucherId | int FK → ReceiptVouchers | Restrict (established FK policy) |
| BankName | string | required |
| CheckNumber | string | required |
| CheckDate | DateOnly | date validation anchor (FR-002/003) |
| Amount | decimal(23,2) | money precision per convention |
| Status | enum CheckStatus (int) | UnderCollection=1, Cleared=2, Bounced=3 |
| ClearedAt | DateTimeOffset? | set by clear |
| BouncedAt | DateTimeOffset? | set by bounce |
| ReplacementVoucherId | int? FK → ReceiptVouchers | set by replace; null+ status Bounced ⇒ pending-replacement lock |
| RowVersion | byte[] | optimistic-concurrency token |
| + BaseAuditableEntity audit fields | | Created/By, LastModified/By |

Domain events raised: `CheckCleared` (exists, src/Domain/Events/Revenue/CheckCleared.cs, IHasSourceEntity → `EventType.CheckCleared`). `CheckBounced` is NOT a posting event — bounce has no ledger effect; status history records it.

### ReceiptVoucher — `src/Domain/Revenue/Entities/ReceiptVoucher.cs` (exists, no change)

Relevant members: `VoucherNumber` (sequence-allocated), `VoucherDate`, `PartyId`, `PaymentMethod` (Cash/Check), `ReceivedFrom`, `Status` (Draft=0/PendingReview=1/Approved=2/Cancelled=3), `DepositSlipId?`, `RowVersion`, `Checks` collection.

Not modified by this feature. The pending-replacement lock is derived (see State Machine) — no new column.

### Replacement ReceiptVoucher (created by ReplaceCheck)

Same shape as ReceiptVoucher; values: number via `IDocumentSequenceService` (same document type as TRE-01), `VoucherDate` = user-supplied, `PartyId`/`ReceivedFrom` copied from source voucher, `PaymentMethod` = chosen method, `Status` = Draft (TRE-01 lifecycle applies: submit → approve → post), Notes reference the bounced check. Amount = original check amount (fixed — enforced as sum of lines).

### Check (replacement) — created only when PaymentMethod = Check

New `Check` row on the replacement voucher from `CreateCheckDto` (bankName/checkNumber/checkDate/amount), `Status = UnderCollection`, enters the same lifecycle.

### DocumentStatusLog — append-only (exists)

Entries on clear and bounce: EntityName=`Check`, DocumentId, FromStatus, ToStatus, ChangedById, ChangedAt, Reason (bounce). Satisfies FR-007/SC-001.

### AccountingEvent / PostingRule / JournalEntry (exists)

Clearing posts via the generic pipeline: `CheckCleared` → `AccountingEvent(EventType.CheckCleared, SourceDocumentType=Check, SourceDocumentId=check.Id, Status=Pending)` → outbox `PostingPipelineHandler` → `PostingRule` match (NEW seed: `EventType="CheckCleared"`) → balanced `JournalEntry` + lines. One AccountingEvent per (SourceTable, SourceId, EventType) — idempotent re-drive (constitution II).

### CheckDetailDto (NEW — Application layer only)

`checkId, bankName, checkNumber, checkDate, amount, status, clearedAt?` — projection for `GET /api/Revenue/Checks/{id}` (R2).

## State Machine (Check)

```text
                 clear(id, clearedAt≥CheckDate, ≤today)      [voucher not Cancelled]
UnderCollection ───────────────────────────────────────────▶ Cleared        (terminal; posts via CheckCleared)
       │
       │      bounce(id, bouncedAt≥CheckDate, ≤today, reason?)   [voucher not Cancelled]
       └──────────────────────────────────────────────────────▶ Bounced
                                                                    │  replace(checkId, method, date, checkDetails?)
                                                                    │  [ReplacementVoucherId == null — else reject FR-005]
                                                                    ▼
                                                        Bounced + ReplacementVoucherId set
                                                        (replacement voucher Draft; new check → UnderCollection)
```

Guards (all server-side, FR-006/FR-011):

- From-state must be `UnderCollection` for clear and bounce; `Bounced` for replace.
- No transition out of Cleared or Bounced (except replace linkage on Bounced).
- Source voucher `Status == Cancelled` ⇒ every action rejected.
- Date rule: transition date ≥ check date, ≤ today.
- Double replace blocked by `ReplacementVoucherId != null` check inside the same guarded write (SC-002 — race-safe: guarded by RowVersion + state re-check in one SaveChanges).

## Derived state: pending-replacement lock

`Check.Status == Bounced && Check.ReplacementVoucherId == null` ⇒ the source voucher is locked for replacement only (FR-003): it cannot join deposit slips or be consumed by other flows while the lock holds. Read side exposes this via the checks list/detail. Cross-feature consumers (TRE-02 slip picker) exclude such vouchers (follow-up recorded in tasks).

## Validation Rules

| Rule | Where |
|---|---|
| clearedAt/bouncedAt: `≥ CheckDate` and `≤ today` | Validator (cheap 400) + handler (authoritative, has the check) |
| replace: `paymentMethod == Check ⇒ checkDetails != null` | Validator + handler |
| replace: status Bounced, ReplacementVoucherId null, voucher not Cancelled | Handler (state-dependent) |
| clear/bounce: status UnderCollection, voucher not Cancelled | Handler (state-dependent) |
| RowVersion required and verified | Validator (presence) + EF token check (conflict) |
| replacement amount == original check amount | Handler (line construction fixed to check.Amount) |
