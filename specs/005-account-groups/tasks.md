---
description: "Task list for ط¥ط¯ط§ط±ط© ظ…ط¬ظ…ظˆط¹ط§طھ ط§ظ„ط­ط³ط§ط¨ط§طھ (Account Groups) â€” 005-account-groups"
---

# Tasks: ط¥ط¯ط§ط±ط© ظ…ط¬ظ…ظˆط¹ط§طھ ط§ظ„ط­ط³ط§ط¨ط§طھ (Account Groups)

**Input**: Design documents from `/specs/005-account-groups/` â€” spec.md (5 stories, 25 FR, 10 SC), plan.md, research.md (D-01..D-13), data-model.md, contracts/account-groups-api.yaml, quickstart.md

**Prerequisites**: plan.md (required), spec.md (required), research.md, data-model.md, contracts/

**Tests**: Constitution XI requires every use case have unit tests (success + failure) + functional tests vs real DB (concurrency, audit, balancing/posting) + frontend Vitest. Included per story.

**Organization**: Tasks grouped by user story for independent implementation/testing. Foundational phase blocks all stories.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (US1..US5)
- Include exact file paths in descriptions

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Verify tech stack, restore, and baseline artifacts per Technical Context

- [X] T001 Verify .NET SDK 10.0.201 + Node >=20 and restore solutions per plan.md â€” run `dotnet restore ERP-Government.slnx` and `dotnet ef database update --project src/Infrastructure --startup-project src/Web` from repo root
- [X] T002 [P] Verify frontend baseline in `src/Web/ClientApp` â€” run `npm ci`, `npm run generate-api` (nswag), `npm run generate-tokens` per quickstart.md
- [X] T003 [P] Inventory existing permissions `Accounting.ChartOfAccounts.Read/Create/Edit` in `src/Application/Common/Security/PermissionCodes.cs` and frontend `src/Web/ClientApp/src/shared/constants/permissions.ts` â€” ensure mapping exists for all three

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Shared contracts, helpers, and audit/security baselines that all stories depend on (Principle I/VI/VII/VIII/X)

**âڑ ï¸ڈ CRITICAL**: No user story work can begin until this phase is complete

- [X] T004 Expand `AccountGroupDto` in `src/Application/Accounting/Common/AccountingDtos.cs` to include `ParentId`, `Level`, `Description`, `RowVersion`, `AncestorPath`/`Children` support and update `Mapping` profile for new fields
- [X] T005 [P] Create `AccountGroupHierarchyHelper` in `src/Application/Accounting/Common/AccountGroupHierarchyHelper.cs` with `ComputeLevel()`, `ValidateMaxDepth5()`, `DetectCycleAsync()`, `ValidateTypeInheritance()` per research D-02..D-04
- [X] T006 [P] Create `AccountGroupGuardHelper` in `src/Application/Accounting/Common/AccountGroupGuardHelper.cs` with `CheckUniqueCodePermanentAsync()` (normalized case-insensitive), `CheckDeepDeactivateGuardAsync()` (CTE/recursive subtree + Accounts check per D-09), `CheckPostedEntriesGuardAsync()` (MoveLineâ†’Move posted check per D-05)
- [X] T007 Update AutoMapper + FluentValidation shared setup â€” ensure `RowVersion` excluded from audit per `AuditTrailChangeTracker.ExcludedProperties` already verified, no code change if verified in `src/Infrastructure/Data/Interceptors/AuditTrailChangeTracker.cs`
- [X] T008 Verify audit immutability: confirm triggers `src/Infrastructure/Migrations/Triggers/AuditTrails_Immutable.sql` and `SecurityAuditLogs_Immutable.sql` enforce INSERT-ONLY, and `AuditableEntityInterceptor` captures `Created/LastModified` â€” document pass in research note
- [X] T009 [P] Verify frontend design-system baseline: `src/Web/ClientApp/src/design-system/tokens.css.scss`, `tokens.ts`, `rtl.scss`, `src/Web/ClientApp/src/components/ui/DataGrid.tsx`, `PageHeader.tsx`, `ConfirmDialog.tsx`, `ConflictDialog.tsx`, `AuditTimeline.tsx`, `StatusBadge.tsx`, `FilterBar.tsx` â€” list available props for reuse per plan.md X
- [X] T010 Add composable error helper for 409/400 Arabic problem-details in `src/Web/ClientApp/src/lib/utils.ts` or `src/Web/Endpoints/Accounting/AccountGroups.cs` result mapping (ensure `DbUpdateConcurrencyException â†’ 409` with `current` payload per contracts)
- [X] T011 Setup test fixtures: extend `tests/Application.FunctionalTests/FunctionalTestSetup.cs` + `Respawn` reset helper for `AccountGroups`/`Accounts`/`Moves` seeding, and `tests/Application.UnitTests/Accounting/AccountGroups/` baseline mocks for `IApplicationDbContext`

**Checkpoint**: Foundation ready â€” DTOs expanded, helpers available, audit/security/RTL baselines verified â€” user stories can begin in parallel

---

## Phase 3: User Story 1 â€” ط¹ط±ط¶ ط§ظ„ظ‚ط§ط¦ظ…ط© ط§ظ„ظ‡ط±ظ…ظٹط© ظˆط§ظ„ط¨ط­ط« ظˆط§ظ„ظپظ„طھط±ط© (Priority: P1) ًںژ¯ MVP

**Goal**: ط¹ط±ط¶ ط´ط¬ط±ط© ظ‡ط±ظ…ظٹط© ظ„ظ„ط¬ط°ظˆط± ظ…ط¹ طھط±ظ‚ظٹظ… ط¹ظ„ظ‰ ط§ظ„ط¬ط°ظˆط± (ط¨ط¯ظˆظ† ط¨ط­ط«) ظˆظ‚ط§ط¦ظ…ط© ظ…ط³ط·ط­ط© ظ…ظپظ„طھط±ط© ظ…ط¹ breadcrumb ط¹ظ†ط¯ ط§ظ„ط¨ط­ط«/ط§ظ„ظپظ„طھط±ط©طŒ ظ…ط¹ ظپظ„طھط±ط© ط¨ط§ظ„ظ†ظˆط¹ ظˆط§ظ„ط­ط§ظ„ط© ظˆطھط±ظ‚ظٹظ… طµظپط­ط§طھ 20 â€” FR-001..004

**Independent Test**: ط§ظپطھط­ `/accounting/account-groups` ط¨طµظ„ط§ط­ظٹط© Read â†’ ط´ط¬ط±ط© ط¬ط°ظˆط± ط¨طھط±ظ‚ظٹظ…طŒ ط§ط¨ط­ط« "101" â†’ ظ…ط³ط·ط­ط© ظ…ط¹ breadcrumbطŒ ظپظ„طھط± Asset+ظ†ط´ط· â†’ طھط±ظ‚ظٹظ… ظ…ط­ظپظˆط¸. ط±ظپط¶ ط¨ط¯ظˆظ† طµظ„ط§ط­ظٹط© â†’ 403. ط±ط§ط¬ط¹ quickstart scenario 1+2.

### Tests for User Story 1 âڑ ï¸ڈ

> **NOTE: Write these tests FIRST, ensure they FAIL before implementation**

- [ ] T012 [P] [US1] Functional test tree mode (no search â†’ mode tree, roots paginated, children nested) in `tests/Application.FunctionalTests/Accounting/AccountGroupsListFunctionalTests.cs`
- [ ] T013 [P] [US1] Functional test flat mode (search/type/isActive â†’ mode flat, ancestorPath, pagination preserves filters) in same file as T012
- [ ] T014 [P] [US1] Frontend Vitest for list page dual-mode in `src/Web/ClientApp/src/features/accounting/__tests__/AccountGroupsListPage.test.tsx` (mock hooks, assert DataGrid vs GroupTree, RTL)

### Implementation for User Story 1

- [X] T015 [P] [US1] Update `GetAccountGroupsListQuery` request model in `src/Application/Accounting/Queries/AccountGroups/GetAccountGroupsList/GetAccountGroupsListQuery.cs` â€” add `Search` (string?), `Page` (int default 1), `PageSize` (int default 20), keep `Type`, `IsActive`, add `[Authorize(Policy=PermissionCodes.ChartOfAccountsRead)]`
- [X] T016 [US1] Implement dual-mode handler in same file: tree mode (Where ParentId==null, OrderBy Code, Skip/Take roots + children query) vs flat mode (Where Code/Name Contains normalized, Type/IsActive, OrderBy Code, Skip/Take + ancestorPath via `AccountGroupHierarchyHelper.BuildAncestorPathAsync`) â€” map to `PaginatedAccountGroupsResponse` per contracts
- [X] T017 [US1] Update endpoint in `src/Web/Endpoints/Accounting/AccountGroups.cs` `MapGet("/")` to return `PaginatedAccountGroupsResponse` with `mode` field and `RequireAuthorization("Accounting.ChartOfAccounts.Read")` â€” regenerate OpenAPI via `nswag`
- [X] T018 [P] [US1] Create frontend types in `src/Web/ClientApp/src/features/accounting/types.ts` (extend shared) + update `contracts/types.ts` mirror per contracts/account-groups-api.yaml
- [X] T019 [P] [US1] Create hook `useAccountGroupsList` in `src/Web/ClientApp/src/features/accounting/hooks/useAccountGroupsList.ts` (TanStack Query, params: search/type/isActive/page/pageSize, queryKey reflects all, staleTime 30s)
- [X] T020 [US1] Create `AccountGroupsListPage` in `src/Web/ClientApp/src/features/accounting/pages/AccountGroupsListPage.tsx` + `GroupTree` in `components/GroupTree.tsx` using `DataGrid`, `PageHeader`, `FilterBar/FilterSelect`, `Input`, `StatusBadge`, `usePermission(PERMISSIONS.ChartOfAccounts.Read)`, logical CSS (inline-start), tokens via `design-system`, dark via `next-themes`, Arabic placeholders
- [X] T021 [US1] Wire route + NAV permission in `src/Web/ClientApp/src/AppRoutes.jsx` and `src/Web/ClientApp/src/shared/constants/permissions.ts`/`layouts` nav config for `accounting/account-groups` with `permission: PERMISSIONS.ChartOfAccounts.Read`

**Checkpoint**: US1 fully functional â€” tree roots paginated without search, flat paginated with search/filters, 403 without permission, UI RTL/dark/tokens pass, tests fail-before-pass proven

---

## Phase 4: User Story 2 â€” ط¥ظ†ط´ط§ط، ظ…ط¬ظ…ظˆط¹ط© ط­ط³ط§ط¨ط§طھ ط¬ط¯ظٹط¯ط© (Priority: P1)

**Goal**: ط¥ظ†ط´ط§ط، ظ…ط¬ظ…ظˆط¹ط© ط¨ظƒظˆط¯ ظپط±ظٹط¯ ط¯ط§ط¦ظ… 20/ط§ط³ظ… 200/ظ†ظˆط¹/ط±طµظٹط¯ ظ…طھظˆط§ظپظ‚/ظˆطµظپ 500/ط£ط¨ ط§ط®طھظٹط§ط±ظٹ ظ…ط¹ Level autoطŒ ظ…ظ†ط¹ طھظƒط±ط§ط±/ط¹ط¯ظ… طھظˆط§ظپظ‚/ط¹ظ…ظ‚>5/ط£ط¨ ظ…ط¹ط·ظ„/ظ†ظˆط¹ ظ…ط®ط§ظ„ظپ â€” FR-005..009 + FR-024

**Independent Test**: ظ…ظ„ط، ظ†ظ…ظˆط°ط¬ ط¥ظ†ط´ط§ط، (AST-100 Asset Debit root Level1 IsActive true â†’ success + audit)طŒ ط§ط¨ظ† Level3 + validation errors (طھظƒط±ط§ط±â†’400طŒ Credit ظ„ظ€ Assetâ†’400طŒ ط£ط¨ ظ…ط¹ط·ظ„â†’400). quickstart scenario 3.

### Tests for User Story 2 âڑ ï¸ڈ

- [ ] T022 [P] [US2] Unit tests `CreateAccountGroupCommandHandler` success + failure paths in `tests/Application.UnitTests/Accounting/AccountGroups/CreateAccountGroupTests.cs` (duplicate permanent, matrix Assetâ†’Debit, type inheritance, depth5, inactive parent)
- [ ] T023 [P] [US2] Validator tests for `CreateAccountGroupCommandValidator` (trim, max lengths, enum, matrix) in same folder as T022
- [ ] T024 [P] [US2] Functional test POST `/api/account-groups` create + audit row in `tests/Application.FunctionalTests/Accounting/CreateAccountGroupFunctionalTests.cs` (incl. permanent unique race via DbUpdateException mapping)
- [ ] T025 [P] [US2] Frontend form Vitest in `src/Web/ClientApp/src/features/accounting/__tests__/AccountGroupFormCreate.test.tsx` (zod validation, Arabic errors, ParentId select disabled for inactive)

### Implementation for User Story 2

- [X] T026 [US2] Enhance `CreateAccountGroupCommand` + Handler in `src/Application/Accounting/Commands/AccountGroups/CreateAccountGroup/CreateAccountGroupCommand.cs` â€” add `ParentId`, inject `AccountGroupHierarchyHelper` + `AccountGroupGuardHelper`, compute Level, enforce normalized permanent unique (handler AnyAsync + catch DbUpdateExceptionâ†’400), type inheritance equality, matrix, depth5, inactive-parent block, trim, `IsActive=true`, `AddDomainEvent` optional
- [X] T027 [US2] Enhance `CreateAccountGroupCommandValidator` in same file: `Code` NotEmpty Max20 Trim, `Name` NotEmpty Max200, `Type` IsInEnum, `NormalBalance` IsInEnum, conditional matrix message "ط§ظ„ط±طµظٹط¯ ط§ظ„ط·ط¨ظٹط¹ظٹ ط؛ظٹط± ظ…طھظˆط§ظپظ‚ ظ…ط¹ ط§ظ„ظ†ظˆط¹", `Description` Max500, `ParentId` >0 if set
- [X] T028 [US2] Verify/adjust POST endpoint in `src/Web/Endpoints/Accounting/AccountGroups.cs` `MapPost("/")` returns `{id}` 200 + `RequireAuthorization("Accounting.ChartOfAccounts.Create")` + 400/403/409 produces per contracts (use-case `[Authorize(PermissionCodes.ChartOfAccountsCreate)]` already present)
- [X] T029 [P] [US2] Create hook `useCreateAccountGroup` in `src/Web/ClientApp/src/features/accounting/hooks/useCreateAccountGroup.ts` (useMutation, invalidates `useAccountGroupsList`, sonner toast Arabic success)
- [X] T030 [US2] Create `AccountGroupForm` component in `src/Web/ClientApp/src/features/accounting/components/AccountGroupForm.tsx` using `react-hook-form` + `zod` + `shadcn` `Input/Select/Textarea`, ParentId async select (active groups only, filter by type), Level read-only preview, RTL logical props, tokens, `ConfirmDialog` on submit, error mapping from problem-details

**Checkpoint**: US1+US2 both work â€” creation respects permanent unique, matrix, depth, inheritance, inactive-parent, Level auto, audit; UI create flow <60s

---

## Phase 5: User Story 3 â€” طھط¹ط¯ظٹظ„ ظ…ط¬ظ…ظˆط¹ط© ظ…ط¹ ظ…ظ†ط¹ ط§ظ„ط­ظ„ظ‚ط§طھ ظˆط§ظ„طھط²ط§ظ…ظ† ط§ظ„ظ…طھظپط§ط¦ظ„ (Priority: P1)

**Goal**: طھط¹ط¯ظٹظ„ Name/Type/NormalBalance/Description/ParentId ظ…ط¹ ط¥ط¹ط§ط¯ط© ط­ط³ط§ط¨ Level ظ„ظ„ط´ط¬ط±ط© ط§ظ„ظپط±ط¹ظٹط©طŒ ظ…ظ†ط¹ ط­ظ„ظ‚ط§طھ (Parent==self ط£ظˆ descendant), ظ…ظ†ط¹ ط¹ظ…ظ‚>5 ظ„ظ„ط´ط¬ط±ط©, ظ…ظ†ط¹ ظ†ظˆط¹/ط±طµظٹط¯ ظ…ط¹ ظ‚ظٹظˆط¯ ظ…ط±ط­ظ„ط©, RowVersion 409 â€” FR-010..012 + FR-008 posted guard

**Independent Test**: طھط¹ط¯ظٹظ„ B طھط­طھ root ط¢ط®ط± â†’ Level recalc ظ„ظ€ C, ظ†ظ‚ظ„ A طھط­طھ B â†’ 400 ط­ظ„ظ‚ط©, concurrent RowVersion â†’ 409 ConflictDialog, Type change ظ…ط¹ ظ‚ظٹظˆط¯ ظ…ط±ط­ظ„ط© â†’ 400, ط¹ظ…ظ‚ 6â†’400. quickstart scenario 4.

### Tests for User Story 3 âڑ ï¸ڈ

- [ ] T031 [P] [US3] Unit tests `UpdateAccountGroupCommandHandler` in `tests/Application.UnitTests/Accounting/AccountGroups/UpdateAccountGroupTests.cs` (success reparent + subtree recalc, cycle direct/indirect, depth overflow, type inheritance fail, matrix fail, posted guard block, concurrency)
- [ ] T032 [P] [US3] Functional tests PUT `/api/account-groups/{id}` + concurrency 409 with `current` payload + posted-guard in `tests/Application.FunctionalTests/Accounting/UpdateAccountGroupFunctionalTests.cs` (real DB, Respawn reset)
- [ ] T033 [P] [US3] Frontend Vitest edit + ConflictDialog in `src/Web/ClientApp/src/features/accounting/__tests__/AccountGroupEdit.test.tsx` (rowVersion stale â†’ 409 modal + refresh)

### Implementation for User Story 3

- [X] T034 [US3] Enhance `UpdateAccountGroupCommand` request in `src/Application/Accounting/Commands/AccountGroups/UpdateAccountGroup/UpdateAccountGroupCommand.cs` â€” add `ParentId`, ensure `RowVersion` required
- [X] T035 [US3] Implement handler logic in same file: load entity + parent, `SetOriginal RowVersion`, check cycle via `DetectCycleAsync`, validate type inheritance equality, validate matrix, check posted-guard via `CheckPostedEntriesGuardAsync(subtreeIds)`, compute newLevel + subtree max depth overflow, trim Name/Description, recalc Level for entity + all descendants via `GetSubtreeIds` + batch update inside transaction, `await SaveChanges` catch `DbUpdateConcurrencyException â†’ Result.Conflict`
- [X] T036 [US3] Enhance `UpdateAccountGroupCommandValidator` in same file: `Id>0`, `Name` NotEmpty Max200, `Type` IsInEnum, `NormalBalance` IsInEnum, `RowVersion` NotEmpty, `ParentId` existence validated in handler (DB)
- [X] T037 [US3] Update endpoint `MapPut("/{id:int}")` in `src/Web/Endpoints/Accounting/AccountGroups.cs` to map `Result.Conflict â†’ 409` with `current` AccountGroup DTO per contracts, `Result.Failure â†’ 400` problem-details Arabic, ensure `[Authorize(PermissionCodes.ChartOfAccountsEdit)]` on use-case + `RequireAuthorization(Edit)` on endpoint
- [X] T038 [P] [US3] Create hook `useUpdateAccountGroup` in `src/Web/ClientApp/src/features/accounting/hooks/useUpdateAccountGroup.ts` (useMutation, on 409 show ConflictDialog payload)
- [X] T039 [US3] Enhance `AccountGroupForm` edit mode in `src/Web/ClientApp/src/features/accounting/components/AccountGroupForm.tsx` + `ConflictDialog` integration (display Arabic message "طھظ… طھط¹ط¯ظٹظ„ ط§ظ„ط¨ظٹط§ظ†ط§طھ ظ…ظ† ظ‚ط¨ظ„ ظ…ط³طھط®ط¯ظ… ط¢ط®ط±" + current values + refresh button) + ParentId select excludes self/descendants

**Checkpoint**: US1..US3 functional â€” reparent recalc correct, cycles/depth/type/posted blocked 100%, concurrency 409 with current, audit diffs recorded

---

## Phase 6: User Story 4 â€” طھط¹ط·ظٹظ„ / طھظپط¹ظٹظ„ ظ…ط¬ظ…ظˆط¹ط© (ط¨ط¯ظˆظ† ط­ط°ظپ ظپظٹط²ظٹط§ط¦ظٹ) (Priority: P2)

**Goal**: طھط¹ط·ظٹظ„ ظٹظ…ظ†ط¹ ط§ظ„ط§ط®طھظٹط§ط± ط¹ظ†ط¯ ط¥ظ†ط´ط§ط، ط­ط³ط§ط¨ ظ„ظƒظ† ظ„ط§ ظٹط­ط°ظپطŒ ظ…ط¹ ظپط­طµ ط¹ظ…ظٹظ‚ (ظƒظ„ ط§ظ„ط£ط­ظپط§ط¯ ط§ظ„ظ†ط´ط·ط© + ظƒظ„ ط­ط³ط§ط¨ط§طھ ط§ظ„ط´ط¬ط±ط©) ظٹظ…ظ†ط¹ ط§ظ„طھط¹ط·ظٹظ„ ط­طھظ‰ طھط¹ط·ظٹظ„ ط§ظ„ظپط±ظˆط¹/ط§ظ„ط­ط³ط§ط¨ط§طھطŒ طھظپط¹ظٹظ„ ظٹظ†ط¬ط­ ط¯ط§ط¦ظ…ظ‹ط§طŒ RowVersionطŒ ظ„ط§ DELETE â€” FR-013..015 + FR-024

**Independent Test**: ط­ط§ظˆظ„ طھط¹ط·ظٹظ„ R ظˆظ‡ظˆ ط£ط¨ ظ„ظ€ C ظ†ط´ط· + ط­ط³ط§ط¨ ظ†ط´ط· â†’ 400 "ط¹ط·ظ‘ظ„ ط§ظ„ظپط±ظˆط¹/ط§ظ„ط­ط³ط§ط¨ط§طھ ط£ظˆظ„ط§ظ‹", ط¹ط·ظ‘ظ„ C+ط§ظ„ط­ط³ط§ط¨ ط«ظ… R â†’ 204 Inactive, ط¥ظ†ط´ط§ط، ط§ط¨ظ† طھط­طھ ط£ط¨ ظ…ط¹ط·ظ„ â†’ 400, طھظپط¹ظٹظ„ Râ†’204, ط¨ط¯ظˆظ† Editâ†’403. quickstart scenario 5.

### Tests for User Story 4 âڑ ï¸ڈ

- [ ] T040 [P] [US4] Unit tests `ToggleAccountGroupActiveCommandHandler` in `tests/Application.UnitTests/Accounting/AccountGroups/ToggleAccountGroupActiveTests.cs` (deactivate blocked by active descendant, blocked by account in subtree, success deactivate, success activate, RowVersion conflict)
- [ ] T041 [P] [US4] Functional tests `POST /api/account-groups/{id}/toggle-active` in `tests/Application.FunctionalTests/Accounting/ToggleAccountGroupActiveFunctionalTests.cs` (deep CTE guard, audit Deactivate/Activate entries, no DELETE endpoint exists)
- [ ] T042 [P] [US4] Frontend Vitest toggle flow in `src/Web/ClientApp/src/features/accounting/__tests__/ToggleActive.test.tsx` (ConfirmDialog â†’ mutate â†’ StatusBadge update)

### Implementation for User Story 4

- [X] T043 [US4] Create `ToggleAccountGroupActiveCommand` + Handler + Validator in `src/Application/Accounting/Commands/AccountGroups/ToggleAccountGroupActive/ToggleAccountGroupActiveCommand.cs` â€” params `Id, IsActive, RowVersion`, `[Authorize(Policy=PermissionCodes.ChartOfAccountsEdit)]`, handler: load entity + RowVersion original, if `!IsActive` â†’ `CheckDeepDeactivateGuardAsync(subtreeIds)` fail 400, if `IsActive` creation-under-inactive not here but documented, set `IsActive`, SaveChanges catch concurrency â†’ 409, interceptor auto audit
- [X] T044 [US4] Add endpoint `MapPost("/{id:int}/toggle-active")` in `src/Web/Endpoints/Accounting/AccountGroups.cs` with `ToggleActiveRequest` body, `RequireAuthorization("Accounting.ChartOfAccounts.Edit")`, maps 400/404/409 problem-details Arabic
- [X] T045 [P] [US4] Create hook `useToggleAccountGroupActive` in `src/Web/ClientApp/src/features/accounting/hooks/useToggleAccountGroupActive.ts` (useMutation, optimistic StatusBadge, toast Arabic, invalidates list+detail)
- [X] T046 [US4] Wire toggle UI in `src/Web/ClientApp/src/features/accounting/pages/AccountGroupsListPage.tsx` + `AccountGroupDetailPage.tsx` â€” `Button` Power/PowerOff + `ConfirmDialog` (Arabic title "طھط£ظƒظٹط¯ ط§ظ„طھط¹ط·ظٹظ„") + `StatusBadge` (ظ†ط´ط·/ظ…ط¹ط·ظ„ via tokens) + `usePermission` gating

**Checkpoint**: US1..US4 functional â€” deep deactivate 100% blocked, activate always, no physical delete, toggle audit, UI confirm

---

## Phase 7: User Story 5 â€” ط¹ط±ط¶ طھظپط§طµظٹظ„ ط§ظ„ظ…ط¬ظ…ظˆط¹ط© ظ…ط¹ ط§ظ„ظپط±ظˆط¹ ظˆط§ظ„ط­ط³ط§ط¨ط§طھ ظˆط³ط¬ظ„ ط§ظ„طھط¯ظ‚ظٹظ‚ (Priority: P2)

**Goal**: طµظپط­ط© طھظپط§طµظٹظ„ طھط¹ط±ط¶ ط§ظ„ط¨ط·ط§ظ‚ط© ط§ظ„ط£ط³ط§ط³ظٹط© + ظپط±ظˆط¹ ظ…ط¨ط§ط´ط±ط© + ط­ط³ط§ط¨ط§طھ ظ…ط±طھط¨ط·ط© + ط³ط¬ظ„ طھط¯ظ‚ظٹظ‚ INSERT-ONLY ط¨طھط±طھظٹط¨ طھظ†ط§ط²ظ„ظٹ â€” FR-016..017

**Independent Test**: ط§ظپطھط­ `/accounting/account-groups/{id}` â†’ 4 ط£ظ‚ط³ط§ظ… ط¯ظ‚ظٹظ‚ط©, ط¨ظ„ط§ ظپط±ظˆط¹/ط­ط³ط§ط¨ط§طھ â†’ ط±ط³ط§ط¦ظ„ ظپط§ط±ط؛ط©, ظ†ظ‚ط± ظپط±ط¹ â†’ طھظپط§طµظٹظ„ظ‡, ظ…ط­ط§ظˆظ„ط© UPDATE AuditTrail ظ…ط¨ط§ط´ط± â†’ trigger fail, ط¨ط¯ظˆظ† Readâ†’403. quickstart scenario 6.

### Tests for User Story 5 âڑ ï¸ڈ

- [ ] T047 [P] [US5] Functional test `GET /api/account-groups/{id}` detail in `tests/Application.FunctionalTests/Accounting/GetAccountGroupDetailFunctionalTests.cs` (includes children, linked accounts IsActive/IsPostable, audit ordered desc, empty states, 404)
- [ ] T048 [P] [US5] Frontend Vitest detail page in `src/Web/ClientApp/src/features/accounting/__tests__/AccountGroupDetailPage.test.tsx` (renders 4 sections, empty messages, AuditTimeline, nav to child)

### Implementation for User Story 5

- [X] T049 [US5] Create `GetAccountGroupDetailQuery` + Handler in `src/Application/Accounting/Queries/AccountGroups/GetAccountGroupDetail/GetAccountGroupDetailQuery.cs` â€” `[Authorize(Policy=PermissionCodes.ChartOfAccountsRead)]`, loads group + `RowVersion` + direct `Children` (Where ParentId==id), `Accounts.Where(AccountGroupId==id)`, `AuditTrails.Where(DocumentType=="AccountGroup" && DocumentId==id) OrderBy Timestamp desc`, maps to `AccountGroupDetailResponse` per `contracts/account-groups-api.yaml`
- [X] T050 [US5] Update endpoint `MapGet("/{id:int}")` in `src/Web/Endpoints/Accounting/AccountGroups.cs` to return `AccountGroupDetailResponse` (group+children+accounts+audit) with 404/403 handling
- [X] T051 [P] [US5] Create `AccountGroupDetailPage` in `src/Web/ClientApp/src/features/accounting/pages/AccountGroupDetailPage.tsx` using `PageHeader`, `StatusBadge`, `AuditTimeline`, `DataGrid` x2 (children + accounts), cards via tokens, logical CSS, dark, breadcrumb parent link, `useAccountGroupDetail` hook in `hooks/useAccountGroupDetail.ts`, `usePermission` gating

**Checkpoint**: All 5 stories independently functional â€” detail shows 4 sections accurately, audit INSERT-ONLY enforced via DB trigger, nav works

---

## Phase 8: Polish & Cross-Cutting Concerns

**Purpose**: Constitution X/XI compliance, performance, security, and quickstart validation across all stories

- [ ] T052 Verify frontend compliance: no hard-coded design values (grep `src/Web/ClientApp/src/features/accounting/` for hex/rgb/px without tokens), RTL logical props (inline-start/end) + `rtl.scss`, dark mode (`next-themes`) pass on all pages â€” fix in `src/Web/ClientApp/src/features/accounting/**/*.{tsx,scss}`
- [ ] T053 [P] Regenerate OpenAPI client: `npm run generate-api` in `src/Web/ClientApp` and commit updated `src/Web/ClientApp/src/web-api-client.ts` after all endpoint changes â€” ensure `AccountGroup` operations match `contracts/account-groups-api.yaml`
- [ ] T054 [P] Perform performance smoke: list <2s for 1000 roots + flat search <2s per SC-010 â€” add benchmark in `tests/Application.FunctionalTests/Accounting/AccountGroupsPerformanceTests.cs` using seeded data
- [ ] T055 [P] Security hardening verification: fail-closed on permission subsystem error + `SecurityAuditLogs` Grant/Deny rows asserted in `tests/Application.FunctionalTests/Accounting/AccountGroupsAuthorizationTests.cs`
- [ ] T056 [P] Audit immutability proof: attempt `UPDATE/DELETE AuditTrails` â†’ trigger throws, asserted in `tests/Application.FunctionalTests/Accounting/AuditImmutabilityTests.cs` + frontend `AuditTimeline` displays `fieldChanges` JSON with Arabic summary
- [ ] T057 Run `quickstart.md` scenarios 1-7 end-to-end manually via `dotnet run --project src/AppHost` + `src/Web/ClientApp` and record pass/fail checklist in `specs/005-account-groups/checklists/`
- [ ] T058 Final build: `dotnet build ERP-Government.slnx --warnaserror` + `npm run lint` in `src/Web/ClientApp` + `npm run build` â€” fix warnings as errors per Constitution build discipline

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies â€” start immediately
- **Foundational (Phase 2)**: Depends on Setup â€” **BLOCKS all user stories**
- **User Stories (Phase 3-7)**: Depend on Foundational completion; can proceed sequentially P1â†’P2 or in parallel if staffed (US1..US3 are P1 and share no files beyond Foundational helpers)
- **Polish (Phase 8)**: Depends on all desired stories (US1 minimum for MVP; US1-5 for full feature)

### User Story Dependencies

- **US1 (P1) list/search/pagination**: No story dependency â€” MVP. Can demo tree/browse alone.
- **US2 (P1) create**: Depends on Foundational helpers (validation, unique) but independent of US1 runtime; integrates visually with US1 list.
- **US3 (P1) update/reparent/cycle/concurrency/posted-guard**: Depends on Foundational helpers; independent of US1/US2 at code level (different handler) but reuses `AccountGroupHierarchyHelper`.
- **US4 (P2) toggle deep guard**: Depends on Foundational guard helper; independent testability (toggle inactive fixture).
- **US5 (P2) detail+children+accounts+audit**: Reads data created by US1-4; can be tested with seeded data without US execution.

### Within Each User Story

- Tests FIRST â†’ FAIL â†’ Implementation â†’ PASS
- Query/Command model â†’ Handler â†’ Validator â†’ Endpoint â†’ Hook â†’ Component/Page
- Commit after each task or logical group

### Parallel Opportunities

- T002 + T003 + T004? No â€” T004 depends on setup verification, but T002 and T003 are [P] within Setup.
- Foundational: T005 + T006 are [P] (different helpers); T009 frontend inventory is [P] with backend helpers.
- Once Foundational done: US1 tests T012/T013/T014 are mutually [P]; US2 tests T022-T025 are [P]; all stories can start in parallel with separate developers.
- Frontend hooks/types are [P] with backend handler tasks when different files.
- Polish: T053-T056 are [P] (regen, perf, security, audit immutability)

---

## Parallel Example: User Story 1

```bash
# All US1 tests in parallel (different files/processes):
Task: "Functional test tree mode in tests/Application.FunctionalTests/Accounting/AccountGroupsListFunctionalTests.cs"  # T012
Task: "Functional test flat mode in same file"  # T013 (same file but distinct test methods â€” run together via NUnit filter)
Task: "Frontend Vitest in src/Web/ClientApp/src/features/accounting/__tests__/AccountGroupsListPage.test.tsx"  # T014

# All US1 frontend scaffolding in parallel:
Task: "Create hook useAccountGroupsList in src/Web/ClientApp/src/features/accounting/hooks/useAccountGroupsList.ts"  # T019
Task: "Create frontend types in src/Web/ClientApp/src/features/accounting/types.ts"  # T018
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1 Setup (T001-T003) â€” 10 min
2. Complete Phase 2 Foundational (T004-T011) â€” helpers + DTOs + audit verify
3. Complete Phase 3 US1 (T012-T021) â€” dual-mode list/search/pagination + RTL/dark/tokens
4. **STOP and VALIDATE**: quickstart scenario 1-2, `dotnet test` filtered US1, Vitest US1, `npm run lint`
5. Deploy/demo MVP â€” browse/search alone delivers value per spec "Why this priority"

### Incremental Delivery

1. Setup + Foundational â†’ foundation ready
2. + US1 â†’ MVP demo (browse/search)
3. + US2 â†’ create flow live
4. + US3 â†’ edit/reparent/cycle/concurrency live (completes P1 trio)
5. + US4 â†’ deactivate/activate live
6. + US5 â†’ detail page live
7. + Polish â†’ perf/security/audit proof + OpenAPI regen

### Parallel Team Strategy

With 3 devs after Foundational:

- Dev A: US1 (list dual-mode) + US5 (detail) â€” both queries
- Dev B: US2 (create) + US4 (toggle) â€” both commands with guards
- Dev C: US3 (update/reparent/cycle/posted-guard/concurrency) â€” hardest handler + frontend ConflictDialog

Each dev owns their story's tests â†’ implementation â†’ endpoint â†’ hook â†’ page, integrates via PR per checkpoint.

---

## Notes

- [P] tasks = different files, no dependencies â€” safe for parallel
- [Story] label maps task to spec US â€” traceability to FR/SC
- Each story independently testable per its "Independent Test" â€” reres quickstart.md
- Verify tests FAIL before implementing; commit per task
- Avoid: vague tasks, same-file conflicts, cross-story file edits in parallel
- Hard-coded design values, physical left/right, missing RowVersion round-trip, missing 409 current payload are defects per Constitution
- Stop at any checkpoint to validate story independently

