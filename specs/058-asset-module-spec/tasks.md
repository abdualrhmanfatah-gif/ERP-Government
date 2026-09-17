# Tasks: Asset Management Module Rebuild (وحدة إدارة الأصول)

**Input**: Design documents from `/specs/058-asset-module-spec/`

**Prerequisites**: plan.md, spec.md, research.md, data-model.md, contracts/api.md, quickstart.md

**Tests**: REQUIRED — Constitution XI (TDD is NON-NEGOTIABLE for new features; test tasks are not optional). Write each test first and observe it FAIL before implementing.

**Organization**: Tasks are grouped by user story. US1–US5 are P1; US6–US9 are P2. Every story is independently testable after the Foundational phase.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: US1–US9 per spec.md
- Exact file paths included in every task

## Path Conventions

- Backend layers: `src/Domain/`, `src/Application/`, `src/Infrastructure/`, `src/Web/`
- Frontend SPA: `src/Web/ClientApp/src/`
- Tests: `tests/Application.UnitTests/`, `tests/Domain.UnitTests/`, `tests/Application.FunctionalTests/`, `tests/Web.AcceptanceTests/`
- Decision records: `docs/decision-records/`

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Prerequisites that unblock all phases — the Constitution XII decision record, numbering, and permissions

- [X] T001 Create DEP-030 decision record (owner, rationale, scope listing all 9 tables, remediation path) in docs/decision-records/DEP-030-drop-legacy-assets-tables.md per FR-129/Constitution XII — MUST be accepted before any migration work
- [X] T002 [P] Register document-sequence prefixes (AssetTransfer→TRF, AssetDisposal→DSP, AssetRevaluation→REV, AssetImpairment→IMP, DepreciationSchedule→DEP, AssetPhysicalCount→CNT) in src/Infrastructure/Services/DocumentSequenceService.cs (R5)
- [X] T003 [P] Add new permission codes (AssetTransfers.View/Create/Execute; AssetDepreciation.View/Run/Post/Reverse; AssetCounts.View/Create/Execute/Review), retire AssetMovements.\*, and update seed data in src/Application/Common/Security/PermissionCodes.cs, src/Infrastructure/Data/Seeds/SecurityPermissionSeedData.cs, src/Infrastructure/Data/Seeds/RolePermissionSeedData.cs (R6)

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: The DEP-030 model reset + shared posting infrastructure. BLOCKS all user stories.

**⚠️ CRITICAL**: No user story work can begin until this phase is complete (the build must be green on the new model).

### Tests (TDD — write first, must fail)

- [X] T004 Write failing functional test: DEP-030 deletion report — 9 legacy tables dropped (data/history), 13 new tables created empty, shared tables (users, employees, accounts, journal entries, locations, funds, cost centers, currencies, fiscal periods) unchanged in tests/Application.FunctionalTests/Assets/ModelResetTests.cs (FR-130/131, SC-001)

### Entities & Events

- [X] T005 [P] Create enum types (AssetTransactionType, AssetTransactionStatus, DepreciationScheduleStatus, CountStatus, CountFoundState, AssetAttributeDataType) in src/Domain/Assets/Enums/
- [X] T006 [P] Recreate AssetGroup entity — reduced account set (`AssetAccountId`, `DepreciationAccountId`, `DisposalAccountId`, `RevaluationAccountId`, `ImpairmentLossAccountId` only; C5) in src/Domain/Assets/Entities/AssetGroup.cs
- [X] T007 [P] Recreate Asset entity — `EmployeeId` custodian (Q1), `CurrencyId` FK (C4), typed decimal classes, no stored department in src/Domain/Assets/Entities/Asset.cs
- [X] T008 [P] Create AssetTransaction unified header entity in src/Domain/Assets/Entities/AssetTransaction.cs
- [X] T009 [P] Create AssetTransferDetail entity (snapshot fields per FR-050/038b) in src/Domain/Assets/Entities/AssetTransferDetail.cs
- [X] T010 [P] Create AssetDisposalDetail entity (stored NetProceeds/GainOrLoss per C2) in src/Domain/Assets/Entities/AssetDisposalDetail.cs
- [X] T011 [P] Create AssetRevaluationDetail entity (stored RevaluationAmount/Type per C2) in src/Domain/Assets/Entities/AssetRevaluationDetail.cs
- [X] T012 [P] Create AssetImpairmentDetail entity (ReversalOfTransactionId link per FR-080) in src/Domain/Assets/Entities/AssetImpairmentDetail.cs
- [X] T013 [P] Recreate DepreciationSchedule entity (snapshots, C8 reversal fields, R3-compatible) in src/Domain/Assets/Entities/DepreciationSchedule.cs
- [X] T014 [P] Recreate AssetPhysicalCount entity (ResolvedScopeLabel field, FR-111b) in src/Domain/Assets/Entities/AssetPhysicalCount.cs
- [X] T015 [P] Recreate AssetPhysicalCountDetail entity (tri-state CountFoundState; UNIQUE count+asset) in src/Domain/Assets/Entities/AssetPhysicalCountDetail.cs
- [X] T016 [P] Create AssetAttributeDefinition entity in src/Domain/Assets/Entities/AssetAttributeDefinition.cs
- [X] T017 [P] Create AssetGroupAttribute entity (composite binding identity) in src/Domain/Assets/Entities/AssetGroupAttribute.cs
- [X] T018 [P] Create AssetAttributeValue entity (typed value columns) in src/Domain/Assets/Entities/AssetAttributeValue.cs
- [X] T019 Delete the 9 legacy entity files in src/Domain/Assets/Entities/ and their references
- [X] T020 [P] Add `LineRole` + TemplateLineRole enum (Fixed, GroupDepreciationAccount) to src/Domain/Accounting/Entities/JournalEntryTemplateLine.cs (R4)
- [X] T021 [P] Add nullable `DepreciationScheduleId` FK to src/Domain/Accounting/Entities/JournalEntryLine.cs (R3)
- [X] T022 [P] Add DepreciationReversed and AssetImpairmentReversed events; extend existing AssetDisposed/AssetRevalued/AssetImpaired/DepreciationPosted payloads with the fields the consumers need (schedule ids, amounts, asset) in src/Domain/Events/Assets/

### Persistence

- [X] T023 [P] Create EF configurations for the 13 tables in src/Infrastructure/Data/Configurations/Assets/ (Restrict FKs, explicit precision, indexes, UNIQUE constraints per data-model.md)
- [X] T024 Remove the 9 legacy EF configurations; update JournalEntryTemplateLineConfiguration (LineRole) and JournalEntryLineConfiguration (DepreciationScheduleId FK Restrict) in src/Infrastructure/Data/Configurations/
- [X] T025 Swap DbSets (remove 9 legacy, add 13) in src/Application/Common/Interfaces/IApplicationDbContext.cs and src/Infrastructure/Data/ApplicationDbContext.cs
- [X] T026 Create the DEP-030 migration (drop 9 + create 13) in src/Infrastructure/Data/Migrations/ via `dotnet ef migrations add` (FR-130/136)
- [X] T027 Update src/Infrastructure/Data/ApplicationDbContextInitialiser.cs (remove legacy asset seed refs) + idempotent seed of the template substitution designation and the trusted configuration key locating the depreciation template in src/Infrastructure/Data/Seeds/ (FR-094a, R4)

### Shared Infrastructure

- [X] T028 Mechanical re-point of the 054/055 slices to the new model so the solution builds green (field renames: custodian→`EmployeeId`, `CurrencyId`, reduced group accounts; no behavior change) across src/Application/Assets/AssetGroups/**, src/Application/Assets/Assets/**, src/Web/Endpoints/Assets/**
- [X] T029 [P] Shared PostingGateValidator (FR-094a gates: group account present, template complete, substitution role defined, entry balanced; IV gates) in src/Application/Assets/Common/PostingGateValidator.cs
- [X] T030 Regenerate nswag API clients and update shared frontend types (npm run generate-api; src/Web/ClientApp/src/features/assets/shared/types.ts)
- [X] T031 Run the deletion-report test (T004) to green + full backend suites to confirm the clean baseline; `dotnet build` warnings-free

**Checkpoint**: Model reset complete, build green, foundation ready — all stories can start

---

## Phase 3: User Story 1 - Configure Asset Groups with Default Accounts and Attributes (Priority: P1)

**Goal**: Groups carry the reduced account set (C5), depreciation defaults, and explicit attribute bindings.

**Independent Test**: Create/update groups with/without accounts, define attributes, bind them; verify cycle rejection, duplicate-code rejection, and binding composite identity.

### Tests for User Story 1

- [X] T032 [P] [US1] Unit tests: Create/UpdateAssetGroup handlers + validators on the new model (success, duplicate code, hierarchy cycle, invalid account refs) in tests/Application.UnitTests/Assets/AssetGroups/
- [X] T033 [P] [US1] Unit tests: attribute definition create/update + group binding set (composite identity, required flag, sort-order fallback) in tests/Application.UnitTests/Assets/AssetAttributes/
- [X] T034 [P] [US1] Functional test: group lifecycle with bindings against the real DB in tests/Application.FunctionalTests/Assets/AssetGroups/

### Implementation for User Story 1

- [X] T035 [US1] Re-point group commands/validators to the reduced account set + hierarchy cycle validation in src/Application/Assets/AssetGroups/Commands/
- [X] T036 [P] [US1] Attribute definition slices (create/update/list) in src/Application/Assets/AssetAttributes/Definitions/
- [X] T037 [P] [US1] Group binding command (set bindings: definitionId, isRequired, sortOrder?) in src/Application/Assets/AssetAttributes/Bindings/
- [X] T038 [US1] Endpoints: group CRUD updates + /api/AssetAttributes/definitions + PUT /api/AssetGroups/{id}/attributes with permissions (R6) in src/Web/Endpoints/Assets/AssetGroups.cs and src/Web/Endpoints/Assets/AssetAttributes.cs
- [X] T039 [US1] Frontend: re-point asset-groups screens (single depreciation account field, bindings editor) in src/Web/ClientApp/src/features/assets/asset-groups/

**Checkpoint**: Group configuration fully functional and independently testable

---

## Phase 4: User Story 2 - Register and Maintain the Asset Card with Flexible Attributes (Priority: P1)

**Goal**: Card carries employee custodian + derived department, typed attribute values validated against bindings.

**Independent Test**: Register → activate an asset with attribute values; verify typed validation, required bindings, derived `currentDepartment`, and lock rules.

### Tests for User Story 2

- [X] T040 [P] [US2] Unit tests: card create/update/deactivate (employee custodian, typed attribute values, required bindings, activation locks) in tests/Application.UnitTests/Assets/Assets/
- [X] T041 [P] [US2] Functional test: register → activate → attribute values persisted; derived `currentDepartment`; custodian-without-department cases in tests/Application.FunctionalTests/Assets/Assets/

### Implementation for User Story 2

- [X] T042 [US2] Re-point register slices (EmployeeId custodian, CurrencyId, attribute-value integration) in src/Application/Assets/Assets/
- [X] T043 [US2] Derived `currentDepartment` read model (custodian employee → `Employees.DepartmentId`, FR-038a) in src/Application/Assets/Assets/Common/
- [X] T044 [US2] Attribute value validation service (typed columns, required from binding, FR-024) in src/Application/Assets/AssetAttributes/Values/
- [X] T045 [US2] Endpoints update: employee custodian filter, detail includes attribute values (R6/contracts) in src/Web/Endpoints/Assets/Assets.cs
- [X] T046 [US2] Frontend: re-point assets screens (employee picker, derived department display, typed attribute fields) in src/Web/ClientApp/src/features/assets/assets/

**Checkpoint**: Register functional; US1+US2 = usable base

---

## Phase 5: User Story 3 - Transfer an Asset (Priority: P1)

**Goal**: Transfer document records before/after snapshots and updates the card atomically.

**Independent Test**: Execute a transfer (location/custodian/department changes); verify snapshots, staleness re-validation, and card update.

### Tests for User Story 3

- [X] T047 [P] [US3] Unit tests: create/execute handlers (staleness re-validation FR-052, snapshot fields FR-050/051, null-destination rule) in tests/Application.UnitTests/Assets/AssetTransfers/
- [X] T048 [P] [US3] Functional test: transfer updates card + preserves history; TRF numbering in tests/Application.FunctionalTests/Assets/AssetTransfers/

### Implementation for User Story 3

- [X] T049 [US3] Transfer slices: create / update-draft / execute in src/Application/Assets/AssetTransactions/Transfers/
- [X] T050 [US3] Endpoints /api/AssetTransfers (View/Create/Execute) in src/Web/Endpoints/Assets/AssetTransfers.cs
- [X] T051 [US3] Frontend: transfers list/create/detail/execute in src/Web/ClientApp/src/features/assets/asset-transactions/transfers/

**Checkpoint**: Transfers independently functional (no GL effect)

---

## Phase 6: User Story 4 - Run, Post, and Reverse Depreciation (Priority: P1)

**Goal**: Calculation with snapshots, posting through the outbox with template substitution (FR-094a), line-level source tagging (Q3: B), reversal from the original entry.

**Independent Test**: Run a period, post, trace the aggregated entry's per-line tagging, reverse one record; verify gates block missing account/role/incomplete template.

### Tests for User Story 4

- [X] T052 [P] [US4] Domain unit tests: DepreciationCalculator (methods, six-decimal, rounding policy, fully-depreciated stop, snapshots) in tests/Domain.UnitTests/Assets/DepreciationCalculatorTests.cs
- [X] T053 [P] [US4] Unit tests: run handler (eligibility, duplicate prevention keyed on period identity per FR-094, snapshots) + gate validator paths (missing account, incomplete template, undefined role, unbalanced) in tests/Application.UnitTests/Assets/Depreciation/
- [X] T054 [P] [US4] Functional tests: event→consumer aggregated entry with line-level source tagging; idempotent re-drive; reversal from ORIGINAL entry's accounts; posting blocks; stored amounts reconcile with the journal in tests/Application.FunctionalTests/Assets/Depreciation/

### Implementation for User Story 4

- [X] T055 [US4] DepreciationCalculator domain service in src/Domain/Assets/Services/DepreciationCalculator.cs
- [X] T056 [US4] Run slice (eligible assets, snapshots, duplicate prevention) in src/Application/Assets/Depreciation/Run/
- [X] T057 [US4] Post slice (gates, emit DepreciationPosted in the same transaction, status → Posting) in src/Application/Assets/Depreciation/Post/
- [X] T058 [US4] Reverse slice (linked record, DepreciationReversed event, ORIGINAL accounts source) in src/Application/Assets/Depreciation/Reverse/
- [X] T059 [US4] DepreciationPostedHandler consumer (template role substitution: debit = group `DepreciationAccountId`, credit = template counterpart; line tagging; writeback `JournalEntryId`/`IsPosted`; idempotent) in src/Application/Accounting/Integration/Assets/DepreciationPostedHandler.cs
- [X] T060 [US4] DepreciationReversedHandler consumer in src/Application/Accounting/Integration/Assets/DepreciationReversedHandler.cs
- [X] T061 [US4] Endpoints /api/AssetDepreciation (schedules, run, post, reverse, preview) in src/Web/Endpoints/Assets/AssetDepreciation.cs
- [X] T062 [US4] Frontend: depreciation run/schedules/post/reverse in src/Web/ClientApp/src/features/assets/asset-depreciation/

**Checkpoint**: Depreciation engine complete; the financial core is live

---

## Phase 7: User Story 5 - Dispose of an Asset (Priority: P1)

**Goal**: Disposal with snapshots, derived gain/loss, posted entry, status transition — history never erased.

**Independent Test**: Dispose an asset; verify snapshots, entry + card status in one consistent result, second disposal rejected, screen deletion impossible.

### Tests for User Story 5

- [X] T063 [P] [US5] Unit tests: create/approve/post (derivation FR-061, duplicate rejection FR-064, snapshots FR-060) in tests/Application.UnitTests/Assets/Disposals/
- [X] T064 [P] [US5] Functional test: disposal→entry→card status; second disposal rejected in tests/Application.FunctionalTests/Assets/Disposals/

### Implementation for User Story 5

- [X] T065 [US5] Disposal slices (create/update-draft/approve/post) in src/Application/Assets/AssetTransactions/Disposals/
- [X] T066 [US5] AssetDisposedHandler consumer (entry + card status writeback) in src/Application/Accounting/Integration/Assets/AssetDisposedHandler.cs
- [X] T067 [US5] Endpoints /api/AssetDisposals in src/Web/Endpoints/Assets/AssetDisposals.cs
- [X] T068 [US5] Frontend: disposal screens in src/Web/ClientApp/src/features/assets/asset-transactions/disposals/

**Checkpoint**: Disposal complete

---

## Phase 8: User Story 6 - Revalue an Asset (Priority: P2)

**Goal**: Revaluation with old/new snapshots, zero-difference policy, posted effect.

**Independent Test**: Revalue up/down/zero; verify snapshots, amount/direction derivation, and card effect after posting.

### Tests for User Story 6

- [X] T069 [P] [US6] Unit tests: create/post (zero-difference not auto-classified FR-071, snapshots FR-070, method required at completion) in tests/Application.UnitTests/Assets/Revaluations/
- [X] T070 [P] [US6] Functional test: revaluation→entry→card effect in tests/Application.FunctionalTests/Assets/Revaluations/

### Implementation for User Story 6

- [X] T071 [US6] Revaluation slices (create/update-draft/approve/post) in src/Application/Assets/AssetTransactions/Revaluations/
- [X] T072 [US6] AssetRevaluedHandler consumer in src/Application/Accounting/Integration/Assets/AssetRevaluedHandler.cs
- [X] T073 [US6] Endpoints /api/AssetRevaluations in src/Web/Endpoints/Assets/AssetRevaluations.cs
- [X] T074 [US6] Frontend: revaluation screens in src/Web/ClientApp/src/features/assets/asset-transactions/revaluations/

**Checkpoint**: Revaluation complete

---

## Phase 9: User Story 7 - Recognize and Reverse Impairment (Priority: P2)

**Goal**: Impairment with linked reversal transaction; original never mutated.

**Independent Test**: Record impairment, post, reverse with reason; verify link rules (self/cycle/same asset/type) and that the original + its entry remain.

### Tests for User Story 7

- [X] T075 [P] [US7] Unit tests: create/approve/post/reverse (link validation FR-081, sign convention fixed once, reversal reason) in tests/Application.UnitTests/Assets/Impairments/
- [X] T076 [P] [US7] Functional test: impairment→entry; reversal carries its own counter-entry; original preserved in tests/Application.FunctionalTests/Assets/Impairments/

### Implementation for User Story 7

- [X] T077 [US7] Impairment slices (create/update-draft/approve/post/reverse) in src/Application/Assets/AssetTransactions/Impairments/
- [X] T078 [US7] AssetImpairedHandler consumer in src/Application/Accounting/Integration/Assets/AssetImpairedHandler.cs
- [X] T079 [US7] AssetImpairmentReversedHandler consumer in src/Application/Accounting/Integration/Assets/AssetImpairmentReversedHandler.cs
- [X] T080 [US7] Endpoints /api/AssetImpairments (+ /reverse) in src/Web/Endpoints/Assets/AssetImpairments.cs
- [X] T081 [US7] Frontend: impairment screens in src/Web/ClientApp/src/features/assets/asset-transactions/impairments/

**Checkpoint**: Impairment + reversal complete

---

## Phase 10: User Story 8 - Conduct Physical Count and Review Discrepancies (Priority: P2)

**Goal**: Explicit scope semantics (FR-111/111a/111b), frozen system snapshots, one audited line per asset, completion gated on definite states.

**Independent Test**: Entity-wide count (labeled «جميع المواقع — جميع الإدارات»), start, observe, re-examine one line, block completion on a NotExamined line, complete, review.

### Tests for User Story 8

- [X] T082 [P] [US8] Domain unit tests: scope semantics (4 cases, custodian-derived department, include/exclude unprovable membership, resolved label) in tests/Domain.UnitTests/Assets/CountScopeTests.cs
- [X] T083 [P] [US8] Unit tests: create/start/observe/complete/review (freeze FR-112, in-place line update FR-116, NotExamined block FR-114, permission coverage FR-111a) in tests/Application.UnitTests/Assets/PhysicalCounts/
- [X] T084 [P] [US8] Functional tests: permission-coverage block; frozen lines; re-examination AuditTrail old/new diffs; UNIQUE (count, asset); completion rules in tests/Application.FunctionalTests/Assets/PhysicalCounts/

### Implementation for User Story 8

- [X] T085 [US8] Count slices (create/start with set-based generation/observe/complete/review) in src/Application/Assets/PhysicalCounts/
- [X] T086 [US8] Endpoints /api/AssetCounts (R6 permissions; scope label in responses) in src/Web/Endpoints/Assets/AssetCounts.cs
- [X] T087 [US8] Frontend: counts screens (scope label, lines grid, discrepancy review) in src/Web/ClientApp/src/features/assets/asset-counts/

**Checkpoint**: Counts complete

---

## Phase 11: User Story 9 - Trace an Asset's Complete History (Priority: P2)

**Goal**: Read-only traceability: card → transactions → journal entries → line-level accounts, with reversal pairing.

**Independent Test**: With data from prior stories, resolve every posting to its actual accounts from tagged lines (never a first line) and show reversal pairs.

### Tests for User Story 9

- [X] T088 [P] [US9] Functional tests: aggregated-entry account attribution per record; as-of-date values; reversal pairing in tests/Application.FunctionalTests/Assets/Traceability/

### Implementation for User Story 9

- [X] T089 [US9] History query slices (transaction aggregation + line-level account attribution, FR-124) in src/Application/Assets/Traceability/
- [X] T090 [US9] Endpoints: GET /api/Assets/{id}/transactions + entry-link resolution in src/Web/Endpoints/Assets/Assets.cs
- [X] T091 [US9] Frontend: asset history tab (dates, statuses, entry links, accounts used, reversal links) in src/Web/ClientApp/src/features/assets/assets/components/AssetHistoryTab.tsx

**Checkpoint**: All 9 stories functional

---

## Phase 12: Polish & Cross-Cutting Concerns

- [X] T092 [P] Update docs/database-schema.md for the 13-table model and the two Accounting additions
- [X] T093 [P] Verify RTL/dark-mode, design tokens, and shared money formatting on all new screens (Constitution X; quickstart frontend smoke)
- [X] T094 [P] Wire navigation permission identifiers to the new codes (AssetTransfers/AssetDepreciation/AssetCounts) in src/Web/ClientApp/src/
- [ ] T095 Write the end-to-end acceptance test for the full lifecycle on the clean register (SC-002) in tests/Web.AcceptanceTests/Features/Assets/
- [X] T096 [P] Record error-handling evidence for new failure paths (codes, safe messages, recovery ownership) per docs/error-handling.md (XIII)
- [ ] T097 Performance validation: 10k-asset list load and a full depreciation run per plan targets (quickstart)
- [X] T098 Constitution compliance review evidence + update DEP-030 with completion/remediation status
- [ ] T099 Run the full quickstart.md validation (all scenarios) + all backend suites + frontend lint/build

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies — start immediately; T001 MUST be accepted before T026
- **Foundational (Phase 2)**: Depends on Setup — BLOCKS all stories
- **User Stories (Phase 3–11)**: All depend on Foundational
  - US1 → US2 → (US3, US5, US6, US7, US8 in parallel) ; US4 needs US1 (accounts/template) + US2 (assets)
  - US9 needs data from the posting stories (US4–US7) and US8 for full coverage
- **Polish (Phase 12)**: Depends on all stories

### User Story Dependencies

- **US1 (P1)**: Foundational only
- **US2 (P1)**: Foundational + US1 (group/bindings for attributes)
- **US3 (P1)**: Foundational + US2 (a registered asset)
- **US4 (P1)**: Foundational + US1 (group account + template role) + US2 (assets)
- **US5 (P1)**: Foundational + US1 (accounts) + US2 (assets)
- **US6 (P2)**: Foundational + US1 + US2
- **US7 (P2)**: Foundational + US1 + US2
- **US8 (P2)**: Foundational + US2 (custodian-derived department)
- **US9 (P2)**: Data from US4–US7; queries independent

### Within Each Story

- Tests FIRST — write, observe FAIL, then implement (Constitution XI)
- Models → services → endpoints → frontend
- Story complete and checkpoint-validated before moving on

### Parallel Opportunities

- Phase 1: T002, T003 in parallel
- Phase 2: T005–T018 (entities), T020–T023 in parallel after T004; T029 parallel
- Each story: all test tasks marked [P] first, then implementation tasks
- After Foundational: US1 first, then US2, then US3/US5/US6/US7/US8 can run in parallel by different developers; US4 and US9 after their dependencies

---

## Parallel Example: User Story 4

```bash
# Launch all tests for US4 together (must fail first):
Task: "Domain unit tests: DepreciationCalculator in tests/Domain.UnitTests/Assets/DepreciationCalculatorTests.cs"
Task: "Unit tests: run handler + gate paths in tests/Application.UnitTests/Assets/Depreciation/"
Task: "Functional tests: aggregated entry tagging, idempotency, reversal in tests/Application.FunctionalTests/Assets/Depreciation/"

# Then implementation core:
Task: "DepreciationCalculator domain service in src/Domain/Assets/Services/DepreciationCalculator.cs"
Task: "Run slice in src/Application/Assets/Depreciation/Run/"
Task: "Post slice in src/Application/Assets/Depreciation/Post/"
```

---

## Implementation Strategy

### MVP First

1. Complete Phase 1 (Setup) and Phase 2 (Foundational — the DEP-030 reset)
2. Complete US1 (groups) — strict MVP per the template
3. **Practical first demo**: US1 + US2 (groups + register) — the minimum a user can operate
4. **STOP and VALIDATE** at each checkpoint before proceeding

### Incremental Delivery

1. Foundation → clean register operational
2. + US1/US2 → asset catalog usable
3. + US3 → custody/location tracking
4. + US4 → depreciation engine (financial core)
5. + US5/US6/US7 → full transaction set
6. + US8 → inventory counts
7. + US9 → audit traceability

Each increment is independently testable and does not break earlier stories.

---

## Notes

- [P] tasks = different files, no dependencies
- Every test task precedes its implementation and MUST be observed failing
- Financial invariants (balancing, gates, reversal, audit immutability, concurrency) have functional-test coverage per Constitution XI
- DEP-030 (T001) is the hard gate: no migration work before the decision record is accepted
- Frontend has no configured test runner (R10) — evidence burden is on backend suites + manual quickstart; lint/build gates apply
