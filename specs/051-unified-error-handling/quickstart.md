# Quickstart Validation Guide: Unified Error Handling

**Date**: 2026-09-14
**Spec**: [spec.md](./spec.md)

## Prerequisites

- .NET 10 SDK installed
- SQL Server running (local or container)
- Node.js 18+ and npm installed
- Project built: `dotnet build src/Web/Web.csproj`
- Frontend built: `cd src/Web/ClientApp && npm run build`

## Validation Scenarios

### V1: Structured Application Failures (US1)

**Goal**: Verify `Result` failures carry Code, Category, Message, Target.

1. Run: `dotnet test tests/Application.UnitTests --filter "ResultStructuredErrors"`
2. Expected: Tests pass verifying `Result.Failure(code, category, message, target)` produces correct structure
3. Expected: Existing `Result.Failure("string")` backward compatibility preserved

### V2: ProblemDetails Contract (US2)

**Goal**: Verify API error responses match the unified ProblemDetails shape.

1. Run: `dotnet test tests/Application.FunctionalTests --filter "ProblemDetailsContract"`
2. Expected: Validation error returns `application/problem+json` with `type: "about:blank"`, `status: 400`, `code: "Request.ValidationFailed"`, `traceId`, `errors` map with canonical field paths
3. Expected: Not-found returns `status: 404`, `code: "Request.NotFound"`
4. Expected: Concurrency conflict returns `status: 409`, `code: "Request.ConcurrencyConflict"`
5. Expected: Unexpected fault returns `status: 500`, `code: "Request.InternalError"`, `traceId` present, no stack trace in `detail`

### V3: HTTP Status Classification (US3)

**Goal**: Verify each error category maps to the correct HTTP status.

1. Run: `dotnet test tests/Web.AcceptanceTests --filter "HttpStatusClassification"`
2. Expected: Validation → 400, Auth → 401, Forbidden → 403, Not Found → 404, Conflict → 409, Internal → 500

### V4: Diagnostic Logging with TraceId (US4)

**Goal**: Verify traceId correlation and safe logging.

1. Run: `dotnet test tests/Infrastructure.IntegrationTests --filter "DiagnosticLogging"`
2. Expected: Trigger unexpected fault; response contains `traceId`; log entry contains same `traceId`
3. Expected: Log entry does NOT contain passwords, tokens, SQL, or stack traces
4. Expected: Validation rejection logged at Info/Warning level, not Error

### V5: Frontend Error Normalization (US5)

**Goal**: Verify all client paths produce `NormalizedError`.

1. Run: `cd src/Web/ClientApp && npm run lint`
2. Expected: No TypeScript errors in `shared/api/` modules
3. Run: `npm run build`
4. Expected: Build succeeds with no errors
5. Manual verification: Open browser dev tools → Network tab → trigger each error type → verify console output matches `NormalizedError` shape

### V6: Form Error Binding (US6)

**Goal**: Verify field errors bind to correct paths.

1. Manual: Navigate to any create form (e.g., Items, Parties)
2. Submit with empty required fields
3. Expected: Field errors appear under correct fields (not just "Name")
4. Expected: Nested paths like `lines.0.amount` preserved (not reduced to `amount`)
5. Expected: Previously entered values retained on resubmit

### V7: Session Recovery (US8)

**Goal**: Verify 401 detection and redirect.

1. Manual: Log in → open browser dev tools → clear auth token from storage → trigger any API call
2. Expected: Redirect to login page
3. Expected: No spurious failure toast during redirect
4. Expected: After re-login, intended destination preserved

### V8: Outbox Recovery (US10)

**Goal**: Verify stalled message detection and recovery.

1. Run: `dotnet test tests/Infrastructure.IntegrationTests --filter "OutboxRecovery"`
2. Expected: Simulated crash (message in Processing with expired LeaseExpiry) is detected and reprocessed
3. Expected: No duplicate journal entries or financial effects after reprocessing
4. Expected: Concurrent claim attempts result in exactly one successful claim

### V9: Backward Compatibility

**Goal**: Verify no regression in existing success contracts.

1. Run full backend test suite: `dotnet test tests/Domain.UnitTests && dotnet test tests/Application.UnitTests && dotnet test tests/Application.FunctionalTests && dotnet test tests/Infrastructure.IntegrationTests && dotnet test tests/Web.AcceptanceTests`
2. Expected: All existing tests pass (no regressions)
3. Run: `cd src/Web/ClientApp && npm run lint && npm run build`
4. Expected: Frontend lint and build succeed

### V10: Migration Evidence

**Goal**: Document all affected failure paths.

1. For each endpoint that currently returns ad-hoc errors (e.g., `Results.BadRequest(result.Errors)`), record:
   - Current payload shape
   - New ProblemDetails classification/code
   - Frontend presentation owner (form, toast, query)
   - Verification result (test name + pass/fail)
2. Store in the implementing plan's migration evidence section

## Expected Outcome

All 10 validation scenarios pass. Backend test suite green. Frontend lint/build clean. Migration evidence documented for all affected paths.
