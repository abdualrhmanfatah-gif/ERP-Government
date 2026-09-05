# Tasks: Remove Payment Sub-Entity Tables and Convert PaymentMethod to Enum

**Input**: Design documents from `/specs/008-remove-payment-tables/`

**Prerequisites**: plan.md, spec.md, research.md, data-model.md

**Tests**: No test tasks generated — spec does not request tests; payment test directories are empty; no new test debt introduced.

**Organization**: Tasks grouped by user story. US1 (P1) and US2 (P1) are tightly coupled and executed together. US3 (P2) is part of the migration. US4 (P1) is verification. US5 (P3) is the migration Down() method.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (US1-US5)
- Include exact file paths in descriptions

## Phase 1: Setup

**Purpose**: Decision record and prerequisite setup

- [x] T001 Create decision record DEP-021 for breaking API endpoint removal (Principle IX) in docs/decision-records/
- [x] T002 Create decision record for table deletion within Payments module (Principle XII, recommended) in docs/decision-records/

---

## Phase 2: Domain Layer — Delete Entities, Enums, Events; Create Enum

**Purpose**: Remove domain artifacts for deleted entities and create the PaymentMethod enum

**?? CRITICAL**: All Application and Infrastructure layers depend on Domain. This phase MUST complete first.

### Delete AdvancePayment domain artifacts

- [x] T003 [P] [US1] Delete AdvancePayment entity in src/Domain/Payments/Entities/AdvancePayment.cs
- [x] T004 [P] [US1] Delete AdvancePaymentStatus enum in src/Domain/Payments/Enums/AdvancePaymentStatus.cs
- [x] T005 [P] [US1] Delete AdvancePaymentCreated event in src/Domain/Events/Payments/AdvancePaymentCreated.cs
- [x] T006 [P] [US1] Delete AdvancePaymentSettled event in src/Domain/Events/Payments/AdvancePaymentSettled.cs

### Delete PaymentExecution domain artifacts

- [x] T007 [P] [US1] Delete PaymentExecution entity in src/Domain/Payments/Entities/PaymentExecution.cs
- [x] T008 [P] [US1] Delete PaymentExecutionStatus enum in src/Domain/Payments/Enums/PaymentExecutionStatus.cs

### Delete PaymentAllocation domain artifacts

- [x] T009 [P] [US1] Delete PaymentAllocation entity in src/Domain/Payments/Entities/PaymentAllocation.cs
- [x] T010 [P] [US1] Delete PaymentAllocationStatus enum in src/Domain/Payments/Enums/PaymentAllocationStatus.cs

### Delete PaymentMethod entity and create enum

- [x] T011 [US1] Delete PaymentMethod entity in src/Domain/Payments/Entities/PaymentMethod.cs
- [x] T012 [US2] Create PaymentMethod enum in src/Domain/Payments/Enums/PaymentMethod.cs with values: Cash=0, BankTransfer=1, Check=2, CreditCard=3, WireTransfer=4, Other=5

### Modify PaymentOrder entity

- [x] T013 [US2] Replace `int PaymentMethodId` with `PaymentMethod PaymentMethod` in src/Domain/Payments/Entities/PaymentOrder.cs (add using for new enum)

**Checkpoint**: Domain layer clean. All deleted entities/enums/events removed. New enum created. PaymentOrder updated.

---

## Phase 3: Application Layer — Delete Commands, Queries, DTOs; Update Remaining

**Purpose**: Remove application-layer artifacts for deleted entities and update remaining PaymentOrder/RevenueReceipt code

### Delete AdvancePayment commands and queries

- [x] T014 [P] [US1] Delete Commands/AdvancePayments folder in src/Application/Payments/Commands/AdvancePayments/
- [x] T015 [P] [US1] Delete Queries/AdvancePayments folder in src/Application/Payments/Queries/AdvancePayments/
- [x] T016 [P] [US1] Delete AdvancePaymentDto in src/Application/Payments/Common/DTOs/AdvancePaymentDto.cs

### Delete PaymentExecution commands and queries

- [x] T017 [P] [US1] Delete Commands/PaymentExecutions folder in src/Application/Payments/Commands/PaymentExecutions/
- [x] T018 [P] [US1] Delete Queries/PaymentExecutions folder in src/Application/Payments/Queries/PaymentExecutions/
- [x] T019 [P] [US1] Delete PaymentExecutionDto in src/Application/Payments/Common/DTOs/PaymentExecutionDto.cs

### Delete PaymentAllocation commands and queries

- [x] T020 [P] [US1] Delete Commands/PaymentAllocations folder in src/Application/Payments/Commands/PaymentAllocations/
- [x] T021 [P] [US1] Delete Queries/PaymentAllocations folder in src/Application/Payments/Queries/PaymentAllocations/

### Delete PaymentMethod commands, queries, and DTO

- [x] T022 [P] [US1] Delete Commands/PaymentMethods folder in src/Application/Payments/Commands/PaymentMethods/
- [x] T023 [P] [US1] Delete Queries/PaymentMethods folder in src/Application/Payments/Queries/PaymentMethods/
- [x] T024 [P] [US1] Delete PaymentMethodDto in src/Application/Payments/Common/DTOs/PaymentMethodDto.cs

### Update PaymentOrder application code

- [x] T025 [US2] Update PaymentOrderDto: replace `int PaymentMethodId` with `PaymentMethod PaymentMethod` (enum) and `string PaymentMethodName` in src/Application/Payments/Common/DTOs/PaymentOrderDto.cs
- [x] T026 [US2] Update CreatePaymentOrderCommand: replace `int PaymentMethodId` with `PaymentMethod PaymentMethod` (enum), remove PaymentMethods.FindAsync validation in src/Application/Payments/Commands/PaymentOrders/CreatePaymentOrder/CreatePaymentOrderCommand.cs
- [x] T027 [US2] Update CreatePaymentOrderCommandValidator: remove GreaterThan(0) rule for PaymentMethodId, add enum validation in src/Application/Payments/Commands/PaymentOrders/CreatePaymentOrder/CreatePaymentOrderCommandValidator.cs
- [x] T028 [US2] Update GetPaymentOrdersQuery: replace PaymentMethodId with PaymentMethod enum in projection in src/Application/Payments/Queries/PaymentOrders/GetPaymentOrders/GetPaymentOrdersQuery.cs
- [x] T029 [US2] Update GetPaymentOrderByIdQuery: replace PaymentMethodId with PaymentMethod enum in projection in src/Application/Payments/Queries/PaymentOrders/GetPaymentOrderById/GetPaymentOrderByIdQuery.cs

### Update RevenueReceipt application code

- [x] T030 [US2] Update RevenueReceiptDtos: replace `int PaymentMethodId` with `PaymentMethod PaymentMethod` (enum) and `string PaymentMethodName` in src/Application/Revenue/Common/DTOs/RevenueReceiptDtos.cs
- [x] T031 [US2] Update CreateRevenueReceiptCommand: replace `int PaymentMethodId` with `PaymentMethod PaymentMethod` (enum), remove PaymentMethods.FindAsync validation in src/Application/Revenue/Commands/RevenueReceipts/CreateRevenueReceipt/CreateRevenueReceiptCommand.cs
- [x] T032 [US2] Update CreateRevenueReceiptCommandValidator: remove PaymentMethodId rules, add enum validation in src/Application/Revenue/Commands/RevenueReceipts/CreateRevenueReceipt/CreateRevenueReceiptCommandValidator.cs
- [x] T033 [US2] Update GetRevenueReceiptsQuery: replace PaymentMethodId with PaymentMethod enum in projection in src/Application/Revenue/Queries/RevenueReceipts/GetRevenueReceipts/GetRevenueReceiptsQuery.cs
- [x] T034 [US2] Update GetRevenueReceiptByIdQuery: replace PaymentMethodId with PaymentMethod enum in projection in src/Application/Revenue/Queries/RevenueReceipts/GetRevenueReceiptById/GetRevenueReceiptByIdQuery.cs

### Update PermissionCodes

- [x] T035 [US1] Remove 19 permission constants from PaymentMethods, PaymentExecutions, PaymentAllocations, AdvancePayments in src/Application/Common/Security/PermissionCodes.cs

**Checkpoint**: Application layer clean. All deleted commands/queries/DTOs removed. PaymentOrder and RevenueReceipt code updated to use enum. PermissionCodes cleaned.

---

## Phase 4: Infrastructure Layer — Delete Configurations, Seed; Update Remaining; Update DbContext

**Purpose**: Remove Infrastructure artifacts for deleted entities and update remaining configurations

### Delete EF configurations for deleted entities

- [x] T036 [P] [US1] Delete AdvancePaymentConfiguration in src/Infrastructure/Data/Configurations/Payments/AdvancePaymentConfiguration.cs
- [x] T037 [P] [US1] Delete PaymentExecutionConfiguration in src/Infrastructure/Data/Configurations/Payments/PaymentExecutionConfiguration.cs
- [x] T038 [P] [US1] Delete PaymentAllocationConfiguration in src/Infrastructure/Data/Configurations/Payments/PaymentAllocationConfiguration.cs
- [x] T039 [P] [US1] Delete PaymentMethodConfiguration in src/Infrastructure/Data/Configurations/Payments/PaymentMethodConfiguration.cs
- [x] T040 [P] [US1] Delete PaymentMethodSeedData in src/Infrastructure/Data/Seeds/PaymentMethodSeedData.cs

### Update PaymentOrderConfiguration

- [x] T041 [US2] Update PaymentOrderConfiguration: drop PaymentMethodId index, add PaymentMethod int column configuration in src/Infrastructure/Data/Configurations/Payments/PaymentOrderConfiguration.cs

### Update RevenueReceiptConfiguration

- [x] T042 [US2] Update RevenueReceiptConfiguration: drop PaymentMethodId index, add PaymentMethod int column configuration in src/Infrastructure/Data/Configurations/Revenue/RevenueReceiptConfiguration.cs

### Update DbContext and Interface

- [x] T043 [US1] Remove 4 DbSet lines (PaymentMethods, PaymentExecutions, PaymentAllocations, AdvancePayments) from src/Infrastructure/Data/ApplicationDbContext.cs
- [x] T044 [US1] Remove 4 DbSet lines from src/Application/Common/Interfaces/IApplicationDbContext.cs

**Checkpoint**: Infrastructure layer clean. All deleted configurations/seed removed. DbContext updated. Remaining configurations updated for enum column.

---

## Phase 5: Web Layer — Delete Endpoints; Update DI

**Purpose**: Remove Web endpoint files and clean up permission policies

### Delete endpoint files

- [x] T045 [P] [US1] Delete AdvancePayments endpoint in src/Web/Endpoints/Payments/AdvancePayments.cs
- [x] T046 [P] [US1] Delete PaymentExecutions endpoint in src/Web/Endpoints/Payments/PaymentExecutions.cs
- [x] T047 [P] [US1] Delete PaymentAllocations endpoint in src/Web/Endpoints/Payments/PaymentAllocations.cs
- [x] T048 [P] [US1] Delete PaymentMethods endpoint in src/Web/Endpoints/Payments/PaymentMethods.cs

### Update DependencyInjection.cs

- [x] T049 [US1] Remove 19 AddPolicy lines for PaymentMethods, PaymentExecutions, PaymentAllocations, AdvancePayments permissions from src/Web/DependencyInjection.cs

**Checkpoint**: Web layer clean. All deleted endpoints removed. Permission policies cleaned.

---

## Phase 6: EF Core Migration — Schema Changes, Data Backfill, Table Drops (US1 + US2 + US3)

**Purpose**: Create the EF Core migration that handles all schema changes, data backfill, deletion reports, audit logging, and table drops

**?? CRITICAL**: This is the core migration task. All previous phases MUST be complete before scaffolding.

- [x] T050 [US1] Scaffold EF Core migration: add nullable PaymentMethod int column to PaymentOrders and RevenueReceipts in src/Infrastructure/Migrations/
- [x] T051 [US2] Add raw SQL to Up() for backfill: UPDATE PaymentOrders/RevenueReceipts SET PaymentMethod = CASE pm.Code mapping FROM PaymentMethods lookup
- [x] T052 [US2] Add raw SQL to Up() for unmapped value check: THROW if NULL PaymentMethod remains after backfill
- [x] T053 [US1] Add raw SQL to Up() for PaymentMethodMigrationReport: CREATE TABLE and INSERT warning rows for deactivated/unmapped records
- [x] T054 [US1] Add raw SQL to Up() to drop PaymentMethodId indexes and columns from PaymentOrders and RevenueReceipts
- [x] T055 [US1] Add raw SQL to Up() to alter PaymentMethod columns to NOT NULL with default 5 (Other)
- [x] T056 [US1] Add raw SQL to Up() to drop tables in FK order: PaymentAllocations, PaymentExecutions, PaymentMethods, AdvancePayments
- [x] T057 [US1] Add raw SQL to Up() to delete DocumentSequence rows for PaymentExecution and AdvancePayment
- [x] T058 [US1] Add raw SQL to Up() to delete SecurityPermission rows (19 permissions)
- [x] T059 [US3] Add raw SQL to Up() to create AdvancePaymentDeletionReport: CREATE TABLE + INSERT SELECT from AdvancePayments before drop
- [x] T060 [US3] Add raw SQL to Up() to create PaymentExecutionDeletionReport: CREATE TABLE + INSERT SELECT from PaymentExecutions before drop
- [x] T061 [US3] Add raw SQL to Up() to insert SecurityAuditLog entries for each table deletion (4 entries: entity=System, action=SchemaDeletion)
- [x] T062 [US1] Add XML documentation to migration class documenting all steps and reversibility

**Checkpoint**: Migration Up() method complete. All schema changes, data backfill, reports, audit, and table drops handled.

---

## Phase 7: Migration Reversibility — Down() Method (US5)

**Purpose**: Implement the Down() migration method for full reversibility

- [x] T063 [US5] Add raw SQL to Down() to recreate PaymentMethods table with original schema and seed data
- [x] T064 [US5] Add raw SQL to Down() to recreate AdvancePayments table with original schema
- [x] T065 [US5] Add raw SQL to Down() to recreate PaymentExecutions table with original schema
- [x] T066 [US5] Add raw SQL to Down() to recreate PaymentAllocations table with original schema
- [x] T067 [US5] Add raw SQL to Down() to restore PaymentMethodId columns and backfill from PaymentMethods lookup
- [x] T068 [US5] Add raw SQL to Down() to restore DocumentSequence rows for PaymentExecution and AdvancePayment
- [x] T069 [US5] Add raw SQL to Down() to restore 19 SecurityPermission rows
- [x] T070 [US5] Add raw SQL to Down() to drop artifact tables (AdvancePaymentDeletionReport, PaymentExecutionDeletionReport, PaymentMethodMigrationReport)
- [x] T071 [US5] Add raw SQL to Down() to drop PaymentMethod enum columns from PaymentOrders and RevenueReceipts

**Checkpoint**: Migration fully reversible. Down() restores original schema, data, permissions, and sequences.

---

## Phase 8: Verification (US4)

**Purpose**: Verify build, tests, and code reference cleanup

- [x] T072 [US4] Run `dotnet build ERP-Government.slnx --warnaserrors` and verify zero warnings, zero errors
- [x] T073 [US4] Run `dotnet test ERP-Government.slnx` and verify all remaining tests pass
- [x] T074 [US4] Grep src/ for references to AdvancePayment, PaymentExecution, PaymentAllocation, PaymentMethod entity class — verify zero matches (excluding migration artifacts and new enum)
- [x] T075 [US4] Verify PaymentOrderDto and RevenueReceiptDto expose PaymentMethod as enum (not int FK)
- [x] T076 [US4] Verify SecurityPermissions table has zero rows for deleted permission codes
- [x] T077 [US4] Verify PaymentOrder.* and RevenueReceipt.* permissions unchanged in SecurityPermissions table

**Checkpoint**: SC-001 through SC-007 all verified. Feature complete.

---

## Phase 9: Polish

**Purpose**: Final cleanup and documentation

- [x] T078 [P] Run quickstart.md validation scenarios V1-V10
- [x] T079 [P] Verify no unused `using` statements remain in modified files
- [x] T080 [P] Verify no dead code references to deleted entities in remaining PaymentOrder/RevenueReceipt files

---

## Dependencies & Execution Order

### Phase Dependencies

- **Phase 1 (Setup)**: No dependencies — can start immediately
- **Phase 2 (Domain)**: Depends on Phase 1 completion — BLOCKS all subsequent phases
- **Phase 3 (Application)**: Depends on Phase 2 completion (needs enum, updated entity)
- **Phase 4 (Infrastructure)**: Depends on Phase 2 completion (needs enum, updated entity)
- **Phase 5 (Web)**: Depends on Phase 3 completion (needs PermissionCodes cleaned)
- **Phase 6 (Migration)**: Depends on Phases 2-5 completion — all code must be clean before scaffolding
- **Phase 7 (Reversibility)**: Depends on Phase 6 completion (Down() mirrors Up())
- **Phase 8 (Verification)**: Depends on Phase 7 completion
- **Phase 9 (Polish)**: Depends on Phase 8 completion

### User Story Dependencies

- **US1 (P1) + US2 (P1)**: Tightly coupled — executed together across Phases 2-6
- **US3 (P2)**: Part of migration (Phase 6) — depends on US1 table drop order
- **US4 (P1)**: Verification phase — depends on all implementation complete
- **US5 (P3)**: Migration Down() — depends on Phase 6 Up() complete

### Parallel Opportunities

- Phase 2: T003-T010 (all delete tasks) can run in parallel
- Phase 3: T014-T024 (all delete tasks) can run in parallel
- Phase 4: T036-T040 (all delete tasks) can run in parallel
- Phase 5: T045-T048 (all delete tasks) can run in parallel
- Phase 3 and Phase 4 can run in parallel (different layers, no cross-dependencies)
- Phase 9: T078-T080 can run in parallel

---

## Implementation Strategy

### MVP First (US1 + US2 + US4)

1. Complete Phase 1: Setup (decision records)
2. Complete Phase 2: Domain layer cleanup
3. Complete Phase 3: Application layer cleanup
4. Complete Phase 4: Infrastructure layer cleanup
5. Complete Phase 5: Web layer cleanup
6. Complete Phase 6: EF Core migration (Up method)
7. Complete Phase 8: Verification (build + test)
8. **STOP and VALIDATE**: System compiles, tests pass, no references to deleted entities

### Incremental Delivery

1. Phases 1-5: Code cleanup (all layers)
2. Phase 6: Migration Up() — schema changes + data backfill + table drops
3. Phase 8: Verification — build + test + grep
4. Phase 7: Migration Down() — reversibility
5. Phase 9: Polish — final validation

### Critical Path

```
Phase 1 → Phase 2 → Phase 3+4 (parallel) → Phase 5 → Phase 6 → Phase 8 → Phase 7 → Phase 9
```

---

## Notes

- [P] tasks = different files, no dependencies
- [Story] label maps task to specific user story for traceability
- This is a deletion/simplification feature — most tasks are file deletions, not new code
- The migration (Phase 6-7) is the most complex part and should be implemented carefully
- Commit after each phase for clean rollback points
- Stop at any checkpoint to validate independently
