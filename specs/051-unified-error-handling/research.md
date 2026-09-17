# Research: Unified Error Handling

**Date**: 2026-09-14
**Spec**: [spec.md](./spec.md)

## R1: Extending Result<T> with structured error fields

**Decision**: Add `Code`, `Category`, `Message`, and `Target` properties to `Result`/`Result<T>` as optional members alongside the existing `Errors` string array.

**Rationale**: The current `Result` carries only `string[] Errors` with no structure. Adding structured fields preserves backward compatibility (existing `Result.Failure("message")` calls continue to work) while enabling the new contract. The `Errors` array is retained for transitional callers; new code uses `Code`/`Category`/`Message`/`Target`.

**Alternatives considered**:
- *Replace `Errors` with structured `ApplicationError` objects*: Rejected — breaks all existing call sites simultaneously, requiring a big-bang migration.
- *New `StructuredResult<T>` type*: Rejected — adds a parallel type that handlers must choose between; increases cognitive load without benefit.
- *Extension methods on existing `Result`*: Rejected — cannot add properties via extension methods; would require wrapper classes.

**Implementation approach**:
- Add to `Result`: `string? Code`, `ErrorCategory? Category`, `string? Message`, `string? Target`
- Add `ErrorCategory` enum: `Validation`, `Authorization`, `NotFound`, `Conflict`, `BusinessRule`, `Internal`
- Add static factory overloads: `Result.Failure(code, category, message, target?)`
- Existing `Result.Failure(string[] errors)` creates `Category = Validation` with joined message for backward compat

## R2: Error code catalog location and format

**Decision**: Single file `src/Application/Common/Errors/ErrorCodes.cs` with nested static classes per module.

**Rationale**: Centralized catalog is discoverable, prevents drift, and aligns with the existing `Application/Common` shared convention. Nested static classes provide module scoping (`ErrorCodes.Budgets.InsufficientAvailability`) without separate files per code.

**Alternatives considered**:
- *Per-module `ErrorCodes.cs` files*: Rejected —分散的文件难以搜索，容易产生重复或冲突的代码。
- *Separate Shared/Errors project*: Rejected — adds a project reference dependency for a single file of constants.

**Format**: `"Module.Cause"` string constants. Example: `"Budgets.InsufficientAvailability"`, `"Parties.DuplicateCode"`, `"Request.ValidationFailed"`.

## R3: ProblemDetails contract and field path canonicalization

**Decision**: Extend ASP.NET `ProblemDetails` with `code`, `traceId`, and `errors` (Dictionary<string, string[]>) at the Web boundary. Canonicalize backend paths to camelCase dot-notation at conversion time.

**Rationale**: ASP.NET's `ProblemDetails` is the RFC 9457 standard. Extending it with `code` and `traceId` follows the Constitution XIII contract. The `errors` map uses `string[]` values (array of messages per field) to support multiple validations per field.

**Path canonicalization rules**:
- `Lines[0].Amount` → `lines.0.amount` (PascalCase to camelCase, brackets to dot notation)
- `Name` → `name` (simple camelCase)
- No path reduction to final segment
- Non-field errors use empty string key `""` or `null` key in the errors map

**Alternatives considered**:
- *Project-specific problem-type URIs*: Deferred per clarification — `about:blank` used initially.
- *Per-error machine codes*: Deferred per clarification — Arabic messages only in initial contract.

## R4: .NET 10 exception handler diagnostic suppression

**Decision**: Add explicit `ILogger.LogError` call in `ProblemDetailsExceptionHandler` for unexpected faults, and configure `SuppressDiagnosticsCallback` to return `false` for exceptions we handle.

**Rationale**: .NET 10 suppresses `IExceptionHandler` diagnostic logs by default. Since `ProblemDetailsExceptionHandler` returns `true` for all exceptions (including the catch-all 500), the middleware exception log is suppressed. We must log explicitly to maintain diagnostics.

**Alternatives considered**:
- *Remove the catch-all 500 arm*: Rejected — would cause unhandled exceptions to produce HTML/empty responses instead of ProblemDetails.
- *Return `false` from the handler*: Rejected — would cause the middleware to also write a response, producing duplicate responses.

**Implementation approach**:
- Log unexpected faults at `Error` level with `traceId` before writing the response
- Log expected rejections (validation, auth) at `Information`/`Warning` level
- Caller cancellation logged at `Debug` level, not `Error`
- Validate `SuppressDiagnosticsCallback` behavior at runtime before relying on it

## R5: Outbox heartbeat-based lease management

**Decision**: Add `LeaseExpiry` timestamp to `OutboxMessages`. Processors renew lease via heartbeat. Stalled claims detected by comparing `LeaseExpiry` to current time.

**Rationale**: Current Outbox only queries `Pending` messages. If a processor crashes after setting status to `Processing`, the message is orphaned. Heartbeat-based lease with configurable 15-minute expiry provides automatic recovery without distributed locks.

**Implementation approach**:
- On claim: set `Status = Processing`, `LeaseExpiry = UTC now + 15 minutes`
- During processing: renew `LeaseExpiry` every 5 minutes (configurable)
- On completion: set `Status = Completed`, clear `LeaseExpiry`
- Recovery query: `WHERE Status = Processing AND LeaseExpiry < UTC NOW`
- Atomic claim: `UPDATE ... SET Status = Processing, LeaseExpiry = ... WHERE Id = @id AND (Status = Pending OR (Status = Processing AND LeaseExpiry < UTC NOW))`
- Idempotency: financial events use the Outbox message ID as idempotency key at the effect boundary

**Alternatives considered**:
- *Distributed lock (Redis/SQL lock)*: Rejected — adds infrastructure dependency; heartbeat is sufficient for single-node deployment.
- *Claim-based without heartbeat*: Rejected — cannot distinguish slow-but-active from crashed processors.

## R6: Frontend error normalization strategy

**Decision**: Create a single `NormalizedError` type and `normalizeError()` function that all clients (NSwag, manual `api`, generic `fetch`) pipe through.

**Rationale**: Current clients throw incompatible shapes (`SwaggerException` with `{ status, response }`, `Error(text)` from `api`, `ApiError` from manual). Normalization at the transport boundary preserves status before parsing attempts, preventing the "lost status" bug documented in the audit.

**`NormalizedError` shape**:
```typescript
interface NormalizedError {
  kind: 'http' | 'network' | 'cancelled' | 'invalid-response';
  status?: number;
  code?: string;
  traceId?: string;
  message: string;
  errors?: Record<string, string[]>;
}
```

**Normalization rules**:
- NSwag `SwaggerException`: extract `status` from `response.status`, parse body as ProblemDetails
- Manual `api` `Error(text)`: attempt JSON parse; if fails, `kind: 'invalid-response'`
- Network failure: `kind: 'network'`, no status
- Cancellation (AbortController): `kind: 'cancelled'`, no status
- Empty success (200/204 with no body): not an error (handled before normalizer)

**Alternatives considered**:
- *Fix each client individually*: Rejected — would create N parallel normalizers; no single contract.
- *Replace manual `api` with NSwag only*: Deferred — legacy clients must migrate first per DEP-029.
