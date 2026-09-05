# Feature Specification: BF-001 Journal Entries Frontend

**Feature Branch**: `003-bf001-journal-entries-frontend`

**Created**: 2026-09-02

**Status**: Draft

**Input**: User description: "Frontend implementation for BF-001 Journal Entries — complete lifecycle management including list, create, detail, move lines, submit, approve, post, cancel, reverse. Arabic RTL, validation, API integration, permission-aware UI, concurrency handling."

## User Scenarios & Testing

### User Story 1 - List and Filter Journal Entries (Priority: P1)

As an Accountant, I want to view all journal entries with filtering by status, date range, and journal, so that I can find and monitor entries efficiently.

**Why this priority**: Foundation for all other operations. Without a working list, users cannot navigate to entries.

**Independent Test**: Navigate to `/accounting/journal-entries`. Verify entries load in a table. Filter by status "مسودة". Verify filtered results. Click an entry number to navigate to detail.

**Acceptance Scenarios**:

1. **Given** the user is on the journal entries list page, **When** the page loads, **Then** a table displays all journal entries with columns: Entry Number, Document Date, Journal Name, System-generated indicator, and Status badge (Arabic labels).
2. **Given** the list is displayed, **When** the user selects a status from the filter dropdown, **Then** only entries matching that status are shown.
3. **Given** the list is displayed, **When** the user enters a date range (from/to), **Then** only entries within that date range are shown.
4. **Given** the list is displayed, **When** the user selects a journal from the filter dropdown, **Then** only entries for that journal are shown.
5. **Given** the list is displayed, **When** the user clicks an entry number, **Then** the browser navigates to that entry's detail page.
6. **Given** the list is displayed, **When** the user clicks "قيد جديد", **Then** the browser navigates to the create page.
7. **Given** no entries match the filters, **When** the list is empty, **Then** an empty state message "لا توجد قيود" is displayed with a link to create a new entry.
8. **Given** entries are loading, **When** the data is being fetched, **Then** a loading indicator is displayed.
9. **Given** an error occurs while loading, **When** the API returns an error, **Then** an error message is displayed with a retry button.

---

### User Story 2 - Create Journal Entry with Lines (Priority: P1)

As an Accountant, I want to create a new journal entry with multiple lines (debit/credit), so that I can record financial transactions.

**Why this priority**: Core business function. Creating entries is the primary action in the accounting module.

**Independent Test**: Navigate to create page. Fill header fields. Add 2+ lines with debit/credit. Verify balance indicator. Submit. Verify redirect to detail page.

**Acceptance Scenarios**:

1. **Given** the user is on the create page, **When** the page loads, **Then** a form displays with Document Date (defaulting to today), Journal selector, Entry Type selector, Reference field, and Narration textarea.
2. **Given** the user changes the Document Date, **When** the date is valid, **Then** the system auto-detects the Fiscal Year and Period and displays them below the date field.
3. **Given** the user changes the Document Date to a date with no open fiscal period, **When** the auto-detection fails, **Then** an error message "لا توجد سنة مالية مفتوحة لهذا التاريخ" is displayed and the submit button is disabled.
4. **Given** the user is adding a line, **When** they select a non-base currency, **Then** the exchange rate is auto-fetched and populated for the document date.
5. **Given** the user has added lines, **When** viewing the lines table, **Then** a balance indicator shows total debit, total credit, and the difference (highlighted in red if unbalanced).
6. **Given** the user has added lines, **When** the total debit does not equal total credit, **Then** the submit button is disabled with a visual indicator.
7. **Given** the user fills all required fields and the lines balance, **When** they click "إنشاء القيد", **Then** the entry is created, a success toast is shown, and the browser navigates to the new entry's detail page.
8. **Given** the user clicks "إنشاء القيد" without any lines, **When** lines count is zero, **Then** the submit action is blocked.
9. **Given** the user clicks "إلغاء", **When** on the create page, **Then** the browser navigates back to the journal entries list.

---

### User Story 3 - View Journal Entry Detail (Priority: P1)

As an Accountant, I want to view the full details of a journal entry including its lines, status, and audit information, so that I can review and verify entries.

**Why this priority**: Essential for reviewing entries before and after workflow transitions.

**Independent Test**: Navigate to a journal entry detail page. Verify header info, status badge, lines table, and workflow action bar are displayed correctly.

**Acceptance Scenarios**:

1. **Given** the user navigates to a journal entry detail page, **When** the entry loads, **Then** a header card displays: Entry Number, Status Badge (Arabic), Document Date, Posting Date (if posted), Journal Name, Reference, Period/Fiscal Year, Narration, and system-generated indicator.
2. **Given** the entry is Posted, **When** the detail loads, **Then** audit information displays: "رحّل بواسطة: [name]" and the posting timestamp.
3. **Given** the entry is Cancelled, **When** the detail loads, **Then** audit information displays: "ألغى بواسطة: [name]" and the cancellation timestamp.
4. **Given** the entry has lines, **When** the detail loads, **Then** a lines table displays with columns: Sequence, Account (code — name), Description, Debit, Credit, Exchange Rate, Currency, Cost Center.
5. **Given** the entry status is Draft, **When** the detail loads, **Then** the lines table allows inline editing (click row to edit), adding new lines, and removing lines.
6. **Given** the entry status is not Draft, **When** the detail loads, **Then** the lines table is read-only.
7. **Given** the entry is a reversal, **When** the detail loads, **Then** the original entry reference and reversal reason are displayed.
8. **Given** the entry is loading, **When** data is being fetched, **Then** a loading indicator is displayed.
9. **Given** the entry is not found, **When** the API returns 404, **Then** an error message "غير موجود" is displayed with a retry button.

---

### User Story 4 - Workflow Transitions (Priority: P1)

As an Accountant or Approver, I want to transition journal entries through their lifecycle (submit, approve, post, cancel, reverse), so that entries follow the proper authorization process.

**Why this priority**: Core business process. Without workflow, entries cannot progress through their lifecycle.

**Independent Test**: Create a Draft entry. Submit it. Approve it. Post it. Reverse it. Verify status transitions and toast notifications at each step.

**Acceptance Scenarios**:

1. **Given** the entry is Draft, **When** the user clicks "إرسال للاعتماد", **Then** the entry status changes to "Submitted" and a success toast "تم الإرسال للاعتماد" is shown.
2. **Given** the entry is Submitted, **When** the user clicks "اعتماد", **Then** the entry status changes to "Approved" and a success toast "تم الاعتماد" is shown.
3. **Given** the entry is Approved, **When** the user clicks "ترحيل", **Then** the entry status changes to "Posted" and a success toast "تم الترحيل" is shown.
4. **Given** the entry is Draft or Submitted, **When** the user clicks "إلغاء", **Then** the entry status changes to "Cancelled" and a success toast "تم الإلغاء" is shown.
5. **Given** the entry is Posted, **When** the user clicks "عكس القيد", **Then** a dialog opens requesting a reversal reason (required, max 500 characters).
6. **Given** the reversal dialog is open, **When** the user provides a reason and confirms, **Then** a new reversal entry is created with debit/credit swapped, the original entry status changes to "Reversed", and the browser navigates to the new reversal entry.
7. **Given** any workflow action fails (e.g., period closed, concurrency conflict), **When** the API returns an error, **Then** an error toast displays the Arabic error message from the server.
8. **Given** a concurrency conflict occurs (RowVersion mismatch), **When** the API returns 409, **Then** a warning toast "تم تعديل القيد من مستخدم آخر" is shown and the entry data is refreshed.
9. **Given** the entry is Posted, Reversed, or Cancelled, **When** the detail loads, **Then** no workflow action buttons are displayed (read-only state).

---

### Edge Cases

- What happens when the user creates a line with both Debit and Credit filled? The system MUST reject the line and show a validation error "يجب إدخال مدين أو دائن فقط".
- What happens when the user tries to post an entry with unbalanced lines? The server rejects with a validation error and the frontend displays it.
- What happens when the fiscal year closes while the user is editing a Draft entry? The server rejects the post/submit with an appropriate error.
- What happens when another user modifies the same entry simultaneously? The RowVersion conflict triggers a refresh with a warning toast.
- What happens when the user tries to reverse an entry that was already reversed? The server rejects with an appropriate error.
- What happens when the user navigates to a non-existent entry? A 404 error page is shown.
- What happens when the API is unreachable? A network error message with retry is shown.
- What happens when the lines table has many entries (100+)? The table scrolls horizontally and remains usable.
- What happens when the user changes the Document Date after adding lines? Lines remain unchanged; only the FY/Period indicator updates.
- What happens when the exchange rate lookup fails for a non-base currency? The exchange rate defaults to 1 and the user can manually override.

## Requirements

### Functional Requirements

- **FR-001**: System MUST display journal entries in a table with columns: Entry Number, Document Date, Journal Name, System-generated indicator, Status (Arabic labels).
- **FR-002**: System MUST filter entries by Entry Status (dropdown: Draft/Submitted/Approved/Posted/Reversed/Cancelled).
- **FR-003**: System MUST filter entries by Date Range (from/to date pickers).
- **FR-004**: System MUST filter entries by Journal (dropdown populated from active journals).
- **FR-005**: System MUST support client-side pagination for the entries list using the shared DataGrid component.
- **FR-005a**: System MUST persist list filter state (status, date range, journal) in URL search parameters, enabling bookmarkable filtered views and correct back-button behavior.
- **FR-006**: System MUST allow creating a new journal entry with header fields: Document Date (required), Journal (optional), Entry Type (optional), Reference (optional), Narration (optional).
- **FR-007**: System MUST auto-detect Fiscal Year and Period from the Document Date and display them to the user.
- **FR-008**: System MUST disable the submit button when no open fiscal period exists for the selected date.
- **FR-009**: System MUST allow adding move lines with: Account (required, postable only), Debit XOR Credit (mutually exclusive, one required), Currency (required, default base), Exchange Rate (auto-fetched for non-base currencies), Cost Center (optional), Description (optional).
- **FR-010**: System MUST display a running balance indicator showing total Debit, total Credit, and the difference.
- **FR-011**: System MUST disable the submit button when lines do not balance (total Debit ≠ total Credit) or when no lines exist.
- **FR-012**: System MUST display journal entry detail with header information, status badge, and lines table.
- **FR-013**: System MUST display audit information (posted by/date, cancelled by/date) when applicable.
- **FR-014**: System MUST display reversal reference and reason when the entry is a reversal.
- **FR-015**: System MUST provide workflow action buttons based on entry status: Draft → Submit + Cancel; Submitted → Approve + Cancel; Approved → Post; Posted → Reverse.
- **FR-016**: System MUST display confirmation dialogs for Cancel, Post, and Reverse actions. The Reverse dialog MUST require a reversal reason (required, max 500 characters). Cancel and Post dialogs MUST show a brief confirmation message.
- **FR-017**: System MUST show success toasts for all workflow transitions in Arabic.
- **FR-018**: System MUST show error toasts with Arabic error messages from the server for failed operations.
- **FR-019**: System MUST handle RowVersion concurrency conflicts by refreshing the entry and showing a warning toast.
- **FR-020**: System MUST allow inline editing of move lines when entry status is Draft (click row to edit).
- **FR-021**: System MUST allow adding and removing move lines when entry status is Draft.
- **FR-022**: System MUST display lines as read-only when entry status is not Draft.
- **FR-023**: System MUST use the shared DataGrid component for the entries list with client-side pagination and sorting.
- **FR-024**: System MUST use Arabic labels, RTL layout, and ar-EG locale formatting for all dates, numbers, and currency.
- **FR-025**: System MUST display loading indicators during data fetching and mutation pending states.
- **FR-026**: System MUST display error states with retry functionality when API calls fail.
- **FR-027**: System MUST display empty states with a call-to-action when no entries exist.
- **FR-028**: System MUST validate all form inputs using Zod schemas with Arabic error messages.
- **FR-029**: System MUST cache server state and reuse it across page navigations, with automatic refetch on stale data.
- **FR-030**: System MUST invalidate relevant cached data after successful mutations to ensure consistency.

### Key Entities

- **Journal Entry (Move)**: A financial recording with double-entry balance. Key attributes: Entry Number (auto-generated), Document Date, Posting Date, Status (Draft/Submitted/Approved/Posted/Reversed/Cancelled), Journal, Fiscal Year, Period, Narration, Reference, Entry Type, System-generated flag, RowVersion.
- **Move Line**: A single debit or credit line within a journal entry. Key attributes: Sequence, Account (code, name), Description, Debit amount, Credit amount, Currency, Exchange Rate, Cost Center, Base Debit, Base Credit, RowVersion.
- **Journal**: Configuration for a type of journal (e.g., General, Sales, Purchases). Key attributes: Code, Name, Type, Account, Suspense Account, Allow Foreign Currency, Require Approval Before Posting.
- **Fiscal Year**: Accounting period definition. Key attributes: Name, Year Number, Start Date, End Date, Status (Open/Closed).
- **Fiscal Period**: Subdivision of a fiscal year. Key attributes: Period Number, Name, Start Date, End Date, Locked for Posting flag.

## Success Criteria

### Measurable Outcomes

- **SC-001**: Users can create a journal entry with 5 lines in under 3 minutes.
- **SC-002**: Users can find a specific journal entry using filters in under 30 seconds.
- **SC-003**: All workflow transitions (submit, approve, post, cancel, reverse) complete with visible feedback within 2 seconds.
- **SC-004**: The journal entries list loads and displays entries within 2 seconds for up to 1,000 entries.
- **SC-005**: Form validation errors are displayed immediately upon user interaction (inline validation).
- **SC-006**: Concurrency conflicts are handled gracefully without data loss — the user is informed and the entry is refreshed.
- **SC-007**: All UI text is in Arabic with correct RTL layout on all screen sizes.
- **SC-008**: 100% of user-facing operations have loading, success, and error states.

## Assumptions

- The backend API at `/api/Moves` and related endpoints is fully implemented and stable.
- The auto-generated NSwag client (`web-api-client.ts`) and hand-written clients (`movesClient`, `moveLinesClient`) are up to date.
- The Fiscal Year auto-detection endpoint (`/api/FiscalYears/by-date`) and Exchange Rate lookup endpoint (`/api/ExchangeRates/lookup`) are functional.
- The `usePermission` hook is a stub that always grants permissions — real RBAC enforcement is out of scope.
- The DataGrid component supports client-side pagination, sorting, and filtering.
- React Query is configured with 30-second stale time and 1 retry.
- The existing Design System tokens and shared UI components are stable and available.
- Arabic is the only language — no i18n switching is needed.
- The testing framework is Vitest with React Testing Library.
- All monetary values use ar-EG locale formatting with tabular numerals.

## Decisions

### Q1: List Pagination — Client-side Initially

**Context**: The DataGrid component supports both server-side and client-side pagination. The current API (`/api/Moves`) returns a flat list without skip/take parameters.

**Decision**: Client-side pagination initially. The API does not currently support skip/take parameters. Server-side pagination can be added when the API is extended. This avoids backend changes (out of scope).

**Impact**: FR-005 uses client-side pagination. May be slow with 1000+ entries. Acceptable for initial implementation.

### Q2: Workflow Confirmation Dialogs — Cancel + Post + Reverse

**Context**: The current MoveDetail.tsx executes Submit and Approve immediately on button click. Only Reverse has a confirmation dialog.

**Decision**: Add confirmation dialogs for Cancel, Post, and Reverse. Submit and Approve execute immediately (low risk, can be cancelled).

**Impact**: Cancel and Post now show ConfirmDialog before execution. Reverse already has ReverseDialog. Submit and Approve remain immediate.

### Q3: MoveCreatePage Form — Hybrid Approach

**Context**: The current MoveCreatePage uses manual `useState` for all form state. MoveForm.tsx exists with RHF/Zod but is not used by MoveCreatePage. All other forms in the codebase use RHF/Zod.

**Decision**: Hybrid approach — React Hook Form + Zod for the header form (Document Date, Journal, Entry Type, Reference, Narration). Manual `useState` for the lines editor (account, debit/credit, currency, cost center, description).

**Impact**: Header form gets proper validation, error display, and form reset via RHF. Lines editor stays manual for flexibility (inline editing, dynamic add/remove). Aligns partially with codebase conventions.

## Clarifications

### Session 2026-09-02

- Q: Should journal entry list filters be persisted in URL search params or component state? → A: URL search params (bookmarkable, shareable, back-button correct). Added as FR-005a.
- Q: Does FR-023 contradict FR-005 regarding pagination? → A: Yes — FR-023 corrected to "client-side pagination" to match Decision Q1.
- Q: Does FR-016 contradict Decision Q2 regarding confirmation dialogs? → A: Yes — FR-016 updated to include Cancel + Post + Reverse confirmations per Decision Q2.
