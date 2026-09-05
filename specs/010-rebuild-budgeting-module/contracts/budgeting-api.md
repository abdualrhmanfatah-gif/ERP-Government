# Contract: Budgeting HTTP API

**Feature**: `010-rebuild-budgeting-module` | Base: `/api` | Auth: `RequireAuthorization(PermissionCodes.*)` on every route; `[Authorize(Policy=...)]` on every use case. Anonymous → 401; wrong permission → 403 (ProblemDetails). Mutating calls round-trip `rowVersion`. `operationId` = handler method name (auto-derived by `EndpointRouteBuilderExtensions`).

Conventions: `POST /{id}/{action}` transitions with `{ id, rowVersion, ... }` bodies and route/body ID-equality checks; `PUT /{id}` full update (Draft-only where specified); `DELETE /{id}` with `rowVersion` via query/body per existing patterns; validation failures → 400 `ValidationProblemDetails`; missing → 404; concurrency conflict → 409-style distinct ProblemDetails.

## BudgetTypes — `/api/BudgetTypes`

| Verb & route | operationId | Permission | Body / query |
|---|---|---|---|
| GET `/` | GetBudgetTypesList | BudgetTypes.View | `?isActive?` |
| GET `/{id}` | GetBudgetTypeById | BudgetTypes.View | — |
| POST `/` | CreateBudgetType | BudgetTypes.Create | `{ code, name, description?, controlMethod, allowOverrun }` → `201 { id }` |
| PUT `/{id}` | UpdateBudgetType | BudgetTypes.Update *(new code; R8)* | `{ id, rowVersion, ... }` → 204 |
| POST `/{id}/toggle-active` | ToggleBudgetTypeActive | BudgetTypes.Update | `{ id, rowVersion, isActive }` → 204 |

`BudgetControlMethod`: 0=None, 1=Warning, 2=Blocking.

## Funds — `/api/Funds` (schema unchanged)

| Verb & route | operationId | Permission |
|---|---|---|
| GET `/`, GET `/{id}` | GetFundsList, GetFundById | Funds.View |
| POST `/` | CreateFund | Funds.Create |
| PUT `/{id}` | UpdateFund | Funds.Update |
| POST `/{id}/activate`, `/{id}/deactivate` | ActivateFund, DeactivateFund | Funds.Activate, Funds.Deactivate |

## BudgetClassifications — `/api/BudgetClassifications`

| Verb & route | operationId | Permission |
|---|---|---|
| GET `/tree` | GetBudgetClassificationsTree | BudgetClassifications.View |
| GET `/{id}` | GetBudgetClassificationById | BudgetClassifications.View |
| POST `/` | CreateBudgetClassification | BudgetClassifications.Create |
| PUT `/{id}` | UpdateBudgetClassification | BudgetClassifications.Update |
| POST `/{id}/toggle-active` | ToggleBudgetClassificationActive | BudgetClassifications.Update *(R8)* |

Tree nodes carry computed `level`; cycle/parent-cross-tree violations → 400.

## Budgets — `/api/Budgets`

| Verb & route | operationId | Permission |
|---|---|---|
| GET `/` | GetBudgetsList | Budgets.View |
| GET `/{id}` | GetBudgetById | Budgets.View |
| POST `/` | CreateBudget | Budgets.Create — server assigns `BudgetNumber` (BGT sequence); body has no number |
| PUT `/{id}` | UpdateBudget | Budgets.Update *(new code; R8)* |
| POST `/{id}/submit` | SubmitBudget | Budgets.Submit |
| POST `/{id}/approve` | ApproveBudget | Budgets.Approve (+ ApprovalHistory snapshot) |
| POST `/{id}/activate` | ActivateBudget | Budgets.Activate |
| POST `/{id}/suspend` | SuspendBudget | Budgets.Suspend |
| POST `/{id}/close` | CloseBudget | Budgets.Close |
| POST `/{id}/cancel` | CancelBudget | Budgets.Cancel |
| GET `/{id}/items/tree` | GetBudgetItemsTree | Budgets.View |
| POST `/{id}/items` | CreateBudgetItem | BudgetItems.Create |
| GET `/items/{itemId}` | GetBudgetItemById | BudgetItems.View |
| PUT `/items/{itemId}` | UpdateBudgetItem | BudgetItems.Update |
| DELETE `/items/{itemId}` | DeleteBudgetItem | BudgetItems.Delete *(new code; R8)* |
| POST `/items/{itemId}/move` | MoveBudgetItem | BudgetItems.Update |

## Appropriations — `/api/Appropriations`

| Verb & route | operationId | Permission |
|---|---|---|
| GET `/` | GetAppropriationsList | Appropriations.View |
| GET `/{id}` | GetAppropriationById | Appropriations.View |
| GET `/availability?budgetItemId={id}` | GetAvailabilityForItem | Appropriations.View |
| POST `/` | CreateAppropriation | Appropriations.Create — server assigns `AppropriationNumber` (APR); runs availability check |
| PUT `/{id}` | UpdateAppropriation | Appropriations.Update *(new; Draft-only)* |
| DELETE `/{id}` | DeleteAppropriation | Appropriations.Delete *(new; Draft-only)* |
| POST `/{id}/submit` | SubmitAppropriation | Appropriations.Submit *(new)* |
| POST `/{id}/approve` | ApproveAppropriation | Appropriations.Approve (+ snapshot; runs availability check) |
| POST `/{id}/activate` | ActivateAppropriation | Appropriations.Activate *(new)* |
| POST `/{id}/suspend` | SuspendAppropriation | Appropriations.Suspend *(new)* |
| POST `/{id}/close` | CloseAppropriation | Appropriations.Close *(new)* |
| POST `/{id}/cancel` | CancelAppropriation | Appropriations.Cancel *(new)* |

No `Reverse` route (no reversal operation). `Adjustment` rows are created through `POST /` with `appropriationType: Adjustment` (signed amount). `Amount` sign rule: `Original/Supplement/Reduction > 0`; `Adjustment ≠ 0` (sign sets direction).

## Encumbrances — `/api/Encumbrances`

| Verb & route | operationId | Permission |
|---|---|---|
| GET `/` | GetEncumbrancesList | Encumbrances.View |
| GET `/{id}` | GetEncumbranceById | Encumbrances.View |
| GET `/availability?appropriationId={id}` | GetAvailabilityForEncumbrance | Encumbrances.View — resolves BudgetItem from the appropriation; totals cover the whole item |
| POST `/` | CreateEncumbrance | Encumbrances.Create — server assigns `EncumbranceNumber` (ENC); whole-item availability check |
| POST `/{id}/submit` | SubmitEncumbrance | Encumbrances.Submit *(new)* |
| POST `/{id}/approve` | ApproveEncumbrance | Encumbrances.Approve (+ snapshot; whole-item availability check) |
| POST `/{id}/activate` | ActivateEncumbrance | Encumbrances.Activate *(new)* |
| POST `/{id}/release` | ReleaseEncumbrance | Encumbrances.Release — `{ releaseAmount? }` (null = full) |
| POST `/{id}/liquidate-partial` | LiquidateEncumbrancePartial | Encumbrances.Release *(transition command; distinct route)* |
| POST `/{id}/liquidate-full` | LiquidateEncumbranceFull | Encumbrances.Release *(transition command; distinct route)* |
| POST `/{id}/close` | CloseEncumbrance | Encumbrances.Close *(new)* |
| POST `/{id}/cancel` | CancelEncumbrance | Encumbrances.Cancel *(new; Draft/PendingApproval only)* |
| POST `/{id}/reverse` | ReverseEncumbrance | Encumbrances.Reverse — `{ reversalReason }`; creates reversal row |

Status codes: 200 (queries/availability), 201 + `{ id }` (creates), 204 (transitions/updates), 400 validation/lifecycle/availability-blocking, 401 anonymous, 403 wrong permission, 404 missing, 409 concurrency conflict (distinct ProblemDetails title).
