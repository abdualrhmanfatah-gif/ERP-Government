# Tasks: Remove Liquidations and BudgetLedgerEntries

**Input**: Design documents from `/specs/009-remove-liquidations-budgetledger/`

**Prerequisites**: plan.md, spec.md, research.md, data-model.md, quickstart.md

**Tests**: Test update tasks included (FR-011 requires compilation + test pass).

**Organization**: Tasks grouped by user story. All stories are P1 except US6 (P2), US7 (P2), US8 (P3).

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (US1-US8)
- Include exact file paths in descriptions

## Phase 1: Setup — Constitution Decision Record

**Purpose**: Constitution Principle XII requires decision record before implementation.

- [x] T001 Create decision record in docs/decision-records/ documenting: rationale for BF-004 removal, scope (2 tables, 7 columns, ~48 files), migration/remediation plan, feature registry update (12 → 11 business features)
- [x] T002 Amend Constitution Principle V chain from "Budget → Appropriation → Encumbrance → Liquidation → Payment" to "Budget → Appropriation → Encumbrance → Payment" in .specify/memory/constitution.md (line 85)
- [x] T003 Update Constitution version from 1.0.0 to 1.1.0 in .specify/memory/constitution.md (line 281)

**Checkpoint**: Decision record exists, Principle V amended, version bumped.

---

## Phase 2: Domain Layer Cleanup

**Purpose**: Delete Liquidation/BudgetLedgerEntry entities, enums, events, and remove counter fields from remaining entities. This phase MUST complete before Application layer changes (commands reference these entities).

### Delete Domain Files [US5]

- [x] T004 [P] [US5] Delete src/Domain/Budgeting/Entities/Liquidation.cs
- [x] T005 [P] [US5] Delete src/Domain/Budgeting/Entities/BudgetLedgerEntry.cs
- [x] T006 [P] [US5] Delete src/Domain/Budgeting/Enums/LiquidationStatus.cs
- [x] T007 [P] [US5] Delete src/Domain/Budgeting/Enums/LiquidationType.cs
- [x] T008 [P] [US5] Delete src/Domain/Budgeting/Enums/BudgetLedgerEntryType.cs
- [x] T009 [P] [US5] Delete src/Domain/Budgeting/Enums/BudgetLedgerDirection.cs
- [x] T010 [P] [US5] Delete src/Domain/Budgeting/Enums/BudgetLedgerStatus.cs
- [x] T011 [P] [US5] Delete src/Domain/Events/Budgeting/LiquidationCreated.cs
- [x] T012 [P] [US5] Delete src/Domain/Events/Budgeting/LiquidationPosted.cs

### Modify Domain Entities [US1, US3, US4]

- [x] T013 [US4] Remove `public decimal LiquidatedAmount { get; set; }` and `public Enums.ReleaseStatus LiquidationStatus { get; set; }` from src/Domain/Budgeting/Entities/Encumbrance.cs (lines 26, 30)
- [x] T014 [US4] Remove `public decimal? LiquidatedAmount { get; set; }` from src/Domain/Budgeting/Entities/Budget.cs (line 21)
- [x] T015 [US4] Remove `public decimal? LiquidatedAmount { get; set; }` from src/Domain/Budgeting/Entities/BudgetItem.cs (line 26)
- [x] T016 [US4] Remove `public decimal LiquidatedAmount { get; set; }` from src/Domain/Budgeting/Entities/Appropriation.cs (line 22)
- [x] T017 [US3] Remove `public int? LiquidationId { get; set; }` and Liquidation navigation property from src/Domain/Payments/Entities/PaymentOrder.cs (line 20)
- [x] T018 [US4] Update comment in src/Domain/Budgeting/Enums/ReleaseStatus.cs line 5: change "Used by Encumbrances.ReleaseStatus and Encumbrances.LiquidationStatus" to "Used by Encumbrances.ReleaseStatus"

**Checkpoint**: Domain layer has zero Liquidation/BudgetLedgerEntry references. Compilation will fail until Application layer is cleaned.

---

## Phase 3: Application Layer Cleanup — Delete Dead Commands/Queries/DTOs

**Purpose**: Delete Liquidation commands, queries, BudgetLedgerEntry query, and DTOs. Remove liquidation fields from remaining DTOs. These are leaf nodes — nothing else references them.

### Delete Application Files [US5]

- [x] T019 [P] [US5] Delete entire directory src/Application/Budgeting/Commands/Liquidations/ (5 commands: Create, Approve, MarkPartiallyPaid, MarkFullyPaid, Reverse)
- [x] T020 [P] [US5] Delete entire directory src/Application/Budgeting/Queries/Liquidations/ (2 queries: GetLiquidationById, GetLiquidationsByEncumbrance)
- [x] T021 [P] [US5] Delete entire directory src/Application/Budgeting/Queries/BudgetLedgerEntries/ (1 query: GetBudgetLedgerEntries)
- [x] T022 [P] [US5] Delete src/Application/Budgeting/Common/LiquidationDto.cs
- [x] T023 [P] [US5] Delete src/Application/Budgeting/Common/BudgetLedgerEntryDto.cs

### Modify Application DTOs [US4]

- [x] T024 [US4] Remove `LiquidatedAmount` and `LiquidationStatus` properties and AutoMapper mapping from src/Application/Budgeting/Common/EncumbranceDto.cs (lines 23, 27, 38)
- [x] T025 [US4] Remove `LiquidatedAmount` property from src/Application/Budgeting/Common/BudgetDto.cs (line 20)
- [x] T026 [US4] Remove `LiquidatedAmount` property from src/Application/Budgeting/Common/BudgetItemDto.cs (line 22)
- [x] T027 [US4] Remove `LiquidatedAmount` property from src/Application/Budgeting/Common/AppropriationDto.cs (line 19)
- [x] T028 [US4] Remove `LiquidatedAmount` property from src/Application/Budgeting/Common/BudgetSummaryDto.cs (line 9)

### Modify Application Commands [US4, US5]

- [x] T029 [US4] Remove `LiquidatedAmount = 0` and `LiquidationStatus = ReleaseStatus.None` from entity initializer in src/Application/Budgeting/Commands/Encumbrances/CreateEncumbrance/CreateEncumbranceCommand.cs (lines 93, 96)
- [x] T030 [US4] Remove `if (entity.LiquidatedAmount > 0)` guard from src/Application/Budgeting/Commands/Encumbrances/CancelEncumbrance/CancelEncumbranceCommand.cs (lines 34-36)
- [x] T031 [US5] Remove BudgetLedgerEntry creation block (lines 38-57) from src/Application/Budgeting/Commands/Encumbrances/ApproveEncumbrance/ApproveEncumbranceCommand.cs — keep status change and Appropriation/Budget/BudgetItem amount updates
- [x] T032 [US5] Remove BudgetLedgerEntry creation block (lines 56-75) from src/Application/Budgeting/Commands/Encumbrances/ReleaseEncumbrance/ReleaseEncumbranceCommand.cs — keep ReleasedAmount/ReleaseStatus updates and amount updates
- [x] T033 [US5] Remove BudgetLedgerEntry creation block (lines 54-73) from src/Application/Budgeting/Commands/Encumbrances/ReverseEncumbrance/ReverseEncumbranceCommand.cs — keep IsReversed/Status change and amount updates
- [x] T034 [US5] Remove BudgetLedgerEntry creation block (lines 50-69) from src/Application/Budgeting/Commands/Encumbrances/CancelEncumbrance/CancelEncumbranceCommand.cs — keep status change and amount updates
- [x] T035 [US5] Remove BudgetLedgerEntry creation block (lines 48-66) from src/Application/Budgeting/Commands/Appropriations/ApproveAppropriation/ApproveAppropriationCommand.cs — keep status change, budget check, and amount updates
- [x] T036 [US5] Remove BudgetLedgerEntry creation block (lines 46-64) from src/Application/Budgeting/Commands/Appropriations/ReverseAppropriation/ReverseAppropriationCommand.cs — keep IsReversed/Status change and amount updates
- [x] T037 [US4] Remove `LiquidatedAmount = 0` from entity initializer in src/Application/Budgeting/Commands/Appropriations/CreateAppropriation/CreateAppropriationCommand.cs (line 98)

### Modify Application Query [US4]

- [x] T038 [US4] Remove `LiquidatedAmount = entity.LiquidatedAmount ?? 0` from src/Application/Budgeting/Queries/Budgets/GetBudgetSummary/GetBudgetSummaryQuery.cs (line 32)

**Checkpoint**: Application layer compiles (references deleted entities removed). Infrastructure layer changes next.

---

## Phase 4: Infrastructure Layer Cleanup

**Purpose**: Delete EF configurations, remove DbSets, remove permission constants, update seeds, remove DocumentSequence prefix.

### Delete Infrastructure Configurations [US5]

- [x] T039 [P] [US5] Delete src/Infrastructure/Data/Configurations/Budgeting/LiquidationConfiguration.cs
- [x] T040 [P] [US5] Delete src/Infrastructure/Data/Configurations/Budgeting/BudgetLedgerEntryConfiguration.cs

### Modify Infrastructure Configurations [US3, US4]

- [x] T041 [US4] Remove LiquidatedAmount (lines 38-40) and LiquidationStatus (lines 54-56) property mappings from src/Infrastructure/Data/Configurations/Budgeting/EncumbranceConfiguration.cs
- [x] T042 [US4] Remove LiquidatedAmount property mapping (lines 43-45) from src/Infrastructure/Data/Configurations/Budgeting/BudgetConfiguration.cs
- [x] T043 [US4] Remove LiquidatedAmount property mapping (lines 39-41) from src/Infrastructure/Data/Configurations/Budgeting/BudgetItemConfiguration.cs
- [x] T044 [US4] Remove LiquidatedAmount property mapping (lines 39-41) from src/Infrastructure/Data/Configurations/Budgeting/AppropriationConfiguration.cs
- [x] T045 [US3] Remove LiquidationId index (line 119) from src/Infrastructure/Data/Configurations/Payments/PaymentOrderConfiguration.cs

### Modify DbContext [US1, US2]

- [x] T046 [US1] Remove `DbSet<Liquidation> Liquidations` and `DbSet<BudgetLedgerEntry> BudgetLedgerEntries` from src/Application/Common/Interfaces/IApplicationDbContext.cs (lines 84-85)
- [x] T047 [US1] Remove `DbSet<Liquidation> Liquidations` and `DbSet<BudgetLedgerEntry> BudgetLedgerEntries` from src/Infrastructure/Data/ApplicationDbContext.cs (lines 89-90)

### Modify Permissions and Seeds [US5]

- [x] T048 [US5] Remove 6 permission constants from src/Application/Common/Security/PermissionCodes.cs: BudgetLedgerEntriesView (line 109), LiquidationsView, LiquidationsCreate, LiquidationsApprove, LiquidationsMarkPaid, LiquidationsReverse (lines 121-125)
- [x] T049 [US5] Remove 6 AddPolicy registrations from src/Web/DependencyInjection.cs: BudgetLedgerEntriesView (line 145), 5 Liquidations policies (lines 157-161)
- [x] T050 [US5] Remove Liquidation references from src/Infrastructure/Data/Seeds/RolePermissionSeedData.cs: Remove `p.Code.StartsWith("Liquidations.") ||` from FIN_MGR filter (line 30), BUD_MGR filter (line 70), and Make() entries for Liquidations.* + BudgetLedgerEntries.View (lines 193, 196)

### Modify DocumentSequence [US5]

- [x] T051 [US5] Remove `["Liquidation"] = "LIQ"` prefix mapping from src/Application/FinancialSettings/Common/Services/DocumentSequenceService.cs (line 13)

### Delete Web Endpoints [US5]

- [x] T052 [P] [US5] Delete src/Web/Endpoints/Budgeting/Liquidations.cs
- [x] T053 [P] [US5] Delete src/Web/Endpoints/Budgeting/BudgetLedgerEntries.cs

**Checkpoint**: All code references removed. Code compiles. Migration and tests next.

---

## Phase 5: EF Core Migration [US1, US2, US3, US4, US7, US8]

**Purpose**: Generate the database migration that creates deletion report tables, populates them, drops tables/columns, and logs audit entries. Includes full reversibility.

- [x] T054 [US7] Generate EF Core migration via `dotnet ef migrations add RemoveLiquidationsAndBudgetLedgerEntries --project src/Infrastructure --startup-project src/Web` — verify snapshot updated correctly
- [x] T055 [US7] In migration Up(): Add SQL to create [LiquidationDeletionReport] table with columns (Id, LiquidationNumber, EncumbranceId, Amount, Status, CreatedAt, DeletedAt) per data-model.md
- [x] T056 [US7] In migration Up(): Add SQL to create [BudgetLedgerEntryDeletionReport] table with columns (Id, BudgetId, EntryType, Amount, Direction, Status, OccurredAt, DeletedAt) per data-model.md
- [x] T057 [US7] In migration Up(): Add SQL to populate both deletion report tables via INSERT...SELECT from source tables before drops
- [x] T058 [US7] In migration Up(): Add SQL to log SecurityAuditLog entries for each table deletion (entity=System, action=SchemaDeletion, target=TableName)
- [x] T059 [US1] In migration Up(): Verify Liquidations table drop including all FKs, indexes, PK, self-FK ReversalOfId
- [x] T060 [US2] In migration Up(): Verify BudgetLedgerEntries table drop including all FKs, indexes, PK
- [x] T061 [US3] In migration Up(): Verify PaymentOrders.LiquidationId column and IX_PaymentOrders_LiquidationId index drop
- [x] T062 [US4] In migration Up(): Verify counter column drops: Encumbrances.LiquidatedAmount, Encumbrances.LiquidationStatus, Budgets.LiquidatedAmount, BudgetItems.LiquidatedAmount, Appropriations.LiquidatedAmount
- [x] T063 [US8] In migration Down(): Implement full reverse — recreate all dropped tables, columns, indexes, and restore data from deletion reports
- [x] T064 [US1] Verify ApplicationDbContextModelSnapshot.cs reflects all deletions (no Liquidations/BudgetLedgerEntries tables, no dropped columns)

**Checkpoint**: Migration applies cleanly on fresh DB. Down migration restores everything.

---

## Phase 6: Tests Update [US5]

**Purpose**: Update test files to remove Liquidation/BudgetLedgerEntry references. Compilation MUST pass.

- [x] T065 [US5] Remove `LiquidationTests` class (lines 253-299) from tests/Domain.UnitTests/Budgeting/EntityTests.cs
- [x] T066 [US5] Remove `encumbrance.LiquidatedAmount.ShouldBe(0m)` (line 215) and `encumbrance.LiquidationStatus.ShouldBe(ReleaseStatus.None)` (line 219) from EncumbranceTests in tests/Domain.UnitTests/Budgeting/EntityTests.cs
- [x] T067 [US5] Remove `LiquidatedAmount = 100000m` from test Budget data in tests/Application.UnitTests/Budgeting/QueryTests.cs (line 134)
- [x] T068 [US5] Remove `prefixMap.ShouldContainKey("Liquidation")` (line 22), `prefixMap["Liquidation"].ShouldBe("LIQ")` (line 42) from tests/Application.UnitTests/FinancialSettings/DocumentSequenceServiceTests.cs
- [x] T069 [US5] Update `prefixMap.Count.ShouldBe(10)` to `prefixMap.Count.ShouldBe(9)` in tests/Application.UnitTests/FinancialSettings/DocumentSequenceServiceTests.cs (line 126)

**Checkpoint**: All tests compile and pass.

---

## Phase 7: Documentation Updates [US6]

**Purpose**: Update database schema docs, feature registry, and architecture docs.

- [x] T070 [US6] Remove Table 49 (Liquidations) and Table 50 (BudgetLedgerEntries) from docs/database-schema.md (lines 889-948)
- [x] T071 [US6] Remove LiquidatedAmount column from Table 45 Budgets (line 774), Table 46 BudgetItems (line 813), Table 47 Appropriations (line 842), Table 48 Encumbrances (line 875) in docs/database-schema.md
- [x] T072 [US6] Remove LiquidationStatus column from Table 48 Encumbrances (line 879) in docs/database-schema.md
- [x] T073 [US6] Remove BF-004 row from feature registry table and update Budget Cycle count from 3 to 2 in docs/final-business-feature-registry.md (lines 20, 47, 176)
- [x] T074 [US6] Remove BF-004 from tree diagram in docs/final-business-feature-registry.md (line 20)

**Checkpoint**: Documentation reflects the removal.

---

## Phase 8: Polish & Cross-Cutting Validation

**Purpose**: Regenerate OpenAPI, run full validation, ensure zero remaining references.

- [x] T075 Regenerate OpenAPI document by running `dotnet run --project src/Web` and capturing updated src/Web/wwwroot/openapi/v1.json — verify no Liquidation/BudgetLedgerEntry schemas or endpoints
- [x] T076 Run `dotnet build ERP-Government.slnx -warnaserror` — verify zero warnings (SC-001)
- [x] T077 Run `dotnet test ERP-Government.slnx --no-build` — verify 100% pass rate (SC-002)
- [x] T078 Run `rg "Liquidation|BudgetLedgerEntry" src/ tests/ --glob '!**/Migrations/**' --glob '!**/DeletionReport*'` — verify zero matches (SC-003)
- [x] T079 Run `rg "Liquidations|BudgetLedgerEntries" src/Infrastructure/Migrations/ApplicationDbContextModelSnapshot.cs` — verify zero matches (SC-004)
- [x] T080 Run `rg "LiquidationId" src/Infrastructure/Migrations/ApplicationDbContextModelSnapshot.cs` — verify zero matches (SC-005)
- [x] T081 Verify Constitution v1.1.0 with amended Principle V and decision record committed (SC-007)

**Checkpoint**: All success criteria met. Feature complete.

---

## Dependencies & Execution Order

### Phase Dependencies

- **Phase 1 (Setup)**: No dependencies — can start immediately
- **Phase 2 (Domain)**: Depends on Phase 1 (decision record must exist before code changes)
- **Phase 3 (Application)**: Depends on Phase 2 (domain entities modified/deleted)
- **Phase 4 (Infrastructure)**: Depends on Phase 3 (application code updated)
- **Phase 5 (Migration)**: Depends on Phase 4 (all code changes complete, model snapshot accurate)
- **Phase 6 (Tests)**: Depends on Phase 4 (code compiles, tests can be updated)
- **Phase 7 (Docs)**: Independent — can run in parallel with Phases 5-6
- **Phase 8 (Polish)**: Depends on Phases 5, 6, 7 (all changes complete)

### Within Each Phase

- Tasks marked [P] can run in parallel
- Within a phase, execute in listed order unless marked [P]
- Each phase produces a checkpoint — verify before proceeding

### Parallel Opportunities

- Phase 2: All 9 domain file deletions (T004-T012) can run in parallel
- Phase 3: All 5 application file deletions (T019-T023) can run in parallel
- Phase 4: Config deletions (T039-T040) and endpoint deletions (T052-T053) can run in parallel
- Phase 6: All 5 test file updates (T065-T069) can run in parallel
- Phase 7: All 5 documentation updates (T070-T074) can run in parallel

---

## Implementation Strategy

### MVP First (US1 + US2 + US3 + US4 — Core Deletions)

1. Complete Phase 1: Decision record + Constitution amendment
2. Complete Phase 2: Domain layer cleanup
3. Complete Phase 3: Application layer cleanup
4. Complete Phase 4: Infrastructure layer cleanup
5. Complete Phase 5: EF Core migration
6. **STOP and VALIDATE**: `dotnet build -warnaserror` + `dotnet test` pass
7. Proceed to Phase 6 (tests), Phase 7 (docs), Phase 8 (polish)

### Incremental Delivery

1. Phase 1 → Decision recorded ✓
2. Phases 2-4 → Code compiles with zero Liquidation/BudgetLedgerEntry references ✓
3. Phase 5 → Migration applies cleanly on fresh DB ✓
4. Phase 6 → All tests pass ✓
5. Phase 7 → Documentation updated ✓
6. Phase 8 → Full validation passed ✓

### Parallel Team Strategy

With multiple developers:
1. Developer A: Phases 1-2 (Constitution + Domain)
2. Developer B: Phase 3-4 (Application + Infrastructure) — can start after Phase 2 completes
3. Developer C: Phase 7 (Documentation) — independent, can run in parallel
4. All converge for Phase 5 (Migration) and Phase 8 (Validation)

---

## Notes

- [P] tasks = different files, no dependencies
- [Story] label maps task to specific user story for traceability
- All stories are deletion-focused — the real constraint is compilation at each phase
- Historical migration files (InitialCreate etc.) are NEVER modified
- The migration is the single source of truth for schema changes
- Commit after each phase checkpoint
