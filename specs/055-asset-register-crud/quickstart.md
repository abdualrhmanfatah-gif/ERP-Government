# Quickstart Validation: Asset Register CRUD

**Feature**: 055-asset-register-crud | **Date**: 2026-09-15

## Prerequisites

- .NET 9 SDK installed
- SQL Server running with latest migrations applied
- Node.js 18+ installed
- User account with `Assets.View` and `Assets.Create` permissions

## Validation Scenarios

### V1: Create Asset (FR-001, FR-002, FR-003)

1. Start the application (`dotnet run` from `src/Web`)
2. Navigate to `/assets` in the browser
3. Click "إضافة أصل جديد" (New Asset)
4. Fill: Name = "طابعة HP", AssetGroup = "أجهزة مكتبية", OriginalValue = 1500, PurchaseDate = 2025-01-15
5. Submit the form
6. **Expected**: Asset appears in list with code `AST-000001`, status "Draft"
7. **Verify**: Open asset detail — all fields display correctly

### V2: List & Filter (FR-005, FR-006)

1. Create 3+ assets with different groups and statuses
2. Navigate to asset list
3. **Expected**: Only Active assets shown by default
4. Click "عرض الكل" (Show All) — all assets appear
5. Filter by AssetGroup — only matching assets shown
6. Search by asset name — matching results appear
7. Search by barcode — matching result appears
8. **Expected**: Empty search shows "لا توجد أصول" message

### V3: Edit Asset (FR-007)

1. Open an asset in Draft status
2. Click "تعديل" (Edit)
3. Change Custodian and Location
4. Submit
5. **Expected**: Changes persist, detail shows updated values

### V4: Field Locking (FR-004)

1. Open an asset in Active status (requires F3 activation — test with manual status update for now)
2. Click "تعديل" (Edit)
3. **Expected**: OriginalValue, PurchaseDate, AssetGroup fields are read-only with lock icon
4. Attempt to change Name — this should succeed
5. **Expected**: Name updates, locked fields unchanged

### V5: Status Transition (FR-010)

1. Open an asset in Draft status
2. Attempt to change status directly to "Disposed"
3. **Expected**: Validation error — transition not allowed (Draft → Disposed is invalid)
4. Change status to Active (via edit or F3)
5. **Expected**: Status changes to Active

### V6: Deactivation (FR-008)

1. Open an Active asset
2. Click "تعطيل" (Deactivate)
3. Confirm the dialog
4. **Expected**: Status changes to Disposed
5. Return to list — asset not shown in default view
6. Click "عرض الكل" — disposed asset appears

### V7: Duplicate Rejection (FR-009)

1. Create an asset with AssetTag = "UNIQUE-001"
2. Create another asset with AssetTag = "UNIQUE-001"
3. **Expected**: Second creation fails with "الوسم مستخدم بالفعل" error

### V8: RTL Layout (FR-018)

1. Navigate to asset list — verify RTL layout
2. Open create form — verify RTL layout
3. Open detail page — verify RTL layout
4. **Expected**: All text right-aligned, fields flow RTL, no layout breakage

### V9: Loading Skeleton (FR-015a)

1. Open asset list with slow network (throttle in DevTools)
2. **Expected**: Skeleton/shimmer rows visible while loading
3. Data replaces skeletons when loaded

### V10: Delete Rejection (FR-016)

1. Send DELETE request to `/api/Assets/1` directly
2. **Expected**: 405 Method Not Allowed or clear message that deactivation is the supported operation

## Backend Unit Test Scenarios

### CreateAssetCommand

- Success: valid data → returns new ID, asset in DB with Draft status, Code starts with "AST-"
- Failure: duplicate AssetTag → returns DuplicateAssetTag error
- Failure: duplicate Barcode → returns DuplicateBarcode error
- Failure: invalid AssetGroupId → returns AssetGroupNotFound error
- Failure: DocumentSequenceService failure → returns user-friendly error

### UpdateAssetCommand

- Success: valid update → fields updated, RowVersion incremented
- Failure: stale RowVersion → returns ConcurrencyConflict
- Failure: edit OriginalValue on Active asset → returns FieldLockedAfterActivation
- Failure: invalid status transition → returns InvalidStatusTransition

### GetAssetsQuery

- Returns paginated results
- Filters by status (default Active)
- Filters by assetGroupId, locationId, custodianId, costCenterId, fundId
- Searches across Code, Name, AssetTag, Barcode, SerialNumber
- Returns empty list when no matches

### DeactivateAssetCommand

- Success: Active → Disposed
- Failure: Draft → Disposed → InvalidStatusTransition
- Failure: asset not found → AssetNotFound
