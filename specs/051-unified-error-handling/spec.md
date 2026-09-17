# Feature Specification: Unified Error Handling

**Feature Branch**: `051-unified-error-handling`

**Created**: 2026-09-14

**Status**: Draft

**Input**: User description: "إصلاح وتوحيد Error Handling في جميع أجزاء ERP-Government وفق Constitution XIII وDEP-029.غطية: أخطاء التطبيق المنظمة، ProblemDetails الموحد، تصنيف HTTP، التسجيل الآمن وtraceId، جميع عملاء الواجهة، أخطاء النماذج والاستعلامات والإجراءات، استعادة الجلسة والعرض، واسترداد Outbox دون تكرار الأثر المالي."

## Clarifications

### Session 2026-09-14

- Q: What should the maximum duration be before an Outbox processing claim is considered stalled and eligible for recovery? → A: 15 minutes as the initial, globally configurable processing lease duration. Active processor must renew lease through heartbeat. Recovery eligibility based on lease expiry, not elapsed time since processing started. Reclaim expired claims atomically. Prevent stale workers from completing reclaimed claims. Enforce idempotency at financial-effect boundary. Per-message configuration is outside initial scope.
- Q: How should the `errors` map in ProblemDetails format multiple validation errors for the same field? → A: Array of safe Arabic messages per canonical frontend field path. Preserve complete nested path and array index. Canonicalize backend paths (e.g., Lines[0].Amount → lines.0.amount) at shared Web boundary. Keep message order deterministic and remove exact duplicates. Do not reduce path to final segment. Use form-level error location for failures without valid field target. Top-level ProblemDetails code as Request.ValidationFailed. Per-error machine codes are outside initial contract.
- Q: Where should the shared error code catalog (`Module.Cause` constants) live in the codebase? → A: Single file at `src/Application/Common/Errors/ErrorCodes.cs` with nested static classes per module. Codes must be globally unique, stable constants (e.g., `ErrorCodes.Budgets.InsufficientAvailability` → `"Budgets.InsufficientAvailability"`). Catalog contains codes only; Arabic messages and HTTP mappings belong to their respective shared layers.
- Q: Should this spec implement rate limiting middleware, or only establish the classification contract? → A: Defer implementation. Define HTTP 429 in the unified error classification contract (application/problem+json, code `Request.RateLimitExceeded`, safe Arabic message, traceId, Retry-After header). Do not document 429 on existing endpoints. Add to shared reusable OpenAPI error schema as reserved classification. Endpoint-level 429 responses declared only when rate limiting is implemented. Rate-limiting middleware, policies, counters, limits, and configuration are outside this spec.
- Q: What should the `type` URI be in ProblemDetails responses? → A: Use `about:blank` for all errors (RFC 9457 default). The `code` extension field is the stable machine-readable identifier. Do not create or maintain project-specific problem URIs in this spec. Introducing documented problem-type URIs later requires a versioned contract decision.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Structured Application Failures (Priority: P1)

A developer returning a business rejection from a use case needs to communicate a stable error code, semantic classification, safe Arabic message, and optional field target without exposing HTTP types or internal details.

**Why this priority**: Every downstream consumer (API conversion, frontend normalization, diagnostics) depends on this contract. Without it, error classification is ad-hoc and inconsistent.

**Independent Test**: Can be fully tested by invoking any use case that returns a `Result` failure and verifying the failure carries a code, category, message, and optional target with no HTTP types leaked into Application or Domain.

**Acceptance Scenarios**:

1. **Given** a use case returns a `Result` failure for a business rejection, **When** the result is inspected, **Then** it contains a stable `Code` (format `Module.Cause`), a `Category` (Validation, Authorization, NotFound, Conflict, BusinessRule, Internal), a safe Arabic `Message`, and an optional `Target` field path.
2. **Given** a use case returns a `Result` failure, **When** the failure is inspected, **Then** it does not contain HTTP status codes, ASP.NET exception types, or stack traces.
3. **Given** a use case encounters an unexpected fault, **When** the fault propagates, **Then** it is not swallowed into a `Result` failure but propagates after cleanup for central translation.

---

### User Story 2 - Unified ProblemDetails API Contract (Priority: P1)

An API consumer receiving an error response needs a single, predictable JSON structure containing a stable error code, trace identifier, safe detail message, and optional field errors mapped to their full paths.

**Why this priority**: Inconsistent error payloads cause lost field mappings, false failure notifications, and inability to correlate with server diagnostics. This is the foundation for all frontend error handling.

**Independent Test**: Can be fully tested by triggering each error category (validation, auth, not-found, conflict, internal) and verifying the response body matches the ProblemDetails contract with correct HTTP status, code, traceId, and field errors.

**Acceptance Scenarios**:

1. **Given** an API request fails with a validation error, **When** the response is received, **Then** the body is `application/problem+json` containing `type` (`about:blank`), `title`, `status` (400), `detail` (safe Arabic), `instance` (safe path), `code` (`Request.ValidationFailed`), `traceId`, and `errors` map where each key is a canonical frontend field path (e.g., `lines.0.amount`) and each value is an array of safe Arabic messages with deterministic order and no duplicates.
2. **Given** an API request fails with a not-found error, **When** the response is received, **Then** the body contains `status` (404) and `code` (`Request.NotFound`) with no stack trace or SQL details.
3. **Given** an API request fails with a concurrency conflict, **When** the response is received, **Then** the body contains `status` (409) and `code` (`Request.ConcurrencyConflict`).
4. **Given** an API request fails with an authorization rejection, **When** the response is received, **Then** the body contains `status` (403) and `code` (`Request.Forbidden`) with no internal policy details.
5. **Given** an API request encounters an unexpected fault, **When** the response is received, **Then** the body contains `status` (500), `code` (`Request.InternalError`), a `traceId` correlating with server logs, and no stack trace or SQL in `detail`.
6. **Given** an API response succeeds with an empty body (200 or 204), **When** the client processes it, **Then** it is not treated as an error.

---

### User Story 3 - Consistent HTTP Status Classification (Priority: P1)

An API consumer needs HTTP status codes that accurately reflect the failure cause, enabling correct retry logic, cache invalidation, and user feedback.

**Why this priority**: Misclassified statuses (e.g., internal errors as 400) cause incorrect client behavior and mask real issues.

**Independent Test**: Can be fully tested by mapping each `Result` category to its HTTP status and verifying the mapping is applied consistently across all endpoints.

**Acceptance Scenarios**:

1. **Given** a request fails validation, **When** the response is returned, **Then** the HTTP status is 400.
2. **Given** a request lacks authentication, **When** the response is returned, **Then** the HTTP status is 401 with `WWW-Authenticate` header.
3. **Given** a request is denied by authorization, **When** the response is returned, **Then** the HTTP status is 403.
4. **Given** a requested resource does not exist, **When** the response is returned, **Then** the HTTP status is 404 (even for mutations).
5. **Given** a request conflicts with current state (optimistic concurrency, duplicate entry), **When** the response is returned, **Then** the HTTP status is 409.
6. **Given** a request exceeds rate limits, **When** the response is returned, **Then** the HTTP status is 429 with `Retry-After` header.
7. **Given** a request encounters an unexpected fault, **When** the response is returned, **Then** the HTTP status is 500.
8. **Given** a service is temporarily unavailable, **When** the response is returned, **Then** the HTTP status is 503.

---

### User Story 4 - Safe Diagnostic Logging with TraceId (Priority: P1)

A system operator troubleshooting a production issue needs to correlate a client error response with server-side logs using a trace identifier, without exposure of credentials, tokens, or sensitive payloads.

**Why this priority**: Without safe diagnostics, production issues cannot be debugged without security risk.

**Independent Test**: Can be fully tested by triggering an unexpected fault and verifying the traceId in the response matches a log entry containing safe context (request path, correlation) but no credentials or SQL.

**Acceptance Scenarios**:

1. **Given** an unexpected fault occurs, **When** the error response is returned, **Then** it contains a `traceId` that correlates with a server log entry.
2. **Given** a fault is logged, **When** the log entry is inspected, **Then** it does not contain passwords, tokens, connection strings, or sensitive payloads.
3. **Given** a validation rejection occurs, **When** the log entry is inspected, **Then** it is classified separately from unexpected faults (info/warn vs error level).
4. **Given** a caller cancellation occurs, **When** the log entry is inspected, **Then** it is not classified as a server fault.
5. **Given** an unexpected fault is caught by the exception handler, **When** the diagnostic log is written, **Then** it is written exactly once (no duplicate primary logs from both middleware and MediatR behavior).

---

### User Story 5 - Frontend Error Normalization (Priority: P1)

A frontend developer needs every API client (NSwag-generated, manual, generic) to normalize errors into a single representation containing kind (HTTP/network/cancelled/invalid-response), status, code, traceId, safe message, and field errors.

**Why this priority**: Inconsistent client error handling causes lost status information, false failure notifications, and invisible form errors.

**Independent Test**: Can be fully tested by simulating each error kind (HTTP with various statuses, network failure, cancellation, malformed response) and verifying the normalized output contains the correct kind, status, code, traceId, message, and errors.

**Acceptance Scenarios**:

1. **Given** an API call returns a ProblemDetails error, **When** the client normalizes it, **Then** the result contains `kind: "http"`, the HTTP `status`, the `code`, `traceId`, safe `message`, and optional `errors` map.
2. **Given** an API call fails with a network error, **When** the client normalizes it, **Then** the result contains `kind: "network"`, no fake status code, and a safe connection message.
3. **Given** an API call is cancelled by the caller, **When** the client normalizes it, **Then** the result contains `kind: "cancelled"` and does not trigger a failure toast or retry.
4. **Given** an API call returns a non-JSON body or malformed response, **When** the client normalizes it, **Then** the result contains `kind: "invalid-response"` with the transport status preserved.
5. **Given** an API call returns an empty success body (200 with no content), **When** the client processes it, **Then** it is treated as success, not as an error.
6. **Given** an API call returns a legacy error format (text message, `Error` object, `ApiError`), **When** the client normalizes it, **Then** the status and structure are preserved at the transport boundary during migration.

---

### User Story 6 - Form Error Binding and Presentation (Priority: P2)

A user submitting a form that fails server validation needs to see field-specific errors bound to the correct form fields, a visible form-level fallback for unmapped errors, and preserved entered values.

**Why this priority**: Invisible or incorrectly bound form errors cause user frustration and data loss.

**Independent Test**: Can be fully tested by submitting a form with validation errors and verifying each error appears on the correct field, general errors appear as form-level messages, and previously entered values are preserved.

**Acceptance Scenarios**:

1. **Given** a form submission fails with a field validation error, **When** the error is displayed, **Then** the error message appears under the correct field (full path preserved, e.g., `lines.0.amount`), and the entered value is retained.
2. **Given** a form submission fails with a non-field error, **When** the error is displayed, **Then** a visible form-level error message is shown (not silently logged or hidden).
3. **Given** a form submission fails, **When** the user corrects and resubmits, **Then** the previous field values are preserved and only the corrected fields change.
4. **Given** a form submission fails with multiple field errors, **When** the errors are displayed, **Then** all errors are shown simultaneously, not just the first one.
5. **Given** a server returns a nested path like `Lines[0].Amount`, **When** the Web boundary canonicalizes it, **Then** it becomes `lines.0.amount` and the full path is preserved in the `errors` map (not reduced to just `Amount`).

---

### User Story 7 - Query and Action Error Feedback (Priority: P2)

A user viewing a data grid or triggering an action needs distinct feedback for query failures (persistent error/retry state), mutation failures (one notification per action), and empty data (not-found vs loading).

**Why this priority**: Confusing query errors with empty data, or showing duplicate notifications, degrades user experience.

**Independent Test**: Can be fully tested by simulating query failure, mutation failure, and empty results, and verifying each produces the correct distinct feedback.

**Acceptance Scenarios**:

1. **Given** a data query fails, **When** the error is displayed, **Then** a persistent error state with retry option is shown (distinct from empty/no-data state).
2. **Given** a mutation action fails, **When** the error is displayed, **Then** exactly one notification is shown per action (no duplicate toasts).
3. **Given** a query returns no results, **When** the UI is displayed, **Then** it shows an empty state (not an error).
4. **Given** a query is still loading, **When** the UI is displayed, **Then** a skeleton placeholder is shown (not an error or empty state).

---

### User Story 8 - Session Recovery and Auth State (Priority: P2)

A user whose session expires mid-operation needs automatic detection, a single safe redirect to login, and preservation of intended destination, without the UI appearing functional while all requests fail.

**Why this priority**: Invisible session expiry causes user confusion and lost work.

**Independent Test**: Can be fully tested by simulating a 401 response during an active session and verifying the user is redirected to login with intended destination preserved, and no further API calls succeed with stale credentials.

**Acceptance Scenarios**:

1. **Given** a user's session expires and an API returns 401, **When** the response is received, **Then** the auth state is cleared, the user is redirected to login, and the intended destination is preserved for post-login redirect.
2. **Given** a user is on the login page and credentials are rejected, **When** the error is displayed, **Then** a field or form-level error is shown without redirecting away from login.
3. **Given** a user receives a 403 (forbidden), **When** the response is received, **Then** permission feedback is shown without clearing auth state or redirecting to login.
4. **Given** a session expires, **When** the UI detects it, **Then** pending requests are cancelled without showing spurious failure notifications.

---

### User Story 9 - Rendering Error Boundaries (Priority: P2)

A user encountering a React rendering error needs a recovery boundary that prevents the entire app from crashing, with a clear recovery action.

**Why this priority**: Unhandled rendering errors crash the UI with no recovery path.

**Independent Test**: Can be fully tested by triggering a component rendering error and verifying the error boundary catches it and displays a recovery UI.

**Acceptance Scenarios**:

1. **Given** a component throws during rendering, **When** the error boundary catches it, **Then** a fallback UI is displayed with a recovery action (retry navigation or return to home).
2. **Given** an async error occurs outside a render, **When** the error is thrown, **Then** it is handled separately from synchronous render errors.

---

### User Story 10 - Outbox Durable Recovery (Priority: P2)

A system operator investigating stalled background processing needs to see messages stuck in `Processing` state, recover them without duplicating financial effects, and verify idempotency of reprocessing.

**Why this priority**: Stalled Outbox messages can silently block financial event processing without any visible failure.

**Independent Test**: Can be fully tested by simulating a crash during Outbox processing and verifying the stalled message is recovered, reprocessed without duplicate effects, and exposed in monitoring.

**Acceptance Scenarios**:

1. **Given** an Outbox message is claimed with a 15-minute processing lease, **When** the processor completes successfully, **Then** the lease is released and the message transitions to Completed.
2. **Given** an Outbox message is claimed and the processor crashes before lease renewal, **When** the lease expires (15 minutes, globally configurable), **Then** the claim is reclaimed atomically and the message becomes eligible for reprocessing.
3. **Given** a stalled message is reprocessed, **When** the effect is applied, **Then** idempotency is preserved at the financial-effect boundary (no duplicate journal entries, payments, or other financial effects).
4. **Given** a message has exceeded maximum retries, **When** it reaches terminal failure, **Then** it is exposed to authorized operators with failure details and does not block other messages.
5. **Given** an Outbox message is being processed by one worker, **When** another worker attempts to process the same message, **Then** the claim is exclusive (no concurrent processing).
6. **Given** an active processor renews its lease before expiry, **When** the renewal succeeds, **Then** the lease duration resets and processing continues without interruption.

---

## Functional Requirements *(mandatory)*

### Application Layer

| ID | Requirement | Priority |
|----|-------------|----------|
| FR-A1 | `Result` and `Result<T>` MUST carry `Code`, `Category`, `Message`, and optional `Target` members | P1 |
| FR-A2 | Application and Domain layers MUST NOT reference HTTP types, ASP.NET types, or status codes | P1 |
| FR-A3 | Business failure codes MUST use `Module.Cause` format and maintain stable meaning across translations | P1 |
| FR-A4 | A shared error code catalog MUST be established with stable identifiers for all recognized failure causes | P1 |
| FR-A5 | FluentValidation exception mechanism MUST be preserved as a recognized boundary translation | P1 |
| FR-A6 | Unexpected faults MUST propagate after cleanup; they MUST NOT be swallowed into `Result` failures | P1 |

### Web/API Layer

| ID | Requirement | Priority |
|----|-------------|----------|
| FR-W1 | All error responses MUST use `application/problem+json` with `type`, `title`, `status`, `detail`, `instance`, `code`, `traceId`, and optional `errors` | P1 |
| FR-W2 | A shared Web conversion MUST translate `Result` failures and recognized exceptions to ProblemDetails | P1 |
| FR-W3 | HTTP status mapping MUST follow Constitution XIII: 400 (validation), 401 (auth), 403 (forbidden), 404 (not found), 409 (conflict), 429 (rate limit — reserved classification, not implemented by this spec), 500 (internal), 503 (unavailable) | P1 |
| FR-W4 | Binding failures, authentication rejections, authorization denials, and API routing failures MUST follow the same ProblemDetails contract | P1 |
| FR-W5 | Already-started or aborted responses MUST NOT be rewritten to fabricate ProblemDetails | P1 |
| FR-W6 | Required protocol headers (`WWW-Authenticate`, `Retry-After`) MUST be preserved | P1 |
| FR-W7 | Exception middleware MUST be placed early enough to cover the intended pipeline | P1 |
| FR-W8 | In .NET 10, diagnostic suppression MUST be accounted for; assumed logging behavior MUST be validated | P1 |
| FR-W9 | OpenAPI documentation MUST include error schemas alongside success schemas | P1 |

### Frontend Layer

| ID | Requirement | Priority |
|----|-------------|----------|
| FR-F1 | All API clients (NSwag, manual, generic) MUST normalize errors into a single representation with `kind`, `status`, `code`, `traceId`, `message`, and `errors` | P1 |
| FR-F2 | Network failures, cancellations, and malformed responses MUST remain distinguishable from HTTP rejection | P1 |
| FR-F3 | Transport status MUST be authoritative; malformed payloads MUST NOT change it or throw parsing exceptions | P1 |
| FR-F4 | Field validation errors MUST bind to canonical frontend field paths with preserved indexes (e.g., `lines.0.amount`), mapped from backend paths at the shared Web boundary | P1 |
| FR-F5 | Non-field errors MUST produce a visible form-level fallback (not silently logged) | P1 |
| FR-F6 | Each failed action MUST have exactly one notification owner (no duplicate toasts) | P1 |
| FR-F7 | Query failures MUST show persistent error/retry state distinct from empty/not-found | P2 |
| FR-F8 | Caller cancellation MUST end pending state without spurious failure toast or retry | P2 |
| FR-F9 | 401 during session MUST trigger central auth-state recovery with one login redirect | P1 |
| FR-F10 | 403 MUST show permission feedback without clearing auth state | P1 |
| FR-F11 | React Error Boundaries MUST catch rendering errors with recovery UI | P2 |
| FR-F12 | Legacy error adapters MUST be removed only after producers/consumers have migrated and evidence is recorded | P2 |

### Background Processing

| ID | Requirement | Priority |
|----|-------------|----------|
| FR-B1 | Outbox claim ownership MUST be exclusive with a 15-minute processing lease (globally configurable) | P1 |
| FR-B2 | Active processors MUST renew their lease through a heartbeat mechanism before expiry | P1 |
| FR-B3 | Recovery eligibility MUST be based on lease expiry, not elapsed time since processing started | P1 |
| FR-B4 | Expired claims MUST be reclaimed atomically; stale workers MUST NOT complete reclaimed claims | P1 |
| FR-B5 | Idempotency MUST be enforced at the financial-effect boundary to prevent duplicate postings | P1 |
| FR-B6 | Terminal failure MUST be exposed to authorized operators | P2 |
| FR-B7 | Cancellation MUST NOT consume retry budget as business failure | P2 |
| FR-B8 | Outbox monitoring MUST display `Processing` state counts alongside `Pending` and `Failed` | P2 |

### Diagnostics

| ID | Requirement | Priority |
|----|-------------|----------|
| FR-D1 | Unexpected faults MUST have a single diagnostic logging owner | P1 |
| FR-D2 | Duplicate primary exception logs MUST be avoided | P1 |
| FR-D3 | Logs MUST exclude credentials, tokens, and sensitive payloads | P1 |
| FR-D4 | Public messages MUST exclude stack traces, SQL, and internal exception details | P1 |
| FR-D5 | Expected rejection and caller cancellation MUST be distinguished from server faults in logs | P1 |

---

## Success Criteria *(mandatory)*

1. All API error responses follow a single ProblemDetails contract with stable `code`, `traceId`, and field `errors` — verifiable by triggering each error category and inspecting the response body.
2. HTTP status codes accurately reflect failure cause (no internal errors classified as 400) — verifiable by mapping each `Result` category to its HTTP status.
3. All frontend clients normalize errors into one representation preserving `kind`, `status`, `code`, `traceId`, and `errors` — verifiable by simulating HTTP, network, cancellation, and malformed responses.
4. Form errors bind to correct field paths with preserved indexes, and non-field errors produce visible form-level feedback — verifiable by submitting forms with validation errors.
5. Session expiry (401) triggers automatic auth-state recovery with single login redirect — verifiable by simulating token expiration.
6. Diagnostic logs correlate with client `traceId` and exclude sensitive data — verifiable by triggering unexpected faults and inspecting logs.
7. Outbox stalled messages are detected and recoverable without duplicate financial effects — verifiable by simulating crash during processing.
8. No regression in existing success contracts — verifiable by running full backend test suite and frontend lint/build.
9. All affected failure paths are documented with classification, payload, feedback, diagnostics, and verification result — verifiable by inspecting migration evidence.

---

## Key Entities

| Entity | Description |
|--------|-------------|
| `Result` / `Result<T>` | Application-layer outcome carrying `Code`, `Category`, `Message`, `Target` |
| `ErrorCategory` | Enum: Validation, Authorization, NotFound, Conflict, BusinessRule, Internal |
| `ProblemDetails` | API error response body with `type`, `title`, `status`, `detail`, `instance`, `code`, `traceId`, `errors` |
| `NormalizedError` | Frontend error representation with `kind`, `status`, `code`, `traceId`, `message`, `errors` |
| `ErrorKind` | Enum for frontend: Http, Network, Cancelled, InvalidResponse |
| `OutboxMessage` | Background job with states: Pending, Processing, Completed, Failed; includes lease expiry timestamp for claim recovery |
| `ErrorCodes` | Shared catalog of stable error code constants at `src/Application/Common/Errors/ErrorCodes.cs` with nested static classes per module (`Module.Cause` format) |

---

## Assumptions

1. The existing `Result` and `Result<T>` types in `src/Application/Common/Models/Result.cs` will be extended with `Code`, `Category`, `Message`, and `Target` members — not replaced.
2. The existing `ProblemDetailsExceptionHandler` in `src/Web/Infrastructure/ProblemDetailsExceptionHandler.cs` will be extended to handle the full ProblemDetails contract — not replaced.
3. The existing NSwag client generation will continue to be the primary frontend API client strategy.
4. The existing Outbox infrastructure in `src/Infrastructure/Services/OutboxProcessorService.cs` will be extended with claim recovery — not replaced.
5. The existing FluentValidation pipeline in `src/Application/Common/Behaviours/ValidationBehaviour.cs` will continue to throw `ValidationException` as the recognized boundary translation.
6. Migration will be incremental: normalize consumers first, then introduce shared conversion, then migrate producers — following the sequence in `docs/error-handling.md`.
7. Frontend testing remains manual (lint/build only) per AGENTS.md governance override — no automated frontend tests will be added.
8. The DEP-027 TDD exception for Spec 045 remains unchanged; this spec does not affect that exception.
9. Existing success contracts and status codes will be preserved throughout migration.
10. Arabic messages will be used for all user-facing error content; English codes are machine identifiers only.

---

## Dependencies

- **Constitution XIII** (`docs/error-handling.md`): Governs error handling guarantees and HTTP semantics.
- **DEP-029** (`docs/decision-records/DEP-029-unified-error-handling.md`): Decision record establishing the standard.
- **DEP-028**: Posting architecture; recovery work must preserve financial effects and idempotency.
- **DEP-027**: TDD exception for Spec 045; remains unchanged.
- **Existing codebase**: `Result.cs`, `ProblemDetailsExceptionHandler.cs`, `ValidationBehaviour.cs`, `AuthorizationBehaviour.cs`, `UnhandledExceptionBehaviour.cs`, `OutboxProcessorService.cs`, frontend shared API modules.

---

## Scope Boundaries

### In Scope

- Structured application failures with Code/Category/Message/Target
- Unified ProblemDetails API contract
- HTTP status classification per Constitution XIII
- Safe diagnostic logging with traceId
- Frontend error normalization across all clients
- Form error binding and presentation
- Query/mutation error feedback
- Session recovery and auth state
- Rendering error boundaries
- Outbox durable recovery with idempotency
- OpenAPI error schema documentation
- Migration evidence for all affected paths

### Out of Scope

- RBAC enforcement wiring (tracked separately in DEP-026)
- Financial posting pipeline changes (governed by DEP-028)
- Frontend automated testing (governed by AGENTS.md override)
- New feature development (this is cross-cutting infrastructure)
- Schema changes or new database tables (unless Outbox claim recovery requires it)
- Changes to the existing `Result` success path

---

## Risk and Mitigation

| Risk | Impact | Mitigation |
|------|--------|------------|
| Breaking existing API consumers during payload migration | High | Migrate consumers to normalizer before changing producers; retain legacy adapters during transition |
| .NET 10 diagnostic suppression causing silent logging gaps | Medium | Validate `SuppressDiagnosticsCallback` behavior; add explicit logging in exception handler |
| Outbox claim recovery introducing duplicate financial effects | High | Prove idempotency with evidence before deploying; test with concurrent workers |
| Frontend normalizer regression losing transport status | Medium | Preserve status at boundary; test with HTTP, network, cancellation, malformed cases |
| Migration scope creep delaying delivery | Medium | Follow incremental sequence in `docs/error-handling.md`; document affected paths per task |

---

## Verification Governance

- Backend test gates: run relevant project's test suite per cycle; full 5-project suite at convergence/pre-merge.
- Frontend: `npm run lint` and `npm run build` only; no automated tests (AGENTS.md governance override).
- Manual evidence: record actual commands/results for each affected failure path in the implementing plan.
- Spec 045 exception (DEP-027): work in `specs/045-payments-group/` scope follows its own verification rules.
- Constitution compliance: check at plan gate, re-check after design, verify at convergence.
