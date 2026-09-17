# Error Handling Implementation Standard

**Authority**: [Constitution XIII](../.specify/memory/constitution.md#xiii-error-handling-diagnostics-and-recovery)
and [DEP-029](decision-records/DEP-029-unified-error-handling.md).
**Status**: Fully implemented 2026-09-14. All migratable query/command handlers and endpoints converted. Backend build clean (0 errors, 0 warnings).

Read this guide before changing failure behavior in an endpoint, use case, transport, form,
query, session flow, or background processor. The Constitution owns the guarantees and HTTP
semantics; this guide owns their implementation contract. The [initial audit](error-handling-review-2026-09-14.md)
is historical evidence to re-check, not a certified inventory of current runtime behavior.

## Ownership and current entry points

| Layer | Responsibility | Implementation |
|---|---|---|
| Application | Expected failure code/category/message/target; business invariants | `src/Application/Common/Models/Result.cs`, `src/Application/Common/Errors/ErrorCodes.cs` |
| Application pipeline | Aggregate validation; recognized authorization rejection | `src/Application/Common/Behaviours/ValidationBehaviour.cs`, `AuthorizationBehaviour.cs` |
| Web | Shared result/exception-to-HTTP conversion; boundary failures; diagnostics | `src/Web/Infrastructure/ProblemDetailsExceptionHandler.cs`, `src/Web/Infrastructure/ResultExtensions.cs`, `src/Web/Infrastructure/HttpErrorMapper.cs` |
| OpenAPI | Publish error schemas and status codes alongside success contracts | `src/Web/Infrastructure/ApiExceptionOperationTransformer.cs` |
| Frontend transport | Normalize every client into the same error representation | `src/Web/ClientApp/src/shared/api/query-error.ts`, `src/Web/ClientApp/src/shared/api/types.ts` |
| Frontend presentation | Select field/form/query/action feedback | `src/Web/ClientApp/src/shared/api/result-to-ui.ts` |
| Session recovery | Coordinate rejected credentials and auth state | `src/Web/ClientApp/src/shared/utils/auth-fetch.ts`, `patch-fetch.ts`, `src/Web/ClientApp/src/shared/hooks/useAuth.tsx` |
| Background processing | Durable attempts, stalled work recovery, duplicate-effect protection | `src/Infrastructure/Services/OutboxProcessorService.cs` |

Extend these owners where appropriate. A feature must not introduce an independent transport
error format or another catch-all handler. The names below describe target contracts; they do
not assert that corresponding classes already exist.

## Application failure contract

Use `Result<T>`/`Result` for expected business rejection, with structured errors carrying:

| Member | Meaning |
|---|---|
| `Code` | Stable identifier for a recognized cause; not a translated sentence |
| `Category` | Semantic classification mapped by Web to Constitution XIII's HTTP statuses |
| `Message` | Safe Arabic explanation the caller may display |
| `Target` | Optional public field path; absent for a form/action-level failure |

Keep HTTP status codes and ASP.NET types out of Domain/Application. Preserve the existing central
FluentValidation exception mechanism as a recognized boundary translation. Unexpected faults
propagate after necessary cleanup; translate a database exception only when its cause is known.
Document how multiple failures select one top-level code/category; do not pick a category by
parsing messages or blindly selecting the first error.

Codes use `Module.Cause` for business failures and `Request.Cause` for transport-wide failures.
The catalog is implemented in `src/Application/Common/Errors/ErrorCodes.cs` with nested static
classes per module (Request, Budgets, Parties, Payments, Accounting, Inventory, Procurement,
Revenue, Banking, Security, Organization, Workflow, SecurityApprovalRules, FinancialSettings,
Committees). A code keeps its meaning across translations; renaming it is a consumer contract change.

## HTTP failure contract

Use `application/problem+json` for an API failure body with these members:

| Member | Contract |
|---|---|
| `type` | Stable problem-type identifier; use `about:blank` until a documented specific URI exists |
| `title` | Short, safe Arabic summary |
| `status` | Same status as the actual HTTP response |
| `detail` | Safe Arabic explanation and useful recovery guidance |
| `instance` | Safe request path or occurrence identifier; exclude sensitive query strings |
| `code` | Stable top-level cause |
| `traceId` | Opaque identifier correlated with internal diagnostics |
| `errors` | Optional map of public field paths to arrays of safe messages |

Example validation response (target shape):

```json
{
  "type": "about:blank",
  "title": "بيانات غير صالحة",
  "status": 400,
  "detail": "راجع الحقول المحددة ثم أعد المحاولة.",
  "instance": "/api/Items",
  "code": "Request.ValidationFailed",
  "traceId": "example-trace-id",
  "errors": {
    "name": ["اسم الصنف مطلوب"]
  }
}
```

Map statuses according to Constitution XIII. Expected state/optimistic-concurrency conflicts
use 409; missing resources use 404 even in mutations. A failed permission dependency still
denies access; distinguish that service failure from an ordinary permission rejection without
weakening fail-closed authorization. Known temporary outages may use 503.

Use a shared Web conversion for result failures and recognized exceptions. Cover non-exception
binding/auth/API-routing failures as well. Scope API fallback handling so browser SPA navigation
still works. Keep required protocol headers such as `WWW-Authenticate` and applicable `Retry-After`.
Record aborted/already-started response failures without attempting a replacement JSON body.

Preserve documented success payloads and status codes. A client reads JSON only when a body is
present and appropriate; 200 with an empty body must not turn a completed write into failure.

## Diagnostics and exception boundaries

Place exception middleware early enough to cover the intended pipeline. Assign one primary
logging owner for unexpected request faults, including handlers outside MediatR. In .NET 10,
explicitly account for handled-exception diagnostic suppression; do not assume returning `true`
from `IExceptionHandler` preserves middleware exception logging. Validate this against the
[official behavior](https://learn.microsoft.com/en-us/aspnet/core/breaking-changes/10/exception-handler-diagnostics-suppressed?view=aspnetcore-10.0)
and actual runtime configuration.

Keep safe contextual identifiers and the trace in internal diagnostics; avoid logging complete
request objects. Classify expected validation/authorization rejection separately from unexpected
faults. Caller cancellation is not a 500 retry trigger. A catch block must perform a recognized
translation, recovery, or cleanup followed by propagation. Rollback must preserve the original
failure for diagnostics if rollback itself also fails.

## Frontend normalization and presentation

The shared normalized error exposes `kind` (HTTP/network/cancelled/invalid-response), optional
HTTP `status`, optional `code` and `traceId`, safe `message`, and optional field `errors`.
Every NSwag/manual/generic client reaches that normalizer. Use transport status as authoritative;
malformed payloads must not change it or throw another parsing exception during error handling.
Never invent a server status for a network failure or display raw JSON/HTML/SQL.

During migration, adapt NSwag `status/response`, manual `problemDetails`, legacy message arrays,
JSON strings, and empty/non-JSON bodies. Preserve their status at the transport boundary rather
than attempting to reconstruct it from `Error.message` later. Remove legacy adapters only after
their producers/consumers have migrated and evidence is recorded.

| Context | Presentation and recovery owner |
|---|---|
| Field validation | Real React Hook Form `setError`; full field path; preserve entered values |
| Unmapped/general form rejection | Visible form-level error, not an empty field name/callback |
| Query failure | Persistent error/retry state distinct from empty/not-found data |
| Mutation/action failure | One owner across hook/page, avoiding duplicate notifications |
| 401 during session | Central auth-state recovery with one safe login redirect; exempt login rejection itself |
| 403 | Permission feedback; keep authentication state intact |
| Rendering failure | Appropriate React Error Boundary with recovery; async errors need their own handling |
| Caller cancellation | End pending state without a spurious failure toast or retry |

Map API paths explicitly to form paths. Preserve indexes (`lines.0.amount`); do not strip a
nested path to its last segment. Unknown targets become a visible general error. Zod validates
client input; server rejection enters through `setError`, not by mutating client schemas.
The shared presentation modules decide display behavior; feature callers supply context.

## Retry and durable recovery

Retry only known transient failures with bounded attempts and delay. Do not retry 400/401/403/404
by default. Before any automatic write retry, prove an idempotency mechanism at the effect owner;
the previous attempt may have committed even when its response was lost.

For Outbox, document claim ownership, when a processing claim expires, how abandoned work becomes
eligible again, and how concurrent workers avoid duplicate effects. Expose stalled and terminally
failed work to authorized operators. A crash after publishing but before marking completion must
be safe to recover. Cancellation must not consume retry budget as a business failure. Persist
only metadata needed for the chosen design; schema changes require their own migration review.

## Migration and completion evidence

1. Inventory affected callers/producers in the relevant existing spec/plan. For each, record
   current payload, desired classification, presentation/recovery owner, and compatibility risk.
2. Implement compatible transport normalization and empty-success handling before switching
   producer payloads. Keep changes scoped to the implementing task.
3. Introduce structured application failures and shared Web conversion, then migrate producers.
   Update OpenAPI and regenerate NSwag; never hand-edit the generated client.
4. Connect real form/query/action/session handling and prove no lost or duplicate feedback.
5. Implement durable recovery with evidence of duplicate-effect protection, then remove obsolete
   adapters only after the coverage inventory proves they are unused.

For each affected path, record: classification/code, public payload or transport kind, user
feedback, diagnostic owner/trace, recovery/retry behavior, and verification result. Mark items
N/A only with a path-specific reason. Completion requires all affected paths accounted for;
publishing these documents does not close any audit finding.

Verify field/nested validation, general rejection, auth failures, missing records, concurrency,
safe internal errors, out-of-MediatR diagnostics, network/cancellation behavior, empty success,
and restart recovery as applicable. Backend test/TDD gates and DEP-027 remain governed by the
existing instructions. Follow the user-supplied AGENTS.md frontend override: manual evidence and
lint/build, no added frontend automated tests. The pre-existing conflict with Constitution XI is
recorded in DEP-029, not silently amended here. Full backend gates run at convergence/pre-merge.

Record actual commands/results and manual evidence in the implementing spec/plan. Document any
blocked check without reporting success. Update this guide's entry points and adoption status
when verified implementation replaces the current starting points.
