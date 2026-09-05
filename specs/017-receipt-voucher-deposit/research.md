# Research: Receipt Voucher & Deposit Slip Workflow

**Date**: 2026-09-05
**Feature**: 017-receipt-voucher-deposit

## Research Tasks

### 1. DocumentSequenceService Voucher Number Generation

**Decision**: Use existing `IDocumentSequenceService` with prefixes `RCV` (ReceiptVoucher) and `DSL` (DepositSlip).

**Rationale**: The service already supports atomic increment with rowversion concurrency check inside a transaction. Prefix map already includes `["ReceiptVoucher"] = "RCV"` and `["DepositSlip"] = "DSL"`. Format: `"{prefix}-{CurrentNumber:D6}"` (e.g., `RCV-000001`).

**Alternatives considered**:
- Inline sequence generation (rejected — duplicates concurrency-safe allocation)
- Separate sequence tables (rejected — over-engineering for existing pattern)

**Impact**: No changes needed to `DocumentSequenceService`. Just ensure `ReceiptVoucher` and `DepositSlip` are registered in the prefix map (already present per codebase exploration).

---

### 2. DepositSlip MERGED DESIGN — No Junction Table

**Decision**: `ReceiptVoucher` has a nullable `DepositSlipId` FK directly on the entity.

**Rationale**: User specified "MERGED DESIGN: no junction table". A voucher can only belong to one slip at a time. When added to a slip, `DepositSlipId` is set. When removed, it's cleared.

**Alternatives considered**:
- Junction table `DepositSlipMember` (rejected — user explicitly excluded)
- Many-to-many via EF Core (rejected — violates single-slip constraint)

**Impact**: Simplifies queries (single FK join). Voucher status must be checked before addition (must be Approved). Removal sets `DepositSlipId = null`.

---

### 3. Form 47/48 Homogeneity Enforcement

**Decision**: Validate at use-case layer when adding vouchers to a slip.

**Rationale**: Constitution Principle III requires server-side business rule enforcement. The check examines all voucher lines: if any line has a check, the voucher is "check-type"; otherwise "cash-type". Form 47 only accepts cash-type vouchers; Form 48 only accepts check-type.

**Alternatives considered**:
- Database check constraint (rejected — complex cross-table logic)
- Frontend validation only (rejected — violates server-side enforcement)

**Impact**: `AddVoucherToSlipCommand` must query voucher lines to determine payment method mix before allowing addition.

---

### 4. Reviewer Gate Enforcement

**Decision**: `ApproveReceiptVoucherCommand` requires the approver to have `AccountsReviewer` role. The `SubmittedBy` and `ApprovedBy` must be different users.

**Rationale**: Constitution Principle VII (Separation of Duties). User specified "Approval without a reviewer is rejected."

**Alternatives considered**:
- Same-user approval with justification (rejected — user explicitly forbade)
- System auto-approval (rejected — violates reviewer gate requirement)

**Impact**: Command handler checks `IUser.Id != voucher.SubmittedById` before allowing approval. `ApprovalHistory` record created with `RequiredRole = "AccountsReviewer"`.

---

### 5. Two-Stage Revenue Recognition

**Decision**: 
- Form 47: Revenue recognized on slip approval → `AccountingEvent` with `EventType.ReceiptVoucherCollected`
- Form 48: Revenue recognized on check clearing → `AccountingEvent` with `EventType.CheckCleared`

**Rationale**: User specified "revenue recognized at collection only (Form 47 on slip approval, Form 48 on check clearing)". Constitution Principle IV requires balanced journal entries.

**Alternatives considered**:
- Single recognition point (rejected — violates two-stage requirement)
- Deferred recognition with accrual (rejected — user specified cash-basis)

**Impact**: Two separate event handlers:
1. `ReceiptVoucherCollectedHandler` — handles Form 47 slip approval posting
2. `CheckClearedHandler` — handles Form 48 check clearing posting

---

### 6. Bounced Check Voucher Reopening

**Decision**: When a check bounces, the system sets `Check.Status = Bounced`, records `BouncedAt`, and creates a new `ReceiptVoucher` in Draft status linked to the original party. The original voucher's `DepositSlipId` is cleared.

**Rationale**: User specified "bounced check reopens the member voucher for a replacement check or cash." The new voucher allows the party to provide replacement payment.

**Alternatives considered**:
- Modify original voucher status (rejected — breaks audit trail)
- Mark original as "Reopened" (rejected — user specified "replacement check or cash" implies new voucher)

**Impact**: `BounceCheckCommand` creates a new voucher. `ReplacementVoucherId` on `Check` links original to replacement.

---

### 7. Migration Strategy — RevenueReceipt → ReceiptVoucher

**Decision**: 
1. Create new tables (`ReceiptVoucher`, `ReceiptVoucherLine`, `Check`, `DepositSlip`)
2. Migrate data from `RevenueReceipt` → `ReceiptVoucher` and `RevenueReceiptLine` → `ReceiptVoucherLine`
3. Update `PostingRules` EventType from `"RevenueReceiptPosted"` → `"ReceiptVoucherCollected"`
4. Drop `RevenueReceipt` and `RevenueReceiptLine` tables after verification

**Rationale**: User specified "DROP RevenueReceipt + RevenueReceiptLine WITH data migration." Constitution Principle VI requires versioned migrations.

**Alternatives considered**:
- Side-by-side operation (rejected — user wants clean replacement)
- Rename tables only (rejected — schema changes needed for new fields)

**Impact**: Single migration with data transformation. Old event type `RevenueReceiptPosted` (8) retained in enum for historical data; new event type `ReceiptVoucherCollected` (12) added.

---

### 8. BankAccount Entity Modification

**Decision**: Drop `BankName`, `CurrentBalance`, `OpeningBalance` columns from `BankAccount` entity. Bank reconciliation delegated outside system.

**Rationale**: User specified "drop BankName, CurrentBalance, OpeningBalance columns (delegated to bank reconciliation outside system — documented m26 deviation)."

**Alternatives considered**:
- Keep columns but mark as deprecated (rejected — user specified drop)
- Create separate BankReconciliation entity (rejected — out of scope)

**Impact**: Register as registered exception under Principle XII. Migration drops columns.

---

### 9. PermissionCodes for New Endpoints

**Decision**: Add new permission codes following existing naming convention:

```csharp
// ReceiptVouchers
public const string ReceiptVouchersView = "ReceiptVouchers.View";
public const string ReceiptVouchersCreate = "ReceiptVouchers.Create";
public const string ReceiptVouchersSubmit = "ReceiptVouchers.Submit";
public const string ReceiptVouchersApprove = "ReceiptVouchers.Approve";
public const string ReceiptVouchersCancel = "ReceiptVouchers.Cancel";

// DepositSlips
public const string DepositSlipsView = "DepositSlips.View";
public const string DepositSlipsCreate = "DepositSlips.Create";
public const string DepositSlipsManage = "DepositSlips.Manage";
public const string DepositSlipsApprove = "DepositSlips.Approve";

// Checks
public const string ChecksView = "Checks.View";
public const string ChecksClear = "Checks.Clear";
public const string ChecksBounce = "Checks.Bounce";
```

**Rationale**: Constitution Principle VII requires named permissions on every endpoint. Follows `Module.Action` pattern.

**Alternatives considered**:
- Reuse existing `RevenueReceipts.*` codes (rejected — different entity, different permissions)
- Generic `Revenue.*` codes (rejected — too broad for separation of duties)

**Impact**: Update `PermissionCodes.cs`. Old codes retained for backward compatibility during migration.

---

### 10. Monthly Statement Generation

**Decision**: Query-only operation that aggregates:
- All `ReceiptVoucher` records for the specified month/fund
- All `DepositSlip` records containing those vouchers
- All `Check` clearing events for those vouchers
- Compute totals by payment method

**Rationale**: User specified "monthly collections statement is generated per the financial law, for any month and fund." No persistence needed — computed on-demand.

**Alternatives considered**:
- Pre-computed materialized view (rejected — user specified on-demand)
- Background job generation (rejected — adds complexity for P3 priority)

**Impact**: `GetMonthlyStatementQuery` with date range and fund parameters. Returns DTO with voucher details, deposit slip references, and clearing events.
