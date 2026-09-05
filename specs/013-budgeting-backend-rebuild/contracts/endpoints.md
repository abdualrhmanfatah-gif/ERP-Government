# API Contracts: Rebuild Budgeting Module Backend

**Date**: 2026-09-05
**Feature**: 013-budgeting-backend-rebuild

All endpoints follow REST conventions with ProblemDetails error responses. Authentication required for all endpoints (Bearer token). Authorization via policy-based permissions.

## Base URL
```
/api
```

## BudgetTypes

### GET /api/BudgetTypes
List all budget types. Requires `BudgetTypes.Read`.

**Response**: `200 OK`
```json
[
  {
    "id": 1,
    "code": "CAPEX",
    "name": "Capital Expenditure",
    "description": "Capital budget type",
    "controlMethod": 2,
    "allowOverrun": false,
    "isActive": true,
    "rowVersion": "AAAAAAAAB9k=",
    "created": "2026-09-05T00:00:00Z",
    "createdBy": "admin"
  }
]
```

### GET /api/BudgetTypes/{id}
Get budget type by ID. Requires `BudgetTypes.Read`.

### POST /api/BudgetTypes
Create budget type. Requires `BudgetTypes.Create`.

**Request**:
```json
{
  "code": "CAPEX",
  "name": "Capital Expenditure",
  "description": "Capital budget type",
  "controlMethod": 2,
  "allowOverrun": false
}
```

**Response**: `201 Created` with Location header

### PUT /api/BudgetTypes/{id}
Update budget type. Requires `BudgetTypes.Update`.

### PATCH /api/BudgetTypes/{id}/toggle-active
Toggle IsActive. Requires `BudgetTypes.ToggleActive`.

---

## Funds

### GET /api/Funds
List all funds. Requires `Funds.Read`.

### GET /api/Funds/{id}
Get fund by ID. Requires `Funds.Read`.

### POST /api/Funds
Create fund. Requires `Funds.Create`.

### PUT /api/Funds/{id}
Update fund. Requires `Funds.Update`.

### PATCH /api/Funds/{id}/toggle-active
Toggle IsActive. Requires `Funds.ToggleActive`.

---

## BudgetClassifications

### GET /api/BudgetClassifications
List all classifications (flat). Requires `BudgetClassifications.Read`.

### GET /api/BudgetClassifications/tree
Get classifications as hierarchical tree. Level computed. Requires `BudgetClassifications.Read`.

**Response**:
```json
[
  {
    "id": 1,
    "code": "1000",
    "name": "Revenue",
    "parentId": null,
    "level": 1,
    "isActive": true,
    "children": [
      {
        "id": 2,
        "code": "1100",
        "name": "Tax Revenue",
        "parentId": 1,
        "level": 2,
        "isActive": true,
        "children": []
      }
    ]
  }
]
```

### GET /api/BudgetClassifications/{id}
Get classification by ID. Requires `BudgetClassifications.Read`.

### POST /api/BudgetClassifications
Create classification. Requires `BudgetClassifications.Create`.

### PUT /api/BudgetClassifications/{id}
Update classification. Requires `BudgetClassifications.Update`.

### PATCH /api/BudgetClassifications/{id}/toggle-active
Toggle IsActive. Requires `BudgetClassifications.ToggleActive`.

---

## Budgets

### GET /api/Budgets
List budgets. Requires `Budgets.Read`.

### GET /api/Budgets/{id}
Get budget by ID with derived fields (AllowOverrun effective). Requires `Budgets.Read`.

### POST /api/Budgets
Create budget. Requires `Budgets.Create`.

**Request**:
```json
{
  "budgetName": "FY2026 Operating Budget",
  "budgetTypeId": 1,
  "fiscalYearId": 1,
  "fundId": 1,
  "totalAmount": 1000000.00,
  "effectiveFrom": "2026-01-01",
  "effectiveTo": "2026-12-31",
  "description": "Annual operating budget"
}
```

**Response**: `201 Created`

### PUT /api/Budgets/{id}
Update budget (Draft only). Requires `Budgets.Update`.

### POST /api/Budgets/{id}/submit
Submit budget. Requires `Budgets.Submit`.

### POST /api/Budgets/{id}/approve
Approve budget. Requires `Budgets.Approve`.

### POST /api/Budgets/{id}/activate
Activate budget. Requires `Budgets.Activate`.

### POST /api/Budgets/{id}/suspend
Suspend budget. Requires `Budgets.Suspend`.

### POST /api/Budgets/{id}/close
Close budget. Requires `Budgets.Close`.

### POST /api/Budgets/{id}/cancel
Cancel budget. Requires `Budgets.Cancel`.

### GET /api/Budgets/{budgetId}/items
List budget items for a budget. Requires `BudgetItems.Read`.

### POST /api/Budgets/{budgetId}/items
Create budget item. Requires `BudgetItems.Create`.

### PUT /api/Budgets/{budgetId}/items/{itemId}
Update budget item. Requires `BudgetItems.Update`.

### DELETE /api/Budgets/{budgetId}/items/{itemId}
Delete budget item (unused, prefer toggle-active). Requires `BudgetItems.Delete`.

### PATCH /api/Budgets/{budgetId}/items/{itemId}/toggle-active
Toggle IsActive. Requires `BudgetItems.ToggleActive`.

---

## Appropriations

### GET /api/Appropriations
List appropriations. Requires `Appropriations.Read`.

### GET /api/Appropriations/{id}
Get appropriation by ID with derived context (Fund, FiscalYear via Budget join). Requires `Appropriations.Read`.

**Response**:
```json
{
  "id": 1,
  "appropriationNumber": "APR-000001",
  "budgetId": 1,
  "budgetItemId": 1,
  "appropriationType": 0,
  "documentType": "PurchaseRequest",
  "documentId": 100,
  "amount": 50000.00,
  "status": 3,
  "fundNumber": "F001",
  "fundName": "General Fund",
  "fiscalYearCode": "FY2026",
  "itemCode": "ITM-001",
  "itemName": "Office Supplies",
  "rowVersion": "AAAAAAAAB9k=",
  "created": "2026-09-05T00:00:00Z",
  "createdBy": "budget_officer"
}
```

### POST /api/Appropriations
Create appropriation. Requires `Appropriations.Create`. Availability check enforced on Active status transition.

**Request**:
```json
{
  "budgetId": 1,
  "budgetItemId": 1,
  "appropriationType": 0,
  "documentType": "PurchaseRequest",
  "documentId": 100,
  "amount": 50000.00
}
```

### PUT /api/Appropriations/{id}
Update appropriation (Draft only). Requires `Appropriations.Update`.

### DELETE /api/Appropriations/{id}
Delete appropriation (Draft only). Requires `Appropriations.Delete`.

### POST /api/Appropriations/{id}/submit
Submit appropriation. Requires `Appropriations.Submit`.

### POST /api/Appropriations/{id}/approve
Approve appropriation. Requires `Appropriations.Approve`.

### POST /api/Appropriations/{id}/activate
Activate appropriation. Requires `Appropriations.Activate`.

### POST /api/Appropriations/{id}/suspend
Suspend appropriation. Requires `Appropriations.Suspend`.

### POST /api/Appropriations/{id}/close
Close appropriation. Requires `Appropriations.Close`.

### POST /api/Appropriations/{id}/cancel
Cancel appropriation. Requires `Appropriations.Cancel`.

### POST /api/Appropriations/{id}/reverse
Reverse appropriation (creates new Adjustment row with negative Amount). Requires `Appropriations.Reverse`.

---

## Encumbrances

### GET /api/Encumbrances
List encumbrances. Requires `Encumbrances.Read`.

### GET /api/Encumbrances/{id}
Get encumbrance by ID with derived context (Budget, BudgetItem, Fund, FiscalYear via Appropriation->Budget join) and IsReversed computed. Requires `Encumbrances.Read`.

**Response**:
```json
{
  "id": 1,
  "encumbranceNumber": "ENC-000001",
  "encumbranceType": 0,
  "appropriationId": 1,
  "vendorId": 5,
  "purchaseOrderId": null,
  "documentType": "PurchaseOrder",
  "documentId": 200,
  "description": "Office furniture order",
  "encumbranceDate": "2026-09-05",
  "amount": 25000.00,
  "status": 3,
  "isReversed": false,
  "budgetNumber": "BGT-000001",
  "budgetName": "FY2026 Operating Budget",
  "itemCode": "ITM-001",
  "fundNumber": "F001",
  "fiscalYearCode": "FY2026",
  "rowVersion": "AAAAAAAAB9k=",
  "created": "2026-09-05T00:00:00Z",
  "createdBy": "procurement_officer"
}
```

### GET /api/Encumbrances/availability?appropriationId={id}
Get available amount for encumbrance against a specific appropriation. Resolves BudgetItem from AppropriationId. Requires `Encumbrances.Read`.

**Response**:
```json
{
  "appropriationId": 1,
  "budgetItemId": 1,
  "netAppropriated": 50000.00,
  "totalEncumbered": 30000.00,
  "availableForEncumbrance": 20000.00,
  "controlMethod": 2
}
```

### POST /api/Encumbrances
Create encumbrance. Requires `Encumbrances.Create`. Availability check enforced.

### POST /api/Encumbrances/{id}/approve
Approve encumbrance. Requires `Encumbrances.Approve`. Availability check enforced.

### POST /api/Encumbrances/{id}/activate
Activate encumbrance. Requires `Encumbrances.Activate`.

### POST /api/Encumbrances/{id}/release
Partially release encumbrance. Requires `Encumbrances.Release`.

### POST /api/Encumbrances/{id}/liquidate
Partially or fully liquidate encumbrance. Requires `Encumbrances.Liquidate`.

### POST /api/Encumbrances/{id}/cancel
Cancel encumbrance (Draft/PendingApproval only). Requires `Encumbrances.Cancel`.

### POST /api/Encumbrances/{id}/reverse
Reverse encumbrance (creates new row with ReversalOfId). Requires `Encumbrances.Reverse`.

---

## Error Responses

All errors use RFC 7807 ProblemDetails:

```json
{
  "type": "https://tools.ietf.org/html/rfc7807",
  "title": "Validation Error",
  "status": 400,
  "errors": {
    "Code": ["The Code field is required."],
    "Name": ["The Name field must not exceed 200 characters."]
  }
}
```

Business rule failures:
```json
{
  "type": "https://tools.ietf.org/html/rfc7807",
  "title": "Business Rule Violation",
  "status": 400,
  "detail": "Encumbrance amount exceeds available budget. Available: 20000.00, Requested: 25000.00"
}
```

Authorization failures:
- `401 Unauthorized` — No token or invalid token
- `403 Forbidden` — Valid token but missing required permission
