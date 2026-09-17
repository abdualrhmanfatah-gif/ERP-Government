# API Error Contract: ProblemDetails

**Date**: 2026-09-14
**Spec**: [spec.md](../spec.md)

## Response Format

All error responses MUST use `application/problem+json` content type with the following shape:

```json
{
  "type": "about:blank",
  "title": "بيانات غير صالحة",
  "status": 400,
  "detail": "راجع الحقول المحددة ثم أعد المحاولة.",
  "instance": "/api/Items",
  "code": "Request.ValidationFailed",
  "traceId": "abc123def456",
  "errors": {
    "name": ["اسم الصنف مطلوب"],
    "lines.0.amount": ["المبلغ مطلوب", "يجب أن يكون المبلغ أكبر من صفر"]
  }
}
```

## Fields

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `type` | `string` | Yes | Always `about:blank` (RFC 9457 default) |
| `title` | `string` | Yes | Short, safe Arabic summary |
| `status` | `integer` | Yes | HTTP status code |
| `detail` | `string` | Yes | Safe Arabic explanation with recovery guidance |
| `instance` | `string` | Yes | Safe request path (no query strings, no sensitive data) |
| `code` | `string` | Yes | Stable machine-readable error code (`Module.Cause` format) |
| `traceId` | `string` | Yes | Opaque identifier correlating with server diagnostic logs |
| `errors` | `object` | No | Map of canonical field paths to arrays of safe Arabic messages |

## Error Code Catalog

### Transport/Request codes

| Code | HTTP Status | Description |
|------|-------------|-------------|
| `Request.ValidationFailed` | 400 | Input validation failed |
| `Request.NotFound` | 404 | Requested resource does not exist |
| `Request.Forbidden` | 403 | Authenticated but not authorized |
| `Request.Unauthorized` | 401 | Not authenticated |
| `Request.ConcurrencyConflict` | 409 | Optimistic concurrency violation |
| `Request.RateLimitExceeded` | 429 | Rate limit exceeded (reserved — not implemented by this spec) |
| `Request.InternalError` | 500 | Unexpected server fault |
| `Request.ServiceUnavailable` | 503 | Temporary unavailability |

### Business codes (examples — full catalog in ErrorCodes.cs)

| Code | HTTP Status | Module |
|------|-------------|--------|
| `Budgets.InsufficientAvailability` | 400 | Budgeting |
| `Budgets.OverrunBlocked` | 400 | Budgeting |
| `Budgets.FiscalYearClosed` | 400 | Budgeting |
| `Parties.DuplicateCode` | 400 | Parties |
| `Parties.NotFound` | 404 | Parties |
| `Payments.InsufficientBalance` | 400 | Payments |
| `Payments.InvalidAccount` | 400 | Payments |

## Field Path Rules

- Canonical format: `camelCase` with dot notation for nesting
- Array indexes use dot notation: `lines.0.amount` (not `Lines[0].Amount`)
- No path reduction to final segment: `lines.0.amount` (not `amount`)
- Non-field errors: no entry in `errors` map; `detail` provides form-level guidance
- Backend paths canonicalized at Web boundary (shared conversion function)

## HTTP Status Classification

| Cause | Status | Notes |
|-------|--------|-------|
| Validation failure | 400 | `errors` map populated |
| Business rule rejection | 400 | Specific `code`, no `errors` map |
| Unauthenticated | 401 | `WWW-Authenticate` header required |
| Forbidden | 403 | No internal policy details in response |
| Not found | 404 | Even for mutations (e.g., DELETE of missing resource) |
| Concurrency conflict | 409 | Optimistic token mismatch |
| Rate limit exceeded | 429 | Reserved; `Retry-After` header when implemented |
| Unexpected fault | 500 | `traceId` for correlation; no stack traces |
| Temporary unavailability | 503 | `Retry-After` header |

## OpenAPI Schema

Error schemas must be documented in the shared OpenAPI document alongside success schemas. The `429` response is documented as a reserved classification on the shared error schema, not on individual endpoints (until rate limiting is implemented).

## Compatibility

- Existing `ValidationProblemDetails` from ASP.NET is a superset; our contract adds `code`, `traceId`, `errors`
- Existing `ApiExceptionOperationTransformer.cs` must be updated to include the new fields
- NSwag client regeneration required after schema changes
- Success responses (200/204) remain unchanged
