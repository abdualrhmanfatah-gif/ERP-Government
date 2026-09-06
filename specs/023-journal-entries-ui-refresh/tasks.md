# Tasks: Journal Entries UI Refresh

**Input**: Design documents from `/specs/023-journal-entries-ui-refresh/`
**Prerequisites**: plan.md, spec.md, data-model.md, contracts/

## Phase 1: Setup (Shared Infrastructure)

- [x] T001 Delete old Move-based page files
- [x] T002 Delete old Move-based component files
- [x] T003 Delete old Move-based hook files
- [x] T004 Delete hand-written client
- [x] T005 Update types — replace MoveDto/MoveLineDto with JournalEntryDto/JournalEntryLineDto

## Phase 2: Foundational

- [x] T006 Create StatusBadge.tsx — EntryStatus → colored badge
- [x] T007 Create DimensionPickers.tsx — fund/project/budgetItem/encumbrance/paymentOrder
- [x] T008 Create useJournalEntries.ts — all lifecycle hooks
- [x] T009 Create useJournalEntryLines.ts — line CRUD hooks
- [x] T010 Update BalanceIndicator.tsx

## Phase 3: User Story 1 — Browse Journal Entries

### Tests
- [x] T011 JournalEntriesListPage.test.tsx — badges, filters, empty, no Move refs
- [x] T012 StatusBadge.test.tsx — each status renders correct color

### Implementation
- [x] T013 JournalEntriesGrid.tsx — DataGrid with columns
- [x] T014 JournalEntriesListPage.tsx — page with filters + grid
- [x] T015 Update routes.tsx — swap to JournalEntriesListPage
- [x] T016 Update index.ts — export

## Phase 4: User Story 2 — Create Journal Entry

### Tests
- [x] T017 JournalEntryCreatePage.test.tsx — form, balance, submit
- [x] T018 JournalEntryLinesEditor.test.tsx — line fields, XOR validation
- [x] T019 DimensionPickers.test.tsx — pickers load data

### Implementation
- [x] T020 JournalEntryHeaderForm.tsx — react-hook-form + zod
- [x] T021 JournalEntryLinesEditor.tsx — inline line editor
- [x] T022 JournalEntryCreatePage.tsx — dedicated page
- [x] T023 Update routes.tsx
- [x] T024 Update index.ts

## Phase 5: User Story 3 — Lifecycle Actions

### Tests
- [x] T025 JournalEntryDetailPage.test.tsx — header, lines, lifecycle, panels
- [x] T026 JournalEntryDetail.test.tsx — component-level tests

### Implementation
- [x] T027 JournalEntryDetail.tsx — full detail view
- [x] T028 JournalEntryDetailPage.tsx — page with loading/error
- [x] T029 Update routes.tsx
- [x] T030 Update index.ts

## Phase 6: User Story 4 — Reversed Entry Links

### Tests
- [x] T031 Add reversal link tests to JournalEntryDetailPage.test.tsx

### Implementation
- [x] T032 Update JournalEntryDetail.tsx — bidirectional reversal links

## Phase 7: Polish

- [x] T033 Audit for remaining Move/MoveLine references
- [x] T034 Audit routes.tsx
- [x] T035 Run npm run lint
- [x] T036 Run npm run test
- [x] T037 Run npm run build

## Field Coverage Verification

Verify every field name from spec.md Field Contract appears in implemented source:

- [ ] T038 Verify header fields: entryNumber, ref, documentDate, postingDate, entryType, journalId, periodId, fiscalYearId, narration, sourceEventId, isSystemGenerated, rowVersion, reversalOfId, reversalReason
- [ ] T039 Verify line fields: sequence, accountId, description, currencyId, exchangeRate, debit, credit, costCenterId, fundId, projectId, budgetItemId, encumbranceId, paymentOrderId, lineId
- [ ] T040 Verify list columns: entryNumber, documentDate, entryStatus, journalName, totalDebit, totalCredit
- [ ] T041 Verify permissions rendered: Read, Create, UpdateLines, Submit, Approve, Post, Reverse, Cancel buttons guarded
- [ ] T042 Verify UI states: loading/empty/error/unauthorized/not-found per page
