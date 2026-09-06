# Tasks: Journal Entry Lifecycle Screens

**Input**: Design documents from `/specs/025-journal-entry-lifecycle/`
**Branch**: `025-journal-entry-lifecycle`

## Phase 1: Setup

- [X] T001 Verify types completeness
- [X] T002 Verify route configuration
- [X] T003 Verify permissions registered

## Phase 2: Foundational

- [X] T004 Create shared client wrapper
- [X] T005 Rebuild useJournalEntries hook
- [X] T006 Rebuild useJournalEntryLines hook

## Phase 3: US1 — Create and Edit Draft

- [X] T007 JournalEntryHeaderForm tests
- [X] T008 JournalEntryLinesEditor tests
- [X] T009 JournalEntryCreatePage tests
- [X] T010 Rebuild BalanceIndicator
- [X] T011 Rebuild FiscalYearIndicator
- [X] T012 Verify DimensionPickers
- [X] T013 Verify StatusBadge
- [X] T014 Rebuild JournalEntryHeaderForm
- [X] T015 Rebuild JournalEntryLinesEditor
- [X] T016 Rebuild JournalEntryCreatePage

## Phase 4: US2 — Submit

- [X] T017 EntryLifecycleActions tests (submit)
- [X] T018 Create EntryLifecycleActions

## Phase 5: US3 — Approve

- [X] T019 EntryLifecycleActions tests (approve)
- [X] T020 Extend EntryLifecycleActions

## Phase 6: US4 — Post

- [X] T021 EntryLifecycleActions tests (post)
- [X] T022 Extend EntryLifecycleActions

## Phase 7: US5 — Reverse

- [X] T023 ReverseDialog tests
- [X] T024 EntryLifecycleActions tests (reverse)
- [X] T025 Rebuild ReverseDialog
- [X] T026 Extend EntryLifecycleActions

## Phase 8: US6 — Cancel

- [X] T027 EntryLifecycleActions tests (cancel)
- [X] T028 Extend EntryLifecycleActions

## Phase 9: US7 — Browse & Audit

- [X] T029 JournalEntriesListPage tests
- [X] T030 JournalEntryDetailPage tests
- [X] T031 Rebuild JournalEntriesGrid
- [X] T032 Rebuild JournalEntryDetail
- [X] T033 Rebuild JournalEntriesListPage
- [X] T034 Rebuild JournalEntryDetailPage

## Phase 10: Polish

- [X] T035 Lint
- [X] T036 Tests
- [X] T037 Build
- [X] T038 Quickstart validation

## Field Coverage Verification

- [ ] T039 Verify entry header fields: entryNumber, entryStatus, documentDate, postingDate, entryType, journalId, periodId, fiscalYearId, narration, ref, reversalOfId, reversalReason, postedById/At, cancelledById/At, isSystemGenerated, rowVersion
- [ ] T040 Verify line fields: sequence, accountId, description, currencyId, exchangeRate, debit, credit, costCenterId, fundId, projectId, budgetItemId, encumbranceId, paymentOrderId
- [ ] T041 Verify state machine: Draft→Submitted→Approved→Posted→Reversed, Cancel from Draft/Submitted only
- [ ] T042 Verify permissions: Read, Create, UpdateLines, Submit, Approve, Post, Reverse, Cancel
