# Feature Specification: ACC-03 — Journals & Templates Management

**Feature Branch**: `acc03-journals-templates`

**Created**: 2026-09-07

**Status**: Draft

**Input**: 16-element spec — Journals and Journal Entry Templates administration for the accounting module. Backend-driven data contract (fields, endpoints, enums, permissions) from web-api-client.ts — not to be re-derived.

## User Scenarios & Testing *(mandatory)*

### User Story 1 — Manage Journals (P1)

An accounting admin browses the journal list filtered by type. Columns show: code, name, type, sequence, approval-required flag, and active status. The admin creates a new journal with all contract fields (code, name, type, accountId, suspenseAccountId, allowForeignCurrency, sequenceId, requireApprovalBeforePosting). Duplicate code is rejected with a clear server message. Editing a journal that is referenced by existing journal entries is blocked on the server with the reason displayed.

**Why this priority**: Without journal definitions, entries cannot be classified and approval routing cannot be enforced per journal. This is the foundation of the accounting entry system.

**Independent Test**: Create a journal of each type (7 total), verify it appears in the list and in the journal picker used for entry creation. Attempt duplicate code — verify rejection.

**Acceptance Scenarios**:

1. **Given** journals exist, **When** the admin browses the list with a type filter, **Then** columns display: code, name, type, sequence, approval-required, and active status.
2. **Given** a new journal, **When** created with a duplicate code, **Then** the server rejects it with a clear message.
3. **Given** a journal referenced by existing journal entries, **When** the admin attempts to edit fields other than isActive and requireApprovalBeforePosting, **Then** the server blocks the change with the reason displayed (code, name, type, accountId, suspenseAccountId, allowForeignCurrency, sequenceId are locked).
4. **Given** a journal with a suspense account set, **When** viewed, **Then** the suspense account is displayed correctly.

---

### User Story 2 — Manage Templates (P2)

An accounting admin creates journal entry templates (header-only: templateName, description, journalId, templateType). Templates classify generated entries (standard/recurring/adjustment) by journal — serving as the source for ACC-04 periodic scheduling. System templates (isSystemTemplate=true) cannot be deleted (no delete endpoint; delete action is hidden in the UI). Non-system templates have no edit or delete restrictions. The isSystemTemplate flag is not shown in the UI — it defaults to false on create and can only be set by the server.

**Why this priority**: Templates are the classification layer for generated entries and the prerequisite for ACC-04 periodic scheduling. Required after journals are in place.

**Independent Test**: Create a template linked to a journal with type Recurring, verify it appears in the list. Attempt to delete a system template — verify the action is not offered.

**Acceptance Scenarios**:

1. **Given** a new template, **When** created with templateName, journalId, and templateType, **Then** it is saved and appears in the list.
2. **Given** a system template (isSystemTemplate=true), **When** the admin attempts to delete it, **Then** the delete action is not offered (no endpoint; UI hides it).
3. **Given** a template linked to a journal, **When** viewed, **Then** the journal name is displayed alongside the template details.

---

### User Story 3 — Feed Entry Creation Pickers (P2)

Only active journals and active templates appear in the journal/template pickers used in the entry creation screen (spec 023). Disabled journals and templates are excluded from selection.

**Why this priority**: Pickers must reflect only operational records to prevent entries from being assigned to inactive journals.

**Independent Test**: Disable a journal, open the entry creation picker, verify the disabled journal does not appear. Same for templates.

**Acceptance Scenarios**:

1. **Given** a disabled journal, **When** the entry creation journal picker is opened, **Then** the disabled journal does not appear.
2. **Given** a disabled template, **When** the entry creation template picker is opened, **Then** the disabled template does not appear.

---

### User Story 4 — Manage Template Lines (P1)

An accounting admin views and maintains the lines of a journal entry template (accounts, debit/credit amounts, currency, cost center). Lines mirror `JournalEntryLine` except parent FK (`TemplateId` instead of `JournalEntryId`). Read pattern mirrors `JournalEntryDetailPage` lines table; edit pattern mirrors `JournalEntryCreatePage` lines editor with balance indicator. Debit/credit XOR enforced, totals must balance before save, sequence auto-incremented, only postable + active accounts selectable.

**Why this priority**: Templates without lines cannot generate entries (ACC-04 recurring depends on balanced lines). Header-only templates are inert.

**Independent Test**: Open template detail, add two lines (debit 1000 / credit 1000, distinct postable accounts), verify balanced totals and save succeeds. Attempt debit+credit on same line — verify rejection.

**Acceptance Scenarios**:

1. **Given** a template, **When** viewed, **Then** lines display: `# | الحساب code-name | مدين | دائن | البيان | عملة/سعر | مركز تكلفة` RTL with `BalanceIndicator`.
2. **Given** a new line, **When** both debit > 0 and credit > 0, **Then** server rejects with clear message.
3. **Given** a new line, **When** debit == 0 and credit == 0, **Then** server rejects.
4. **Given** lines with unbalanced totals, **When** saving template, **Then** client blocks save (`canSave = lines > 0 && balanced`).
5. **Given** account non-postable or inactive, **When** selected, **Then** server rejects.
6. **Given** lines added, **When** saved, **Then** sequence = max + 1 ordered by Sequence.

---

### Edge Cases

- Duplicate journal code → server rejection with clear message.
- Journal without sequenceId → behavior for auto-numbering is undefined (Open Question).
- Template without a linked journal → should be prevented on create.
- suspenseAccount requirement for Cash/Bank journal types → not enforced at spec level (Open Question).
- Concurrent edit conflict (stale RowVersion) → server rejects; client surfaces conflict.
- Template line with negative amounts → server rejects.
- Template line with missing account/currency → server rejects.
- Unbalanced template (totalDebit != totalCredit) → generation skipped at runtime (ACC-04), blocked at edit time in UI.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST manage journals (list, create, update) with all contract fields.
- **FR-002**: System MUST prevent deletion of journals (no DELETE endpoint; delete action hidden in UI).
- **FR-003**: System MUST manage templates (list, create, update) — header only: templateName, description, journalId, templateType.
- **FR-004**: System MUST display only active journals and templates in picker/selector components.
- **FR-005**: System MUST block edits to code, name, type, accountId, suspenseAccountId, allowForeignCurrency, and sequenceId on journals referenced by existing journal entries. isActive and requireApprovalBeforePosting remain editable. The server returns the reason for the block.
- **FR-006**: System MUST reject duplicate journal codes on create/update with a clear server message.
- **FR-007**: System MUST prevent deletion of system templates (isSystemTemplate=true) — delete action hidden in UI.
- **FR-008**: System MUST support toggling journal/template active status via update (no lifecycle state machine — isActive is a field).
- **FR-009**: System MUST manage template lines (list, create, update, delete) per template with fields: sequence (auto), accountId, description, currencyId, exchangeRate, debit, credit, costCenterId.
- **FR-010**: System MUST enforce line XOR: exactly one of debit / credit > 0, non-negative, account must exist + IsPostable + IsActive, currency required, exchangeRate > 0.
- **FR-011**: System MUST return lines ordered by Sequence with account code/name resolved and template totals (totalDebit/totalCredit/balanced) on detail.
- **FR-012**: System MUST block unbalanced template save in UI (`lines.length > 0 && balanced`); server MAY accept unbalanced draft but runtime generation MUST skip it.

### Key Entities

- **Journal**: Accounting book with classification and approval-routing properties. Fields: id, code (unique), name, type (enum: General/Purchase/Sale/Cash/Bank/Adjustment/Closing), accountId, suspenseAccountId, allowForeignCurrency, sequenceId, requireApprovalBeforePosting, isActive.
- **JournalEntryTemplate**: Header + lines template classifying generated entries by journal and type. Fields: id, templateName, description, journalId, journalName (read-only), templateType (enum: Standard/Recurring/Adjustment), isSystemTemplate, isActive, lines (read-only list, ordered by Sequence), totalDebit/totalCredit/balanced (computed, never stored).
- **JournalEntryTemplateLine**: Strict mirror of `JournalEntryLine` except parent FK. Fields: id, templateId (FK → JournalEntryTemplate, required), sequence (auto max+1), accountId (FK → Account, postable+active only), accountCode/accountName (read-only), description, currencyId (FK → Currency), exchangeRate (default 1), debit decimal(23,2), credit decimal(23,2), costCenterId? (FK → CostCenter, nullable).

### Data Contract *(binding — from web-api-client.ts)*

#### JournalDto

| Field | Type | Required | Notes |
|-------|------|----------|-------|
| `id` | int | Auto | Primary key |
| `code` | string | Yes | Unique |
| `name` | string | Yes | Display name |
| `type` | enum (int) | Yes | General=0, Purchase=1, Sale=2, Cash=3, Bank=4, Adjustment=5, Closing=6 |
| `accountId` | int? | No | Default account |
| `suspenseAccountId` | int? | No | Suspense account |
| `allowForeignCurrency` | bool | Yes | Foreign currency support |
| `sequenceId` | int? | No | Numbering sequence |
| `requireApprovalBeforePosting` | bool | Yes | Approval gate |
| `isActive` | bool | Yes | Active/inactive toggle |

#### CreateJournalCommand

Same as JournalDto minus id and isActive.

#### JournalEntryTemplateDto

| Field | Type | Required | Notes |
|-------|------|----------|-------|
| `id` | int | Auto | Primary key |
| `templateName` | string | Yes | Display name |
| `description` | string? | No | Optional description |
| `journalId` | int | Yes | Linked journal |
| `journalName` | string | Read-only | Resolved from journalId |
| `templateType` | enum (int) | Yes | Standard=0, Recurring=1, Adjustment=2 |
| `isSystemTemplate` | bool | Yes | System template flag |
| `isActive` | bool | Yes | Active/inactive toggle |

#### CreateTemplateCommand

| Field | Type | Required | Notes |
|-------|------|----------|-------|
| `templateName` | string | Yes | |
| `description` | string? | No | |
| `journalId` | int | Yes | |
| `templateType` | enum (int) | Yes | |
| `isSystemTemplate` | bool | Yes | |

### API Endpoints *(binding — from web-api-client.ts)*

| Method | Path | Action |
|--------|------|--------|
| GET | /api/Accounting/Journals | List journals |
| POST | /api/Accounting/Journals | Create journal |
| GET | /api/Accounting/Journals/{id} | Get journal by id |
| PUT | /api/Accounting/Journals/{id} | Update journal |
| GET | /api/Accounting/Templates | List templates |
| POST | /api/Accounting/Templates | Create template |
| GET | /api/Accounting/Templates/{id} | Get template by id |
| PUT | /api/Accounting/Templates/{id} | Update template |

No DELETE endpoints for either entity.

### Permissions *(binding — from web-api-client.ts)*

| Code | Action |
|------|--------|
| `Accounting.Journals.View` | List + detail |
| `Accounting.Journals.Create` | Create journal |
| `Accounting.Journals.Update` | Update journal |
| `Accounting.Templates.View` | List + detail |
| `Accounting.Templates.Create` | Create template |
| `Accounting.Templates.Update` | Update template |

### UI States Required

| Page | Loading | Empty | Error | Unauthorized | Not Found | Normal |
|------|---------|-------|-------|--------------|-----------|--------|
| Journal List | Skeleton | "لا توجد دفاتر" | Toast | Guard | N/A | DataGrid + type filter |
| Journal Create/Edit | N/A | N/A | Toast on error | Guard | N/A | Form with all fields |
| Template List | Skeleton | "لا توجد قوالب" | Toast | Guard | N/A | DataGrid + journal filter |
| Template Create/Edit | N/A | N/A | Toast on error | Guard | N/A | Form (header) + TemplateLinesTable + TemplateLineEditor + BalanceIndicator |
| Template Detail/Lines | Skeleton | "لا توجد أسطر بعد" (dashed) | Toast | Guard | "القالب غير موجود" | Read table `# | الحساب | مدين | دائن | الوصف | عملة/سعر | مركز تكلفة` RTL |

### Tests Expected

| ID | Page | Test |
|----|------|------|
| T-028-001 | Journal List | Type filter works across all 7 types |
| T-028-002 | Journal List | Columns render: code, name, type, sequence, approval, active |
| T-028-003 | Journal Create | Create with all contract fields succeeds |
| T-028-004 | Journal Create | Duplicate code rejected with message |
| T-028-005 | Journal Edit | Edit succeeds for unused journal |
| T-028-006 | Journal Edit | Edit blocked for used journal with reason |
| T-028-007 | Template List | Templates display with journal name |
| T-028-008 | Template Create | Create with templateName, journalId, templateType succeeds |
| T-028-009 | Template Edit | System template delete action hidden |
| T-028-010 | Pickers | Only active journals appear in entry picker |
| T-028-011 | Pickers | Only active templates appear in entry picker |
| T-028-012 | Shared | isActive toggle via update works |
| T-028-013 | Template Lines | Lines table renders `# | الحساب | مدين | دائن | الوصف | عملة/سعر | مركز تكلفة` + BalanceIndicator |
| T-028-014 | Template Lines | Add line with debit-only / credit-only succeeds, sequence auto |
| T-028-015 | Template Lines | debit+credit together rejected; zero/zero rejected; negative rejected |
| T-028-016 | Template Lines | Non-postable / inactive account rejected with message |
| T-028-017 | Template Lines | Save blocked when unbalanced or zero lines (`canSave`) |
| T-028-018 | Template Lines | Update + delete line succeed, totals recomputed |

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Creating a journal of any type (7 total) completes successfully.
- **SC-002**: 100% of active templates appear in the entry creation picker (spec 023 integration).
- **SC-003**: Zero successful edits to journals that are referenced by existing entries.
- **SC-004**: Duplicate journal code rejection returns a clear, user-readable message.
- **SC-005**: System template deletion is never possible through the UI.
- **SC-006**: 100% of saved templates with intent to generate have balanced lines (totalDebit == totalCredit, lines > 0).
- **SC-007**: Zero lines with debit+credit both > 0 or both == 0 persist.
- **SC-008**: Template detail renders lines table + balance state in < 2s.

## Assumptions

- The backend API for journal and template CRUD is fully built and operational (web-api-client.ts contract).
- The backend enforces: duplicate code validation, used-journal edit blocking, and isActive-only status updates.
- Journal deletion is prevented server-side (no DELETE endpoint) and hidden client-side.
- Template deletion for system templates is prevented server-side and hidden client-side.
- The entry creation pickers (spec 023) will consume the active-only filtered lists from these endpoints.
- Arabic-first RTL layout is required throughout.
- Dark mode support is required via design tokens.
- The data contract (field names, endpoint paths, enum values, permission codes) is binding and sourced from web-api-client.ts — not to be re-derived or renamed.

## Open Questions

- **OQ1**: Exact permission code names for Journals and Templates — values listed above are from the contract but need final verification against the backend policy registration.
- **OQ2**: Behavior when sequenceId is null — does the journal use auto-numbering or is numbering disabled?
- **OQ3**: Is suspenseAccount mandatory for Cash/Bank journal types? Currently optional in the contract.

## Clarifications

### Session 2026-09-07

- Q: Which fields are blocked from editing when a journal is referenced by existing entries? → A: All fields except isActive and requireApprovalBeforePosting (code, name, type, accountId, suspenseAccountId, allowForeignCurrency, sequenceId are locked).
- Q: Can non-system templates referenced by ACC-04 periodic scheduling be edited or deleted? → A: No restrictions — any non-system template can be edited or deleted regardless of ACC-04 usage.
- Q: How is the isSystemTemplate flag set when creating a template? → A: Hidden from UI, defaults to false server-side. Only the server can set it to true.
