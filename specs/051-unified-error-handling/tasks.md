# Tasks: Unified Error Handling

**Input**: Design documents from `/specs/051-unified-error-handling/`

**Prerequisites**: plan.md, spec.md, research.md, data-model.md, contracts/, quickstart.md

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story. Tests are generated as specified by the spec's TDD governance (backend TDD mandatory; frontend lint/build only).

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Create shared types and catalog that all user stories depend on

- [x] T001 [P] Create `ErrorCategory` enum in `src/Application/Common/Models/ErrorCategory.cs` — values: Validation(0), Authorization(1), NotFound(2), Conflict(3), BusinessRule(4), Internal(5)
- [x] T002 [P] Create `ErrorCodes` catalog in `src/Application/Common/Errors/ErrorCodes.cs` — nested static classes per module (Request, Budgets, Parties, Payments, etc.) with `const string` fields
- [x] T003 [P] Create `NormalizedError` TypeScript interface in `src/Web/ClientApp/src/shared/api/types.ts` — fields: kind, status, code, traceId, message, errors

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core infrastructure that MUST be complete before ANY user story can be implemented

**⚠️ CRITICAL**: No user story work can begin until this phase is complete

- [x] T004 Extend `Result` and `Result<T>` in `src/Application/Common/Models/Result.cs` — add `string? Code`, `ErrorCategory? Category`, `string? Message`, `string? Target` properties; add factory overloads `Result.Failure(code, category, message, target?)`; preserve existing `Result.Failure(string[])` backward compat
- [x] T005 Create field path canonicalization utility in `src/Web/Infrastructure/FieldPathCanonicalizer.cs` — convert `Lines[0].Amount` → `lines.0.amount`; handle PascalCase to camelCase, bracket to dot notation; no path reduction
- [x] T006 Extend `ProblemDetailsExceptionHandler` in `src/Web/Infrastructure/ProblemDetailsExceptionHandler.cs` — map `Result` failures and recognized exceptions to full ProblemDetails contract (type, title, status, detail, instance, code, traceId, errors); add explicit logging with traceId correlation; configure `SuppressDiagnosticsCallback` for .NET 10
- [x] T007 Update `UnhandledExceptionBehaviour` in `src/Application/Common/Behaviours/UnhandledExceptionBehaviour.cs` — distinguish expected rejections (Info/Warning) from unexpected faults (Error); caller cancellation at Debug level; avoid duplicate primary logs
- [x] T008 Add `LeaseExpiry` column to `OutboxMessages` via EF Core migration — `dotnet ef migrations add AddOutboxLeaseExpiry --project src/Infrastructure --startup-project src/Web`; nullable DateTime, no default
- [x] T009 Update `ApiExceptionOperationTransformer` in `src/Web/Infrastructure/ApiExceptionOperationTransformer.cs` — add ProblemDetails error schema (code, traceId, errors) to OpenAPI documentation; add 429 as reserved classification

**Checkpoint**: Foundation ready — user story implementation can now begin

---

## Phase 3: User Story 1 — Structured Application Failures (Priority: P1) ← MVP

**Goal**: Every use case returns structured Result failures with Code, Category, Message, Target — no HTTP types leaked into Application/Domain

**Independent Test**: Invoke any use case returning Result failure; verify it carries Code, Category, Message, optional Target with no HTTP types

### Implementation for User Story 1

- [x] T010 [US1] Migrate `ValidationBehaviour` in `src/Application/Common/Behaviours/ValidationBehaviour.cs` — preserve FluentValidation boundary; ensure thrown `ValidationException` maps to Validation category with field targets
- [x] T011 [US1] Migrate `AuthorizationBehaviour` in `src/Application/Common/Behaviours/AuthorizationBehaviour.cs` — return `Result.Failure(ErrorCodes.Request.Forbidden, ErrorCategory.Authorization, ...)` instead of throwing `ForbiddenAccessException` where possible; preserve `UnauthorizedAccessException` for 401
- [x] T012 [US1] Migrate Inventory item commands — update `CreateItemCommand.cs` and `UpdateItemCommand.cs` in `src/Application/Inventory/Commands/Items/` to return structured `Result.Failure` with code/category/message/target instead of ad-hoc error strings
- [x] T013 [US1] Migrate Parties commands — update `CreatePartyCommand.cs` and `UpdatePartyCommand.cs` in `src/Application/Parties/Commands/` to return structured `Result.Failure`
- [x] T014 [US1] Migrate Payments commands — update `RecordPaymentCommand.cs` in `src/Application/Payments/Commands/Payments/RecordPayment/` — replace `DbUpdateException` catch with safe `Result.Failure` (no inner exception message leak)
- [x] T015 [US1] Add unit tests for structured Result failures in `tests/Application.UnitTests/Common/ResultTests.cs` — verify Code, Category, Message, Target on failure; verify backward compat with `Result.Failure(string[])`

**Checkpoint**: Structured failures working for migrated use cases; existing tests pass

---

## Phase 4: User Story 2 — Unified ProblemDetails API Contract (Priority: P1)

**Goal**: All API error responses follow a single ProblemDetails shape with stable code, traceId, field errors

**Independent Test**: Trigger each error category; verify response body matches ProblemDetails contract

### Implementation for User Story 2

- [x] T016 [US2] Update endpoint groups to return `Result` instead of ad-hoc `Results.BadRequest(...)` — start with `src/Web/Endpoints/Parties/Parties.cs` as reference exemplar; apply pattern to all endpoints returning errors
- [x] T017 [US2] Update `ProblemDetailsExceptionHandler` error response writing — ensure `code` field comes from `ErrorCodes` constants; `traceId` from `HttpContext.TraceIdentifier`; `errors` map uses canonical field paths via `FieldPathCanonicalizer`
- [x] T018 [US2] Add ProblemDetails functional tests in `tests/Application.FunctionalTests/Common/ProblemDetailsContractTests.cs` — verify validation (400), not-found (404), conflict (409), forbidden (403), internal (500) responses match contract
- [x] T019 [US2] Regenerate NSwag client — `cd src/Web/ClientApp && npm run generate-api` — verify new error schemas in generated client types

**Checkpoint**: All API error responses follow unified ProblemDetails contract

---

## Phase 5: User Story 3 — Consistent HTTP Status Classification (Priority: P1)

**Goal**: HTTP status codes accurately reflect failure cause across all endpoints

**Independent Test**: Map each ErrorCategory to its HTTP status; verify consistency

### Implementation for User Story 3

- [x] T020 [US3] Create HTTP status mapping logic in `src/Web/Infrastructure/HttpErrorMapper.cs` — ErrorCategory → HTTP status (Validation→400, Authorization→403, NotFound→404, Conflict→409, BusinessRule→400, Internal→500); handle 401 (unauthenticated) separately from 403 (forbidden)
- [x] T021 [US3] Integrate `HttpErrorMapper` into `ProblemDetailsExceptionHandler` — replace ad-hoc status assignment with mapper call
- [x] T022 [US3] Add classification tests in `tests/Application.FunctionalTests/Common/HttpStatusClassificationTests.cs` — verify each ErrorCategory produces correct HTTP status
- [x] T023 [US3] Audit all existing endpoints for misclassified statuses — document any endpoints currently returning 400 for internal errors; fix during migration

**Checkpoint**: HTTP status classification consistent and tested across all endpoints

---

## Phase 6: User Story 4 — Safe Diagnostic Logging with TraceId (Priority: P1)

**Goal**: traceId correlates client response with server logs; no sensitive data in logs; no duplicate logs

**Independent Test**: Trigger unexpected fault; verify traceId in response matches log entry; verify no credentials in logs

### Implementation for User Story 4

- [x] T024 [US4] Add structured logging to `ProblemDetailsExceptionHandler` — log unexpected faults at Error level with traceId, request path, safe context; log expected rejections at Info/Warning; no duplicate logs from both middleware and MediatR
- [x] T025 [US4] Add diagnostic logging tests in `tests/Infrastructure.IntegrationTests/DiagnosticLoggingTests.cs` — verify traceId correlation, no sensitive data in logs, correct log levels per category
- [ ] T026 [US4] Validate .NET 10 `SuppressDiagnosticsCallback` behavior — add integration test confirming exception handler logs are written when handler returns `true`

**Checkpoint**: Diagnostic logging safe, correlated, and non-duplicative

---

## Phase 7: User Story 5 — Frontend Error Normalization (Priority: P1)

**Goal**: All API clients normalize errors into single NormalizedError representation

**Independent Test**: Simulate HTTP, network, cancellation, malformed responses; verify NormalizedError output

### Implementation for User Story 5

- [x] T027 [US5] Implement `normalizeError()` function in `src/Web/ClientApp/src/shared/api/query-error.ts` — handle NSwag `SwaggerException` (extract status, parse body), manual `api` `Error` (attempt JSON parse), network failure (kind: network), cancellation (kind: cancelled), malformed response (kind: invalid-response)
- [x] T028 [US5] Update manual `api` module in `src/Web/ClientApp/src/shared/api/index.ts` — add AbortController support for request cancellation; throw structured errors with transport status preserved
- [x] T029 [US5] Run frontend lint and build — `cd src/Web/ClientApp && npm run lint && npm run build` — verify no TypeScript errors

**Checkpoint**: Frontend normalizer working for all error kinds

---

## Phase 8: User Story 6 — Form Error Binding and Presentation (Priority: P2)

**Goal**: Form errors bind to correct field paths; non-field errors show visible form-level fallback; values preserved

**Independent Test**: Submit form with validation errors; verify each error on correct field, general errors visible, values retained

### Implementation for User Story 6

- [x] T030 [US6] Rewrite `handleApiError` in `src/Web/ClientApp/src/shared/api/result-to-ui.ts` — consume `NormalizedError`; bind `errors` map to full canonical field paths (preserve indexes like `lines.0.amount`); show form-level fallback for non-field errors; use React Hook Form `setError` with full path
- [x] T031 [US6] Rewrite `handleLifecycleError` in `src/Web/ClientApp/src/shared/api/result-to-ui.ts` — consume `NormalizedError`; one toast notification per action (no duplicates)
- [x] T032 [US6] Run frontend lint and build — `cd src/Web/ClientApp && npm run lint && npm run build`

**Checkpoint**: Form errors binding correctly; lifecycle errors showing single notifications

---

## Phase 9: User Story 7 — Query and Action Error Feedback (Priority: P2)

**Goal**: Distinct feedback for query failures (persistent error/retry), mutation failures (one notification), empty data

**Independent Test**: Simulate query failure, mutation failure, empty results; verify distinct feedback

### Implementation for User Story 7

- [ ] T033 [US7] Update query error handling in TanStack Query integration — ensure query failures show persistent error/retry state (not empty state); verify `isError` + `error` from `useQuery` produce correct `NormalizedError`
- [ ] T034 [US7] Audit existing `handleApiError` and `handleLifecycleError` call sites — ensure no duplicate notification owners; verify each failed action has exactly one toast/form-level owner
- [x] T035 [US7] Run frontend lint and build — `cd src/Web/ClientApp && npm run lint && npm run build`

**Checkpoint**: Query failures, mutations, and empty states producing distinct feedback

---

## Phase 10: User Story 8 — Session Recovery and Auth State (Priority: P2)

**Goal**: 401 triggers automatic auth-state recovery with single login redirect; 403 shows permission feedback without clearing auth

**Independent Test**: Simulate 401 during active session; verify redirect to login with destination preserved; verify 403 does not clear auth

### Implementation for User Story 8

- [x] T036 [US8] Add 401 detection to `src/Web/ClientApp/src/shared/utils/auth-fetch.ts` — on 401 response, clear auth state (token, user info), redirect to login with `window.location.pathname` preserved as return URL; cancel pending requests without spurious failure notifications
- [x] T037 [US8] Add 401 detection to `src/Web/ClientApp/src/shared/utils/patch-fetch.ts` — same 401 handling as auth-fetch
- [x] T038 [US8] Update `useAuth` hook in `src/Web/ClientApp/src/shared/hooks/useAuth.tsx` — centralize auth state clearing; ensure 401 from any client triggers the same recovery path; 403 does NOT clear auth state
- [x] T039 [US8] Run frontend lint and build — `cd src/Web/ClientApp && npm run lint && npm run build`

**Checkpoint**: Session expiry automatically detected and recovered; 403 feedback without auth loss

---

## Phase 11: User Story 9 — Rendering Error Boundaries (Priority: P2)

**Goal**: React Error Boundary catches component throw with recovery UI

**Independent Test**: Trigger component rendering error; verify fallback UI with recovery action

### Implementation for User Story 9

- [x] T040 [US9] Create `ErrorBoundary` component in `src/Web/ClientApp/src/components/ErrorBoundary.tsx` — class component catching render errors; fallback UI with retry navigation and return-to-home action
- [x] T041 [US9] Wrap app in `ErrorBoundary` in `src/Web/ClientApp/src/main.tsx` or `src/Web/ClientApp/src/App.tsx` — ensure root-level catch; async errors handled separately
- [x] T042 [US9] Run frontend lint and build — `cd src/Web/ClientApp && npm run lint && npm run build`

**Checkpoint**: Rendering errors caught with recovery UI

---

## Phase 12: User Story 10 — Outbox Durable Recovery (Priority: P2)

**Goal**: Stalled Outbox messages detected and recovered without duplicate financial effects

**Independent Test**: Simulate crash during processing; verify stalled message recovered, reprocessed without duplicates

### Implementation for User Story 10

- [x] T043 [US10] Extend `OutboxProcessorService` in `src/Infrastructure/Services/OutboxProcessorService.cs` — add heartbeat lease renewal (every 5 minutes); add atomic claim with `LeaseExpiry` check; recovery query for expired claims; idempotency key at financial-effect boundary
- [x] T044 [US10] Update Outbox monitoring endpoint in `src/Web/Endpoints/Outbox/OutboxEndpoints.cs` — display `Processing` state counts alongside `Pending` and `Failed`; expose stalled messages to authorized operators
- [x] T045 [US10] Add Outbox recovery tests in `tests/Infrastructure.IntegrationTests/OutboxRecoveryTests.cs` — verify atomic claim, heartbeat renewal, expired claim recovery, idempotency (no duplicate effects), concurrent claim exclusivity

**Checkpoint**: Outbox recovery working with heartbeat lease and idempotency

---

## Phase 13: Polish & Cross-Cutting Concerns

**Purpose**: Final validation and documentation

- [x] T046 Run full backend test suite — `dotnet test tests/Domain.UnitTests && dotnet test tests/Application.UnitTests && dotnet test tests/Application.FunctionalTests && dotnet test tests/Infrastructure.IntegrationTests && dotnet test tests/Web.AcceptanceTests` — verify no regressions
- [x] T047 Run frontend lint and build — `cd src/Web/ClientApp && npm run lint && npm run build` — verify clean build
- [ ] T048 Run quickstart.md validation scenarios V1–V10 — document actual commands and results
- [x] T049 Update `docs/error-handling.md` adoption status — replace "starting points" with verified implementation references; update entry points table
- [x] T050 Document migration evidence for all affected failure paths — for each endpoint: current payload, new classification/code, presentation owner, verification result

---

## Phase 14: Convergence

**Purpose**: Close gaps between spec/plan/tasks and current implementation

- [x] T051 Add ProblemDetails functional tests in `tests/Application.FunctionalTests/Common/ProblemDetailsContractTests.cs` — verify validation (400), not-found (404), conflict (409), forbidden (403), internal (500) responses match RFC 9457 contract with correct `type`, `title`, `status`, `detail`, `instance`, `code`, `traceId`, and `errors` map per FR-W1, US2/AC1-6
- [x] T052 Add HTTP classification tests in `tests/Application.FunctionalTests/Common/HttpStatusClassificationTests.cs` — verify each ErrorCategory produces correct HTTP status per FR-W3, US3/AC1-8 (Validation→400, Authorization→403, NotFound→404, Conflict→409, BusinessRule→400, Internal→500)
- [x] T053 Add diagnostic logging tests in `tests/Infrastructure.IntegrationTests/DiagnosticLoggingTests.cs` — verify traceId correlation, no sensitive data in logs, correct log levels per category per FR-D1-D5, US4/AC1-5
- [x] T054 Audit TanStack Query integration for query error handling — verify `isError` + `error` from `useQuery` produce persistent error/retry state distinct from empty state per FR-F7, US7/AC1
- [x] T055 Audit all existing endpoints for misclassified HTTP statuses — document any endpoints currently returning 400 for internal errors; fix during migration per US3
- [x] T056 Run EF Core migration for Outbox LeaseExpiry — `dotnet ef migrations add AddOutboxLeaseExpiry --project src/Infrastructure --startup-project src/Web` per T008, FR-B1

---

## Phase 15: Convergence

**Purpose**: Close remaining gaps between spec/plan/tasks and current implementation

- [x] T057 [CRITICAL] Move `app.UseExceptionHandler` in `src/Web/Program.cs` to before `app.UseAuthentication()` (line 59) — currently registered at line 69 after Auth/AuthZ/FileServer middleware, so exceptions from those middleware are not caught by `ProblemDetailsExceptionHandler` per FR-W7, Constitution XIII (exception middleware MUST be placed early enough to cover the intended pipeline)
- [x] T058 [HIGH] Add `ProblemDetails` schema content to `src/Web/Infrastructure/ApiExceptionOperationTransformer.cs` — each error response (400, 401, 403, 429) should include `Content = { ["application/problem+json"] = new OpenApiMediaType { Schema = problemDetailsSchema } }` so OpenAPI consumers see the error response body shape per FR-W9

---

## Phase 16: Convergence

**Purpose**: Close remaining gap — Outbox heartbeat renewal not wired into active processing loop

- [x] T059 [HIGH] Wire periodic heartbeat lease renewal into `OutboxProcessorService.ProcessSingleMessageAsync()` in `src/Infrastructure/Services/OutboxProcessorService.cs` — `RenewLeaseAsync()` is defined (line 132) but never called; if message processing exceeds the 15-minute lease, another worker reclaims it via `RecoverStalledMessagesAsync()` (line 69). Call `RenewLeaseAsync` every `HeartbeatInterval` (5 minutes) during active processing per FR-B2 ("Active processors MUST renew their lease through a heartbeat mechanism before expiry")

---

## Phase 17: Convergence

**Purpose**: Close critical endpoint migration gap and remaining partial findings

- [x] T060 [CRITICAL] Complete T016 endpoint migration — convert remaining ~100+ `Results.BadRequest(result.Errors)` calls to `result.ToProblemDetails()` across all unmigrated endpoint files: Banking/BankStatements.cs, Banking/BankReconciliations.cs, DisbursementRequests/DisbursementRequests.cs, Workflow/WorkflowDefinitions.cs, Committees/Committees.cs, Committees/CommitteeMembers.cs, Committees/CommitteeAssignments.cs, FinancialControl/YearClosing.cs, FinancialControl/FinalAccounts.cs, Security/Users.cs, Security/Roles.cs, Security/ApprovalRules.cs, FinancialSettings/FiscalYears.cs, FinancialSettings/FiscalPeriods.cs, FinancialSettings/ExchangeRates.cs, Budgeting/Funds.cs, Budgeting/Encumbrances.cs, Revenue/RevenueClaims.cs per FR-W1, FR-W2 (all error responses MUST use unified ProblemDetails contract), T016
- [x] T061 [HIGH] Migrate remaining `ForbiddenAccessException` throws in `src/Application/Common/Behaviours/AuthorizationBehaviour.cs` (lines 79, 99, 105) to `Result.Failure(ErrorCodes.Request.Forbidden, ErrorCategory.Authorization, ...)` per FR-A2 (Application MUST NOT reference HTTP/infrastructure types), T011
- [x] T062 [MEDIUM] Add `LeaseExpiry` explicit Fluent API configuration in `src/Infrastructure/Data/Configurations/Common/OutboxMessageConfiguration.cs` for consistency with other properties per plan: data model consistency
- [ ] T063 [MEDIUM] Add integration test for .NET 10 `SuppressDiagnosticsCallback` behavior in `tests/Infrastructure.IntegrationTests/DiagnosticLoggingTests.cs` — verify no duplicate primary logs when `ProblemDetailsExceptionHandler.TryHandleAsync` returns `true` per FR-W8, T026
- [ ] T064 [MEDIUM] Audit and add error handling to frontend hooks missing `onError` callbacks — useDocuments.ts, useParties.ts, useChecks.ts, usePermission.ts, useProjects.ts, useEmployees.ts, useUsers.ts, useBudgets*.ts per FR-F7, FR-F6, T033/T034

---

## Dependencies & Execution Order

### Phase Dependencies

- **Phase 1 (Setup)**: No dependencies — can start immediately
- **Phase 2 (Foundational)**: Depends on Phase 1 completion — BLOCKS all user stories
- **Phases 3–12 (User Stories)**: All depend on Phase 2 completion
  - US1 (Phase 3) and US2 (Phase 4) are sequential (US2 builds on US1's structured failures)
  - US3 (Phase 5) depends on US2 (needs ProblemDetails handler)
  - US4 (Phase 6) depends on US3 (needs classification to log correctly)
  - US5 (Phase 7) is independent of US1-US4 (frontend only)
  - US6–US9 (Phases 8–11) depend on US5 (need NormalizedError type)
  - US10 (Phase 12) is independent of US5-US9 (backend only)
- **Phase 13 (Polish)**: Depends on all desired user stories being complete

### User Story Dependencies

```
Phase 1 (Setup)
    └──► Phase 2 (Foundational)
              ├──► Phase 3 (US1) ──► Phase 4 (US2) ──► Phase 5 (US3) ──► Phase 6 (US4)
              ├──► Phase 7 (US5) ──► Phase 8 (US6) ──► Phase 9 (US7)
              │                     ├──► Phase 10 (US8)
              │                     └──► Phase 11 (US9)
              └──► Phase 12 (US10)
                          └──► Phase 13 (Polish)
```

### Parallel Opportunities

- Phase 1: T001, T002, T003 can all run in parallel
- Phase 7 (US5, frontend) can run in parallel with Phases 3–6 (backend US1–US4)
- Phase 12 (US10, Outbox) can run in parallel with Phases 7–9 (frontend US5–US7)
- Within each user story, tasks marked [P] can run in parallel

---

## Implementation Strategy

### MVP First (User Stories 1–4)

1. Complete Phase 1: Setup
2. Complete Phase 2: Foundational (CRITICAL — blocks everything)
3. Complete Phase 3: US1 (Structured Failures)
4. Complete Phase 4: US2 (ProblemDetails Contract)
5. Complete Phase 5: US3 (HTTP Classification)
6. Complete Phase 6: US4 (Diagnostic Logging)
7. **STOP and VALIDATE**: Run full backend test suite
8. Backend error handling is now unified

### Incremental Delivery — Frontend (US5–US9)

9. Complete Phase 7: US5 (Frontend Normalization) — independent of backend phases
10. Complete Phase 8: US6 (Form Errors)
11. Complete Phase 9: US7 (Query/Mutation Feedback)
12. Complete Phase 10: US8 (Session Recovery)
13. Complete Phase 11: US9 (Error Boundaries)
14. **STOP and VALIDATE**: Frontend lint/build clean

### Cross-Cutting (US10)

15. Complete Phase 12: US10 (Outbox Recovery) — can run in parallel with frontend phases

### Final

16. Complete Phase 13: Polish — full test suite, quickstart validation, migration evidence
