# Frontend API Contract: Budget Classifications

**Date**: 2026-09-04

## Cache Key Factory

```typescript
budgetingKeys.budgetClassifications.all   // ['budgeting', 'budget-classifications']
budgetingKeys.budgetClassifications.tree() // ['budgeting', 'budget-classifications', 'tree']
budgetingKeys.budgetClassifications.detail(id) // ['budgeting', 'budget-classifications', 'detail', id]
```

## Client Functions (from spec #1 shared/client.ts)

| Function | HTTP | Endpoint | Returns |
|----------|------|----------|---------|
| `budgetClassificationsClient.tree()` | GET | `/api/BudgetClassifications/tree` | `BudgetClassificationTreeDto[]` |
| `budgetClassificationsClient.getById(id)` | GET | `/api/BudgetClassifications/{id}` | `BudgetClassificationDto` |
| `budgetClassificationsClient.create(data)` | POST | `/api/BudgetClassifications` | `{ id: number }` |
| `budgetClassificationsClient.update(id, data)` | PUT | `/api/BudgetClassifications/{id}` | `void` |
| `budgetClassificationsClient.toggleActive(id, data)` | POST | `/api/BudgetClassifications/{id}/toggle-active` | `void` |

## Query Hooks

| Hook | Query Key | Fetches | Enabled |
|------|-----------|---------|---------|
| `useClassificationsTree()` | `budgetClassifications.tree()` | Full tree | Always |
| `useClassificationDetail(id)` | `budgetClassifications.detail(id)` | Single node | `Number.isFinite(id)` |

## Mutation Hooks

| Hook | Invalidates | Optimistic? |
|------|-------------|-------------|
| `useCreateClassification()` | `budgetClassifications.all` | No |
| `useUpdateClassification()` | `budgetClassifications.all` | No |
| `useToggleClassificationActive()` | `budgetClassifications.all` | No |

## Error Handling

All errors surface through `ApiError` class (from shared/client.ts):
- 400: Validation errors → display field-level errors from ProblemDetails
- 401/403: Permission errors → toast notification
- 404: Not found → toast notification
- 409: RowVersion conflict → error toast + refetch tree

## Cycle Prevention Algorithm

```typescript
function getExcludedDescendantIds(
  tree: BudgetClassificationTreeDto[],
  excludeId: number
): Set<number> {
  // BFS from excludeId to collect all descendant IDs
  // Return set of IDs to exclude from parent tree-select
}
```
