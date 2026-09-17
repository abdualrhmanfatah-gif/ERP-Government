# Quickstart Validation: Asset Groups CRUD

**Feature**: 054-asset-groups-crud
**Date**: 2026-09-14

## Prerequisites

- SQL Server running with `AssetGroups` table seeded (AG-001, AG-002, AG-003)
- Backend API running on `https://localhost:5001`
- Frontend dev server running on `https://localhost:5002`

## Validation Scenarios

### V1: View Asset Groups (List + Tree)

1. Open browser to `/assets/asset-groups`
2. **Expected**: List shows 3 seeded groups (AG-001, AG-002, AG-003)
3. **Expected**: Tree view shows 3 root nodes with expand/collapse
4. **Expected**: Search "مركبات" filters to AG-003 only
5. **Expected**: RTL layout, Arabic labels

### V2: Create Asset Group

1. Click "إضافة مجموعة" button
2. Fill: Code = "AG-004", Name = "أجهزة حاسوب", Parent = none
3. Fill: DepreciationMethod = "StraightLine", DefaultUsefulLifeYears = 4
4. Click save
5. **Expected**: Redirects to list; AG-004 appears as root node
6. **Expected**: Toast success message

### V3: Create Child Group

1. Click "إضافة مجموعة" button
2. Fill: Code = "AG-005", Name = "شاشات عرض", Parent = "AG-004" (أجهزة حاسوب)
3. Click save
4. **Expected**: AG-005 appears nested under AG-004 in tree

### V4: Edit Asset Group

1. Click on AG-004 in the list
2. Change Name to "أجهزة حاسوب ومعدات تقنية"
3. Fill GL account fields (select from dropdowns)
4. Click save
5. **Expected**: Name updated in list and tree
6. **Expected**: GL accounts saved

### V5: Deactivate Asset Group

1. Click on AG-005 (شاشات عرض)
2. Click "تعطيل" button
3. **Expected**: Confirmation dialog
4. Confirm
5. **Expected**: Group shows as inactive (muted styling, badge)
6. **Expected**: AG-005 not in asset creation dropdown

### V6: Reactivate Asset Group

1. Click on inactive AG-005
2. Click "تفعيل" button
3. **Expected**: Group becomes active, badge removed
4. **Expected**: AG-005 reappears in asset creation dropdown

### V7: Prevent Deactivation with Assets

1. Ensure AG-001 (مباني) has linked assets
2. Try to deactivate AG-001
3. **Expected**: Error: "لا يمكن تعطيل مجموعة مرتبطة بأصول"

### V8: Prevent Circular Reference

1. Create group AG-006 under AG-001
2. Try to edit AG-001 and set parent to AG-006
3. **Expected**: Error: "لا يمكن إنشاء حلقة في التسلسل الهرمي"

### V9: Prevent Max Depth

1. Create AG-007 under AG-006 (level 2)
2. Create AG-008 under AG-007 (level 3)
3. Try to create AG-009 under AG-008
4. **Expected**: Error: "تم تجاوز الحد الأقصى لمستويات التصنيف"

### V10: Duplicate Code Prevention

1. Try to create group with Code = "AG-001"
2. **Expected**: Error: "كود المجموعة مستخدم بالفعل"

### V11: Permission Enforcement

1. Log in as user without `AssetGroups.Create` permission
2. **Expected**: "إضافة مجموعة" button not visible
3. Try direct API call `POST /api/AssetGroups`
4. **Expected**: 403 Forbidden

### V12: Optimistic Concurrency

1. Open AG-001 in two browser tabs
2. Edit Name in tab 1, save
3. Edit Name in tab 2, save
4. **Expected**: Error in tab 2: "تم تعديل المجموعة من مستخدم آخر"

## API Smoke Test (cURL)

```bash
# List groups
curl -s https://localhost:5001/api/AssetGroups | jq .

# Create group
curl -s -X POST https://localhost:5001/api/AssetGroups \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer <token>" \
  -d '{"code":"AG-010","name":"تجربة","depreciationMethod":"StraightLine","assetCategory":"Tangible"}' | jq .

# Deactivate
curl -s -X POST https://localhost:5001/api/AssetGroups/1/deactivate \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer <token>" \
  -d '{"rowVersion":"AAAAAAAAB9k="}' | jq .
```

## Frontend Build Check

```bash
cd src/Web/ClientApp
npm run build    # Must pass with zero errors
npm run lint     # Must pass
```

## Backend Build Check

```bash
dotnet build src/Web/Web.csproj --no-restore
dotnet test tests/Unit/Assets/
dotnet test tests/Functional/Assets/
```
