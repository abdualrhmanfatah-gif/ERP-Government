# Feature Specification: Recurring Entries (ACC-04)

**Feature Branch**: `029-recurring-entries`

**Created**: 2026-09-07

**Status**: Draft

**Input**: User description: "SPEC: ACC-04 — القيود الدورية (recurring-entries)"

## User Scenarios & Testing

### User Story 1 — Create Recurring Entry Schedule (Priority: P1)

As an accountant, I want to create a recurring journal entry schedule so that the system automatically generates entries on a defined frequency, eliminating manual monthly entry creation.

**Why this priority**: Core value — eliminates repetitive manual work and ensures entries are never missed.

**Independent Test**: Create a schedule from a template with Monthly frequency → appears Active with calculated nextExecutionDate.

**Acceptance Scenarios**:

1. **Given** an existing journal template, **When** I create a schedule (journalId, name, frequency, startDate, amount), **Then** it is saved as Active with nextExecutionDate computed based on frequency and startDate.
2. **Given** a missing required field in the form, **When** I submit the form, **Then** a validation error appears on the required field.
3. **Given** a schedule with endDate before startDate, **When** I submit, **Then** a validation error rejects the submission.

---

### User Story 2 — Control Recurring Entry Schedule (Priority: P1)

As an accountant, I want to pause, resume, or cancel a recurring entry schedule so that I can respond to business changes while preserving an intentional audit trail.

**Why this priority**: Control actions are critical for operational flexibility — without them, schedules cannot adapt to changing conditions.

**Independent Test**: Pause a schedule with reason → status becomes Paused and no generation occurs; Resume → Active with preserved nextExecution; Cancel → permanent.

**Acceptance Scenarios**:

1. **Given** an Active schedule, **When** I pause it with a reason, **Then** status becomes Paused and no entries are generated while paused.
2. **Given** a Paused schedule, **When** I resume it, **Then** status returns to Active with the previously saved nextExecutionDate.
3. **Given** a cancelled schedule, **When** I look for the Resume button, **Then** the Resume button is hidden.
4. **Given** a Paused schedule, **When** I attempt to pause it again, **Then** the action is rejected.
5. **Given** an Active schedule, **When** I attempt to resume it, **Then** the action is rejected.

---

### User Story 3 — Monitor Recurring Entry Schedule (Priority: P2)

As an accountant, I want to view schedule details including the last generated entry and next execution date so that I can verify the system is producing entries correctly.

**Why this priority**: Monitoring provides confidence that automated generation is working — secondary to creation and control but essential for trust.

**Independent Test**: Open schedule details → see lastExecutedAt and a link to the generated journal entry.

**Acceptance Scenarios**:

1. **Given** a schedule that has generated a journal entry, **When** I open its details, **Then** a link to generatedJournalEntryId opens the journal entry screen (023).
2. **Given** a schedule with no generation yet, **When** I open its details, **Then** an explicit empty state is displayed indicating no entries have been generated.

---

### Edge Cases

- Pause on a Paused schedule → reject the action.
- Resume on an Active schedule → reject the action.
- End date before start date → reject with validation error.
- Schedule execution falls in a closed fiscal period → server-side rejection.
- Schedule reaches its endDate → transition to Completed automatically.
- Cancel on a Completed or Cancelled schedule → reject (schedule already terminal).
- Schedule with no template and no amount → reject creation.

## Requirements

### Functional Requirements

- **FR-001**: System MUST manage recurring entry schedules (list, create, view details) — no update; a new schedule replaces an old one (recreate pattern).
- **FR-002**: System MUST display nextExecutionDate at all times on the schedule list and details.
- **FR-003**: System MUST provide pause (with reason), resume, and cancel (with reason) actions, with buttons conditionally enabled based on current status.
- **FR-004**: System MUST link the last generated journal entry to the schedule details view.
- **FR-005**: System MUST transition a schedule to Completed when its endDate is reached.
- **FR-006**: System MUST generate the nextExecutionDate based on the schedule frequency (Weekly, Monthly, Quarterly, Yearly) from the startDate.
- **FR-007**: System MUST persist pause reason and cancel reason as part of the audit trail.
- **FR-008**: System MUST assign a unique entry number to each recurring entry schedule using the document sequence service.
- **FR-009**: System MUST validate that endDate is not before startDate.
- **FR-010**: System MUST reject schedule execution in a closed fiscal period server-side.
- **FR-011**: System MUST support template-based schedule creation (templateId optional) — no manual line items.
- **FR-012**: System MUST persist the full set of dimensional fields: currency, fund, cost center, project (all optional per schedule).
- **FR-013**: System MUST reject duplicate control actions (pause on Paused, resume on Active, pause/resume/cancel on Cancelled or Completed).
- **FR-014**: System MUST record the lastExecutedAt timestamp when a schedule generates an entry.
- **FR-015**: System MUST display an explicit empty state when no entries have been generated yet.
- **FR-016**: System MUST transition a schedule to Cancelled when a cancel action is performed; Cancelled is a terminal state.
- **FR-017**: System MUST reject schedule creation if neither the schedule's amount nor the referenced template's amount is provided.

### Key Entities

- **RecurringEntry**: A schedule defining when and how journal entries are automatically generated from a template. Carries dimensional attributes (fund, cost center, project, currency), a frequency, date range, and status. Tracks the last generated entry and next execution date.
  - Statuses: Active, Paused, Completed, Cancelled.
  - Frequency enum (RecurringFrequency): Weekly, Monthly, Quarterly, Yearly.
  - Status enum (RecurringEntryStatus): Active, Paused, Completed, Cancelled.
  - Key relationships: `journalId` references the target journal definition (account structure for the generated entry); `templateId` optionally references a JournalTemplate that provides line details and amounts. Both are independent — journalId is always required, templateId is optional.

## Success Criteria

### Measurable Outcomes

- **SC-001**: Users can create a recurring entry schedule for any of the four frequencies (Weekly, Monthly, Quarterly, Yearly) in under 1 minute.
- **SC-002**: 100% of control actions (pause, resume, cancel) are blocked when attempted against an invalid status, including Cancelled and Completed.
- **SC-003**: Every pause and cancel action is recorded with its reason in the audit trail.
- **SC-004**: Schedule details always display the nextExecutionDate, regardless of generation history.
- **SC-005**: 100% of schedules created without an amount (from either schedule or template) are rejected at creation time.

## Assumptions

- Journal templates (ACC-028) exist and can be referenced by templateId; schedule creation uses template data as the source for generated entries.
- The background job engine (SYS-03) will consume RecurringEntry records and produce JournalEntry instances — this spec covers the schedule CRUD and lifecycle, not the generation engine.
- The system has an active fiscal period calendar; schedule execution checks fiscal period openness server-side.
- Document numbering follows the existing DocumentSequenceService pattern (PREFIX-D6).
- Cancel results in a terminal state with status `Cancelled` (4th enum value added to RecurringEntryStatus).
- "No amount on template" edge case: when both schedule amount and template amount are absent, creation is rejected (FR-017).

## Open Questions

- **OQ1**: What is the exact status value after cancel? Options: (A) a new enum value `Cancelled` in RecurringEntryStatus, (B) set `isActive = false` with status remaining at its last value, (C) status becomes `Completed` with a cancelReason field. Recommendation: add `Cancelled` as a fourth enum value for clarity.
- **OQ2**: Does the background job engine (SYS-03) need to be implemented as part of this spec, or is it a dependency tracked separately? Recommendation: out of scope per Out of Scope section — track separately.
- **OQ3**: When both schedule.amount and template.amount are null, how is the generated entry's amount determined? Recommendation: require amount at schedule level; reject creation if neither schedule nor template provides one.

## Clarifications

### Session 2026-09-07

- Q: What status value should a recurring entry have after being cancelled? → A: Add `Cancelled` as a 4th value in RecurringEntryStatus enum.
- Q: When both the schedule's amount and its referenced template's amount are null, how should the system handle creation? → A: Reject creation — require amount at schedule level if template has none.
- Q: What does the `journalId` field on the schedule represent, given `templateId` is optional? → A: `journalId` = target journal definition (account structure); `templateId` = line template source. Both are independent references.
