# Implementation Plan: Budget Officer Workspace

**Branch**: `021-budget-officer-workspace` | **Date**: 2026-09-06 | **Spec**: [spec.md](spec.md)

## Summary

Transactional workspace for budget officers: budget CRUD + item tree, appropriations (including transfers), encumbrances with live availability, monthly plan editor, execution drill-down. UI-only — no backend changes.

## FIELD-COVERAGE TABLE

| Entity | Field | Screen | Component | Mode | Permission |
|--------|-------|--------|-----------|------|------------|
| Budget | budgetNumber | List, Detail | DataGrid, Header | Read-only | View |
| Budget | name | List, Detail, Create | DataGrid, Header, Input | R/W | View/Create/Update |
| Budget | fiscalYearId | List, Detail, Create | DataGrid, Header, Select | R/W (create) | View/Create |
| Budget | fundId | List, Detail, Create | DataGrid, Header, Select | R/W (create) | View/Create |
| Budget | typeId | List, Detail, Create | DataGrid, Header, Select | R/W (create) | View/Create |
| Budget | status | List, Detail | StatusBadge | Read-only | View |
| BudgetItem | itemCode | Tree, Edit | Tree node, Input | R/W | View/Create/Update |
| BudgetItem | itemName | Tree, Edit | Tree node, Input | R/W | View/Create/Update |
| BudgetItem | parentId | Tree (move) | Tree drag | R/W (Draft) | Move |
| BudgetItem | totalAppropriated | Detail (drill-down) | Computed | Read-only | View |
| BudgetItem | totalEncumbered | Detail (drill-down) | Computed | Read-only | View |
| BudgetItem | totalAvailable | Detail (drill-down) | Computed | Read-only | View |
| Appropriation | number | List, Detail | DataGrid, Header | Read-only | View |
| Appropriation | itemId | Create | Select | R/W (create) | Create |
| Appropriation | type | List, Create | DataGrid, Select | R/W (create) | View/Create |
| Appropriation | amount | List, Create, Edit | DataGrid, Input | R/W (Draft) | View/Create/Update |
| Appropriation | status | List, Detail | StatusBadge | Read-only | View |
| Appropriation | targetBudgetItemId | Create (Transfer) | Select | R/W (create) | Create |
| Encumbrance | number | List, Detail | DataGrid, Header | Read-only | View |
| Encumbrance | itemId | Create | Select | R/W (create) | Create |
| Encumbrance | vendorPartyId | Create | Select | R/W (create) | Create |
| Encumbrance | amount | List, Create, Edit | DataGrid, Input | R/W (Draft) | View/Create/Update |
| Encumbrance | encumbranceDate | Create | Input | R/W (create) | Create |
| Encumbrance | status | List, Detail | StatusBadge | Read-only | View |
| MonthlyPlan | month 1-12 | Detail (plan editor) | 12-column editor | R/W (client-side) | View |

## DESIGN SECTION

### Budgets List
- DataGrid: budgetNumber, name, fiscalYear, fund, status badge, audit
- FilterBar: status, fiscal year, fund

### Budget Detail
- Header card: budgetNumber + name + status badge + lifecycle actions
- Item tree panel: expand/collapse, inline edit, add/delete (Draft), move (Draft)
- Drill-down: item → appropriations → encumbrances with totals

### Appropriations
- Create form: type (Original/Supplement/Reduction/Transfer), amount, item picker
- Transfer: target item picker (excludes source + Closed/Cancelled)
- Availability indicator: green/amber/red per controlMethod

### Encumbrances
- Create form: item, vendor, amount, date
- Live availability indicator: re-fetches on amount change

### Monthly Plan
- 12-column editor (Jan–Dec), total display, variance badge

## STATE MATRIX

| Page | Loading | Empty | Error | Unauthorized | Not Found | Normal |
|------|---------|-------|-------|--------------|-----------|--------|
| Budgets List | Skeleton | "لا توجد موازنات" | Toast | Guard | N/A | DataGrid |
| Budget Detail | Skeleton | N/A | Error card | Guard | Not found msg | Info + tree + actions |
| Budget Create | N/A | N/A | Toast | Guard | N/A | Form |
| Appropriations | Skeleton | "لا توجد تحويلات" | Toast | Guard | N/A | DataGrid + availability |
| Encumbrances | Skeleton | "لا توجد التزامات" | Toast | Guard | N/A | DataGrid + availability |

## TEST MAP

| Test ID | File | Description |
|---------|------|-------------|
| T-021-001 | BudgetsListPage.test.tsx | Status badges |
| T-021-002 | BudgetDetailPage.test.tsx | Item tree |
| T-021-003 | BudgetCreatePage.test.tsx | Form validates |
| T-021-004 | BudgetDetailPage.test.tsx | Lifecycle buttons |
| T-021-005 | BudgetItemTree.test.tsx | Add/edit/delete |
| T-021-006 | BudgetItemTree.test.tsx | Move (no cycle) |
| T-021-007 | AppropriationsListPage.test.tsx | Create all types |
| T-021-008 | AppropriationCreatePage.test.tsx | Transfer target |
| T-021-009 | EncumbrancesListPage.test.tsx | Availability |
| T-021-010 | EncumbranceCreatePage.test.tsx | Blocks exceeding |
| T-021-011 | ReverseDialog.test.tsx | Reason required |
| T-021-012 | MonthlyPlanEditor.test.tsx | 12-month persist |
| T-021-013 | MonthlyPlanEditor.test.tsx | Variance badge |
| T-021-014 | BudgetExecution.test.tsx | Totals per level |
| T-021-015 | AvailabilityIndicator.test.tsx | Green/Amber/Red |

## COMPLETENESS GATE

```
Budget: budgetNumber ✅ name ✅ fiscalYearId ✅ fundId ✅ typeId ✅ status ✅
BudgetItem: itemCode ✅ itemName ✅ parentId ✅ totalAppropriated ✅ totalEncumbered ✅ totalAvailable ✅
Appropriation: number ✅ itemId ✅ type ✅ amount ✅ status ✅ targetBudgetItemId ✅
Encumbrance: number ✅ itemId ✅ vendorPartyId ✅ amount ✅ encumbranceDate ✅ status ✅ reversalOfId ✅ reversalReason ✅
MonthlyPlan: month 1-12 ✅ (localStorage)
```

## Constitution Check

| Principle | Status |
|-----------|--------|
| I. Layered Architectural Integrity | ✅ PASS |
| II. Bounded Contexts | ✅ PASS |
| III. Server-Side Business-Rule Integrity | ✅ PASS |
| IV. Financial Integrity | ✅ PASS |
| V. Budget Control | ✅ PASS |
| VI. Data Integrity | ✅ PASS |
| VII. Authorization | ✅ PASS |
| VIII. Approval Workflows | ✅ PASS |
| IX. API and Frontend Contract Integrity | ✅ PASS |
| X. UI and Design System Consistency | ✅ PASS |
| XI. Testing, Verification, and Evidence | ✅ PASS |
| XII. Controlled Architectural Change | ✅ PASS |

**Gate**: PASS
