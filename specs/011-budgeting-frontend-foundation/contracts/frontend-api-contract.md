# Frontend API Contract: Budgeting Shared Client

**Feature**: `011-budgeting-frontend-foundation` | **Date**: 2026-09-04

This document defines the typed client functions and cache key factory that the frontend uses to communicate with the budgeting backend. Every function mirrors the backend endpoints defined in `specs/010-rebuild-budgeting-module/contracts/budgeting-api.md`.

## Cache Key Factory

```ts
export const budgetingKeys = {
  all: ['budgeting'] as const,

  budgetTypes: {
    all: [...budgetingKeys.all, 'budget-types'] as const,
    list: (filters?: BudgetTypeFilters) => [...budgetingKeys.budgetTypes.all, 'list', filters] as const,
    detail: (id: number) => [...budgetingKeys.budgetTypes.all, 'detail', id] as const,
  },

  funds: {
    all: [...budgetingKeys.all, 'funds'] as const,
    list: (filters?: FundFilters) => [...budgetingKeys.funds.all, 'list', filters] as const,
    detail: (id: number) => [...budgetingKeys.funds.all, 'detail', id] as const,
  },

  budgetClassifications: {
    all: [...budgetingKeys.all, 'budget-classifications'] as const,
    tree: () => [...budgetingKeys.budgetClassifications.all, 'tree'] as const,
    detail: (id: number) => [...budgetingKeys.budgetClassifications.all, 'detail', id] as const,
  },

  budgets: {
    all: [...budgetingKeys.all, 'budgets'] as const,
    list: (filters?: BudgetFilters) => [...budgetingKeys.budgets.all, 'list', filters] as const,
    detail: (id: number) => [...budgetingKeys.budgets.all, 'detail', id] as const,
    tree: (id: number) => [...budgetingKeys.budgets.all, 'tree', id] as const,
  },

  appropriations: {
    all: [...budgetingKeys.all, 'appropriations'] as const,
    list: (filters?: AppropriationFilters) => [...budgetingKeys.appropriations.all, 'list', filters] as const,
    detail: (id: number) => [...budgetingKeys.appropriations.all, 'detail', id] as const,
    availability: (budgetItemId: number) => [...budgetingKeys.appropriations.all, 'availability', budgetItemId] as const,
  },

  encumbrances: {
    all: [...budgetingKeys.all, 'encumbrances'] as const,
    list: (filters?: EncumbranceFilters) => [...budgetingKeys.encumbrances.all, 'list', filters] as const,
    detail: (id: number) => [...budgetingKeys.encumbrances.all, 'detail', id] as const,
    availability: (appropriationId: number) => [...budgetingKeys.encumbrances.all, 'availability', appropriationId] as const,
  },
};
```

## Client Functions

### BudgetTypesClient

| Function | Method | Endpoint | Permission | Body/Query |
|----------|--------|----------|------------|------------|
| `getBudgetTypesList(isActive?)` | GET | `/api/BudgetTypes` | BudgetTypes.View | `?isActive?` |
| `getBudgetTypeById(id)` | GET | `/api/BudgetTypes/{id}` | BudgetTypes.View | — |
| `createBudgetType(data)` | POST | `/api/BudgetTypes` | BudgetTypes.Create | `{ code, name, description?, controlMethod, allowOverrun }` → 201 `{ id }` |
| `updateBudgetType(id, data)` | PUT | `/api/BudgetTypes/{id}` | BudgetTypes.Update | `{ id, rowVersion, ... }` → 204 |
| `toggleBudgetTypeActive(id, data)` | POST | `/api/BudgetTypes/{id}/toggle-active` | BudgetTypes.Update | `{ id, rowVersion, isActive }` → 204 |

### FundsClient

| Function | Method | Endpoint | Permission | Body/Query |
|----------|--------|----------|------------|------------|
| `getFundsList(isActive?)` | GET | `/api/Funds` | Funds.View | `?isActive?` |
| `getFundById(id)` | GET | `/api/Funds/{id}` | Funds.View | — |
| `createFund(data)` | POST | `/api/Funds` | Funds.Create | `{ fundNumber, fundName, ... }` → 201 `{ id }` |
| `updateFund(id, data)` | PUT | `/api/Funds/{id}` | Funds.Update | `{ id, rowVersion, ... }` → 204 |
| `activateFund(id, data)` | POST | `/api/Funds/{id}/activate` | Funds.Activate | `{ id, rowVersion }` → 204 |
| `deactivateFund(id, data)` | POST | `/api/Funds/{id}/deactivate` | Funds.Deactivate | `{ id, rowVersion }` → 204 |

### BudgetClassificationsClient (for shared types completeness)

| Function | Method | Endpoint | Permission |
|----------|--------|----------|------------|
| `getBudgetClassificationsTree()` | GET | `/api/BudgetClassifications/tree` | BudgetClassifications.View |
| `getBudgetClassificationById(id)` | GET | `/api/BudgetClassifications/{id}` | BudgetClassifications.View |
| `createBudgetClassification(data)` | POST | `/api/BudgetClassifications` | BudgetClassifications.Create |
| `updateBudgetClassification(id, data)` | PUT | `/api/BudgetClassifications/{id}` | BudgetClassifications.Update |
| `toggleBudgetClassificationActive(id, data)` | POST | `/api/BudgetClassifications/{id}/toggle-active` | BudgetClassifications.Update |

### BudgetsClient (for shared types completeness)

| Function | Method | Endpoint | Permission |
|----------|--------|----------|------------|
| `getBudgetsList(filters?)` | GET | `/api/Budgets` | Budgets.View |
| `getBudgetById(id)` | GET | `/api/Budgets/{id}` | Budgets.View |
| `createBudget(data)` | POST | `/api/Budgets` | Budgets.Create |
| `updateBudget(id, data)` | PUT | `/api/Budgets/{id}` | Budgets.Update |
| `submitBudget(id, data)` | POST | `/api/Budgets/{id}/submit` | Budgets.Submit |
| `approveBudget(id, data)` | POST | `/api/Budgets/{id}/approve` | Budgets.Approve |
| `activateBudget(id, data)` | POST | `/api/Budgets/{id}/activate` | Budgets.Activate |
| `suspendBudget(id, data)` | POST | `/api/Budgets/{id}/suspend` | Budgets.Suspend |
| `closeBudget(id, data)` | POST | `/api/Budgets/{id}/close` | Budgets.Close |
| `cancelBudget(id, data)` | POST | `/api/Budgets/{id}/cancel` | Budgets.Cancel |
| `getBudgetItemsTree(budgetId)` | GET | `/api/Budgets/{id}/items/tree` | Budgets.View |
| `createBudgetItem(budgetId, data)` | POST | `/api/Budgets/{id}/items` | BudgetItems.Create |
| `getBudgetItemById(itemId)` | GET | `/api/Budgets/items/{itemId}` | BudgetItems.View |
| `updateBudgetItem(itemId, data)` | PUT | `/api/Budgets/items/{itemId}` | BudgetItems.Update |
| `deleteBudgetItem(itemId, data)` | DELETE | `/api/Budgets/items/{itemId}` | BudgetItems.Delete |
| `moveBudgetItem(itemId, data)` | POST | `/api/Budgets/items/{itemId}/move` | BudgetItems.Update |

### AppropriationsClient (for shared types completeness)

| Function | Method | Endpoint | Permission |
|----------|--------|----------|------------|
| `getAppropriationsList(filters?)` | GET | `/api/Appropriations` | Appropriations.View |
| `getAppropriationById(id)` | GET | `/api/Appropriations/{id}` | Appropriations.View |
| `getAvailabilityForItem(budgetItemId)` | GET | `/api/Appropriations/availability?budgetItemId={id}` | Appropriations.View |
| `createAppropriation(data)` | POST | `/api/Appropriations` | Appropriations.Create |
| `updateAppropriation(id, data)` | PUT | `/api/Appropriations/{id}` | Appropriations.Update |
| `deleteAppropriation(id, data)` | DELETE | `/api/Appropriations/{id}` | Appropriations.Delete |
| `submitAppropriation(id, data)` | POST | `/api/Appropriations/{id}/submit` | Appropriations.Submit |
| `approveAppropriation(id, data)` | POST | `/api/Appropriations/{id}/approve` | Appropriations.Approve |
| `activateAppropriation(id, data)` | POST | `/api/Appropriations/{id}/activate` | Appropriations.Activate |
| `suspendAppropriation(id, data)` | POST | `/api/Appropriations/{id}/suspend` | Appropriations.Suspend |
| `closeAppropriation(id, data)` | POST | `/api/Appropriations/{id}/close` | Appropriations.Close |
| `cancelAppropriation(id, data)` | POST | `/api/Appropriations/{id}/cancel` | Appropriations.Cancel |

### EncumbrancesClient (for shared types completeness)

| Function | Method | Endpoint | Permission |
|----------|--------|----------|------------|
| `getEncumbrancesList(filters?)` | GET | `/api/Encumbrances` | Encumbrances.View |
| `getEncumbranceById(id)` | GET | `/api/Encumbrances/{id}` | Encumbrances.View |
| `getAvailabilityForEncumbrance(appropriationId)` | GET | `/api/Encumbrances/availability?appropriationId={id}` | Encumbrances.View |
| `createEncumbrance(data)` | POST | `/api/Encumbrances` | Encumbrances.Create |
| `submitEncumbrance(id, data)` | POST | `/api/Encumbrances/{id}/submit` | Encumbrances.Submit |
| `approveEncumbrance(id, data)` | POST | `/api/Encumbrances/{id}/approve` | Encumbrances.Approve |
| `activateEncumbrance(id, data)` | POST | `/api/Encumbrances/{id}/activate` | Encumbrances.Activate |
| `releaseEncumbrance(id, data)` | POST | `/api/Encumbrances/{id}/release` | Encumbrances.Release |
| `liquidateEncumbrancePartial(id, data)` | POST | `/api/Encumbrances/{id}/liquidate-partial` | Encumbrances.Release |
| `liquidateEncumbranceFull(id, data)` | POST | `/api/Encumbrances/{id}/liquidate-full` | Encumbrances.Release |
| `closeEncumbrance(id, data)` | POST | `/api/Encumbrances/{id}/close` | Encumbrances.Close |
| `cancelEncumbrance(id, data)` | POST | `/api/Encumbrances/{id}/cancel` | Encumbrances.Cancel |
| `reverseEncumbrance(id, data)` | POST | `/api/Encumbrances/{id}/reverse` | Encumbrances.Reverse |

## Error Handling

All client functions surface errors as structured `ProblemDetails`:

```ts
interface ProblemDetails {
  type?: string;
  title?: string;
  status: number;
  detail?: string;
  errors?: Record<string, string[]>;
}
```

Status code semantics:
- 400: Validation failure ( ValidationProblemDetails with `errors` map)
- 401: Anonymous access (no token)
- 403: Wrong permission
- 404: Resource not found
- 409: RowVersion concurrency conflict (distinct `title`)

## Conventions

- All mutating functions send `rowVersion` in the request body
- POST creates return 201 with `{ id }`
- PUT updates and POST transitions return 204 (no body)
- GET list returns 200 with array of DTOs
- GET detail returns 200 with single DTO
- All responses are JSON
