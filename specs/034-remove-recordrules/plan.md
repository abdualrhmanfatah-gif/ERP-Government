# Plan: Remove RecordRules Table

**Branch**: `034-remove-recordrules`
**Decision Record**: DEP-026
**Blast radius**: minimal (4 code files + migrations + 1 doc)

## Constitution Check

- **I (Layers)**: deletion order Domain → context interfaces → Infrastructure config → migration. PASS.
- **VI (Data Integrity)**: deletion report preserves pre-drop data; Down recreates schema. PASS.
- **XII (Controlled Change)**: DEP-026 accepted before implementation. PASS.
- **XI (Testing)**: no existing tests; full suite gate at merge proves no regression. PASS.

## Technical Approach

1. Delete `src/Domain/Security/Entities/RecordRule.cs` (Domain first — transient compile break is intentional discovery test).
2. Remove DbSet line from `src/Application/Common/Interfaces/IApplicationDbContext.cs` (line 38) and `src/Infrastructure/Data/ApplicationDbContext.cs` (line 40).
3. Delete `src/Infrastructure/Data/Configurations/Security/RecordRuleConfiguration.cs`.
4. Build `src/Web/Web.csproj` — must be green before migration.
5. `dotnet ef migrations add RemoveRecordRules --project src/Infrastructure --startup-project src/Web`.
6. Edit migration: prepend deletion-report creation + INSERT...SELECT + SecurityAuditLog insert; keep drops. Verify Down recreates.
7. Docs: remove `docs/database-tables-complete.md` §2.7.
8. Full 5-project suite → merge `--no-ff` to main.

## Violations / Complexity Tracking

None. No simplification markers justified.

## Verification

- `dotnet build src/Web/Web.csproj` after code deletion.
- `dotnet ef migrations script` dry-check for DROP order.
- Full suite: Domain.UnitTests, Application.UnitTests, Application.FunctionalTests, Infrastructure.IntegrationTests, Web.AcceptanceTests.
