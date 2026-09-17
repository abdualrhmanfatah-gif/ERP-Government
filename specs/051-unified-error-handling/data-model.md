# Data Model: Unified Error Handling

**Date**: 2026-09-14
**Spec**: [spec.md](./spec.md)

## Entities

### 1. ApplicationError (value object, in-memory only)

Used within `Result`/`Result<T>` — not persisted.

| Field | Type | Nullable | Description |
|-------|------|----------|-------------|
| `Code` | `string` | No | Stable identifier (`Module.Cause` format) |
| `Category` | `ErrorCategory` | No | Semantic classification |
| `Message` | `string` | No | Safe Arabic explanation |
| `Target` | `string` | Yes | Canonical frontend field path (e.g., `lines.0.amount`) |

### 2. ErrorCategory (enum, stored as int)

| Value | Int | HTTP Mapping |
|-------|-----|--------------|
| `Validation` | 0 | 400 |
| `Authorization` | 1 | 403 |
| `NotFound` | 2 | 404 |
| `Conflict` | 3 | 409 |
| `BusinessRule` | 4 | 400 (with specific code) |
| `Internal` | 5 | 500 |

### 3. OutboxMessage (existing entity, extended)

| Field | Type | Nullable | Description |
|-------|------|----------|-------------|
| `Id` | `Guid` | No | Primary key (existing) |
| `TypeName` | `string` | No | Event type name (existing) |
| `Payload` | `string` | No | Serialized event (existing) |
| `Status` | `OutboxMessageStatus` | No | Pending / Processing / Completed / Failed (existing) |
| `RetryCount` | `int` | No | Current retry count (existing) |
| `NextRetryAt` | `DateTime?` | Yes | Next eligible processing time (existing) |
| `CreatedAt` | `DateTime` | No | Creation timestamp (existing) |
| `ProcessedAt` | `DateTime?` | Yes | Completion timestamp (existing) |
| `LeaseExpiry` | `DateTime?` | Yes | **NEW**: Lease expiry timestamp for claim recovery |
| `ErrorMessage` | `string?` | Yes | Last error message (existing) |

**Migration**: Add nullable `LeaseExpiry` column (no default needed — existing rows remain null). Update queries to include `LeaseExpiry` check for `Processing` status recovery.

### 4. ErrorCodes (static catalog, not persisted)

Location: `src/Application/Common/Errors/ErrorCodes.cs`

Structure: nested static classes per module, each containing `const string` fields.

```csharp
public static class ErrorCodes
{
    public static class Request
    {
        public const string ValidationFailed = "Request.ValidationFailed";
        public const string NotFound = "Request.NotFound";
        public const string Forbidden = "Request.Forbidden";
        public const string Unauthorized = "Request.Unauthorized";
        public const string ConcurrencyConflict = "Request.ConcurrencyConflict";
        public const string RateLimitExceeded = "Request.RateLimitExceeded";
        public const string InternalError = "Request.InternalError";
        public const string ServiceUnavailable = "Request.ServiceUnavailable";
    }

    public static class Budgets
    {
        public const string InsufficientAvailability = "Budgets.InsufficientAvailability";
        public const string OverrunBlocked = "Budgets.OverrunBlocked";
        public const string FiscalYearClosed = "Budgets.FiscalYearClosed";
        // ... additional codes per module
    }

    public static class Parties
    {
        public const string DuplicateCode = "Parties.DuplicateCode";
        public const string NotFound = "Parties.NotFound";
        // ...
    }

    public static class Payments
    {
        public const string InsufficientBalance = "Payments.InsufficientBalance";
        public const string InvalidAccount = "Payments.InvalidAccount";
        // ...
    }
}
```

## State Transitions

### OutboxMessage Status

```
Pending ──(claim)──► Processing ──(success)──► Completed
    ▲                     │
    │                     ├──(failure, retries remain)──► Pending (with NextRetryAt)
    │                     │
    │                     ├──(failure, max retries)──► Failed
    │                     │
    │                     └──(lease expired)──► Pending (reclaimed)
    │
    └──(reprocess after Failed)── reset by operator action
```

### Result Category to HTTP Status Mapping

```
Validation    ──► 400 Bad Request
Authorization ──► 403 Forbidden (or 401 if unauthenticated)
NotFound      ──► 404 Not Found
Conflict      ──► 409 Conflict
BusinessRule  ──► 400 Bad Request (with specific business code)
Internal      ──► 500 Internal Server Error
```

### Frontend ErrorKind Mapping

```
HTTP 2xx with body   ──► success (not an error)
HTTP 2xx empty       ──► success (not an error)
HTTP 4xx/5xx         ──► kind: "http"
Network failure      ──► kind: "network"
AbortController      ──► kind: "cancelled"
Non-JSON response    ──► kind: "invalid-response"
```

## Validation Rules

### ApplicationError

| Rule | Field | Constraint |
|------|-------|-----------|
| R1 | `Code` | Must match `Module.Cause` format; must be non-empty; must be a known constant from `ErrorCodes` |
| R2 | `Category` | Must be a valid `ErrorCategory` enum value |
| R3 | `Message` | Must be safe Arabic; must not contain SQL, stack traces, or internal details |
| R4 | `Target` | Optional; if present, must be canonical frontend path (camelCase, dot notation) |

### OutboxMessage (new constraint)

| Rule | Field | Constraint |
|------|-------|-----------|
| R5 | `LeaseExpiry` | Must be UTC; must be null for non-Processing statuses |
| R6 | `LeaseExpiry` | Must be set to UTC now + configurable duration on claim |

## Relationships

```
Result ──contains──► ApplicationError (0..*)
ApplicationError ──references──► ErrorCategory (enum)
ApplicationError ──references──► ErrorCodes (constant lookup)
OutboxMessage ──owns──► LeaseExpiry (nullable timestamp)
```

No new database relationships. No new entities beyond the `LeaseExpiry` column on existing `OutboxMessages`.
