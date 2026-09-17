# Frontend Error Contract: NormalizedError

**Date**: 2026-09-14
**Spec**: [spec.md](../spec.md)

## NormalizedError Type

```typescript
interface NormalizedError {
  kind: 'http' | 'network' | 'cancelled' | 'invalid-response';
  status?: number;        // HTTP status code (absent for network/cancelled)
  code?: string;          // Stable error code (e.g., "Request.ValidationFailed")
  traceId?: string;       // Correlation ID for server diagnostics
  message: string;        // Safe Arabic user-facing message
  errors?: Record<string, string[]>;  // Field path → messages (HTTP only)
}
```

## Normalization Rules

### From NSwag `SwaggerException`

```typescript
// SwaggerException shape: { status: number, response: string, message: string }
function fromSwaggerException(err: any): NormalizedError {
  const status = err.status;
  const body = tryParseJSON(err.response);
  return {
    kind: 'http',
    status,
    code: body?.code,
    traceId: body?.traceId,
    message: body?.detail || body?.title || err.message || 'خطأ غير معروف',
    errors: body?.errors,
  };
}
```

### From manual `api` module `Error`

```typescript
// api module throws: Error(responseText)
// Attempt JSON parse; preserve transport status from fetch
function fromApiError(err: any, status?: number): NormalizedError {
  const body = tryParseJSON(err.message);
  if (body?.status) {
    // Server returned ProblemDetails
    return {
      kind: 'http',
      status: body.status,
      code: body.code,
      traceId: body.traceId,
      message: body.detail || body.title || err.message,
      errors: body.errors,
    };
  }
  if (status) {
    return { kind: 'http', status, message: err.message || 'خطأ غير معروف' };
  }
  return { kind: 'invalid-response', message: err.message || 'استجابة غير صالحة' };
}
```

### From network failure

```typescript
function fromNetworkError(err: TypeError): NormalizedError {
  return {
    kind: 'network',
    message: 'تعذر الاتصال بالخادم. تحقق من اتصال الشبكة وأعد المحاولة.',
  };
}
```

### From cancellation (AbortController)

```typescript
function fromCancelled(): NormalizedError {
  return {
    kind: 'cancelled',
    message: '',  // No message — not a failure
  };
}
```

## Error Presentation Rules

### Field validation errors

- Bind to canonical field path from `errors` map
- Use React Hook Form `setError` with full path (e.g., `lines.0.amount`)
- Preserve entered values on all fields
- Show all errors simultaneously, not just the first

### Non-field errors

- Show visible form-level error message (not silently logged)
- Use `setError('root', { message })` or equivalent visible container

### Query failures

- Persistent error/retry state (not empty state)
- Distinct from "no data" empty state
- Retry button visible

### Mutation failures

- One notification owner per action (no duplicate toasts)
- `handleApiError` for forms; `handleLifecycleError` for lifecycle transitions

### Session expiry (401)

- Clear auth state (token, user info)
- Redirect to login with intended destination preserved
- Cancel pending requests without spurious failure notifications
- Do not show permission feedback (that's 403, not 401)

### Forbidden (403)

- Show permission feedback message
- Do NOT clear auth state
- Do NOT redirect to login

### Caller cancellation

- End pending state
- Do NOT show failure toast
- Do NOT trigger retry

### Rendering errors

- React Error Boundary catches component throw
- Show fallback UI with recovery action (retry navigation, return home)
- Async errors outside render handled separately

## Migration Strategy

1. **Phase 1**: Add `normalizeError()` function; adapt `parseApiError` to produce `NormalizedError`
2. **Phase 2**: Update `handleApiError` and `handleLifecycleError` to consume `NormalizedError`
3. **Phase 3**: Add 401 detection to `auth-fetch.ts` and `patch-fetch.ts`
4. **Phase 4**: Add `ErrorBoundary` component to `main.tsx` or `App.tsx`
5. **Phase 5**: Remove legacy adapters after all producers/consumers migrated

Legacy adapters (current `parseApiError`, `handleApiError`, `handleLifecycleError`) remain during migration. Removal requires evidence that all error paths have been migrated.
