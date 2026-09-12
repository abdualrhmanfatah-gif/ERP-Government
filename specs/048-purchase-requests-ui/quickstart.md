# Quickstart: Purchase Requests UI Validation

**Date**: 2026-09-11

## Prerequisites

- Backend running on https://localhost:5001 (or configured port)
- Frontend dev server: `npm run dev` from `src/Web/ClientApp`
- User logged in with procurement permissions
- At least 1 Item and 1 Unit exist in the database

## Validation Scenarios

### SC1: List Page (FR-001 to FR-006)

1. Navigate to `/procurement/purchase-requests`
2. **Expected**: Table with columns, skeleton while loading
3. Type a request number in search → **Expected**: Filtered results
4. Select status filter → **Expected**: Filtered by status
5. Select priority filter → **Expected**: Filtered by priority
6. Click pagination → **Expected**: Next page loads
7. If no results → **Expected**: Empty state "لا توجد طلبات شراء"

### SC2: Create Purchase Request (FR-007 to FR-011)

1. Click "طلب شراء جديد" on list page (or navigate to `/procurement/purchase-requests/create`)
2. **Expected**: Empty form with request date, priority selector, empty lines table
3. Select item from combobox → **Expected**: Searchable dropdown with item code + name
4. Select unit from combobox → **Expected**: Searchable dropdown
5. Enter quantity → **Expected**: Line total computed (qty × unitCost)
6. Click "إضافة بند" → **Expected**: New empty line row
7. Click remove on a line → **Expected**: Line removed
8. Click "حفظ" → **Expected**: Validation errors if invalid; success → navigate to detail page

### SC3: Detail Page (FR-013 to FR-016)

1. Navigate to `/procurement/purchase-requests/:id`
2. **Expected**: Header with request number (LTR), status badge, priority badge
3. **Expected**: Basic info card with dates, department, cost center, estimated total
4. **Expected**: Lines table with item, unit, qty, cost, total
5. Draft request → **Expected**: Submit and Edit buttons visible
6. Submitted request → **Expected**: Approve and Reject buttons visible

### SC4: Edit Purchase Request (FR-012)

1. Navigate to `/procurement/purchase-requests/:id/edit` with Draft request
2. **Expected**: Form pre-populated with existing data
3. Modify a field → **Expected**: Changes saved on submit
4. Navigate to edit page for non-Draft request → **Expected**: Redirect to detail page

### SC5: Lifecycle Actions (FR-016 to FR-021)

1. On Draft detail → Click "تقديم" → **Expected**: Status changes to Submitted
2. On Submitted detail → Click "اعتماد" → **Expected**: Status changes to Approved
3. On Submitted detail → Click "رفض" → **Expected**: Dialog asks for reason, must provide reason
4. On Approved detail → Click "إلغاء" → **Expected**: Dialog asks for confirmation
5. Double-click button → **Expected**: Button disabled during mutation

### SC6: RTL & Arabic (FR-023, FR-024, SC-006)

1. All text is Arabic
2. No physical CSS properties (ml/mr/pl/pr) in source
3. Logical properties only (ms-/me-/ps-/pe-)
4. Request numbers render LTR
5. Amounts show "ر.ي" suffix with tabular-nums

## Commands

```bash
# Frontend lint
cd src/Web/ClientApp && npm run lint

# Frontend build
cd src/Web/ClientApp && npm run build

# Regenerate API client (after any endpoint changes)
cd src/Web/ClientApp && npm run generate-api
```
