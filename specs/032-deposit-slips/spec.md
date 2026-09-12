# Feature Specification: Deposit Slips (TRE-02)

**Feature Branch**: `tre02-deposit-slips`

**Created**: 2026-09-07

**Status**: Draft

**Input**: User description: "SPEC: TRE-02 — بطاقات الإيداع (deposit-slips)"

## Binding Contract Note

The Data Contract section below is **literal and binding** for implementation — field names, DTO shapes, API routes, lifecycle, enums, and permission codes MUST be implemented exactly as written here. Deviations require a spec amendment, not silent divergence.

## User Scenarios & Testing

### User Story 1 — Create Deposit Slip with Members (Priority: P1)

As a cashier, I want to gather a day's approved receipt vouchers into one homogeneous deposit slip (Form 47 cash / Form 48 checks) so that the day's collection batch is closed and ready to hand to the treasury.

**Why this priority**: The slip is the gateway that closes the collection batch and triggers every downstream effect (revenue posting, check transition). Without it, collections remain loose vouchers.

**Independent Test**: Create a Form47 slip with two approved cash vouchers → the slip exists as Draft with a DSL number and a server-computed total; approved check vouchers are not offered by the voucher picker.

**Acceptance Scenarios**:

1. **Given** approved cash vouchers, **When** I create a Form47 slip passing their IDs as a batch, **Then** the slip is saved as Draft with a sequential DSL number and the members are visible on it with a server-computed total.
2. **Given** an approved check voucher exists, **When** I start a Form47 slip and open the voucher picker, **Then** the check voucher is not offered (eligibility pre-filtered by form type).
3. **Given** a voucher of the wrong method (a check voucher for a Form47 slip), **When** the creation or an add-member request reaches the server, **Then** it is rejected server-side with an explicit homogeneity error.

---

### User Story 2 — Manage Slip Members (Priority: P1)

As a cashier, I want to add or remove member vouchers from a Draft slip (removal with a reason) so that the batch is corrected before approval.

**Why this priority**: Correction is only legitimate before approval; after approval the batch is closed and financially final.

**Independent Test**: Remove a member from a Draft slip with a reason → succeeds and the total updates; attempt the same on an Approved slip → denied server-side.

**Acceptance Scenarios**:

1. **Given** a Draft slip, **When** I remove a member with a reason, **Then** the voucher is detached and the slip total is recomputed server-side.
2. **Given** an Approved slip, **When** I attempt to add or remove a member (or the UI exposes the controls), **Then** the action is denied server-side and the add/remove buttons are hidden.

---

### User Story 3 — Approval and Its Ledger Effect (Priority: P1)

As a manager, I want to approve a deposit slip (with confirmation and an optional reason) so that cash revenue is recognized in the ledger and checks move to "under collection".

**Why this priority**: Approval is the point of financial recognition — Form47 triggers the revenue posting, Form48 transitions its member checks to UnderCollection. This is the control point of the whole feature.

**Independent Test**: Approve a Form47 slip → an accounting posting effect is visible; approve a Form48 slip → its member checks move to UnderCollection; approve an empty slip → denied.

**Acceptance Scenarios**:

1. **Given** a Form47 slip with members, **When** the manager approves it (confirmation + optional reason), **Then** the slip becomes Approved (approver and time recorded) and the revenue posting effect is produced and visible.
2. **Given** a Form48 slip with check members, **When** it is approved, **Then** every member check transitions to UnderCollection.
3. **Given** a slip with no members, **When** approval is attempted, **Then** it is denied server-side.

---

### User Story 4 — Monthly Statement (Priority: P2)

As a treasury officer, I want a per-fund monthly statement so that I can reconcile the month's collections with the central treasury at month-end.

**Why this priority**: Month-end reconciliation is periodic, not blocking the daily flow; it consumes already-approved data.

**Independent Test**: Select a month + fund → a summary of six figures plus the month's vouchers and check clearings tables is displayed; a month with no activity shows explicit zeros, not an empty screen.

**Acceptance Scenarios**:

1. **Given** a month with activity in a fund, **When** I open the statement, **Then** I see the six-figure summary (cash collections / check collections / deposited / under collection / cleared / bounced) plus the member voucher and clearing tables.
2. **Given** a month with no activity for the matched fund, **When** I open the statement, **Then** explicit zeros and empty tables are shown — never a blank screen or an error.

---

### Edge Cases

- Slip date earlier than the latest member voucher date → rejected.
- Slip date in the future → rejected.
- A member voucher is cancelled after joining the slip → the server-side guard blocks approval and flags the invalid membership (an Approved slip can never contain a cancelled voucher).
- Concurrent edit (optimistic-concurrency token mismatch) → operation rejected with a distinct concurrency error; user reloads and retries.
- Monthly statement requested for a fund with no matching activity → explicit zeros, not an error.
- Removing a voucher from a slip where the removal would leave the slip empty → allowed while Draft (empty slips are simply not approvable).

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The system MUST enforce homogeneity: Form47 members are Cash vouchers exclusively, Form48 members are Check vouchers exclusively — via pre-filtered eligibility in the voucher picker AND a server-side validation on every membership change.
- **FR-002**: The system MUST validate the slip date: at or after the latest member voucher date, and no later than today.
- **FR-003**: The system MUST allow member add/remove only while the slip is Draft; removal MUST require a reason; after approval both operations MUST be rejected server-side.
- **FR-013**: The system MUST allow a voucher removed from a Draft slip to rejoin the eligible pool and be added to a different Draft slip — a voucher holds at most one active slip membership at any time; assignment on an Approved slip is permanent.
- **FR-004**: The system MUST translate approval into the correct financial effect: Form47 → revenue posting into the ledger; Form48 → transition of every member check to UnderCollection.
- **FR-005**: The system MUST deny approval of a slip with no members.
- **FR-006**: The system MUST present the monthly statement per fund, with the six-figure summary and the month's voucher and clearing detail.
- **FR-007**: The system MUST compute slip totals server-side only (no client-trusted totals).
- **FR-008**: The system MUST verify the concurrency token on every membership change and approval, and MUST surface token conflicts as a distinct concurrency failure.
- **FR-009**: The system MUST assign the DSL slip number at Draft creation (server-allocated, same transaction).
- **FR-010**: The system MUST record the approval decision (approver, time, optional reason) in the append-only approval/status history — never as inline-only columns.
- **FR-011**: The system MUST guarantee a Draft→Approved terminal lifecycle: no cancellation, no deletion, no reopening of an approved slip.
- **FR-012**: The system MUST enforce every operation behind a declared permission (View / Create / Approve; member add/remove under Update).
- **FR-014**: The system MUST NOT restrict self-approval: a slip's creator may approve it, provided the actor holds the Approve permission; the approval history records the actual actor in every case.
- **FR-015**: The system MUST allow creating a slip with an initially empty member set (the member list is populated while Draft); an empty Draft slip is valid but never approvable (FR-005).
- **FR-016**: The statement's `vouchers[]` MUST list ALL approved receipt vouchers of the matched fund for the month (full activity, not only slip members); the summary figures are computed over that activity so deposited vs. un-deposited vouchers are reconcilable within one view.

### Key Entities *(include if feature involves data)*

- **DepositSlip**: A homogeneous deposit slip (Form47 cash / Form48 checks) holding member receipt vouchers, a slip date, a server-computed total, a sequential DSL number, and a Draft→Approved terminal lifecycle.
- **ReceiptVoucher (member)**: An approved voucher (TRE-01) joined to at most one slip at a time; a voucher removed from a Draft slip returns to the eligible pool and may join another Draft slip, but membership on an Approved slip is permanent; its payment method determines which slip form may accept it.
- **Check (via Form48)**: A member check of a Form48 slip that transitions to UnderCollection when the slip is approved; later clearing/bounce is out of scope (TRE-03).
- **MonthlyStatement**: A read-only per-fund, per-month view: six-figure summary plus the month's vouchers (all approved vouchers of the fund — full activity) and check clearings.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: A full day's vouchers can be gathered into one deposit slip in under 5 minutes.
- **SC-002**: Zero mixed-method slips exist in the data (homogeneity is provable by inspection at any time).
- **SC-003**: Every Form47 approval is paired with a visible, traceable posting effect in the ledger.
- **SC-004**: A month-end reconciliation using the statement completes without manual tallying outside the system.

## Data Contract *(binding — literal)*

### DepositSlipDto

`id, slipNumber, slipDate*, formType* (enum FormType: Form47/Form48), formTypeName, status (enum DepositSlipStatus: Draft/Approved), statusName, totalAmount, approvedById?, approvedAt?, receiptVouchers[], rowVersion, created, createdBy, lastModified, lastModifiedBy`

### Commands

- `CreateDepositSlipCommand: {slipDate, formType, voucherIds[]}`
- `AddVoucherToSlip: {slipId, voucherId, rowVersion}`
- `RemoveVoucherFromSlip: {slipId, voucherId, reason?, rowVersion}`
- `ApproveDepositSlip: {id, reason?, rowVersion}`

### MonthlyStatementDto

`year, month, fundId, fundName, generatedAt, summary {totalCashCollections, totalCheckCollections, totalDeposited, totalUnderCollection, totalCleared, totalBounced}, vouchers[], clearings[]`

### API

- `GET /api/Revenue/DepositSlips` · `POST /api/Revenue/DepositSlips` · `GET /api/Revenue/DepositSlips/{id}`
- `GET /api/Revenue/DepositSlips/monthly-statement`
- `POST /api/Revenue/DepositSlips/{id}/add-voucher` | `remove-voucher` | `approve`

### Status / Lifecycle

`Draft → Approved` (terminal — no cancel, no delete)

### Permissions

Revenue codes are currently absent — added per contract: `DepositSlips.View` / `DepositSlips.Create` / `DepositSlips.Approve`; member add/remove under `DepositSlips.Update`.

## Assumptions

- Removal after approval is rejected server-side explicitly (per FR-003; answers OQ1 — the Draft-only rule is enforced at the use-case layer, not only hidden in the UI).
- A slip is not fund-scoped; the slip operates on the single cash box of the cashier. Fund granularity appears only in the monthly statement query (answers OQ2 — `fundId` exists on the statement, not on the slip).
- Member vouchers must already be Approved (TRE-01) to be eligible for a slip; Draft or Cancelled vouchers are never eligible; a voucher already member of another active slip is excluded from the picker.
- Eligibility is not date-restricted: the picker offers any Approved, unassigned voucher of the matching payment method regardless of its voucher date; the slip date must simply be at or after the latest member voucher date (FR-002).

## Clarifications

### Session 2026-09-07

- Q: When a voucher is removed from a Draft slip, may it later be added to a different slip, or is it permanently bound to its first slip? → A: A removed voucher returns to the eligible pool and may join a different Draft slip (one active membership at a time); membership on an Approved slip is permanent (FR-013).
- Q: Must the person approving a slip differ from its creator, or may the same person create and approve? → A: No restriction — the creator may approve their own slip; only the Approve permission is required, and the approval history records the actual actor (FR-014).
- Q: What period may a slip's member vouchers cover — the same day only, or any unassigned approved vouchers regardless of their dates? → A: Any Approved, unassigned voucher of the matching payment method regardless of voucher date; the slip date only has to be at or after the latest member voucher date (FR-002).
- Q: May a slip be created empty (no vouchers) and have members added afterwards, or must every new slip contain at least one voucher at creation? → A: Empty creation allowed — members are populated while Draft; an empty Draft slip is valid but never approvable (FR-015).
- Q: What should the monthly statement's vouchers[] table list — all approved receipt vouchers of the fund in the month, or only vouchers attached to approved slips? → A: All approved vouchers of the matched fund for the month (full activity); summary figures computed over that activity so deposited vs. un-deposited amounts reconcile in one view (FR-016).
- All six statement figures are computed over approved slips and their voucher/check state for the selected month and fund.
- An approved Form47 slip's revenue posting follows the established ledger-posting pipeline (domain event → accounting event → posting rules), not a direct ledger write.

## Out of Scope

- Receipt vouchers themselves (TRE-01).
- Check clearing / bounce handling (TRE-03) — the statement's cleared/bounced figures read existing check status only.
- Bank deposit confirmation or treasury-side acceptance workflow.
