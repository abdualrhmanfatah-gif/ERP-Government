# Tasks: Remove RecordRules Table

**Prerequisites**: DEP-026, spec.md, plan.md

## Phase 1: Setup

- [x] T001 Accept decision record DEP-026 (shared across specs 034–042)

## Phase 2: Domain Layer (compile break = discovery test)

- [x] T002 [P] Delete src/Domain/Security/Entities/RecordRule.cs

**Checkpoint**: Domain build broken only by context references.

## Phase 3: Context + Infrastructure

- [x] T003 [P] Remove `DbSet<RecordRule> RecordRules` from src/Application/Common/Interfaces/IApplicationDbContext.cs (line 38)
- [x] T004 [P] Remove `DbSet<RecordRule> RecordRules` from src/Infrastructure/Data/ApplicationDbContext.cs (line 40)
- [x] T005 [P] Delete src/Infrastructure/Data/Configurations/Security/RecordRuleConfiguration.cs
- [x] T006 Build src/Web/Web.csproj — green

**Checkpoint**: Build green, zero RecordRule symbols outside migrations history.

## Phase 4: Migration

- [x] T007 `dotnet ef migrations add RemoveRecordRules --project src/Infrastructure --startup-project src/Web`
- [x] T008 Edit migration: create RecordRuleDeletionReport + INSERT...SELECT + SecurityAuditLog before drop; verify Down recreates
- [x] T009 Docs: remove docs/database-tables-complete.md §2.7

## Phase 5: Gate

- [x] T010 Full 5-project suite green
- [x] T011 Merge --no-ff to main
