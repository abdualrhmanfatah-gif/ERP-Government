# Tasks: Posting Pipeline

**Input**: Design documents from `/specs/002-posting-pipeline/`

**Prerequisites**: plan.md, spec.md, research.md, data-model.md, contracts/api-contracts.md

**Tests**: Tests are included per spec Principle XI (testing verification)

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (US1, US2, US3, US4)
- Include exact file paths in descriptions

---

## Phase 1: Setup

**Purpose**: Verify existing infrastructure and dependencies

- [ ] T001 Verify existing OutboxProcessorService registration in src/Infrastructure/DependencyInjection.cs
- [ ] T002 [P] Verify existing AccountingEvent, PostingRule, PostingRuleLine entity configurations compile
- [ ] T003 [P] Verify existing seed data in src/Infrastructure/Data/Seeds/PostingRuleSeedData.cs

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Orphan recovery and structured logging infrastructure that ALL user stories depend on

**⚠ CRITICAL**: No user story work can begin until this phase is complete

- [x] T004 Add ResetStaleProcessingEventsAsync method to src/Application/Accounting/EventHandlers/AccountingEventAuditor.cs (orphan recovery)
- [x] T005 Add orphan recovery call to src/Infrastructure/Services/OutboxProcessorService.cs ExecuteAsync (call before main loop)
- [x] T006 [P] Add structured logging to src/Application/Accounting/EventHandlers/DomainEventHandler.cs (CorrelationId, AccountingEventId)
- [x] T007 [P] Add structured logging to src/Application/Accounting/eventHandlers/PostingPipelineHandler.cs (CorrelationId, AccountingEventId, MoveId)
- [x] T008 [P] Add structured logging to src/Application/Accounting/EventHandlers/MoveGenerator.cs (CorrelationId, MoveId)
- [x] T009 Create src/Application/Accounting/Commands/PostingRules/DeletePostingRule/DeletePostingRuleCommand.cs with pending event guard
- [x] T010 [P] Create src/Application/Accounting/Commands/PostingRules/DeletePostingRule/DeletePostingRuleCommandValidator.cs
- [x] T011 Add PermissionCodes.PostingRulesDelete constant to src/Application/Common/Security/PermissionCodes.cs
- [x] T012 [P] Add DELETE endpoint to src/Web/Endpoints/Accounting/PostingRules.cs

**Checkpoint**: Foundation ready — orphan recovery, structured logging, delete guard all in place

---

## Phase 3: User Story 1 — Automatic Journal Entry from Domain Events (Priority: P1) — MVP

**Goal**: Domain events automatically create journal entry shells (Moves) via PostingRules

**Independent Test**: Raise a PurchaseOrderApproved event → verify Move created with correct JournalId, SourceEventId, IsSystemGenerated=true

### Tests for User Story 1

- [x] T013 [P] [US1] Add test: DomainEventHandler creates AccountingEvent from IHasSourceEntity event in tests/Application.UnitTests/EventHandlers/DomainEventHandlerTests.cs
- [x] T014 [P] [US1] Add test: PostingRuleMatcher filters inactive rules in tests/Application.UnitTests/EventHandlers/PostingRuleMatcherTests.cs
- [x] T015 [P] [US1] Add test: PostingRuleMatcher orders by Priority ascending in tests/Application.UnitTests/EventHandlers/PostingRuleMatcherTests.cs

### Implementation for User Story 1

**Checkpoint**: US1 complete — events auto-create Moves

---

## Phase 4: User Story 2 — Durable Event Tracking with Retry (Priority: P1)

**Goal**: Failed events tracked, auto-retried up to 3 times, manual retry after 3 failures

**Independent Test**: Create AccountingEvent with Status=Failed, RetryCount=3 → verify ProcessAccountingEventCommand resets to Pending

### Tests for User Story 2

- [x] T016 [P] [US2] Add test: AccountingEventAuditor.MarkFailedAsync increments RetryCount in tests/Application.UnitTests/EventHandlers/AccountingEventAuditorTests.cs
- [x] T017 [P] [US2] Add test: AccountingEventAuditor.ResetForRetryAsync resets to Pending in tests/Application.UnitTests/EventHandlers/AccountingEventAuditorTests.cs
- [x] T018 [P] [US2] Add test: ProcessAccountingEventCommand rejects non-Failed events in tests/Application.UnitTests/EventHandlers/ProcessAccountingEventCommandTests.cs
- [x] T019 [P] [US2] Add test: RetryAccountingEventCommand rejects RetryCount < 3 in tests/Application.UnitTests/EventHandlers/ProcessAccountingEventCommandTests.cs

### Implementation for User Story 2

- [x] T020 [US2] Add orphan recovery test: ResetStaleProcessingEventsAsync resets events >5min old in tests/Application.UnitTests/EventHandlers/AccountingEventAuditorTests.cs

**Checkpoint**: US2 complete — retry lifecycle fully functional

---

## Phase 5: User Story 3 — Configurable Posting Rules (Priority: P2)

**Goal**: Finance administrators can create, update, and delete PostingRules

**Independent Test**: Create PostingRule → verify it appears in GetPostingRulesListQuery → delete it → verify gone

### Tests for User Story 3

- [x] T021 [P] [US3] Add test: DeletePostingRuleCommand rejects deletion with pending events in tests/Application.UnitTests/Commands/PostingRules/DeletePostingRuleTests.cs

### Implementation for User Story 3

- [x] T022 [P] [US3] Add test: DeletePostingRuleCommand allows deletion with no pending events in tests/Application.UnitTests/Commands/PostingRules/DeletePostingRuleTests.cs

**Checkpoint**: US3 complete — PostingRule CRUD with delete guard

---

## Phase 6: User Story 4 — Manual Event Processing and Monitoring (Priority: P2)

**Goal**: Administrators can view pending events, process failed events, monitor pipeline

**Independent Test**: Call GET /accounting-events/pending → verify pending events returned

### Tests for User Story 4

- [x] T023 [P] [US4] Add test: GET /accounting-events/pending returns pending events in tests/Application.FunctionalTests/Accounting/PostingPipelineTests.cs

### Implementation for User Story 4

- [x] T024 [P] [US4] Add test: POST /accounting-events/{id}/process resets Failed event in tests/Application.FunctionalTests/Accounting/PostingPipelineTests.cs
- [x] T025 [P] [US4] Add test: POST /accounting-events/{id}/retry rejects RetryCount < 3 in tests/Application.FunctionalTests/Accounting/PostingPipelineTests.cs

**Checkpoint**: US4 complete — full API surface for monitoring

---

## Phase 7: Polish & Cross-Cutting Concerns

**Purpose**: End-to-end validation and documentation

- [x] T026 Run quickstart.md Scenario 1: Automatic Journal Entry validation
- [x] T027 Run quickstart.md Scenario 5: Delete PostingRule with Pending Events validation
- [x] T028 Run quickstart.md Scenario 6: Orphan Recovery on Startup validation
- [x] T029 [P] Run quickstart.md Scenario 7: Structured Logging validation
- [x] T030 Run all unit tests: dotnet test tests/Application.UnitTests
- [x] T031 Run all functional tests: dotnet test tests/Application.FunctionalTests
- [x] T032 Verify Constitution compliance: all new endpoints have PermissionCodes, no SaveChangesAsync in handlers

---

## Dependencies & Execution Order

### Phase Dependencies

- **Phase 1 (Setup)**: No dependencies — can start immediately
- **Phase 2 (Foundational)**: Depends on Phase 1 — BLOCKS all user stories
- **Phase 3 (US1)**: Depends on Phase 2 — can start after foundation
- **Phase 4 (US2)**: Depends on Phase 2 — can start after foundation
- **Phase 5 (US3)**: Depends on Phase 2 — can start after foundation
- **Phase 6 (US4)**: Depends on Phase 2 — can start after foundation
- **Phase 7 (Polish)**: Depends on all user stories being complete

### User Story Dependencies

- **US1 (P1)**: Can start after Phase 2 — No dependencies on other stories
- **US2 (P1)**: Can start after Phase 2 — Can parallel with US1
- **US3 (P2)**: Can start after Phase 2 — Can parallel with US1/US2
- **US4 (P2)**: Can start after Phase 2 — Can parallel with US1/US2/US3

### Within Each User Story

- Tests MUST be written and verified before implementation
- Models/entities before services
- Services before endpoints
- Core implementation before integration

### Parallel Opportunities

- **Phase 1**: T002, T003 can run in parallel
- **Phase 2**: T006, T007, T008, T010, T012 can run in parallel
- **Phase 3**: T013, T014, T015 can run in parallel
- **Phase 4**: T016, T017, T018, T019 can run in parallel
- **Phase 5**: T021, T022 can run in parallel
- **Phase 6**: T023, T024, T025 can run in parallel
- **Phase 7**: T026-T029 can run in parallel

---

## Parallel Example: Phase 2 (Foundation)

```bash
# Launch all structured logging tasks together:
Task: "Add structured logging to DomainEventHandler.cs"
Task: "Add structured logging to PostingPipelineHandler.cs"
Task: "Add structured logging to MoveGenerator.cs"

# Launch delete guard tasks together:
Task: "Create DeletePostingRuleCommand.cs"
Task: "Create DeletePostingRuleCommandValidator.cs"
Task: "Add DELETE endpoint to PostingRules.cs"
```

---

## Implementation Strategy

### MVP First (US1 + US2 Only)

1. Complete Phase 1: Setup
2. Complete Phase 2: Foundational (CRITICAL)
3. Complete Phase 3: US1 — Automatic Journal Entry
4. Complete Phase 4: US2 — Durable Event Tracking
5. **STOP and VALIDATE**: Test US1 + US2 independently
6. Deploy/demo if ready

### Incremental Delivery

1. Setup + Foundational → Foundation ready
2. Add US1 → Test independently → Deploy/Demo (MVP!)
3. Add US2 → Test independently → Deploy/Demo
4. Add US3 → Test independently → Deploy/Demo
5. Add US4 → Test independently → Deploy/Demo
6. Each story adds value without breaking previous stories

---

## Notes

- [P] tasks = different files, no dependencies
- [Story] label maps task to specific user story for traceability
- Each user story is independently completable and testable
- Existing code already handles most of US1-US4 — this spec adds structured logging, orphan recovery, and delete guard
- MoveLine generation deferred to FEATURE-027 (out of scope)
