# AGENTS.md — ERP-Government

Yemeni government ERP (Financial Law 8/1990). Spec-Driven Development (Spec Kit, .specify/).
READ THIS FIRST. Conventions below are verified as-built — follow them, do not re-derive or invent alternatives.

## Stack
.NET 10 / C# 13, EF Core + SQL Server, MediatR, FluentValidation, minimal APIs, .NET Aspire (src/AppHost + ServiceDefaults).
Frontend: React 19 + TypeScript + Vite in src/Web/ClientApp.

## Build & Verify (exact commands — NO .sln exists, always target projects directly)
- Backend build: dotnet build src/Web/Web.csproj
- Backend tests (run relevant project; FULL suite before merge):
  dotnet test tests/Domain.UnitTests
  dotnet test tests/Application.UnitTests
  dotnet test tests/Application.FunctionalTests
  dotnet test tests/Infrastructure.IntegrationTests
  dotnet test tests/Web.AcceptanceTests
- Frontend (cd src/Web/ClientApp): npm run lint | npm run test | npm run build
  (prebuild/prestart auto-run `npm run generate-api` — NSwag API client regeneration; regenerate after endpoint changes)
- EF migration: dotnet ef migrations add <Name> --project src/Infrastructure --startup-project src/Web
- TDD per-cycle: single filtered test (red) → touched project's suite only (green/refactor). 
  Full 5-project suite runs ONLY at gates: session preflight, /speckit.converge, pre-merge.
  Project scoping for cycle economy is NOT test-filtering-to-green (Hard Rule 4); no assertion
  is ever weakened, skipped, or deleted — full suite still gates every merge.

## Layer map
- src/Domain/<Feature>/Entities/ — EF entities. src/Domain/Common/ = BaseEntity (int PK), BaseAuditableEntity (+audit +RowVersion). ALWAYS inherit one.
- src/Domain/<Feature>/Enums/ — enums (stored as int).
- src/Domain/Events/<Feature>/ — domain events, inherit BaseEvent, implement IHasSourceEntity (src/Domain/Events/Common/).
- src/Application/<Feature>/Commands/<Entity>/<Action>/ — one folder per action: Command + Handler + Validator together.
- src/Application/<Feature>/Queries/<Entity>/ — same pattern.
- src/Web/Endpoints/<Feature>/<Entity>s.cs — IEndpointGroup (src/Web/Infrastructure/IEndpointGroup.cs), auto-registered via WebApplicationExtensions (MapEndpointGroups).
- src/Infrastructure/Data/ — DbContext, Migrations/ (NEVER edit an applied migration; always add new).
- src/Web/ClientApp/src/features/<domain>/ — pages/ components/ hooks/ shared/.
  API clients: NSwag-generated (npm run generate-api) + manual fetch wrappers in features/<domain>/shared/client.ts — follow whichever the feature already uses.

## Pattern Exemplars — open the exemplar first, mimic it, do not search for alternatives

| Building | Exemplar (verified as-built) |
|---|---|
| EF entity | src/Domain/Parties/Entities/Party.cs |
| Enum | src/Domain/Parties/Enums/PartyType.cs |
| Command+Handler+Validator (one file, simple CRUD) | src/Application/Parties/Commands/CreateParty/CreatePartyCommand.cs |
| Command with approval + lifecycle | src/Application/Budgeting/Commands/Appropriations/ApproveAppropriationCommand.cs |
| Query (list + filter) | src/Application/Parties/Queries/GetParties/GetPartiesQuery.cs |
| Query (by id) | src/Application/Parties/Queries/GetPartyById/GetPartyByIdQuery.cs |
| Endpoint group | src/Web/Endpoints/Parties/Parties.cs (simple) · src/Web/Endpoints/Budgeting/Appropriations.cs (lifecycle-heavy) |
| Status logging | src/Application/Parties/Common/IDocumentStatusLogger.cs |
| Document numbering | src/Application/FinancialSettings/Common/Services/ (DocumentSequenceService) |
| Availability check | src/Application/Budgeting/Common/BudgetAvailabilityService |
| Functional test | tests/Application.FunctionalTests/Budgeting/AppropriationEdgeCaseTests.cs · EncumbranceLifecycleTests.cs |
| Frontend feature folder | src/Web/ClientApp/src/features/budgeting/ (pages/components/hooks/shared) |
| Frontend test | src/Web/ClientApp/src/features/budgeting/__tests__/BudgetItemTree.test.tsx |

Maintenance: when a newer better exemplar lands (e.g. ReceiptVouchers after treasury spec, JournalEntries after 014 rename), repoint this table in the same task — never leave it stale.

## Binding conventions
- Minimal APIs only — NO controllers. Route /api/{ClassName}. Lifecycle transitions via PATCH {id}/lifecycle.
- Every handler returns Result<T> (src/Application/Common/Models/Result.cs).
- Every endpoint: [Authorize(Policy = PermissionCodes.<Code>)] (src/Application/Common/Security/PermissionCodes.cs).
- Money: decimal(23,2). NO stored computed columns — totals computed in queries/handlers.
- Document numbering: IDocumentSequenceService (src/Application/FinancialSettings/Common/Services/) — {PREFIX}-{D6}, allocated in the same transaction, number at Draft creation.
- Approvals/status: ApprovalHistory + DocumentStatusLog (append-only) via IDocumentStatusLogger — never inline approval columns.
- Ledger posting: domain event → AccountingEvent (unique SourceTable+SourceId+EventType) → PostingRules → JournalEntry + lines.
- Availability: BudgetAvailabilityService (src/Application/Budgeting/Common/) — item-level net appropriations − open encumbrances.
## Authentication & Authorization
- Auth: JWT Bearer (src/Infrastructure/DependencyInjection.cs). Key from config "Jwt:Key". No issuer/audience validation (dev config).
- Login: POST /api/Users/login → { token, userId, role }. Passwords: ASP.NET Identity PasswordHasher (User.PasswordHash). Lockout: 5 failed attempts → 15 min. Token claims: NameIdentifier, Name, Role (single role code) + permission claims loaded from RolePermissions.
- Single role per user: User.RoleId → SecurityRole (one role only — binding). Never introduce multi-role.
- Every endpoint declares its permission: .RequireAuthorization(PermissionCodes.X) on the route OR [Authorize(Policy = PermissionCodes.X)]. Codes live in src/Application/Common/Security/PermissionCodes.cs — format {Module}.{Action}. Add new codes there + register the policy in src/Web/DependencyInjection.cs.
- RBAC model: SecurityRole / RolePermission / SecurityPermission (+ UserPermission overrides); SoDMatrix, FieldSecurityPolicy, RecordRule — src/Domain/Security/Entities.
- Approval authorization: IApprovalService + IApprovalRuleEvaluationService (src/Application/Security/Common). Every approval decision recorded via ApprovalHistory only — never inline.
- Audit: SecurityAuditLog + AuditTrail (append-only).
- KNOWN DEV STATE: all policies are currently registered as RequireAssertion(_ => true) — open placeholder, RBAC enforcement wiring is pending tracked work. Do NOT treat endpoints as access-controlled yet; do NOT remove placeholder registrations inside feature specs; keep new policies consistent until the enforcement spec lands.
## Spec workflow (Spec Kit)
- Active spec pointer: .specify/feature.json. Pipeline: /speckit.specify → /speckit.plan → /speckit.tasks → /speckit.implement → /speckit.converge.
- Git: branch per spec (auto-created at specify). Merge after converge: git checkout main; git merge --no-ff <branch>.
- NEVER create a new spec for an existing feature — amend its spec.md/plan.md instead. Check specs/ before specifying.
- Before implementing: read the spec's data-model.md AND verify against current entities — specs can lag code.
- TDD mandatory (constitution XI): test first, observe red, implement, green. Never weaken/skip/delete tests.

## Docs
- DESIGN.md — Agent-facing UI design tokens + rationale (YAML front matter + prose). Agents: read DESIGN.md before any UI work; tokens.ts remains SSOT; sync DESIGN.md ← tokens.ts in every token-change task.
- docs/database-schema.md — table source of truth; update on every schema change.
- docs/feature-architecture-map-v1.0.md — feature/domain map.
- .specify/memory/constitution.md — 12 binding principles; plan gates enforce.

## Don't
- No controllers. No stored computed values. No inline approval columns. No spec duplication. No editing applied migrations. No multi-entity concepts (single-entity deployment).

## Maintenance
- When conventions, commands, or schema change: update this file + docs/database-schema.md in the same task.