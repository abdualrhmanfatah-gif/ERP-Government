# AGENTS.md — ERP-Government

Yemeni government ERP (Financial Law 8/1990). Spec-Driven Development (Spec Kit, .specify/).
READ THIS FIRST. Conventions below are verified as-built — follow them, do not re-derive or invent alternatives.

## Stack
.NET 10 / C# 13, EF Core + SQL Server, MediatR, FluentValidation, minimal APIs, .NET Aspire (src/AppHost + ServiceDefaults).
Frontend stack — see `## Frontend Architecture` below.

## Build & Verify (exact commands — NO .sln exists, always target projects directly)
- Backend build: dotnet build src/Web/Web.csproj
- Backend tests (run relevant project; FULL suite before merge):
  dotnet test tests/Domain.UnitTests
  dotnet test tests/Application.UnitTests
  dotnet test tests/Application.FunctionalTests
  dotnet test tests/Infrastructure.IntegrationTests
  dotnet test tests/Web.AcceptanceTests
- Frontend (cd src/Web/ClientApp): npm run lint | npm run build
  (prebuild/prestart auto-run `npm run generate-api` — NSwag API client regeneration; regenerate after endpoint changes.
  Also available: `npm run generate-tokens`, `npm run design:lint`, `npm run design:check`.)
- EF migration: dotnet ef migrations add <Name> --project src/Infrastructure --startup-project src/Web
- TDD per-cycle: single filtered test (red) → touched project's suite only (green/refactor).
  Full 5-project suite runs ONLY at gates: session preflight, /speckit.converge, pre-merge.
  Project scoping for cycle economy is NOT test-filtering-to-green (Hard Rule 4); no assertion
  is ever weakened, skipped, or deleted — full suite still gates every merge.
- **Spec 045 exception (DEP-027)**: work scoped to `specs/045-payments-group/` does not use
  TDD and does not add automated tests. Use scoped builds, frontend lint/build, manual quickstart
  evidence, and the existing five backend test projects as convergence/pre-merge regression gates.
  Existing tests remain intact; update fixtures only where the approved contract requires it.
- **Frontend has NO test suite by governance decision** (see Frontend Architecture). Frontend
  `npm run lint` runs ESLint + dependency-cruiser only.

## Layer map
- src/Domain/<Feature>/Entities/ — EF entities. src/Domain/Common/ = BaseEntity (int PK), BaseAuditableEntity (+audit +RowVersion). ALWAYS inherit one.
- src/Domain/<Feature>/Enums/ — enums (stored as int).
- src/Domain/Events/<Feature>/ — domain events, inherit BaseEvent, implement IHasSourceEntity (src/Domain/Events/Common/).
- src/Application/<Feature>/Commands/<Entity>/<Action>/ — one folder per action: Command + Handler + Validator together.
- src/Application/<Feature>/Queries/<Entity>/ — same pattern.
- src/Web/Endpoints/<Feature>/<Entity>s.cs — IEndpointGroup (src/Web/Infrastructure/IEndpointGroup.cs), auto-registered via WebApplicationExtensions (MapEndpointGroups).
- src/Infrastructure/Data/ — DbContext, Migrations/ (NEVER edit an applied migration; always add new).
- Frontend layer map — see `## Frontend Architecture` below.

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
| Frontend feature folder (top reference) | src/Web/ClientApp/src/features/budgeting/ (entity-based: appropriations/, classifications/, funds/, ...) |
| Frontend canonical page | src/Web/ClientApp/src/features/budgeting/appropriations/pages/AppropriationsListPage.tsx |

Maintenance: when a newer better exemplar lands (e.g. ReceiptVouchers after treasury spec, JournalEntries after 014 rename), repoint this table in the same task — never leave it stale.

## Binding conventions
- Minimal APIs only — NO controllers. Route /api/{ClassName}. Lifecycle transitions via PATCH {id}/lifecycle.
- Every handler returns Result<T> (src/Application/Common/Models/Result.cs).
- Every endpoint: [Authorize(Policy = PermissionCodes.<Code>)] (src/Application/Common/Security/PermissionCodes.cs).
- Money: decimal(23,2). NO stored computed columns — totals computed in queries/handlers.
- Document numbering: IDocumentSequenceService (src/Application/FinancialSettings/Common/Services/) — {PREFIX}-{D6}, allocated in the same transaction, number at Draft creation.
- Approvals/status: ApprovalHistory + DocumentStatusLog (append-only) via IDocumentStatusLogger — never inline approval columns.
- Ledger posting: domain event → PostingRules → JournalEntry + lines (AccountingEvents staging removed, DEP-026; balances computed live from JournalEntryLines).
- Availability: BudgetAvailabilityService (src/Application/Budgeting/Common/) — item-level net appropriations − open encumbrances.

## Frontend Architecture

### Stack
React 19 + TypeScript + Vite. Path: `src/Web/ClientApp`.
- Routing: React Router v7 (declarative, route guards via loaders).
- Server state: TanStack Query (NSwag-generated clients + query hooks).
- Client state: Zustand (UI-only state; never for server data).
- Forms: React Hook Form + Zod (Zod schemas co-located in `features/<x>/<entity>/shared/schemas.ts`).
- Styling: Tailwind v3 + **shadcn** (the primitives source — `src/components/ui/`). Theme bridged from `src/Web/ClientApp/src/components/tokens.ts` (SSOT, read-only).
- Charts: `@nivo/{bar,line,pie,core}`. Tables: `@tanstack/react-table`. Icons: `lucide-react`. Toasts: `sonner`. Dates: `date-fns` + `react-day-picker`. Headless primitives: `@base-ui/react`.
- Language: Arabic only, hardcoded strings. `<html dir="rtl" lang="ar">` fixed at boot.
- Vite aliases: `@app`, `@shared`, `@features`, `@routes`.

### Layer map
```
src/Web/ClientApp/src/
├── app/                    # bootstrap: router, providers, QueryClient
├── components/             # ALL UI components in ONE place (shadcn primitives + feature-scoped, domain-prefixed). NO `components/` folders inside features/.
├── shared/
│   ├── zod-schemas/        # shared input shapes (Money, dates, ...)
│   └── api/                # query keys factory, Result<T>→UI helpers, error mappers
├── features/<domain>/
│   ├── <entity>/           # entity-scoped subfolders (appropriations, classifications, funds, ...)
│   │   ├── pages/          # route components
│   │   ├── hooks/          # entity-scoped hooks
│   │   └── shared/         # client.ts, types.ts, schemas.ts (entity-scoped)
│   ├── hooks/              # cross-entity hooks
│   ├── shared/             # cross-entity types, schemas
│   └── utils/              # cross-entity helpers
└── routes/                 # route definitions consumed by app/router
```

### API Strategy
- Default: NSwag-generated client (`npm run generate-api`).
- Manual wrapper in `features/<domain>/<entity>/shared/client.ts` ONLY when needed for custom retry, request cancellation, transform, or multi-endpoint composition. Header must state WHY in a comment.
- All API errors funnel through `shared/api/result-to-ui.ts` — maps `Result<T>.Errors` to toast or inline by severity.

### Cross-cutting Rules
- Forms: every form has a Zod schema in `features/<domain>/<entity>/shared/schemas.ts`; same schema reused for `defaultValues` + validation.
- Server errors: 4xx → inline field error (via Zod refine), 5xx → toast. Never swallow.
- Loading states: every query has `isPending` skeleton + `isError` retry UI. No spinners-as-default.
- RTL: logical CSS properties only (`ms-/me-`, `ps-/pe-`, `start/end`). Physical properties (`margin-left`, `padding-right`) prohibited.
- Strings: hardcoded Arabic directly in JSX. No `t()` calls, no locale files, no language switcher.
- Accessibility (manual review): ARIA-labelled, keyboard-reachable, focus-visible. No automated tier.

### Dependency Rules
- `features/` NEVER imports from other features/. Cross-feature sharing via `shared/` or events only.
- `features/<domain>/` NEVER has a `components/` subfolder. Feature-scoped components live in `src/components/` with a domain prefix (e.g. `BudgetingAppropriationsFilters.tsx`).
- Acyclic graph enforced by `dependency-cruiser` at `src/Web/ClientApp/.dependency-cruiser.cjs`. CI fails on violation.
- `components/` is a leaf — cannot import from `features/` or `routes/`. Features may import from `components/`.

### Exemplar maintenance
- Top reference (folder): `src/Web/ClientApp/src/features/budgeting/` — entity-based nesting (`appropriations/`, `classifications/`, `funds/`, ...) with cross-entity `hooks/`/`shared/`/`utils/` at root.
- Canonical page: `src/Web/ClientApp/src/features/budgeting/appropriations/pages/AppropriationsListPage.tsx` — list + filtering + server state + Arabic RTL UI.
- UI primitives (shadcn) and feature-scoped components live in `src/Web/ClientApp/src/components/`, organized by domain prefix (e.g. `Button.tsx`, `BudgetingAppropriationsFilters.tsx`).
- When a newer/better pattern lands, repoint this section in the same task — never leave it stale.

### Frontend Governance Override
Frontend code does not use TDD or frontend automated tests. Backend testing requirements remain unchanged. This is an AGENTS.md frontend convention and does not amend `.specify/memory/constitution.md`.

## Authentication & Authorization
- Auth: JWT Bearer (src/Infrastructure/DependencyInjection.cs). Key from config "Jwt:Key". No issuer/audience validation (dev config).
- Login: POST /api/Users/login → { token, userId, role }. Passwords: ASP.NET Identity PasswordHasher (User.PasswordHash). Lockout: 5 failed attempts → 15 min. Token claims: NameIdentifier, Name, Role (single role code) + permission claims loaded from RolePermissions.
- Single role per user: User.RoleId → SecurityRole (one role only — binding). Never introduce multi-role.
- Every endpoint declares its permission: .RequireAuthorization(PermissionCodes.X) on the route OR [Authorize(Policy = PermissionCodes.X)]. Codes live in src/Application/Common/Security/PermissionCodes.cs — format {Module}.{Action}. Add new codes there + register the policy in src/Web/DependencyInjection.cs.
- RBAC model: SecurityRole / RolePermission / SecurityPermission (+ UserPermission overrides); SoDMatrix, FieldSecurityPolicy, RecordRule removed (DEP-026) — src/Domain/Security/Entities.
- Approval authorization: IApprovalService + IApprovalRuleEvaluationService (src/Application/Security/Common). Every approval decision recorded via ApprovalHistory only — never inline.
- Audit: SecurityAuditLog + AuditTrail (append-only).
- KNOWN DEV STATE: all policies are currently registered as RequireAssertion(_ => true) — open placeholder, RBAC enforcement wiring is pending tracked work. Do NOT treat endpoints as access-controlled yet; do NOT remove placeholder registrations inside feature specs; keep new policies consistent until the enforcement spec lands.
## Spec workflow (Spec Kit)
- Active spec pointer: .specify/feature.json. Pipeline: /speckit.specify → /speckit.plan → /speckit.tasks → /speckit.implement → /speckit.converge.
- Git: branch per spec (auto-created at specify). Merge after converge: git checkout main; git merge --no-ff <branch>.
- NEVER create a new spec for an existing feature — amend its spec.md/plan.md instead. Check specs/ before specifying.
- Before implementing: read the spec's data-model.md AND verify against current entities — specs can lag code.
- TDD mandatory (constitution XI): test first, observe red, implement, green. Never weaken/skip/delete tests. Spec 045 alone follows the DEP-027 exception above.

## Docs
- DESIGN.md — Agent-facing UI design tokens + rationale (YAML front matter + prose). Agents: read DESIGN.md before any UI work; tokens.ts remains SSOT (read-only by convention — edit only when justified); sync DESIGN.md ← tokens.ts in every token-change task.
- docs/database-schema.md — table source of truth; update on every schema change.
- docs/feature-architecture-map-v1.0.md — feature/domain map.
- .specify/memory/constitution.md — 12 binding principles; plan gates enforce.

## Don't
- No controllers. No stored computed values. No inline approval columns. No spec duplication. No editing applied migrations. No multi-entity concepts (single-entity deployment).
- No frontend test files (Vitest/RTL/Playwright/axe prohibited — see Frontend Architecture). No cross-feature imports. No CSS physical properties (logical only). No hardcoded strings bypassing `components/` primitives when a primitive exists. No mock data layer (MSW/mock servers/fake APIs prohibited — backend live only).

## Maintenance
- When conventions, commands, or schema change: update this file + docs/database-schema.md in the same task.
