# API Contracts: Budget Officer Workspace

**Feature**: 021-budget-officer-workspace
**Date**: 2026-09-06

## Overview

This feature consumes existing backend endpoints. No new endpoints are created. The contracts below document the API surface used by the UI.

## Endpoints Consumed

### Budgets

| Method | Path | Used By | Notes |
|--------|------|---------|-------|
| GET | /api/Budgets | BudgetsListPage | List with filters (fiscalYearId, fundId, status) |
| GET | /api/Budgets/{id} | BudgetDetailPage | Single budget with metadata |
| POST | /api/Budgets | BudgetCreatePage | Create draft |
| PUT | /api/Budgets/{id} | BudgetDetailPage | Update draft |
| POST | /api/Budgets/{id}/submit | LifecycleActions | Submit for approval |
| POST | /api/Budgets/{id}/approve | LifecycleActions | Approve |
| POST | /api/Budgets/{id}/activate | LifecycleActions | Activate |
| POST | /api/Budgets/{id}/suspend | LifecycleActions | Suspend |
| POST | /api/Budgets/{id}/close | LifecycleActions | Close |
| POST | /api/Budgets/{id}/cancel | LifecycleActions | Cancel |

### Budget Items

| Method | Path | Used By | Notes |
|--------|------|---------|-------|
| GET | /api/Budgets/{budgetId}/items/tree | BudgetDetailPage | Item tree for display |
| POST | /api/Budgets/{budgetId}/items | BudgetDetailPage | Add item to tree |
| PUT | /api/Budgets/items/{itemId} | BudgetItemEditPage | Edit item details |
| DELETE | /api/Budgets/items/{itemId} | BudgetDetailPage | Delete item (Draft only) |

### Appropriations

| Method | Path | Used By | Notes |
|--------|------|---------|-------|
| GET | /api/Appropriations | AppropriationsListPage | List by budgetId/budgetItemId |
| GET | /api/Appropriations/{id} | AppropriationCreatePage | Detail for edit |
| GET | /api/Appropriations/availability?budgetItemId={id} | AvailabilityIndicator | Item-level availability |
| POST | /api/Appropriations | AppropriationCreatePage | Create |
| PUT | /api/Appropriations/{id} | AppropriationCreatePage | Update (Draft only) |
| DELETE | /api/Appropriations/{id} | AppropriationsListPage | Delete (Draft only) |
| POST | /api/Appropriations/{id}/submit | LifecycleActions | Submit |
| POST | /api/Appropriations/{id}/approve | LifecycleActions | Approve |
| POST | /api/Appropriations/{id}/activate | LifecycleActions | Activate |
| POST | /api/Appropriations/{id}/suspend | LifecycleActions | Suspend |
| POST | /api/Appropriations/{id}/close | LifecycleActions | Close |
| POST | /api/Appropriations/{id}/cancel | LifecycleActions | Cancel |

### Encumbrances

| Method | Path | Used By | Notes |
|--------|------|---------|-------|
| GET | /api/Encumbrances | EncumbrancesListPage | List by appropriationId |
| GET | /api/Encumbrances/{id} | EncumbranceCreatePage | Detail for edit |
| GET | /api/Encumbrances/availability?appropriationId={id} | AvailabilityIndicator | Encumbrance-level availability |
| POST | /api/Encumbrances | EncumbranceCreatePage | Create |
| POST | /api/Encumbrances/{id}/submit | LifecycleActions | Submit |
| POST | /api/Encumbrances/{id}/approve | LifecycleActions | Approve |
| POST | /api/Encumbrances/{id}/activate | LifecycleActions | Activate |
| POST | /api/Encumbrances/{id}/release | LifecycleActions | Partial release |
| POST | /api/Encumbrances/{id}/close | LifecycleActions | Close |
| POST | /api/Encumbrances/{id}/cancel | LifecycleActions | Cancel |
| POST | /api/Encumbrances/{id}/reverse | LifecycleActions | Reverse (requires reason) |

## Client Classes Used

All from `src/Web/ClientApp/src/web-api-client.ts` (NSwag-generated):

- `BudgetsClient` — budget CRUD + lifecycle
- `AppropriationsClient` — appropriation CRUD + lifecycle + availability
- `EncumbrancesClient` — encumbrance CRUD + lifecycle + availability

## Error Contract

All endpoints return `ProblemDetails` on error:

```typescript
interface ProblemDetails {
  type?: string;
  title?: string;
  status: number;       // 400, 401, 403, 404, 409, 500
  detail?: string;
  errors?: Record<string, string[]>;
}
```

Concurrency conflicts return 409 with `detail` describing the conflict.

## Monthly Plan (Client-Side Only)

No API contract. Data stored in localStorage:

- Key: `monthly-plan-{budgetItemId}`
- Value: JSON `{ budgetItemId, months: number[12], updatedAt }`
