# Feature Specification: Checks & Monthly Statement (TRE-03)

**Feature Branch**: `033-checks-statements`

**Created**: 2026-09-07

**Status**: Draft

**Input**: User description: "SPEC: TRE-03 — الشيكات والكشف الشهري (checks-statements)"

## Binding Contract Note

The Data Contract section below is **literal and binding** for implementation — field names, DTO shapes, API routes, lifecycle, enums, and permission codes MUST be implemented exactly as written here. Deviations require a spec amendment, not silent divergence.

## User Scenarios & Testing

### User Story 1 — Under-Collection Checks List (Priority: P1)

As a cashier, I want to open a checks screen that shows every check still under collection for the period, derived from its receipt voucher, so that I always know what the banks are holding.

**Why this priority**: Without this view the cashier has no visibility of outstanding checks; it is the read foundation every later action (clear, bounce) operates on.

**Independent Test**: Create receipt vouchers with checks for a period → open the checks list → every check appears with its current state (number / bank / date / amount / source voucher).

**Acceptance Scenarios**:

1. **Given** receipt vouchers with Check payment method in a period, **When** I open the checks list (defaulting to the current month, or after picking a date range), **Then** their checks appear with their current status and details (check number, bank, date, amount, source voucher).
2. **Given** a check already Cleared, **When** I filter the list to under-collection only, **Then** the cleared check does not appear.

---

### User Story 2 — Clearing (Priority: P1)

As a cashier, I want to clear an under-collection check with a confirmation and a clearing date so that revenue recognition for check collections happens at the moment the bank honors the check.

**Why this priority**: Clearing is the revenue-recognition moment for check collections — the financial heart of this feature.

**Independent Test**: Clear an under-collection check with a date → the check becomes Cleared and a posting effect is visible.

**Acceptance Scenarios**:

1. **Given** a check UnderCollection, **When** I clear it (confirmation + clearedAt), **Then** the check becomes Cleared and a ledger posting effect is produced and visible.
2. **Given** a check already Bounced, **When** I attempt to clear it, **Then** the request is rejected server-side.

---

### User Story 3 — Bounce and Replacement (Priority: P1)

As a cashier, I want to bounce an under-collection check (date + reason) so that the source voucher is reopened for replacement — by a new check or cash — without losing the collection.

**Why this priority**: A bounced check must not silently vanish; the voucher must reopen and the collection be recovered through a traceable replacement.

**Independent Test**: Bounce a check with a reason → the voucher becomes replaceable → replace with a chosen method → a linked replacement voucher is created.

**Acceptance Scenarios**:

1. **Given** a check UnderCollection, **When** I bounce it (bouncedAt + reason), **Then** the check becomes Bounced and its source voucher is locked for replacement only (no other flow may consume it until the replacement completes).
2. **Given** a Bounced check, **When** I replace it with cash (paymentMethod=Cash, supplying the voucher date), **Then** a replacement voucher of the same amount as the original check is created and linked (replacementVoucherId).
3. **Given** a Bounced check, **When** I replace it with a new check, **Then** new check details are required (bank / number / date / amount).
4. **Given** an already-replaced check, **When** I attempt to replace it a second time, **Then** the request is rejected server-side.

---

### User Story 4 — Monthly Statement Clearings (Priority: P2)

As a treasury officer, I want the monthly statement to show the month's check clearings so that month-end reconciliation covers cleared checks.

**Why this priority**: Periodic reporting, not blocking the daily flow; it consumes the state produced by US2.

**Independent Test**: Clear checks in a month → the monthly statement's clearings table lists them (number / bank / date / amount).

**Acceptance Scenarios**:

1. **Given** checks cleared within a month, **When** I open the monthly statement (tab of TRE-02), **Then** the clearings table lists each clearing with check number, bank, clearedAt, and amount.
2. **Given** a month with no clearings, **When** I open the statement, **Then** an explicitly empty clearings table is shown — never a blank screen or an error.

---

### Edge Cases

- Clearing a bounced check → rejected server-side.
- Bouncing a cleared check → rejected server-side.
- clearedAt/bouncedAt before the check date or in the future → rejected server-side.
- Double replacement → rejected server-side (replacementVoucherId already set).
- Source voucher cancelled before a check action → the action is rejected (a cancelled voucher's check cannot transition).
- Optimistic-concurrency token mismatch → operation rejected with a distinct concurrency error; user reloads and retries.
- Month with no clearings → explicitly empty table, not an error.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The system MUST build the under-collection checks list from the period's Check-method receipt vouchers (no standalone check registry endpoint — confirmed); the period is a user-picked date range (from/to) defaulting to the current month, matched against the source voucher date.
- **FR-002**: The system MUST offer clearing with a clearedAt date and confirmation; the date MUST be on or after the check date and no later than today (back-dating to the check date allowed, future dates rejected).
- **FR-003**: The system MUST offer bouncing with a bouncedAt date and a reason; the date MUST satisfy the same rule (>= check date, <= today); bouncing MUST lock the source voucher for replacement only — the voucher stays Approved but cannot be consumed by any other flow (slips, other replacements) until the replacement completes, via a pending-replacement link held by the bounced check.
- **FR-004**: The system MUST offer replacement with an explicit method (Cash/Check): the user supplies the replacement voucher date, the amount is fixed equal to the original check; when Check, the user additionally supplies the new check details (bank / number / date / amount) and those details are required.
- **FR-005**: The system MUST prevent double replacement: a check with a replacementVoucherId already set MUST be blocked from replacing again.
- **FR-006**: The system MUST verify the check's current status server-side before every action (clear / bounce / replace) — the state never lies.
- **FR-007**: The system MUST record every check status transition with its date (and the reason on bounce) in the append-only status history — never as inline-only columns.
- **FR-008**: The system MUST verify the concurrency token on every check action and MUST surface token conflicts as a distinct concurrency failure.
- **FR-009**: The system MUST produce a ledger posting effect for every clearing, through the established ledger-posting pipeline (domain event → accounting event → posting rules) — never a direct ledger write.
- **FR-010**: The system MUST enforce every operation behind a declared permission (View / Clear / Bounce / Replace).
- **FR-011**: The system MUST honor the lifecycle `UnderCollection → Cleared` and `UnderCollection → Bounced` only; no transition out of Cleared or Bounced exists.
- **FR-012**: The monthly statement's clearings table MUST list the month's clearings (check number, bank, clearedAt, amount) and MUST be explicitly empty when none exist.

### Key Entities *(include if feature involves data)*

- **Check**: A receipt-voucher check with a lifecycle (UnderCollection / Cleared / Bounced), transition dates (clearedAt / bouncedAt), a bounce reason, and a link to its replacement voucher when replaced. Created by TRE-01/TRE-02 flows; this feature owns its post-approval lifecycle.
- **ReceiptVoucher (source)**: The check voucher that, on bounce, stays Approved but is locked for replacement only (pending-replacement link held by the bounced check) until the replacement completes; the replacement produces a new linked voucher.
- **MonthlyStatement (clearings tab)**: The TRE-02 statement's clearings table, fed by this feature's cleared checks.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Every check status transition is recorded with its date (and reason on bounce) — provable by inspection of the history at any time.
- **SC-002**: Zero double replacements exist in the data (provably impossible, not merely discouraged).
- **SC-003**: Every clearing is paired with a visible, traceable posting effect in the ledger.

## Data Contract *(binding — literal)*

### CheckDto

`id, receiptVoucherId, bankName, checkNumber, checkDate, amount, status (enum CheckStatus: UnderCollection/Cleared/Bounced), statusName, clearedAt?, bouncedAt?, replacementVoucherId?, created`

### Commands

- `ClearCheck: {id, clearedAt, rowVersion}`
- `BounceCheck: {id, bouncedAt, reason?, rowVersion}`
- `ReplaceCheck: {checkId, paymentMethod, checkDetails?: CreateCheckDto, rowVersion}`

### Read DTOs

- `CheckClearingDto: checkNumber, bankName, clearedAt, amount`
- `CheckDetailDto: checkId, bankName, checkNumber, checkDate, amount, status, clearedAt?`

### API

- `POST /api/Revenue/Checks/{id}/clear` | `bounce` | `replace`
- `GET /api/Revenue/DepositSlips/monthly-statement` (clearings tab — TRE-02 endpoint)

### Status / Lifecycle

`UnderCollection → Cleared` · `UnderCollection → Bounced → (replace → new linked voucher)`

### Permissions

Revenue codes are currently absent — added per contract: `Checks.View` / `Checks.Clear` / `Checks.Bounce` / `Checks.Replace`.

## Clarifications

### Session 2026-09-07

- Q: What time period does the checks list (US1) cover — how does the user pick "the period"? → A: User-picked date range (from/to), defaulting to the current month; the range filters checks by their source voucher date (FR-001).
- Q: When a check bounces, what exactly happens to the source voucher to make it "replaceable"? → A: The voucher stays Approved but is locked for replacement only (a pending-replacement link via the bounced check) until the replacement completes; no other flow may consume it meanwhile (FR-003).
- Q: What details of the replacement voucher does the user provide vs. the system auto-creating? → A: The user supplies the voucher date; the amount is fixed equal to the original check; for a check replacement the user supplies the check details (bank / number / date / amount per CreateCheckDto) (FR-004).
- Q: What validation applies to clearedAt and bouncedAt dates? → A: The date must be on or after the check date and no later than today — back-dating to the check date is allowed, future dates rejected (FR-002, FR-003).

## Assumptions

- A single-check detail endpoint `GET /api/Revenue/Checks/{id}` returning `CheckDetailDto` is added (answers OQ1) — the "no standalone endpoint" constraint (FR-001) applies to the list, which is built from vouchers, not to a single-detail read.
- Cash replacement requires no additional fields beyond the method (answers OQ2) — the replacement cash voucher is a standard receipt voucher linked via `replacementVoucherId`; no deposit reference is demanded.
- Only UnderCollection checks may be cleared, bounced, or replaced; a check whose source voucher is cancelled cannot transition (Edge Cases; enforced server-side per FR-006).
- The under-collection list is filtered by period and status; the cleared check's exclusion from the under-collection filter is a consequence of FR-001 + FR-006.
- Replacement vouchers follow TRE-01's voucher creation rules (numbering, permissions, posting) — this feature only orchestrates and links them; the replacement amount is fixed equal to the original check (partial replacement is out of scope).

## Out of Scope

- Receipt voucher creation itself (TRE-01).
- Deposit slips and the statement's summary/vouchers tabs (TRE-02) — this feature only feeds the clearings tab.
- Bank-side acceptance or confirmation workflows.
