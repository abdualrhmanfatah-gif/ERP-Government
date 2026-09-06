# Feature Specification: Journal Entry Lifecycle Screens

**Feature Branch**: `025-journal-entry-lifecycle`

**Created**: 2026-09-06

**Status**: Draft

**Input**: User description: "Journal entry lifecycle screens for the general ledger. The backend enforces a strict six-state machine (Draft → Submitted → Approved → Posted → Reversed / Cancelled) with fiscal-period guards and reversals. This feature is the accountant's complete working surface for that lifecycle."

## User Scenarios & Testing *(mandatory)*

### User Story 1 — Create and Edit a Draft (Priority: P1)

An accountant opens the journal entry create screen, selects an open fiscal period, and enters header details (document date within the period, fiscal year, journal, entry type, narration, reference). The entry is saved as Draft. The accountant then adds, edits, and removes lines — each line carries account, sequence, description, currency with exchange rate, and a debit-or-credit amount. Optional analytic dimensions (cost center, fund, project, budget item, encumbrance, payment order) may be attached. Running debit and credit totals update live; unbalanced totals are flagged immediately. A draft with zero lines cannot be submitted. System-generated entries (born from automatic postings) open as read-only with a system badge and no manual lifecycle actions.

**Why this priority**: Without the ability to create and compose journal entries, no accounting workflow can begin. This is the foundation of the entire ledger lifecycle.

**Independent Test**: Can be fully tested by creating a draft entry, adding lines, verifying live totals, and confirming the draft persists. Delivers a working entry composition surface.

**Acceptance Scenarios**:

1. **Given** an open fiscal period, **When** an accountant creates a journal entry with document date within the period, fiscal year, journal, entry type, narration, and reference, **Then** it is saved as Draft with at least one line.
2. **Given** a draft, **When** lines are added, edited, or removed (account, sequence, description, currency + exchange rate, debit XOR credit, and optional analytic dimensions), **Then** the running debit/credit totals update live and unbalanced totals are flagged.
3. **Given** a draft with zero lines, **When** submit is attempted, **Then** it is rejected with "must have at least one line".
4. **Given** a system-generated entry (born from an automatic posting), **When** opened, **Then** it is read-only with a system badge and no manual lifecycle actions.

---

### User Story 2 — Submit for Approval (Priority: P1)

An accountant reviews a balanced Draft and submits it. The entry transitions to Submitted and awaits approval. Submitting an unbalanced or empty-lines draft is blocked with the specific reason displayed.

**Why this priority**: Submission is the gatekeeper to the approval pipeline. Without it, entries cannot progress through the lifecycle.

**Independent Test**: Can be tested by creating a balanced draft and submitting it, then verifying it transitions to Submitted. Also test that unbalanced drafts are rejected on submit.

**Acceptance Scenarios**:

1. **Given** a balanced Draft, **When** submitted, **Then** it becomes Submitted and awaits approval.
2. **Given** an unbalanced or empty-lines draft, **When** submitted, **Then** the attempt is blocked with the specific reason.

---

### User Story 3 — Approve (Priority: P1)

An approver reviews a Submitted entry and approves it. The entry transitions to Approved and becomes eligible for posting.

**Why this priority**: Approval is the authorization gate before financial commitment (posting). Required for the full lifecycle.

**Independent Test**: Can be tested by submitting an entry, then approving it as an approver, and verifying it transitions to Approved.

**Acceptance Scenarios**:

1. **Given** a Submitted entry, **When** an approver approves it, **Then** it becomes Approved and eligible for posting.

---

### User Story 4 — Post with Fiscal-Period Guards (Priority: P1)

An accountant posts an Approved entry. The entry transitions to Posted; posting date and poster are recorded, and balances are updated. If the fiscal period is locked, posting is rejected with "fiscal period is locked for posting". If the document date falls outside the period range, posting is rejected with the range message.

**Why this priority**: Posting is the financial commitment point. Fiscal-period guards enforce accounting integrity. Together with create/submit/approve, this completes the forward lifecycle.

**Independent Test**: Can be tested by posting an approved entry and verifying it transitions to Posted. Also test rejection when the period is locked or the date is out of range.

**Acceptance Scenarios**:

1. **Given** an Approved entry, **When** posted, **Then** it becomes Posted, posting date and poster are recorded, and balances are updated.
2. **Given** the entry's fiscal period is locked, **When** posting is attempted, **Then** it is rejected with "fiscal period is locked for posting".
3. **Given** the document date falls outside the period range, **When** posting is attempted, **Then** it is rejected with the range message.

---

### User Story 5 — Reverse a Posted Entry (Priority: P1)

An accountant reverses a Posted entry with a mandatory reason. A counter-entry is created (all lines mirrored debit↔credit), linked to the original. The original becomes Reversed; the counter-entry itself is Posted. If the original period is locked or finalized, reversal is rejected with the fiscal control reason. A Reversed entry cannot be reversed again — reversals are never reversed. When any entry is viewed, reversal linkage shows both directions: the original displays its reversal (with reason), the reversal displays its origin.

**Why this priority**: Reversal is the only legal correction mechanism for posted entries (Principle IV). Required for financial integrity.

**Independent Test**: Can be tested by posting an entry, reversing it with a reason, and verifying the counter-entry is Posted and linked. Also test double-reversal rejection and locked-period rejection.

**Acceptance Scenarios**:

1. **Given** a Posted entry, **When** an accountant reverses it with a mandatory reason, **Then** a counter-entry is created (all lines mirrored debit↔credit), linked to the original, the original becomes Reversed, and the counter-entry itself is Posted.
2. **Given** the original period is locked or the period is finalized (balances closed), **When** reversal is attempted, **Then** it is rejected with the fiscal control reason.
3. **Given** a Reversed entry, **When** reversal is attempted again, **Then** it is rejected — reversals are never reversed.
4. **Given** any entry, **When** viewed, **Then** reversal linkage shows both directions: the original displays its reversal (with reason), the reversal displays its origin.

---

### User Story 6 — Cancel Before Commitment (Priority: P2)

An accountant cancels a Draft or Submitted entry with confirmation. The entry transitions to Cancelled with cancelled-by/at recorded. Cancel is not offered for Approved or Posted entries — approved work must be reversed after posting, not cancelled.

**Why this priority**: Cancellation is a convenience for early-stage entries. Not critical to the core lifecycle but improves usability.

**Independent Test**: Can be tested by creating a draft, cancelling it, and verifying it transitions to Cancelled. Also test that cancel is not offered for Approved or Posted entries.

**Acceptance Scenarios**:

1. **Given** a Draft or Submitted entry, **When** cancelled with confirmation, **Then** it becomes Cancelled with cancelled-by/at recorded.
2. **Given** an Approved or Posted entry, **When** cancel is attempted, **Then** it is not offered (approved work must be reversed after posting, not cancelled).

---

### User Story 7 — Browse and Audit (Priority: P2)

An accountant browses the journal entry list with filtering by status (all six), fiscal year, period, journal, and fund. Searching by entry number works. When any entry screen is open, the approvals panel and the append-only status log render, plus posted/cancelled identity and timestamps when present.

**Why this priority**: Browsing and audit are essential for daily operations but are read-only views that don't block other work.

**Independent Test**: Can be tested by navigating the list with various filters, opening an entry detail, and verifying the approvals panel and status log render.

**Acceptance Scenarios**:

1. **Given** the ledger, **When** an accountant browses entries, **Then** filtering by status (all six), fiscal year, period, journal, and fund works, and searching by entry number works.
2. **Given** any entry, **When** its screen is open, **Then** the approvals panel and the append-only status log render, plus posted/cancelled identity and timestamps when present.

---

### Edge Cases

- What happens when a concurrent edit conflict (stale RowVersion) occurs on any action? The server rejects and the screen must surface the conflict and offer refetch.
- What happens during a lock-the-period race between approve and post? The server enforces the period state at transaction time; the client receives the rejection.
- What happens when reversal is attempted on a period whose year is closed? Rejected with the fiscal control reason.
- What happens during a submit → approve race by two users? The second user sees the current state and acts accordingly; the server serializes transitions.
- What happens when an entry whose source is an accounting event (disbursement/treasury posting) is opened? Show source link.
- What happens when lines are edited on a non-Draft entry? Blocked client-side and server-side.
- What happens when debit AND credit are both zero? Invalid line — blocked.
- What happens when debit AND credit are both non-zero? Violates XOR rule — blocked with message.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST render only lifecycle actions valid for the current state (Draft: edit lines/submit/cancel; Submitted: approve/cancel; Approved: post; Posted: reverse; Reversed/Cancelled: none).
- **FR-002**: System MUST show live balance totals before submit.
- **FR-003**: System MUST require a reversal reason when reversing a posted entry.
- **FR-004**: System MUST surface every server rejection verbatim.
- **FR-005**: System MUST display entry number at creation.
- **FR-006**: System MUST gate every action by its permission code.
- **FR-007**: System MUST treat system-generated entries as read-only.
- **FR-008**: System MUST block line editing on non-Draft entries client-side.
- **FR-009**: System MUST enforce debit XOR credit on every line (exactly one of debit or credit must be non-zero).
- **FR-010**: System MUST flag unbalanced totals (total debits ≠ total credits) and prevent submission of unbalanced entries.
- **FR-011**: System MUST require at least one line before submission.
- **FR-012**: System MUST display the six-state status badge (Draft, Submitted, Approved, Posted, Reversed, Cancelled) with Arabic labels.
- **FR-013**: System MUST show reversal linkage bidirectionally: the original displays its reversal with reason, the reversal displays its origin.
- **FR-014**: System MUST record and display posted-by/posted-at and cancelled-by/cancelled-at timestamps.
- **FR-015**: System MUST render the approvals panel and append-only status log on the detail screen.
- **FR-016**: System MUST support filtering by status, fiscal year, period, journal, fund, and search by entry number.
- **FR-017**: System MUST surface concurrent edit conflicts (stale RowVersion) with a refetch option.
- **FR-018**: System MUST display a source link for entries originating from accounting events (disbursement/treasury).
- **FR-019**: System MUST display a system badge on system-generated entries.
- **FR-020**: System MUST support entry type selection (e.g., standard, adjusting, closing).
- **FR-021**: System MUST support optional analytic dimensions on lines: cost center, fund, project, budget item, encumbrance, payment order.

### Key Entities

- **Journal Entry Header**: The top-level record — entry number, document date, fiscal year, journal, entry type, narration, reference, status, posting date, poster identity, cancelled-by/at, reversal linkage (reversedById, reversedByNumber, reversalOfId, reversalOfNumber), system-generated flag, RowVersion.
- **Journal Entry Line**: Individual debit or credit record — account, sequence, description, currency, exchange rate, debit amount, credit amount, analytic dimensions (cost center, fund, project, budget item, encumbrance, payment order).
- **Fiscal Period**: Time-bounded accounting period — period number, start/end date, locked-for-posting flag, fiscal year reference.
- **Approval History**: Append-only record of approval decisions — actor, decision, timestamp, reason, rule evaluation snapshot.
- **Document Status Log**: Append-only record of every status transition — from-status, to-status, actor, timestamp.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: An accountant can run a journal entry from creation to posting and its reversal entirely from the screen.
- **SC-002**: No invalid state transition is ever clickable — lifecycle buttons are rendered only when the action is valid for the current state.
- **SC-003**: Every fiscal-period rejection displays a clear, specific explanation (locked period, date out of range).
- **SC-004**: Live balance totals are visible during draft composition and update within 500ms of any line change.
- **SC-005**: All server rejections are surfaced verbatim to the user without generic error messages.
- **SC-006**: System-generated entries are visually distinct (system badge) and completely read-only.
- **SC-007**: Reversal linkage is visible from both the original entry and the counter-entry.

## Assumptions

- The backend API for journal entry CRUD, lifecycle transitions, and fiscal-period validation is fully built and operational.
- The shared UI component library (StatusBadge, FilterBar, ConfirmDialog, PageHeader, etc.) is available.
- The `usePermission` hook is available for gating actions by permission codes.
- Arabic-first RTL layout is required throughout.
- Dark mode support is required via design tokens.
- No new backend entities are created — this feature is UI over existing backend.
- Existing accounting page patterns (JournalEntriesListPage, JournalEntryCreatePage, JournalEntryDetailPage) serve as exemplars for the new implementation.
- Fiscal period and fiscal year data are fetched from existing endpoints.
- The entry number is displayed at creation (generated server-side).
